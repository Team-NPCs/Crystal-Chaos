using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletScript : NetworkBehaviour {
    
    private void OnTriggerEnter2D(Collider2D other) {
        // We have the following cases:
        // 1. The bullet collides with the map --> DESTROYED.
        // 2. The bullet collides with another bullet --> CHECK WHAT HAPPENS USING THE MATRIX IN THE GDD.
        // 3. The bullet collides with a player --> apply the health decrease to this player.
        // 4. The bullet collides with an item --> do nothing.

        // Also only the hosting player / server will handle the collision.
        if (NetworkManager.Singleton.IsServer == false) {
            // Only the host is allowed to check for the collision.
            return;
        }
        if (other.CompareTag("Bullet")) {
            // Do the further stuff here.
            return;
        }
        if (other.CompareTag("Player")) {
            // The player got hit.
            ApplyDamageServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId);
            // Delete the bullet.
            NetworkObject.Despawn(true);
            return;
        }
        if (other.CompareTag("Item")) {
            // An item (potion / crystal ball).
            return;
        }

        // Checking the collision with a tilemap is quite complicated. Therefore we simply check if 
        // we got through the end (no other objects were involved in the collision, so it has to be the map).
        NetworkObject.Despawn(true);
    }

    private void OnTriggerStay2D (Collider2D other) {
        // Do the same as in the enter version. We just need to check this too.
        OnTriggerEnter2D(other);
    }

    // The server needs to do the damage.
    [ServerRpc]
    private void ApplyDamageServerRpc (ulong targetPlayerNetworkObjectId) {
        NetworkObject targetPlayerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerNetworkObjectId];
        PlayerStats targetPlayerStats = targetPlayerNetworkObject.GetComponent<PlayerStats>();
        targetPlayerStats.DecreaseHealth(20);
    }
}
