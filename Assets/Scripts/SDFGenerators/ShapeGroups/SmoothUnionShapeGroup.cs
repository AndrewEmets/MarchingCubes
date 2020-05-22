using UnityEngine;

public class SmoothUnionShapeGroup : SDFShapeGroup
{
    [SerializeField] private float power = 1;

    public override float Combine(float r1, float r2)
    {
        var h = Mathf.Clamp01(0.5f + 0.5f * (r2 - r1) / power);
        return Mathf.Lerp(r2, r1, h) - power * h * (1.0f - h);
    }
}
