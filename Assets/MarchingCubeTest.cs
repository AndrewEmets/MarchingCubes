using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarchingCubeTest : MonoBehaviour
{
    [SerializeField] private MarchingCubesVertices vertices;

    [SerializeField, Range(0, 255)] private int index;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            index++;
            OnValidate();
        }
    }

    private Mesh mesh
    {
        get
        {
            var m = GetComponent<MeshFilter>().sharedMesh; 
            if (m == null)
                GetComponent<MeshFilter>().sharedMesh = new Mesh();
            
            return GetComponent<MeshFilter>().sharedMesh;
        }
    }
    private List<Vector3> verticesList = new List<Vector3>();
    private List<int> tris = new List<int>();
    
    private void OnValidate()
    {
        var trianglesIndices = vertices.Vertices[index];

        if (trianglesIndices != null)
        {
            mesh.Clear();
            verticesList.Clear();
            tris.Clear();
            //DrawPolygonGizmo(trianglesIndices.vertIndices.Select(ConvertIndexToPosition).ToArray());

            foreach (var v in trianglesIndices.vertIndices.Select(ConvertIndexToPosition))
            {
                verticesList.Add(v);
                tris.Add(tris.Count);
            }

            mesh.SetVertices(verticesList);
            mesh.SetTriangles(tris, 0);
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        for (var i = 0; i < 8; i++)
        {
            var x = (i & 1) >= 1 ? 1 : 0;
            var y = (i & 2) >= 1 ? 1 : 0;
            var z = (i & 4) >= 1 ? 1 : 0;

            var marked = ((index >> i) & 1) >= 1;

            Gizmos.color = marked ? Color.white : new Color(1, 1, 1, 0.2f);

            Gizmos.DrawSphere(new Vector3(x, y, z), 0.1f);
        }

        var trianglesIndices = vertices.Vertices[index];

        if (trianglesIndices != null)
        {
            //DrawPolygonGizmo(trianglesIndices.vertIndices.Select(ConvertIndexToPosition).ToArray());
        }
    }

    private Vector3 ConvertIndexToPosition(int index)
    {
        switch (index)
        {
            case 0: return new Vector3(.5f, 0, 0);
            case 1: return new Vector3(0, .5f, 0);
            case 2: return new Vector3(.5f, 1, 0);
            case 3: return new Vector3(1, .5f, 0);

            case 4: return new Vector3(0, 1, .5f);
            case 5: return new Vector3(1, 1, .5f);
            case 6: return new Vector3(1, 0, .5f);
            case 7: return new Vector3(0, 0, .5f);


            case 8: return new Vector3(.5f, 0, 1);
            case 9: return new Vector3(0, .5f, 1);
            case 10: return new Vector3(.5f, 1, 1);
            case 11: return new Vector3(1, .5f, 1);
        }
        
        throw new Exception("Wrong vertex index");
    }

    private static void DrawPolygonGizmo(Vector3[] positions)
    {
        for (var i = 0; i < positions.Length / 3; i++)
        {
            var k = i * 3;

            Gizmos.color = Color.white;
            for (var j = 0; j < 3; j++)
            {
                Gizmos.DrawLine(positions[k + j], positions[k + (j + 1) % 3]);
            }

            var normal = Vector3.Cross(positions[k + 1] - positions[k], positions[k + 2] - positions[k]).normalized * 0.1f;
            var avg = (positions[k + 1] + positions[k] + positions[k + 2]) / 3f;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(avg, normal);
        }
    }
}
