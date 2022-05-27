using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour {

    #region Variables

    public float speed;
    public ulong owner;

    #endregion

    #region Métodos

    private void OnTriggerEnter2D(Collider2D collision) {

        //Si es el servidor, este comprueba si la bala ha colisionado con otro jugador

        if (IsServer) {

            var otherPlayer = collision.GetComponent<Player>();

            if (otherPlayer != null) {

                if (owner != otherPlayer.OwnerClientId) {

                    //player.Damage();

                }
            }

        }

        //Método que se encarga de eliminar las balas que han colisionado
        DestruirBalas();
    }


    public void DestruirBalas() {

        //Destroy(gameObject);--> en local
        GetComponent<NetworkObject>().Despawn();

    }

    #endregion
}