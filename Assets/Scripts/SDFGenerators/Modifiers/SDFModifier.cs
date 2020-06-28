using UnityEngine;

public abstract class SDFModifier : MonoBehaviour
{
    public abstract float ModifySDF(Vector3 pos, float s);
}
