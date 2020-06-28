using UnityEngine;

public abstract class ProbePointModifier : MonoBehaviour
{
    protected Vector3 objectPosition;

    public abstract Vector3 ModifyPoint(Vector3 p);

    public virtual void Init()
    {
        objectPosition = gameObject.transform.position;
    }
}
