using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Coroutiner : MonoBehaviour
{
}

public class MarchingCubesMeshGenerator
{
    public Vector3 Offset { get; set; }
    
    private readonly List<int> triangles = new List<int>(30000);
    private readonly List<Vector3> vertices = new List<Vector3>(30000);
    private readonly List<Vector3> normals = new List<Vector3>(30000);
    private readonly List<Color> colors = new List<Color>(30000);
    private readonly Cache cache = new Cache();

    private Coroutiner _coroutiner;
    private Coroutiner coroutiner
    {
        get
        {
            if (_coroutiner == null)
            {
                _coroutiner = Create();
            }

            return _coroutiner;

            Coroutiner Create()
            {
                var go = new GameObject("Coroutiner");
                go.hideFlags = HideFlags.HideAndDontSave;
                return go.AddComponent<Coroutiner>();
            }
        }
    }

    private Func<Vector3, float> getSdfFunc;
    private Bounds bounds;
    private Vec3Int resolution;

    private Matrix4x4 gridToWorldMatrix;

    public MarchingCubesMeshGenerator()
    {
    }

    public void GenerateMeshSync(Mesh mesh, Func<Vector3, float> getSDF, float step, Bounds bounds)
    {
        this.bounds = bounds;
        getSdfFunc = getSDF;
        this.resolution = new Vec3Int(
            Mathf.CeilToInt(bounds.size.x / step), 
            Mathf.CeilToInt(bounds.size.y / step), 
            Mathf.CeilToInt(bounds.size.z / step));

        Vector3 scale = new Vector3(
            bounds.size.x / resolution.x, 
            bounds.size.y / resolution.y, 
            bounds.size.z / resolution.z);

        gridToWorldMatrix = Matrix4x4.TRS(bounds.min, Quaternion.identity, scale);

        mesh.Clear();

        GenerateMeshInternal();
        
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);

        mesh.RecalculateBounds();
    }

    public void GenerateMeshAsync(Mesh mesh)
    {
        coroutiner.StartCoroutine(StartGeneratingMeshAsync(mesh));
    }

    private bool done;
    private IEnumerator StartGeneratingMeshAsync(Mesh mesh)
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
        //colors.Clear();
        
        for (var i = 0; i < resolution.x; i++)
        for (var j = 0; j < resolution.y; j++)
        for (var k = 0; k < resolution.z; k++)
        {
            var cellIndex = new Vec3Int(i, j, k);

            int indexMask = getIndexMask(cellIndex);
            var trianglesInCell = MarchingCubesLookupTable.TrianglesData[indexMask];

            for (var index = 0; index < trianglesInCell.verticesIndices.Length; index++)
            {
                var tv = trianglesInCell.verticesIndices[index];
                
                if (cache.TryGetVertexIndexInList(cellIndex, tv, out var vertexIndexInList))
                {
                    triangles.Add(vertexIndexInList);
                }
                else
                {
                    var gridSpacePosition = ConvertVertexIndexToPosition(tv, cellIndex) + cellIndex;
                    var worldSpacePosition = gridToWorldMatrix * gridSpacePosition;

                    var newInd = vertices.Count;
                    vertices.Add(worldSpacePosition);
                    triangles.Add(newInd);
                    normals.Add(getNormal(gridSpacePosition));

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

    private Vector4 ConvertVertexIndexToPosition(int index, Vec3Int ijk)
    {
        switch (index)
        {
            case 0:
            {
                return new Vector4(g(ijk, Vec3Int.right), 0, 0, 1);
            }
            case 1:
            {
                return new Vector4(0, g(ijk, Vec3Int.up), 0, 1);
            }
            case 2:
            {
                return new Vector4(g(ijk + Vec3Int.up, Vec3Int.right), 1, 0, 1);
            }
            case 3:
            {
                return new Vector4(1, g(ijk + Vec3Int.right, Vec3Int.up), 0, 1);
            }
            case 4:
            {
                return new Vector4(0, 1, g(ijk + Vec3Int.up, Vec3Int.forward), 1);
            }
            case 5:
            {
                return new Vector4(1, 1, g(ijk + new Vec3Int(1, 1, 0), Vec3Int.forward), 1);
            }
            case 6:
            {
                return new Vector4(1, 0, g(ijk + Vec3Int.right, Vec3Int.forward), 1);
            }
            case 7:
            {
                return new Vector4(0, 0, g(ijk, Vec3Int.forward), 1);
            }
            case 8:
            {
                return new Vector4(g(ijk + Vec3Int.forward, Vec3Int.right), 0, 1, 1);
            }
            case 9:
            {
                return new Vector4(0, g(ijk + Vec3Int.forward, Vec3Int.up), 1, 1);
            }
            case 10:
            {
                return new Vector4(g(ijk + new Vec3Int(0, 1, 1), Vec3Int.right), 1, 1, 1);
            }
            case 11:
            {
                return new Vector4(1, g(ijk + new Vec3Int(1, 0, 1), Vec3Int.up), 1, 1);
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
        const float h = 0.001f;
        var normal = (new Vector3(1, -1, -1) * getSDF(p + new Vector3(1, -1, -1) * h) +
                      new Vector3(-1, -1, 1) * getSDF(p + new Vector3(-1, -1, 1) * h) +
                      new Vector3(-1, 1, -1) * getSDF(p + new Vector3(-1, 1, -1) * h) +
                      new Vector3(1, 1, 1) * getSDF(p + new Vector3(1, 1, 1) * h)).normalized;
        
        return normal;
    }
    
    private float getSDF(Vector3 p)
    {
        p = gridToWorldMatrix * new Vector4(p.x, p.y, p.z, 1);
        var result = getSdfFunc(p);

        return result;
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

    /*
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        DrawBounds();
    }

    private void DrawBounds()
    {
        var tempMatrix = Gizmos.matrix;
        
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
    */
}