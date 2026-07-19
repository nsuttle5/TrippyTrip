#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DrugRegistryAutoSync : AssetPostprocessor
{
    private const string ResourcesFolderPath = "Assets/Resources";
    private const string RegistryAssetPath = "Assets/Resources/DrugRegistry.asset";

    private static bool _isSyncing;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.delayCall += TrySyncRegistry;
    }

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (!ShouldSync(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths))
        {
            return;
        }

        TrySyncRegistry();
    }

    private static bool ShouldSync(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (ContainsRegistryPath(importedAssets) ||
            ContainsRegistryPath(deletedAssets) ||
            ContainsRegistryPath(movedAssets) ||
            ContainsRegistryPath(movedFromAssetPaths))
        {
            return true;
        }

        if (ContainsDrugAsset(importedAssets) || ContainsDrugAsset(movedAssets))
        {
            return true;
        }

        // Deleted/moved-from assets cannot be type-checked reliably by path,
        // so sync when any asset delete/move happens.
        return deletedAssets.Length > 0 || movedFromAssetPaths.Length > 0;
    }

    private static bool ContainsRegistryPath(string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            if (string.Equals(paths[i], RegistryAssetPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsDrugAsset(string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            Type typeAtPath = AssetDatabase.GetMainAssetTypeAtPath(paths[i]);
            if (typeAtPath == typeof(drug))
            {
                return true;
            }
        }

        return false;
    }

    private static void TrySyncRegistry()
    {
        if (_isSyncing)
        {
            return;
        }

        _isSyncing = true;
        try
        {
            DrugRegistry registry = LoadOrCreateRegistryAsset();
            SyncRegistryEntries(registry);
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private static DrugRegistry LoadOrCreateRegistryAsset()
    {
        DrugRegistry registry = AssetDatabase.LoadAssetAtPath<DrugRegistry>(RegistryAssetPath);
        if (registry != null)
        {
            return registry;
        }

        EnsureResourcesFolderExists();
        registry = ScriptableObject.CreateInstance<DrugRegistry>();
        AssetDatabase.CreateAsset(registry, RegistryAssetPath);
        AssetDatabase.ImportAsset(RegistryAssetPath);
        return registry;
    }

    private static void EnsureResourcesFolderExists()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
    }

    private static void SyncRegistryEntries(DrugRegistry registry)
    {
        List<ProposedEntry> proposedEntries = BuildProposedEntries();

        if (IsAlreadyInSync(registry, proposedEntries))
        {
            return;
        }

        SerializedObject serializedRegistry = new SerializedObject(registry);
        SerializedProperty entriesProperty = serializedRegistry.FindProperty("entries");
        entriesProperty.ClearArray();

        for (int i = 0; i < proposedEntries.Count; i++)
        {
            ProposedEntry proposed = proposedEntries[i];
            int newIndex = entriesProperty.arraySize;
            entriesProperty.InsertArrayElementAtIndex(newIndex);

            SerializedProperty entryProperty = entriesProperty.GetArrayElementAtIndex(newIndex);
            entryProperty.FindPropertyRelative("name").stringValue = proposed.name;
            entryProperty.FindPropertyRelative("drug").objectReferenceValue = proposed.drug;
        }

        serializedRegistry.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
    }

    private static List<ProposedEntry> BuildProposedEntries()
    {
        string[] drugGuids = AssetDatabase.FindAssets("t:drug");
        List<ProposedEntry> proposedEntries = new List<ProposedEntry>(drugGuids.Length);

        for (int i = 0; i < drugGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(drugGuids[i]);
            drug drugAsset = AssetDatabase.LoadAssetAtPath<drug>(assetPath);
            if (drugAsset == null)
            {
                continue;
            }

            string displayName = string.IsNullOrWhiteSpace(drugAsset.DrugName) ? drugAsset.name : drugAsset.DrugName;
            proposedEntries.Add(new ProposedEntry(displayName, drugAsset));
        }

        proposedEntries.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        return proposedEntries;
    }

    private static bool IsAlreadyInSync(DrugRegistry registry, List<ProposedEntry> proposedEntries)
    {
        IReadOnlyList<DrugRegistry.DrugEntry> currentEntries = registry.Entries;
        if (currentEntries.Count != proposedEntries.Count)
        {
            return false;
        }

        for (int i = 0; i < proposedEntries.Count; i++)
        {
            DrugRegistry.DrugEntry current = currentEntries[i];
            ProposedEntry proposed = proposedEntries[i];

            if (current.Drug != proposed.drug)
            {
                return false;
            }

            if (!string.Equals(current.Name, proposed.name, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private readonly struct ProposedEntry
    {
        public readonly string name;
        public readonly drug drug;

        public ProposedEntry(string name, drug drug)
        {
            this.name = name;
            this.drug = drug;
        }
    }
}
#endif
