using UnityEngine;

public class SDFAbsModifier : SDFModifier
{
    [SerializeField] private float addRadiusAfterwards = 0f;

    public override float ModifySDF(Vector3 pos, float s)
    {
        return Mathf.Abs(s) - addRadiusAfterwards;
    }
}