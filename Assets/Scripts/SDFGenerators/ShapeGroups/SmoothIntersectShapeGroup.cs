using UnityEngine;

public class SmoothIntersectShapeGroup : SDFShapeGroup
{
    [SerializeField] private float power;

    public override float Combine(float r1, float r2)
    {
        float h = Mathf.Clamp01(0.5f - 0.5f*(r2 - r1) / power);
        return Mathf.LerpUnclamped(r2, r1, h ) + power * h * (1f - h);
    }
}