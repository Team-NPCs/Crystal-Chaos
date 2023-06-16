using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority {
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform {
        // Set this to false so that the host can not move the client.
        // But we need to set this to true if the server shall reset the players position (respawn).
        // So in order that both work, we need to network the movement.
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}
