using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShapeBuilder
{
    public class SDFTree
    {
        public SDFNode rootNode;

        public float GetSDF(Vector3 p)
        {
            return rootNode.GetSDF(p);
        }

        public Bounds GetBounds()
        {
            return rootNode.GetBounds();
        }
    }

    public abstract class SDFNode
    {
        public List<ProbePointModifier> pointModifiers = new List<ProbePointModifier>();
        public List<SDFModifier> sdfModifiers = new List<SDFModifier>();

        public float GetSDF(Vector3 pos)
        {
            for (int i = 0; i < pointModifiers.Count; i++)
            {
                pos = pointModifiers[i].ModifyPoint(pos);
            }

            var res = GetSDFInternal(pos);

            for (int i = 0; i < sdfModifiers.Count; i++)
            {
                res = sdfModifiers[i].ModifySDF(pos, res);
            }

            return res;
        }

        protected abstract float GetSDFInternal(Vector3 pos);

        public abstract Bounds GetBounds();
    }

    public class SDFNodeSingleShape : SDFNode
    {
        public SDFShape shape;

        public override Bounds GetBounds()
        {
            var localBounds = shape.GetLocalBounds();
            var bounds = new Bounds(shape.gameObject.transform.TransformPoint(localBounds.center), Vector3.zero);

            for (int i = 0; i <= 1; i++)
            for (int j = 0; j <= 1; j++)
            for (int k = 0; k <= 1; k++)
            {
                var p = localBounds.min + Vector3.Scale(new Vector3(i, j, k), localBounds.size);

                Debug.DrawRay(p, Vector3.up * 0.05f, Color.blue, 0.5f);

                p = shape.gameObject.transform.TransformPoint(p);
                Debug.DrawRay(p, Vector3.up * 0.05f, Color.yellow);

                bounds.Encapsulate(p);
            }

            return bounds;
        }

        protected override float GetSDFInternal(Vector3 pos)
        {
            var res = shape.GetSDF(pos);
            return res;
        }
    }

    public class SDFNodeShapeGroup : SDFNode
    {
        public List<SDFNode> childShapes = new List<SDFNode>();

        public Func<float, float, float> combine;

        public override Bounds GetBounds()
        {
            var bounds = childShapes[0].GetBounds();

            for (int i = 1; i < childShapes.Count; i++)
            {
                bounds.Encapsulate(childShapes[i].GetBounds());
            }

            return bounds;
        }

        protected override float GetSDFInternal(Vector3 pos)
        {
            var res = childShapes[0].GetSDF(pos);

            for (int i = 1; i < childShapes.Count; i++)
            {
                var r = childShapes[i].GetSDF(pos);
                res = combine(res, r);
            }

            return res;
        }
    }
}