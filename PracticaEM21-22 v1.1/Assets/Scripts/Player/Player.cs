using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using Unity.Collections; //Para poder utilizar FixedString64Bytes, ya que networkVariable no permite usar string
using UnityEngine.UI; // Para poder usar String de la UI

public class Player : NetworkBehaviour
{
   
    #region Variables

    // https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable
    public NetworkVariable<PlayerState> State;
    public NetworkVariable<FixedString32Bytes> Nombre; //Variable compartida del nombre del personaje.
    public Text textoEnCabeza; // Variable para conectar con el asset. En Unity enlazar el texto de la cabeza del personaje con esta variable.
    private GameObject[] respawns;


    //Vida
    public int NumHealth;

    public NetworkVariable<int> Vidas;
    public NetworkVariable<bool> isDead;

    public Player player;

    public NetworkVariable<int> Deaths;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        // NetworkManager.OnClientConnectedCallback += ConfigurePlayer;
        //ConfigurePlayer(this.OwnerClientId);
        respawns = GameObject.FindGameObjectsWithTag("Respawn");
        State = new NetworkVariable<PlayerState>();
        Nombre = new NetworkVariable<FixedString32Bytes>(); //Inicializamos el nombre


        //Vida
        Vidas = new NetworkVariable<int>(NumHealth);
        isDead = new NetworkVariable<bool>(false);
        Deaths = new NetworkVariable<int>(0);
    }
    private void Start()
    {
        ConfigurePlayer(this.OwnerClientId);
    }
    private void OnEnable()
    {
        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged += OnPlayerStateValueChanged;

        //Actualización de la vida
        Vidas.OnValueChanged += UpdateLives;

        Nombre.OnValueChanged += OnPlayerNombreValueChanged; //
    }

    private void OnDisable()
    {
        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged -= OnPlayerStateValueChanged;

        Vidas.OnValueChanged -= UpdateLives;

        Nombre.OnValueChanged -= OnPlayerNombreValueChanged; //
    }

    #endregion

    #region Config Methods

    void ConfigurePlayer(ulong clientID)
    {
        if (IsLocalPlayer)
        {
            ConfigurePlayer();
            ConfigureCamera();
            ConfigureControls();
            UIManager.Instance.UpdateLifeUI(NumHealth - Vidas.Value);
        }
        string thisName = UIManager.Instance.inputFieldNamePlayer.text.ToString(); //Recogemos el nombre de la UI
        if (thisName.Length == 0) //Si en la UI no han puesto nombre, le llamamos "Jugador [ID del cliente]"
        {
            thisName = $"Jugador {this.OwnerClientId}";
        }
        UpdatePlayerNameServerRpc(thisName); // hacer metodo antes de esto para comprobar que no esta vacio
        textoEnCabeza.text = Nombre.Value.ToString(); // Asignamos el nombre de la variable compratida al asset.
    }

    void ConfigurePlayer()
    {
       
       // PlayerRespawnPositionServerRpc();
        UpdatePlayerStateServerRpc(PlayerState.Grounded);
    }

    void ConfigureCamera()
    {
        // https://docs.unity3d.com/Packages/com.unity.cinemachine@2.6/manual/CinemachineBrainProperties.html
        var virtualCam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera;

        virtualCam.LookAt = transform;
        virtualCam.Follow = transform;
    }

    void ConfigureControls()
    {
        GetComponent<InputHandler>().enabled = true;
    }

    #endregion

    #region RPC

    #region ServerRPC

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        State.Value = state;
    }
    [ServerRpc]
    public void PlayerRespawnPositionServerRpc()
    {
        int randomNumber = Random.Range(1, 10);
        transform.position = respawns[randomNumber].transform.position;
    }
    [ServerRpc]
    public void UpdatePlayerNameServerRpc(FixedString32Bytes nombreActual) //
    {
        Nombre.Value = nombreActual; //
        textoEnCabeza.text = Nombre.Value.ToString(); // Asignamos el nombre de la variable compratida al asset.
    }
    #endregion

    #endregion

    #region Netcode Related Methods

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    void OnPlayerStateValueChanged(PlayerState previous, PlayerState current)
    {
        State.Value = current;
    }

    void OnPlayerNombreValueChanged(FixedString32Bytes previous, FixedString32Bytes current) //
    {
        Nombre.Value = current; //
        textoEnCabeza.text = Nombre.Value.ToString(); // Asignamos el nombre de la variable compratida al asset.
    }


    //Vida

    //Se actualiza la vida
    private void UpdateLives(int anterior, int actual)
    {
        //Si es el jugador local
        if (IsLocalPlayer)
        {
            UIManager.Instance.UpdateLifeUI(NumHealth - actual);
        }
    }


    //Se calcula la vida restante tras el impacto del disparo
    public void DamageDisparo()
    {
        if (isDead.Value == true)
        {
            //Si el personaje está muerto no se hace nada
            return;
        }
        else
        {
            var aux = Vidas.Value;
            //Se reducen las vidas que quedan
            var leftLives = Vidas.Value - 1;
            UpdateLives(aux, Vidas.Value);
            if (leftLives == 0)
            {
                Vidas.Value--;

                if (Vidas.Value == 0)
                {
                    //Muerte del personaje
                    isDead.Value = true;
                    Deaths.Value++;
                    player.GetComponent<NetworkObject>().Despawn();

                    
                    UIManager.Instance.FinishGame();
                    
                    UIManager.Instance.ActivateGameOverLocal();
                }

            }
            else
            {
                Vidas.Value = leftLives;

                //Aquí va el respawn

            }
        }
    }
    #endregion
}

public enum PlayerState
{
    Grounded = 0,
    Jumping = 1,
    Hooked = 2
}
