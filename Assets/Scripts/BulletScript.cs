using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletScript : NetworkBehaviour {
    // These values define how much damage the hitted player gets.
    // They are set in the shooting script depending on the crystal type of the spell.
    public int spellDamageNormalAttackBody;
    public int spellDamageNormalAttackHead;
    public NetworkVariable<CrystalType> crystalType = new();
    private SpriteRenderer bulletRenderer;

    private void Start () {
        bulletRenderer = GetComponent<SpriteRenderer>();
        SetBulletSprite();
    }

    private void SetBulletSprite () {
        if (crystalType.Value == CrystalType.Fire) {
            bulletRenderer.sprite = Resources.Load<Sprite>("Elemental Bullets/bullet-fire");
        }
        else if (crystalType.Value == CrystalType.Water) {
            bulletRenderer.sprite = Resources.Load<Sprite>("Elemental Bullets/bullet-water");
        }
        else if (crystalType.Value == CrystalType.Earth) {
            bulletRenderer.sprite = Resources.Load<Sprite>("Elemental Bullets/bullet-earth");
        }
        else if (crystalType.Value == CrystalType.Air) {
            bulletRenderer.sprite = Resources.Load<Sprite>("Elemental Bullets/bullet-air");
        }
        else if (crystalType.Value == CrystalType.Void) {
            bulletRenderer.sprite = Resources.Load<Sprite>("Elemental Bullets/bullet-void");
        }
    }
    
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
            // Within the GDD there is a matrix that lists the interaction of the spells.
            // Get the type of the other bullet.
            BulletScript otherBulletScript = other.GetComponent<BulletScript>();
            // Fire gets erased by water.
            if ((crystalType.Value == CrystalType.Fire) && (otherBulletScript.crystalType.Value == CrystalType.Water)) {
                // Delete the bullet.
                if (NetworkObject.IsSpawned) {
                    NetworkObject.Despawn(true);
                }
            }
            return;
        }
        if (other.CompareTag("Player")) {
            // The player got hit. Currently we only support normal hits (so on the body and not on the head).
            ApplyDamageServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId);
            // Delete the bullet.
            if (NetworkObject.IsSpawned) {
                NetworkObject.Despawn(true);
            }
            return;
        }
        if (other.CompareTag("Item")) {
            // An item (potion / crystal ball).
            return;
        }

        // Checking the collision with a tilemap is quite complicated. Therefore we simply check if 
        // we got through the end (no other objects were involved in the collision, so it has to be the map).
        if (NetworkObject.IsSpawned) {
            NetworkObject.Despawn(true);
        }
    }

    private void OnTriggerStay2D (Collider2D other) {
        // Do the same as in the enter version. We just need to check this too.
        OnTriggerEnter2D(other);
    }

    // The server needs to do the damage.
    [ServerRpc]
    private void ApplyDamageServerRpc (ulong targetPlayerNetworkObjectId) {
        // Find the target player's NetworkObject using the network object ID and the player stats.
        NetworkObject targetPlayerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerNetworkObjectId];
        PlayerStats targetPlayerStats = targetPlayerNetworkObject.GetComponent<PlayerStats>();
        bool playerDied = targetPlayerStats.DecreaseHealth(spellDamageNormalAttackBody);
        // If the player died, reset the inventory and add a random crystal ball.
        if (playerDied) {
            // Reset the inventory. The player loses all when he dies.
            PlayerInventory targetPlayerInventory = targetPlayerNetworkObject.GetComponent<PlayerInventory>();
            targetPlayerInventory.ResetCrystal();
            // Add a new random crystal ball to the inventory.
            targetPlayerInventory.AddRandomCrystal();
        }
    }
}
