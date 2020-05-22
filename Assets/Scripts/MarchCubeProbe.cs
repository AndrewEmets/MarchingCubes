using System;
using UnityEngine;

public class MarchCubeProbe : MonoBehaviour
{
    [SerializeField] private Vec3Int index;

    [SerializeField] private int res;
    
    private void OnValidate()
    {
        var gener = GetComponent<MarchingCubesMeshGenerator>();

        res = gener.getIndexMask(index);
    }
}