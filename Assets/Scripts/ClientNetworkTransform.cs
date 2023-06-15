using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority {
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform {
        // We want that the server can reset the position. We need this when a death happened.
        protected override bool OnIsServerAuthoritative() {
            return true;
        }
    }
}
