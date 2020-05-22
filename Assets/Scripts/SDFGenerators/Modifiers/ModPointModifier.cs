using UnityEngine;

public class ModPointModifier : ProbePointModifier
{
    [SerializeField] Vector3 period = Vector3.one;

    public override Vector3 ModifyPoint(Vector3 p)
    {
        p.x = Mathf.Repeat(p.x, period.x);
        p.y = Mathf.Repeat(p.y, period.y);
        p.z = Mathf.Repeat(p.z, period.z);
        
        return p;
    }
}