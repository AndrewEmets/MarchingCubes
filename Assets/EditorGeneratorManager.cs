using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorGeneratorManager : MonoBehaviour
{
    [SerializeField] private float step;
    [SerializeField] private Transform rootGroupTransform;
    [SerializeField] private Bounds globalBounds;

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

        var sdfTree = CreateSDFTree();

        Bounds bounds = sdfTree.GetBounds();
        bounds.Expand(2 * step);
        bounds.Encapsulate(globalBounds);
        lastBounds = bounds;
        generator.GenerateMeshSync(mesh, sdfTree.GetSDF, step, bounds);
    }

    private SDFTree CreateSDFTree()
    {
        var sdfTree = new SDFTree();
        var rootNode = new SDFNodeShapeGroup();
        rootNode.combine = (r1, r2) => Mathf.Min(r1, r2);
        sdfTree.rootNode = rootNode;

        int depth = 0;        
        ProcessChild(rootGroupTransform, rootNode);

        return sdfTree;

        void ProcessChild(Transform t, SDFNodeShapeGroup parentNode)
        {
            if (depth++ > 10)
                return;

            foreach (Transform child in t)
            {
                var shape = child.GetComponent<SDFShape>();
                if (shape != null)
                {
                    var shapeNode = new SDFNodeSingleShape();
                    shapeNode.shape = shape;
                    
                    child.GetComponents<ProbePointModifier>(shapeNode.pointModifiers);
                    child.GetComponents<SDFModifier>(shapeNode.sdfModifiers);

                    parentNode.childShapes.Add(shapeNode);
                }

                var shapeGroup = child.GetComponent<SDFShapeGroup>();
                if (shapeGroup != null)
                {
                    var groupNode = new SDFNodeShapeGroup();
                    groupNode.combine = shapeGroup.Combine;
                    
                    child.GetComponents<ProbePointModifier>(groupNode.pointModifiers);                    
                    child.GetComponents<SDFModifier>(groupNode.sdfModifiers);

                    parentNode.childShapes.Add(groupNode);

                    ProcessChild(child, groupNode);
                }
            }
        }
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawWireCube(lastBounds.center, lastBounds.size);        
    }
}
