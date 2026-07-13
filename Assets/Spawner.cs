using System;
using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [Serializable] 
    public class SpawnCommand
    {
        public GameObject prefab;
        public int amount;
        public float time;
    }

    public Vector3 point;
    public float radius;

    public List<SpawnCommand> spawnCommands = new List<SpawnCommand>();
    void Spawn(SpawnCommand command)
    {
        Vector2 random = UnityEngine.Random.insideUnitCircle * radius;
        Vector3 spawnPosition = point + new Vector3(random.x, 0, random.y);

        for (int i = 0; i < command.amount; i++)
        {
            Instantiate(command.prefab, spawnPosition, Quaternion.identity);
        }
    }
}
