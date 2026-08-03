using UnityEngine;
using Unity.Netcode;

public class DisableLocalPlayerMesh : NetworkBehaviour
{
    [SerializeField] private GameObject playerMesh;

    private void Start()
    {
        if (IsOwner)
        {
            playerMesh.SetActive(false);
        }
    }
}
