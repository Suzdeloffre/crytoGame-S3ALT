using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public GameObject sharedObjectPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            var obj = Instantiate(sharedObjectPrefab, Vector3.zero, Quaternion.identity);
            obj.GetComponent<NetworkObject>().Spawn();
        }
    }
}
