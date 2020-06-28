using System;
using UnityEngine;

public class CosSDF : SDFShape
{
    [SerializeField] private float scale = 1;

    public override Bounds GetLocalBounds()
    {
        return new Bounds();
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        p *= scale;
        return (float)(Math.Cos(p.x)*Math.Cos(p.y)*Math.Cos(p.z)) / scale;
    }
}

