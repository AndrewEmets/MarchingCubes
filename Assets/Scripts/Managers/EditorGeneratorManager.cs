using System;
using System.Collections;
using System.Collections.Generic;
using MarchingCubes.Generator;
using ShapeBuilder;
using UnityEngine;

public class EditorGeneratorManager : MonoBehaviour
{
    [SerializeField] private float step;
    //[SerializeField] private Bounds globalBounds;

    [SerializeField] private SDFHierarchy sdfHierarchy;
    [SerializeField] private float expandRadius;

    private MarchingCubesMeshGenerator generator;

    [ContextMenu("Generate")]
    public void Generate()
    {
        OnValidateTest();
    }

    private Bounds lastBounds;

    private void OnValidateTest() 
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh == null)
        {
            meshFilter.sharedMesh = new Mesh();
        }

        if (generator == null)
        {
            generator = new MarchingCubesMeshGenerator();
        }

        var mesh = meshFilter.sharedMesh;

        var sdfTree = sdfHierarchy.CreateSDFTree();

        Bounds bounds = sdfTree.GetBounds();
        bounds.Expand(2 * step);
        //bounds.Encapsulate(globalBounds);
        bounds.Expand(expandRadius);
        lastBounds = bounds;
        generator.GenerateMesh(mesh, sdfTree.GetSDF, step, bounds);
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawWireCube(lastBounds.center, lastBounds.size);        
    }
}
