using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarchingCubes.Entities;
using UnityEngine;

namespace MarchingCubes.Generator
{
    public class Coroutiner : MonoBehaviour
    {
    }

    public class MarchingCubesMeshGenerator
    {        
        private readonly List<int> triangles = new List<int>(30000);
        private readonly List<Vector3> vertices = new List<Vector3>(30000);
        private readonly List<Vector3> normals = new List<Vector3>(30000);
        //private readonly List<Color> colors = new List<Color>(30000);
        private readonly VerticesCache cache = new VerticesCache();

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
        private Mesh mesh;

        public void GenerateMesh(Mesh mesh, Func<Vector3, float> getSDF, float step, Bounds bounds)
        {
            this.mesh = mesh;
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

            if (Application.isPlaying)
            {
                GenerateMeshAsync();
            }
            else
            {
                GenerateMeshSync();
            }
        }

        private void GenerateMeshSync()
        {
            mesh.Clear();

            done = false;
            GenerateMeshInternalAsync();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            //mesh.SetColors(colors);

            mesh.RecalculateBounds();
        }

        private void GenerateMeshAsync()
        {
            coroutiner.StartCoroutine(StartGeneratingMeshAsync());
        }

        private bool done;
        private IEnumerator StartGeneratingMeshAsync()
        {
            mesh.Clear();

            done = false;
            var t = new Thread(GenerateMeshInternalAsync);
            t.Start();

            yield return new WaitWhile(() => !done);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            //mesh.SetColors(colors);
            
            mesh.RecalculateBounds();
        }

        object locker = new object();
        void GenerateMeshInternalAsync()
        {
            cache.Validate(resolution);
            cache.Clear();
            
            triangles.Clear();
            vertices.Clear();
            normals.Clear();
            //colors.Clear();
            
            var m = resolution.x * resolution.y * resolution.z;
            var ryz = resolution.y * resolution.z;
            Parallel.For(0, m, Iterate);

            void Iterate(int n)
            {
                int i = n / ryz;
                int j = (n / resolution.z) % resolution.y;
                int k = n % resolution.z;

                var cellIndex = new Vec3Int(i, j, k);

                int indexMask = getIndexMask(cellIndex);
                var trianglesInCell = MarchingCubesLookupTable.TrianglesData[indexMask];

                var trianglesGroupsCount = trianglesInCell.verticesIndices.Length / 3;
                Parallel.For(0, trianglesGroupsCount, tgi => 
                {
                    lock (locker)
                    for (var ti = 0; ti < 3; ti++)
                    {
                        var index = tgi*3 + ti;
                        var tv = trianglesInCell.verticesIndices[index];
                        
                        if (cache.TryGetVertexIndexInList(cellIndex, tv, out var vertexIndexInList))
                        {
                            triangles.Add(vertexIndexInList);
                        }
                        else
                        {
                            var gridSpacePosition = ConvertVertexIndexToPosition(tv, cellIndex) + cellIndex;
                            var worldSpacePosition = gridToWorldMatrix.MultiplyPoint3x4(gridSpacePosition);

                            var newInd = vertices.Count;
                            vertices.Add(worldSpacePosition);
                            triangles.Add(newInd);
                            normals.Add(getNormal(gridSpacePosition));

                            cache.SetVertexIndex(cellIndex, tv, newInd);
                        }
                    }
                });
            }

            done = true;
        }


        private int getIndexMask(Vec3Int ijk)
        {
            var result = 0;

            Parallel.For(0, 8, Iterate);

            return result;

            void Iterate(int i)
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
        }

        private Vector3 ConvertVertexIndexToPosition(int index, Vec3Int ijk)
        {
            switch (index)
            {
                case 0: return new Vector3(g(ijk, Vec3Int.right), 0, 0);
                case 1: return new Vector3(0, g(ijk, Vec3Int.up), 0);
                case 2: return new Vector3(g(ijk + Vec3Int.up, Vec3Int.right), 1, 0);
                case 3: return new Vector3(1, g(ijk + Vec3Int.right, Vec3Int.up), 0);
                case 4: return new Vector3(0, 1, g(ijk + Vec3Int.up, Vec3Int.forward));
                case 5: return new Vector3(1, 1, g(ijk + new Vec3Int(1, 1, 0), Vec3Int.forward));
                case 6: return new Vector3(1, 0, g(ijk + Vec3Int.right, Vec3Int.forward));
                case 7: return new Vector3(0, 0, g(ijk, Vec3Int.forward));
                case 8: return new Vector3(g(ijk + Vec3Int.forward, Vec3Int.right), 0, 1);
                case 9: return new Vector3(0, g(ijk + Vec3Int.forward, Vec3Int.up), 1);
                case 10: return new Vector3(g(ijk + new Vec3Int(0, 1, 1), Vec3Int.right), 1, 1);
                case 11: return new Vector3(1, g(ijk + new Vec3Int(1, 0, 1), Vec3Int.up), 1);
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

        const float h = 0.01f;

        private readonly Vector3[] vArray = new Vector3[]
        {
            new Vector3(1, -1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(-1, 1, -1),
            new Vector3(1, 1, 1)
        };

        private readonly Vector3[] hArray = new Vector3[]
        {
            new Vector3(h, -h, -h),
            new Vector3(-h, -h, h),
            new Vector3(-h, h, -h),
            new Vector3(h, h, h)
        };

        object calculateNormalLocker = new object();

        Vector3 getNormal(Vector3 p) // for function f(p)
        {
            Vector3 sum = Vector3.zero;

            Parallel.For(0, 4, Iterate);

            var normal = sum.normalized;
            return normal;

            void Iterate(int i)
            {
                var res = vArray[i] * getSDF(p + hArray[i]);
                lock(calculateNormalLocker)
                    sum += res;
            }

            /*
            var normal = (vArray[0] * getSDF(p + hArray[0]) +
                        vArray[1] * getSDF(p + hArray[1]) +
                        vArray[2] * getSDF(p + hArray[2]) +
                        vArray[3] * getSDF(p + hArray[3])).normalized;
            */
        }
        
        private float getSDF(Vector3 p)
        {
            p = gridToWorldMatrix.MultiplyPoint3x4(p);
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
    }
}