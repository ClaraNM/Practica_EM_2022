using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{

    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameObject prefab;
    [SerializeField] UIManager uiManager;
    private GameObject[] respawns;
   public NetworkVariable<bool> canStart = new NetworkVariable<bool>();
    int lastRespawn;
    int maxPlayers=4;
    int minPlayers = 2;
    Dictionary<ulong,GameObject> PlayersConnected = new Dictionary<ulong, GameObject>();
    public NetworkVariable<int> currentPlayers = new NetworkVariable<int>();
    private void Awake()
    {
        //Busca los puntos de respawn que hay en el mapa
        respawns = GameObject.FindGameObjectsWithTag("Respawn");
        currentPlayers.Value = 0;
        //Observa si se ha iniciado el servidor o si se han conectado clientes
        networkManager.OnServerStarted += OnServerReady;
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        uiManager.OnStartGame.AddListener(SpawnPlayers);
        currentPlayers.OnValueChanged += OnStateChanged;
        canStart.Value = false;
    }

    private void OnDestroy()
    {
        networkManager.OnServerStarted -= OnServerReady;
        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        currentPlayers.OnValueChanged -= OnStateChanged;
        uiManager.OnStartGame.RemoveListener(SpawnPlayers);


    }
    private void OnServerReady()
    {

    }

    private void OnClientConnected(ulong clientId)
    {
        
        

        //si un cliente se conecta lo inicializa en uno de los puntos de spawn de manera aleatoria
        if (networkManager.IsServer)
        {
            int randomNumber = Random.Range(1, 7);
            if (randomNumber == lastRespawn)
            {
                randomNumber = Random.Range(1, 7);
            }
            lastRespawn = randomNumber;
            var player = Instantiate(prefab, respawns[randomNumber].transform);
            Debug.Log(clientId);
            PlayersConnected.Add(clientId, player);
            currentPlayers.Value++;
            UpdateLobbyOnClientRpc(currentPlayers.Value);
            // player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
    private void OnClientDisconnected(ulong clientId)
    {



        //si un cliente se conecta lo inicializa en uno de los puntos de spawn de manera aleatoria
        if (networkManager.IsServer)
        {
          PlayersConnected.Remove(clientId);
           currentPlayers.Value--;
            UpdateLobbyOnClientRpc(currentPlayers.Value);
            // player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
    void OnStateChanged(int previous, int current)
    {
        if (current>=minPlayers && current <=maxPlayers)
        {
            canStart.Value = true;
            uiManager.activatePlay();
        }
    }
    [ClientRpc]
    void UpdateLobbyOnClientRpc(int currentPlayerIdx)
    {
        for (int i = 1; i <= currentPlayerIdx; i++)
        {
            Debug.Log(currentPlayerIdx);
            uiManager.setPlayerOnLobby(i, prefab);
        }
        
    }
    void SpawnPlayers()
    {
        if (networkManager.IsServer)
        {
            foreach (var item in PlayersConnected)
            {
                var client_id = item.Key;
                var gObject = item.Value;
                gObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(client_id);
            }
        }
    }
}
