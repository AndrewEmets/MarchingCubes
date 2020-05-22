using UnityEngine;

public class SphereSDF : SDFShape
{
    [SerializeField] private float sphereRadius;

    public override Bounds GetLocalBounds()
    {
        return new Bounds(Vector3.zero, Vector3.one * sphereRadius * 2);
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        return p.magnitude - sphereRadius;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.matrix = gameObject.transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, sphereRadius);        
    }
}

