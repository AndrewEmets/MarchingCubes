using System;
using MarchingCubes.Generator;
using UnityEngine;

public abstract class SDFShape : MonoBehaviour
{    
    private MarchingCubesMeshGenerator meshGenerator;

    public abstract Bounds GetLocalBounds();

    private float maxScale;

    private Matrix4x4 worldToLocalMatrix;

    public void Init()
    {
        worldToLocalMatrix = gameObject.transform.worldToLocalMatrix;
        var scale = gameObject.transform.localScale;
        maxScale = Math.Max(Math.Max(scale.x, scale.y), scale.z);
    }

    public float GetSDF(Vector3 p)
    {
        p = worldToLocalMatrix.MultiplyPoint3x4(p);
        var result = GetSDFInternal(p) * maxScale;

        return result;
    }

    protected virtual void OnValidate() 
    {
        //meshGenerator = GetComponentInParent<MarchingCubesMeshGenerator>();
        //parentGenerator = meshGenerator.transform;
    }

    protected abstract float GetSDFInternal(Vector3 p);
}

