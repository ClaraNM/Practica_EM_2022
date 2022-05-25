using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameObject prefab;

    private void Awake()
    {
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
            var player = Instantiate(prefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
}
