using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameObject prefab;
    private GameObject[] respawns;
    int lastRespawn;
    private void Awake()
    {
        respawns = GameObject.FindGameObjectsWithTag("Respawn");
        networkManager.OnServerStarted += OnServerReady;
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        networkManager.OnServerStarted -= OnServerReady;
        networkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnServerReady()
    {

    }

    private void OnClientConnected(ulong clientId)
    {
        if (networkManager.IsServer)
        {

            int randomNumber = Random.Range(1, 7);
            if (randomNumber == lastRespawn)
            {
                randomNumber = Random.Range(1, 7);
            }
            lastRespawn = randomNumber;
            var player = Instantiate(prefab, respawns[randomNumber].transform);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
}
