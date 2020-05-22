using System;
using UnityEngine;

public class TwoHolesSDF : SDFShape
{
    public override Bounds GetLocalBounds()
    {
        return new Bounds(Vector3.zero, Vector3.one * 2f);
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        float x = p.x, y = p.y, z = p.z;
        var result = 2 * y * (y * y - 3 * x * x) * (1 - z * z) + Mathf.Pow(x * x + y * y, 2) -
                (9 * z * z - 1) * (1 - z * z);

        float sphereSDF = p.magnitude - 2f;
        result = Math.Max(result, sphereSDF);

        return result;
    }
}

