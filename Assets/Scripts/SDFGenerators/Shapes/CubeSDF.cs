using System;
using UnityEngine;

public class CubeSDF : SDFShape
{
    [SerializeField] private Vector3 size = Vector3.one;
    [SerializeField] private float radius;

    public override Bounds GetLocalBounds()
    {
        var result = new Bounds(Vector3.zero, size * 2f);
        result.Expand(radius);

        return result;
    }

    protected override float GetSDFInternal(Vector3 p)
    {        
        var q = Vector3Math.Abs(p) - size;
        var res = Vector3.Magnitude(Vector3Math.Max(q,0));
        res += Math.Min(Math.Max(q.x, Math.Max(q.y,q.z)), 0);
        res -= radius;

        return res;
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1,1,1,0.5f);
        Gizmos.DrawCube(Vector3.zero, (size + Vector3.one * radius)*2f);
    }
}

