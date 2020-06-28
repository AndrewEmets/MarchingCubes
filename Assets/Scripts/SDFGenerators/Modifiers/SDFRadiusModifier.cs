using UnityEngine;

public class SDFRadiusModifier : SDFModifier
{
    [SerializeField] private float addRadius;

    public override float ModifySDF(Vector3 p, float s)
    {
        return s - addRadius;
    }
}