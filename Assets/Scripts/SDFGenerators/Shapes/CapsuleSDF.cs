using System;
using UnityEngine;

public static class Vector3Math
{
    public static Vector3 Abs(Vector3 vec)
    {
        return new Vector3(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
    }

    public static Vector3 Max(Vector3 vec, float val)
    {
        return new Vector3(Math.Max(vec.x, val), Math.Max(vec.y, val), Math.Max(vec.z, val));
    }
}

public class CapsuleSDF : SDFShape
{
    [SerializeField] private Vector3 start = Vector3.zero, end = Vector3.up;
    [SerializeField] private float radius = 0.25f;

    public override Bounds GetLocalBounds()
    {
        var result = new Bounds(start, Vector3.zero);
        result.Encapsulate(end);
        result.Expand(radius * 2);

        return result;
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
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(start, radius);
        Gizmos.DrawSphere(end, radius);
        Gizmos.DrawLine(start, end);

        //GizmosX.DrawSphere(start, radius);
    }
}

