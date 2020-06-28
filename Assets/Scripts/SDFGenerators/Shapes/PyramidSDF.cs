using UnityEngine;

public class PyramidSDF : SDFShape
{
    [SerializeField] private float height;

    public override Bounds GetLocalBounds()
    {
        return new Bounds(new Vector3(0, height / 2f, 0), new Vector3(1, height, 1));
    }

    protected override float GetSDFInternal(Vector3 p)
    {
        Vector3 po = p;

        float h = height;

        if (p.y < 0)
        {
            var pp = p;
            pp.x = Mathf.Clamp(pp.x, -0.5f, 0.5f);
            pp.z = Mathf.Clamp(pp.z, -0.5f, 0.5f);
            pp.y = 0;

            return Vector3.Distance(p, pp);
        }


        float m2 = h*h + 0.25f;

        p.x = Mathf.Abs(p.x);
        p.z = Mathf.Abs(p.z);

        if (p.z > p.x)
        {
            var temp = p.x;
            p.x = p.z;
            p.z = temp;
        }

        p.x -= 0.5f;
        p.z -= 0.5f;

        var q = new Vector3(p.z, h*p.y - 0.5f*p.x, h*p.x + 0.5f*p.y);
        
        float s = Mathf.Max(-q.x, 0f);
        float t = Mathf.Clamp01((q.y - 0.5f*p.z) / (m2 + 0.25f));
            
        float a = m2 * (q.x+s) * (q.x+s) + q.y*q.y;
        float b = m2*(q.x + 0.5f*t)*(q.x + 0.5f*t) + (q.y - m2*t)*(q.y - m2*t);
            
        float d2 = Mathf.Min(q.y,-q.x*m2 - q.y*0.5f) > 0 ? 0 : Mathf.Min(a,b);
            
        var result = Mathf.Sqrt((d2+q.z*q.z) / m2) * Mathf.Sign(Mathf.Max(q.z,-p.y));

        return result;
    }
}

