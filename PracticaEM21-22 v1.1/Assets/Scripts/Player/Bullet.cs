using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour {
    public ulong owner;

    private void OnTriggerEnter2D(Collider2D col) {
        if (IsServer) {
            var player = col.GetComponent<Player>();
            if (player != null) {
                if (owner != player.OwnerClientId) {
                    player.Damage();
                }
            }
        }

        Despawn();
    }

    public void Despawn() {
        GetComponent<NetworkObject>().Despawn();
    }
}