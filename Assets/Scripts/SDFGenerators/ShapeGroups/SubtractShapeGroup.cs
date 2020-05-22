using UnityEngine;

public class SubtractShapeGroup : SDFShapeGroup
{
    public override float Combine(float r1, float r2)
    {
        r2 = -r2;
        return r1 > r2 ? r1 : r2;
    }
}
