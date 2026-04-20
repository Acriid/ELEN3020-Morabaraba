using UnityEngine;

using Unity.Netcode;

public class whatIsStatus : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("I am the owner");
        }
        if (IsClient)
            Debug.Log("i am client");
        if (IsHost)
            Debug.Log("i am the host");
    }

}
