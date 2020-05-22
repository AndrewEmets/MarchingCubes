using System;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Abs(this Vector3 vec)
    {
        return new Vector3(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
    }

    public static Vector3 Max(this Vector3 vec, float val)
    {
        return new Vector3(Math.Max(vec.x, val), Math.Max(vec.y, val), Math.Max(vec.z, val));
    }
}

public class CapsuleSDF : SDFShape
{
    [SerializeField] private Vector3 start, end;
    [SerializeField] private float radius;

    public override Bounds GetLocalBounds()
    {
        return new Bounds(Vector3.zero, Vector3.one * 3);
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        var a = start;
        var b = end;

        var pa = p - a;
        var ba = b - a;
        
        float h = Mathf.Clamp01(Vector3.Dot(pa,ba) / Vector3.Dot(ba,ba));
        
        return (pa - ba*h).magnitude - radius;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, radius);        
        Gizmos.DrawWireSphere(end, radius);        
    }
}

