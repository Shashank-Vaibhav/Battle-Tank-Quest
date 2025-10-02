using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("Reference")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private float muzzleFlashDuration = 2f;

    private bool isFired;
    private float previousFireTime;
    private float muzzleFlashTimer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

   

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }
        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }
    private void Update()
    {
        if(muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if(muzzleFlashTimer <= 0f)
            {
                muzzleFlash.SetActive(false);
            }
        }
        if (!IsOwner) { return; }
        if(Time.time < (1/fireRate) + previousFireTime) { return; }

        if(isFired)
        {
            PrimaryFireServerRPC();
            SpawnDummyProjectile(); 
        }
        previousFireTime = Time.time;
    }

    [ServerRpc]
    private void PrimaryFireServerRPC()
    {
        GameObject projectileInstance = Instantiate(serverProjectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        projectileInstance.GetComponent<Rigidbody2D>().AddForce(projectileSpawnPoint.up * projectileSpeed, ForceMode2D.Impulse);
        PrimaryFireClientRPC();
    }
    [ClientRpc]
    private void PrimaryFireClientRPC()
    {
        if (IsOwner) { return; }
        SpawnDummyProjectile();
    }

    private void SpawnDummyProjectile()
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;
        GameObject projectileInstance = Instantiate(clientProjectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        projectileInstance.GetComponent<Rigidbody2D>().AddForce(projectileSpawnPoint.up * projectileSpeed, ForceMode2D.Impulse);

    }

    private void HandlePrimaryFire(bool isFired)
    {
        this.isFired = isFired;
    }
}
