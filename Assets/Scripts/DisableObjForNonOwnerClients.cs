using Unity.Netcode;

public class DisableObjForNonOwnerClients : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
    }
}
