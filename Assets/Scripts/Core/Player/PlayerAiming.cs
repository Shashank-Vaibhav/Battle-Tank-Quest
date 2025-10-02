using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turretTransform;

    private Vector2 worldAimPosition;
    private Vector2 aimVector;

    public override void OnNetworkSpawn()
    {
        
    }

    private void LateUpdate()
    {
        if (!IsOwner) { return; }
        worldAimPosition = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);
        aimVector = worldAimPosition - (Vector2)turretTransform.position;
        turretTransform.up = aimVector;
    }
}
