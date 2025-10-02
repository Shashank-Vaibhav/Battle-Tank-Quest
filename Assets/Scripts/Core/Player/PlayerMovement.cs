using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float turningRate = 30f;

    private Vector2 movementInput;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        inputReader.MoveEvent += HandleMove;
        
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }
        inputReader.MoveEvent -= HandleMove;
        
    }
    private void Update()
    {
        if (!IsOwner) { return; }

        Turn();
    }
    private void FixedUpdate()
    {
        if(!IsOwner) { return; }
        MoveForward();
    }

    private void HandleMove(Vector2 movementInput)
    {
        this.movementInput = movementInput;
    }

    private void Turn()
    {
        float zRotation = -movementInput.x * turningRate * Time.deltaTime;
        bodyTransform.Rotate(0, 0, zRotation) ;

    }
    private void MoveForward()
    {
        rb.velocity = (Vector2)bodyTransform.up * movementSpeed * movementInput.y * Time.fixedDeltaTime;
    }
    

    
    
    

}
