using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority {
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform {
        // Set this to false so that the host can not move the client.
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}
