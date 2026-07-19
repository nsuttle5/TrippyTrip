using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DrugRegistry", menuName = "TrippyTrip/Drug Registry")]
public class DrugRegistry : ScriptableObject
{
    [SerializeField] private List<DrugEntry> entries = new List<DrugEntry>();

    private static DrugRegistry _cachedRegistry;

    [Serializable]
    public class DrugEntry
    {
        [SerializeField] private string name;
        [SerializeField] private drug drug;

        public string Name => name;
        public drug Drug => drug;
    }

    public IReadOnlyList<DrugEntry> Entries => entries;

    public static IReadOnlyList<DrugEntry> GetEntries()
    {
        return GetOrLoadRegistry().Entries;
    }

    public static bool TryGetDrug(string name, out drug drug)
    {
        DrugRegistry registry = GetOrLoadRegistry();
        IReadOnlyList<DrugEntry> list = registry.Entries;

        for (int i = 0; i < list.Count; i++)
        {
            DrugEntry entry = list[i];
            if (string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                drug = entry.Drug;
                return drug != null;
            }
        }

        drug = null;
        return false;
    }

    private static DrugRegistry GetOrLoadRegistry()
    {
        if (_cachedRegistry != null)
        {
            return _cachedRegistry;
        }

        _cachedRegistry = Resources.Load<DrugRegistry>("DrugRegistry");
        if (_cachedRegistry == null)
        {
            throw new InvalidOperationException("DrugRegistry not found. Create one via Create > TrippyTrip > Drug Registry and place it in a Resources folder.");
        }

        return _cachedRegistry;
    }
}
