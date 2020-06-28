using UnityEngine;

public class SummationShapeGroup : SDFShapeGroup
{
    [SerializeField] private float modifier = 1f;

    public override float Combine(float r1, float r2)
    {
        return r1 + r2 * modifier;
    }
}
