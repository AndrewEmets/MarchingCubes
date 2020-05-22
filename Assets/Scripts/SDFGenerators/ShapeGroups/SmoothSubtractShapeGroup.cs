using UnityEngine;

public class SmoothSubtractShapeGroup : SDFShapeGroup
{
    [SerializeField] private float power;

    public override float Combine(float r1, float r2)
    {
        float h = Mathf.Clamp01(0.5f - 0.5f*(r1+r2)/power);
        return Mathf.Lerp(r1, -r2, h) + power * h * (1 - h); 
    }
}