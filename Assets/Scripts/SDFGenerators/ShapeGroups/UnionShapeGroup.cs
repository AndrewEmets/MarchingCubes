using UnityEngine;

public class UnionShapeGroup : SDFShapeGroup
{
    public override float Combine(float r1, float r2)
    {
        return r1 < r2 ? r1 : r2;
    }
}
