using System;
using System.Collections.Generic;
using UnityEngine;

public class SideSpawner : MonoBehaviour
{

    // Z value the object will be destroyed
    [SerializeField]
    private float _destroyZ;

    // max X value objects can be spawned
    [SerializeField]
    private float _maxSpawnRange = 40.0f;
    // min X value objects can be spawned
    [SerializeField]
    private float _minSpawnRange = 8.0f;

    [Serializable]
    private class LevelObject
    {
        [SerializeField]
        public GameObject _levelPrefab;

        // percent chance the given object is spawned 
        [SerializeField, Range(0.0f, 0.05f)]
        public float _spawnFrequency = 0.02f;

        // min X value the item will spawn as a percentage of the whole spawn range
        [SerializeField, Range(0.0f, 1.0f)]
        public float _itemMinSpawnRange = 0.0f;

        // max X value the item will spawn as a percentage of the whole spawn range
        [SerializeField, Range(0.0f, 1.0f)]
        public float _itemMaxSpawnRange = 1.0f;
    }

    [Serializable]
    private class Level
    {
        [SerializeField]
        public List<LevelObject> _levelModels;
    }

    [SerializeField]
    private List<Level> _levels;

    private int m_level = 1;
    private bool m_spawnActive = true;
    private float m_spawnRange;

    // Makes sure the spawn ranges are valid
    // Otherwise swaps min and max vals
    void Start()
    {
        if (_minSpawnRange > _maxSpawnRange)
            (_minSpawnRange, _maxSpawnRange) = (_maxSpawnRange, _minSpawnRange);

        m_spawnRange = _maxSpawnRange - _minSpawnRange;

        foreach (Level level in _levels)
        {
            foreach (LevelObject levelObject in level._levelModels)
            {
                if (levelObject._itemMinSpawnRange > _maxSpawnRange)
                    (levelObject._itemMaxSpawnRange, levelObject._itemMinSpawnRange) = (levelObject._itemMinSpawnRange, levelObject._itemMaxSpawnRange);
            }
        }
    }

    void Update()
    {
        if (m_spawnActive)
        {
            SpawnLevel();
        }
    }

    private void SpawnLevel()
    {
        if (m_level > _levels.Count || _levels[m_level - 1]._levelModels == null)
            Debug.LogError("Level " + m_level + " does not have obstacles set.");

        foreach (LevelObject levelModel in _levels[m_level - 1]._levelModels)
        {
            System.Random rand = new System.Random();

            // spawns item on the left
            if (levelModel._spawnFrequency > rand.NextDouble())
            {
                Spawn(levelModel, false);
            }

            // spawns item on the right
            if (levelModel._spawnFrequency > rand.NextDouble())
            {
                Spawn(levelModel, true);
            }
        }
    }

    private void Spawn(LevelObject levelModel, bool right)
    {
        System.Random rand = new System.Random();
        int side = right ? 1 : -1;

        float minX = m_spawnRange * levelModel._itemMinSpawnRange;
        float maxX = m_spawnRange * levelModel._itemMaxSpawnRange;

        float xSpawnPos = ((maxX - minX) * (float)rand.NextDouble() + _minSpawnRange) * side;

        Instantiate(levelModel._levelPrefab, new(xSpawnPos, 0, transform.position.z), Quaternion.identity);
    }
}
