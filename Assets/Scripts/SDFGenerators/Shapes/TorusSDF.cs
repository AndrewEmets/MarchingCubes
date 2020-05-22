using UnityEngine;

public class TorusSDF : SDFShape
{
    [SerializeField] private float radius = 1;
    [SerializeField] private float widthRadius = 0.1f;

    public override Bounds GetLocalBounds()
    {
        var result = new Bounds(Vector3.zero, new Vector3(radius * 2, radius * 2, 0));
        result.Expand(widthRadius * 2);
        return result;
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        var q = new Vector2(((Vector2)p).magnitude - radius, p.z);
        return q.magnitude - widthRadius;
    }
}

