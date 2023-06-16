using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletScript : NetworkBehaviour {
    private void OnCollisionEnter2D(Collision2D collision) {
        // We have the following cases:
        // 1. The bullet collides with the map --> DESTROYED.
        // 2. The bullet collides with another bullet --> CHECK WHAT HAPPENS USING THE MATRIX IN THE GDD.
        // 3. The bullet collides with a player --> apply the health decrease to this player.
        // 4. The bullet collides with an item --> do nothing.

        // Also only the hosting player / server will handle the collision.
        Debug.Log("Collision detected.");
        NetworkObject.Despawn(true);
        if (NetworkManager.Singleton.IsHost == false) {
            // Only the host is allowed to check for the collision.
            return;
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        // Do the same as in the enter version. We just need to check this too.
        OnCollisionEnter2D(collision);
    }
}
