using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MarchingCubesMeshGeneratorAsync : MonoBehaviour
{
    [SerializeField] private Vec3Int resolution;
    [SerializeField] private float scale = 1f;
    [SerializeField, Range(-0.41f, 0.32f)] private float step = 0.5f;
    [SerializeField] private SurfaceType surface;

    public Vector3 Offset { get; set; }
    public Vec3Int Resolution => resolution;
    
    private readonly List<int> triangles = new List<int>(30000);
    private readonly List<Vector3> vertices = new List<Vector3>(30000);

    private readonly List<Vector3> normals = new List<Vector3>(30000);
    //private readonly List<Color> colors = new List<Color>(30000);

    private readonly Cache cache = new Cache();

    public void GenerateMesh(Mesh mesh)
    {
        StartCoroutine(StartGeneratingMesh(mesh));
    }

    private bool done;
    private IEnumerator StartGeneratingMesh(Mesh mesh)
    {
        mesh.Clear();

        done = false;
        var t = new Thread(GenerateMeshInternal);
        t.Start();

        yield return new WaitWhile(() => !done);
        
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        //mesh.SetColors(colors);
        
        mesh.RecalculateBounds();
    }

    private void GenerateMeshInternal()
    {
        cache.Validate(resolution);
        cache.Clear();
        
        triangles.Clear();
        vertices.Clear();
        normals.Clear();
        
        for (var i = 0; i < resolution.x; i++)
        for (var j = 0; j < resolution.y; j++)
        for (var k = 0; k < resolution.z; k++)
        {
            var cellIndex = new Vec3Int(i, j, k);

            var trianglesInCell = MarchingCubesLookupTable.TrianglesData[getIndexMask(cellIndex)];

            for (var index = 0; index < trianglesInCell.verticesIndices.Length; index++)
            {
                var tv = trianglesInCell.verticesIndices[index];
                
                if (cache.TryGetVertexIndexInList(cellIndex, tv, out var vertexIndexInList))
                {
                    triangles.Add(vertexIndexInList);
                }
                else
                {
                    var v = ConvertVertexIndexToPosition(tv, cellIndex) + cellIndex;
                    var newInd = vertices.Count;
                    vertices.Add(v);
                    triangles.Add(newInd);

                    normals.Add(getNormal(v));

                    cache.SetVertexIndex(cellIndex, tv, newInd);
                }
            }
        }

        done = true;
    }

    public int getIndexMask(Vec3Int ijk)
    {
        var result = 0;

        for (var i = 0; i < 8; i++)
        {
            var x = (i & 1) >= 1 ? 1 : 0;
            var y = (i & 2) >= 1 ? 1 : 0;
            var z = (i & 4) >= 1 ? 1 : 0;

            var sdf = getCachedSDFInCell(ijk + new Vec3Int(x, y, z));

            if (sdf <= 0)
            {
                result |= 1 << i;
            }
        }

        return result;
    }

    private Vector3 ConvertVertexIndexToPosition(int index, Vec3Int ijk)
    {
        switch (index)
        {
            case 0:
            {
                return new Vector3(g(ijk, Vec3Int.right), 0, 0);
            }
            case 1:
            {
                return new Vector3(0, g(ijk, Vec3Int.up), 0);
            }
            case 2:
            {
                return new Vector3(g(ijk + Vec3Int.up, Vec3Int.right), 1, 0);
            }
            case 3:
            {
                return new Vector3(1, g(ijk + Vec3Int.right, Vec3Int.up), 0);
            }
            case 4:
            {
                return new Vector3(0, 1, g(ijk + Vec3Int.up, Vec3Int.forward));
            }
            case 5:
            {
                return new Vector3(1, 1, g(ijk + new Vec3Int(1, 1, 0), Vec3Int.forward));
            }
            case 6:
            {
                return new Vector3(1, 0, g(ijk + Vec3Int.right, Vec3Int.forward));
            }
            case 7:
            {
                return new Vector3(0, 0, g(ijk, Vec3Int.forward));
            }
            case 8:
            {
                return new Vector3(g(ijk + Vec3Int.forward, Vec3Int.right), 0, 1);
            }
            case 9:
            {
                return new Vector3(0, g(ijk + Vec3Int.forward, Vec3Int.up), 1);
            }
            case 10:
            {
                return new Vector3(g(ijk + new Vec3Int(0, 1, 1), Vec3Int.right), 1, 1);
            }
            case 11:
            {
                return new Vector3(1, g(ijk + new Vec3Int(1, 0, 1), Vec3Int.up), 1);
            }
        }

        throw new Exception("Wrong vertex index");
        
        float f(float a, float b)
        {
            return a / (a - b);
        }

        float g(Vec3Int ind, Vec3Int offset)
        {
            return f(getCachedSDFInCell(ind), getCachedSDFInCell(ind + offset));
        }
    }

    Vector3 getNormal(Vector3 p) // for function f(p)
    {
        const float h = 0.01f;
        var normal = (new Vector3(1, -1, -1) * getSDF(p + new Vector3(1, -1, -1) * h) +
                      new Vector3(-1, -1, 1) * getSDF(p + new Vector3(-1, -1, 1) * h) +
                      new Vector3(-1, 1, -1) * getSDF(p + new Vector3(-1, 1, -1) * h) +
                      new Vector3(1, 1, 1) * getSDF(p + new Vector3(1, 1, 1) * h)).normalized;
        
        return normal;
    }

    float sdTorus(Vector3 p, Vector2 t)
    {
        var q = new Vector2(new Vector2(p.x, p.z).magnitude - t.x, p.y);
        return q.magnitude - t.y;
    }
    
    private float getSDF(Vector3 p)
    {
        p += Offset;
        
        switch (surface)
        {
            case SurfaceType.Toruses:
            {
                const float r1 = 10;
                const float r2 = 2;
                const float k = 1f;
            
                var t1 = sdTorus(p - Vector3.one * 15, new Vector2(r1, r2));
                var t2 = sdTorus(new Vector3(p.y, p.z, p.x) - Vector3.one * 15, new Vector2(r1, r2));
                var t3 = sdTorus(new Vector3(p.z, p.x, p.y) - Vector3.one * 15, new Vector2(r1, r2));
            
                var t = smin(t1, t2, k);
                t = smin(t, t3, k);
                return t;
            }
            
            case SurfaceType.TwoHoles:
            {
                p -= Vector3.one * 15;
                p *= scale;
                float x = p.x, y = p.y, z = p.z;
                return 2 * y * (y * y - 3 * x * x) * (1 - z * z) + Mathf.Pow(x * x + y * y, 2) -
                       (9 * z * z - 1) * (1 - z * z);
            }
            
            case SurfaceType.PerlinNoise:
            {
                var n = Perlin.Fbm(p * scale, 1) - step;

                return n;
            }
            
            default:
                return 0;
        }
    }

    float getCachedSDFInCell(Vec3Int index)
    {
        if (cache.TryGetSdf(index, out var cachedSdf))
        {
            return cachedSdf;
        }

        var sdf = getSDF(index);
        cache.SetSdf(index, sdf);

        return sdf;
    }
    
    private static float smin(float a, float b, float k)
    {
        var h = Mathf.Clamp01(0.5f + 0.5f * (b - a) / k);
        return Mathf.Lerp(b, a, h) - k * h * (1.0f - h);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        DrawBounds();
    }

    private void DrawBounds()
    {
        var tempMatrix = Gizmos.matrix;
        Gizmos.matrix *= Matrix4x4.Scale(resolution);
        
        Gizmos.DrawLine(Vector3.zero, Vector3.right);
        Gizmos.DrawLine(Vector3.zero, Vector3.up);
        Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        Gizmos.DrawLine(Vector3.up, Vector3.up + Vector3.right);
        Gizmos.DrawLine(Vector3.right, Vector3.up + Vector3.right);
        Gizmos.DrawLine(Vector3.right, Vector3.right + Vector3.forward);
        Gizmos.DrawLine(Vector3.right, Vector3.right + Vector3.forward);


        Gizmos.DrawLine(Vector3.one - Vector3.zero, Vector3.one - Vector3.right);
        Gizmos.DrawLine(Vector3.one - Vector3.zero, Vector3.one - Vector3.up);
        Gizmos.DrawLine(Vector3.one - Vector3.zero, Vector3.one - Vector3.forward);
        Gizmos.DrawLine(Vector3.one - Vector3.up, Vector3.one - Vector3.up - Vector3.right);
        Gizmos.DrawLine(Vector3.one - Vector3.right, Vector3.one - Vector3.up - Vector3.right);
        Gizmos.DrawLine(Vector3.one - Vector3.right, Vector3.one - Vector3.right - Vector3.forward);
        Gizmos.DrawLine(Vector3.one - Vector3.right, Vector3.one - Vector3.right - Vector3.forward);

        Gizmos.matrix = tempMatrix;
    }
}

public enum SurfaceType
{
    Toruses,
    TwoHoles,
    PerlinNoise
}
