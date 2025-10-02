using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class JoinHost : MonoBehaviour
{
    public void Join()
    {
        NetworkManager.Singleton.StartClient();
    }
    public void Host()
    {
        NetworkManager.Singleton.StartHost();
    }

}
