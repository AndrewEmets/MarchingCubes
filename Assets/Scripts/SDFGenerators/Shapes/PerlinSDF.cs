using UnityEngine;

public class PerlinSDF : SDFShape
{
    [SerializeField] private float step = 0.25f;
    [SerializeField] private float scale = 1f;

    public override Bounds GetLocalBounds()
    {
        return new Bounds();
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        var n = Perlin.Fbm(p * scale, 1) + step;
        return n;
    }
}

