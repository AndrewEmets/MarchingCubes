using System;
using UnityEngine;

public class CubeSDF : SDFShape
{
    [SerializeField] private Vector3 size;
    [SerializeField] private float radius;

    public override Bounds GetLocalBounds()
    {
        var result = new Bounds(Vector3.zero, size * 2f);
        result.Expand(radius);

        return result;
    }

    protected override float GetSDFInternal(Vector3 p)
    {        
        var q = p.Abs() - size;
        var res = Vector3.Magnitude(q.Max(0));
        res += Math.Min(Math.Max(q.x, Math.Max(q.y,q.z)), 0);
        res -= radius;

        return res;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, (size + Vector3.one * radius)*2f);
    }
}

