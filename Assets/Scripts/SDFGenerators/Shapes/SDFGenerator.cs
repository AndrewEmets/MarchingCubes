using System;
using UnityEngine;

public abstract class SDFShape : MonoBehaviour
{    
    private MarchingCubesMeshGenerator meshGenerator;

    public abstract Bounds GetLocalBounds();

    private float maxScale;

    public float GetSDF(Vector3 p)
    {
        if (gameObject.transform.hasChanged)
        {
            var scale = gameObject.transform.localScale;
            maxScale = Math.Max(Math.Max(scale.x, scale.y), scale.z);
        }

        p = gameObject.transform.InverseTransformPoint(p);
        var result = GetSDFInternal(p)*maxScale;

        return result;
    }

    protected virtual void OnValidate() 
    {
        //meshGenerator = GetComponentInParent<MarchingCubesMeshGenerator>();
        //parentGenerator = meshGenerator.transform;
    }

    protected abstract float GetSDFInternal(Vector3 p);
}

