using UnityEngine;

public class SDFRadiusModifier : SDFModifier
{
    [SerializeField] private float addRadius;

    public override float ModifySDF(float s)
    {
        return s - addRadius;
    }
}