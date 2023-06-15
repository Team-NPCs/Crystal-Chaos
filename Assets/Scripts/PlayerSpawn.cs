using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private float minDistance = 5f;

    void Start()
    {
        Debug.Log("Start.");
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Spawn.");
            GameObject[] spawnPositions = GameObject.FindGameObjectsWithTag("Respawn");
            Vector3 spawnPosition = FindValidSpawnPosition(spawnPositions);
            transform.position = spawnPosition;
        }
    }

    private Vector3 FindValidSpawnPosition(GameObject[] spawnPositions)
    {
        Vector3 position = Vector3.zero;
        bool validPosition = false;

        while (!validPosition)
        {
            int randomSpawnIndex = Random.Range(0, spawnPositions.Length);
            position = spawnPositions[randomSpawnIndex].transform.position;

            if (IsDistanceValid(position))
            {
                validPosition = true;
            }
        }

        return position;
    }

    private bool IsDistanceValid(Vector3 position)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(player.transform.position, position);
            if (distance < minDistance)
            {
                return false;
            }
        }

        return true;
    }
}