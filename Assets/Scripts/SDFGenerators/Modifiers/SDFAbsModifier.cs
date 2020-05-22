using UnityEngine;

public class SDFAbsModifier : SDFModifier
{
    public override float ModifySDF(float s)
    {
        return Mathf.Abs(s);
    }
}
