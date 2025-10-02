using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5.0f;
    private void OnEnable()
    {
        Destroy(this.gameObject, lifeTime);
    }
    
}
