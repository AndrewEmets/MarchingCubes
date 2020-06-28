using ShapeBuilder;
using UnityEngine;

public class SDFHierarchy : MonoBehaviour
{
    public SDFTree CreateSDFTree()
    {
        var sdfTree = new SDFTree();
        var rootNode = new SDFNodeShapeGroup();
        rootNode.combine = (r1, r2) => Mathf.Min(r1, r2);
        sdfTree.rootNode = rootNode;

        ProcessChild(this.transform, rootNode);

        return sdfTree;

        void ProcessChild(Transform t, SDFNodeShapeGroup parentNode)
        {
            foreach (Transform child in t)
            {
                var shape = child.GetComponent<SDFShape>();
                if (shape != null)
                {
                    shape.Init();
                    var shapeNode = new SDFNodeSingleShape();
                    shapeNode.shape = shape;
                    
                    child.GetComponents<ProbePointModifier>(shapeNode.pointModifiers);
                    child.GetComponents<SDFModifier>(shapeNode.sdfModifiers);
                    for (int i = 0; i < shapeNode.pointModifiers.Count; i++)
                    {
                        shapeNode.pointModifiers[i].Init();
                    }

                    parentNode.childShapes.Add(shapeNode);
                }

                var shapeGroup = child.GetComponent<SDFShapeGroup>();
                if (shapeGroup != null)
                {
                    var groupNode = new SDFNodeShapeGroup();
                    groupNode.combine = shapeGroup.Combine;
                    
                    child.GetComponents<ProbePointModifier>(groupNode.pointModifiers);                    
                    child.GetComponents<SDFModifier>(groupNode.sdfModifiers);
                    for (int i = 0; i < groupNode.pointModifiers.Count; i++)
                    {
                        groupNode.pointModifiers[i].Init();
                    }
                    
                    parentNode.childShapes.Add(groupNode);

                    ProcessChild(child, groupNode);
                }
            }
        }
    }
}
