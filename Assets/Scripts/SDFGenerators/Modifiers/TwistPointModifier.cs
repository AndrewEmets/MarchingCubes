using UnityEngine;

public class TwistPointModifier : ProbePointModifier
{
    [SerializeField] private Vector3 direction;
    [SerializeField] private float amount;

    private void OnValidate() 
    {
        direction.Normalize();
    }

    public override Vector3 ModifyPoint(Vector3 p)
    {
        var rot = Quaternion.AngleAxis(amount * (p - objectPosition).y, direction);
        return rot * p;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.matrix = gameObject.transform.localToWorldMatrix;

        Gizmos.DrawRay(Vector3.zero, direction);        
    }
}
