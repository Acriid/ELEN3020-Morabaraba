using UnityEngine;
using Unity.Netcode;

public class SetStatus : NetworkBehaviour
{
    public void setAsHost()
    {
        if (!IsHost)
        {
            FindAnyObjectByType<NetworkManager>().StartHost();
        }
    }
    public void setAsClient()
    {
        if (!IsClient)
            FindAnyObjectByType<NetworkManager>().StartClient();
    }
}
