using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 angularSpeed;

    [SerializeField] private float multiplier;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Time.deltaTime * multiplier * angularSpeed);
    }
}
