using UnityEngine;

public class SphereSDF : SDFShape
{
    [SerializeField] private float sphereRadius = 1f;

    public override Bounds GetLocalBounds()
    {
        return new Bounds(Vector3.zero, Vector3.one * sphereRadius * 2);
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        return p.magnitude - sphereRadius;
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.matrix = gameObject.transform.localToWorldMatrix;
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawSphere(Vector3.zero, sphereRadius);
    }
}

