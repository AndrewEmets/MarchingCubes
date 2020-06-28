using UnityEngine;

public class PlaneSDF : SDFShape
{
    public override Bounds GetLocalBounds()
    {
        return new Bounds();
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        return p.y;
    }
}

