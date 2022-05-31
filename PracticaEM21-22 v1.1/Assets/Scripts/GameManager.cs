using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;

public class GameManager : NetworkBehaviour
{
    #region Variables
    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameObject [] playerPrefabs;
    [SerializeField] UIManager uiManager;
     GameObject[] respawns;
   public NetworkVariable<bool> canStart = new NetworkVariable<bool>();
    int lastRespawn;
    int maxPlayers=4;
    int minPlayers = 2;
    int deaths = 0;
    Dictionary<ulong,PlayerSettings> PlayersConnected = new Dictionary<ulong, PlayerSettings>();
    public NetworkVariable<int> currentPlayers = new NetworkVariable<int>();
    #endregion

    #region Callbacks
    private void Awake()
    {
        //Busca los puntos de respawn que hay en el mapa
        respawns = GameObject.FindGameObjectsWithTag("Respawn");
        currentPlayers.Value = 0;

        //Observa si se ha iniciado el servidor o si se han conectado clientes
        networkManager.OnServerStarted += OnServerReady;
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        uiManager.OnStartGame.AddListener(StartGameOnServerRpc);
        uiManager.OnPlayerDead.AddListener(OnDeadSignClientRpc);
        currentPlayers.OnValueChanged += OnStateChanged;

        canStart.Value = false;
    }

    private void OnDestroy()
    {
        networkManager.OnServerStarted -= OnServerReady;
        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        currentPlayers.OnValueChanged -= OnStateChanged;
        uiManager.OnStartGame.RemoveListener(StartGameOnServerRpc);
        uiManager.OnPlayerDead.AddListener(OnDeadSignClientRpc);
    }
    #endregion

    #region Server/Client Connection
    private void OnServerReady()
    {

    }

    private void OnClientConnected(ulong clientId)
    {
        // Se envia la skin elegida por el cliente al servidor
        if (networkManager.IsClient)
        {
            SendSkinChoiceToServerRpc(clientId,uiManager.skinChoice);
        }

        if (networkManager.IsServer)
        {
            currentPlayers.Value++;

            //Si se conecta un cliente nuevo y sobrepasa el limite de jugadores se le echa.
            if (currentPlayers.Value > maxPlayers)
            {
                var lastClient = networkManager.ConnectedClientsIds[currentPlayers.Value-1];
                DisconnectedByServer_ClientRpc(lastClient);
            }
            else
            {
                //Cuando el cliente se conecta, se le atribuye uno de los puntos de spawn de manera aleatoria
                int randomNumber = Random.Range(1, 7);
                if (randomNumber == lastRespawn)
                {
                    randomNumber = Random.Range(1, 7);
                }
                lastRespawn = randomNumber;
                
                //Los datos para poder instanciar al jugador se almacenan en PlayerSettings
                PlayerSettings playerSettings = new PlayerSettings("3", 0, randomNumber, clientId);

                //El servidor almacena los jugadores que se han conectado
                PlayersConnected.Add(clientId, playerSettings);

                UpdateLobbyOnClientRpc(currentPlayers.Value);
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        //si un cliente se desconecta se quita de la lista de jugadores en linea y se actualiza el lobby para borrarlo
        if (networkManager.IsServer)
        {
          PlayersConnected.Remove(clientId);
           currentPlayers.Value--;
         UpdateLobbyOnClientRpc(currentPlayers.Value);
        }
    }

    void OnStateChanged(int previous, int current)
    {
        // Se cuenta la cantidad maxima de jugador en el lobby que es 4 y el minimo para jugar que es 2
        if (current>=minPlayers && current <=maxPlayers)
        {
            canStart.Value = true;
            uiManager.activatePlay();
        }
        else
        {
            canStart.Value=false;
        }
    }
    #endregion

    #region ClientRPC
    // El servidor echa al cliente porque ha sobrepasado el limite, y le salta un aviso desde el uiManager
    [ClientRpc]
    void DisconnectedByServer_ClientRpc(ulong lastClientID)
    {
        if(networkManager.LocalClientId == lastClientID)
        {
            uiManager.ActivateLobbyFullWarning();
            NetworkManager.Singleton.Shutdown();
        }
    }

    // Se envia la actualizacion del lobby con la cantidad de personas conectadas
    [ClientRpc]
    void UpdateLobbyOnClientRpc(int currentPlayerIdx)
    {
        for (int i = 0; i < maxPlayers; i++)
        {
            if (i < currentPlayerIdx)
            {
                uiManager.setPlayerOnLobby(i);
            }
            else
            {
               uiManager.setNoneOnLobby(i);
            }
        }
        
    }

    // El servidor comunica al jugar muerto que le aparezca el mensaje de muerte
    [ClientRpc]
    void OnDeadSignClientRpc(ulong clientTarget)
    {
        if (clientTarget == networkManager.LocalClientId)
        {
            uiManager.ActivateDeadSign();
            //Una vez confirmada la muerte, desde el cliente se envá un mensaje al servidor confirmando la muerte para que la cuente.
            CountDeathsServerRpc();
        }
    }

    // Se comunica a los clientes que usen la interfaz de partida
    [ClientRpc]
    void StartGameOnClientRpc()
    {
        uiManager.ActivateInGameHUD();
    }

    // Se comunica a los clientes que la partida ya ha terminado para que en ellos se actualice la Uçinterfaz
    [ClientRpc]
    void GameEndedClientRpc()
    {
        uiManager.DeactivateDeadSign();
    }
    #endregion

    #region ServerRPC
    // El servidor recorre el diccionario que almacena a los jugadores y los instancia como Player Object
    [ServerRpc(RequireOwnership = false)]
    void StartGameOnServerRpc()
    {

        foreach (var item in PlayersConnected)
        {
            var client_id = item.Key;
            var plyrStt = item.Value;
            var player = Instantiate(playerPrefabs[plyrStt.skin], respawns[plyrStt.respawnID].transform);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client_id);
        }
        StartGameOnClientRpc();
    }

    //El servidor actualiza su conteo de muertes, si solo queda un jugador vivo, entonces la partida acaba y se lo comunica a los clientes
    [ServerRpc(RequireOwnership = false)]
    void CountDeathsServerRpc()
    {
        deaths++;
        if (networkManager.IsServer)
        {
            if (deaths == currentPlayers.Value - 1)
            {
                deaths = 0;
                canStart.Value = false;
                GameEndedClientRpc();
            }
        }
    }

    // Se registra la skin elegida por un cliente en el diccionario de jugadores del servidor
    [ServerRpc(RequireOwnership =false)]
    void SendSkinChoiceToServerRpc(ulong client, int skinChoosen)
    {
        PlayersConnected[client].skin = skinChoosen;
    }
    #endregion

}


//Clase que almacena las preferencias del jugador sobre su personaje
 class PlayerSettings
{
    public string name;
    public int skin;
    public int respawnID;
    public ulong clientID;

   public PlayerSettings(string _name, int _skin, int _respawnID, ulong _clientID)
    {
        this.name = _name;
        this.skin = _skin;
        this.respawnID = _respawnID;
        this.clientID = _clientID;
    }
    
}