using System;
using UnityEngine;

public class GyroidSDF : SDFShape
{
    [SerializeField] private float scale = 1f;
    [SerializeField] private float step = 0f;

    public override Bounds GetLocalBounds()
    {
        return new Bounds();
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        p *= scale;
        var g = (Math.Sin(p.x) * Math.Cos(p.y) + Math.Sin(p.y)*Math.Cos(p.z) + Math.Sin(p.z)*Math.Cos(p.x));
        g /= scale;
        return (float)g - step;
    }
}

