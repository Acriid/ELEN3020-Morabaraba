using UnityEngine;
using Unity.Netcode.Components;

// To synced up object's transforms across servers.
// We override the default behaviour to set ServerAuthoritative to false
public class ClientNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
