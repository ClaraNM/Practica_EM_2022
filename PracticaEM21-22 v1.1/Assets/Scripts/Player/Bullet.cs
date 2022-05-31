using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour {

    #region Variables
    public ulong owner;
    #endregion

    #region Métodos
    private void OnTriggerEnter2D(Collider2D collision) {

        //Si es el servidor, este comprueba si la bala ha colisionado con otro jugador o con plataformas
        if (IsServer) {

            if (collision.gameObject.tag == "Platform") {
                DestruirBalas();
            }
            else if (collision.gameObject.tag == "Player")
            {
                //Si la bala contacta con otro pj que no sea el propietario desaparece la bala y hace daño
                if (owner != collision.GetComponent<Player>().OwnerClientId)
                {
                    DestruirBalas();
                    collision.GetComponent<Player>().DamageDisparo();
                }

            }

        }

    }

    public void DestruirBalas() {

        //Destroy(gameObject);--> en local
        GetComponent<NetworkObject>().Despawn();

    }
    #endregion
}