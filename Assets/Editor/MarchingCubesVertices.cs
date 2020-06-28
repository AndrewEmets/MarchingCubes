using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MarchingCubes.Entities;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Marching cubes vertices")]
public class MarchingCubesVertices : ScriptableObject
{
    [SerializeField] private List<VerticesArray> vertices;

    public List<VerticesArray> Vertices => vertices;

    [CustomEditor(typeof(MarchingCubesVertices))] 
    public class MarchingCubesVerticesEditor : Editor
    {
        private MarchingCubesVertices t;

        private void OnEnable()
        {
            t = target as MarchingCubesVertices;
        }

        [SerializeField, HideInInspector]
        private string indexString, trianglesString;
        
        public override void OnInspectorGUI()
        {
            indexString = EditorGUILayout.TextField("Index", indexString);
            trianglesString = EditorGUILayout.TextField("Triangles", trianglesString);
            if (GUILayout.Button("Add new patter"))
            {
                AddNewPattern(indexString, trianglesString);
                EditorUtility.SetDirty(t);
            }

            if (GUILayout.Button("Generate data file"))
            {
                GenerateDataFile(t.vertices);
            }
            
            if (GUILayout.Button("Clear all"))
            {
                Undo.RecordObject(t, "Clear triangles data");
                
                foreach (var v in t.vertices)
                {
                    v.vertIndices = new int[0];
                }

                EditorUtility.SetDirty(t);
            }
            
            for (int i = 0; i < t.vertices.Count; i++)
            {
                var sb = new StringBuilder();
                for (var j = 0; j < t.vertices[i].vertIndices.Length; j++)
                {
                    var ar = t.vertices[i].vertIndices[j];
                    sb.Append(ar + (j == t.vertices[i].vertIndices.Length - 1 ? string.Empty : ", "));
                }

                GUILayout.Label(i + " – " + sb.ToString());
            }

            base.OnInspectorGUI();
        }

        public void GenerateDataFile(List<VerticesArray> tVertices)
        {
            var sb = new StringBuilder();

            sb.AppendLine("public static class MarchingCubesLookupTable");
            sb.AppendLine("{");
            {
                sb.AppendLine("\tpublic static TrianglesData[] TrianglesData = new[]");
                sb.AppendLine("\t{");
                for (int i = 0; i < tVertices.Count; i++)
                {
                    var sb2 = new StringBuilder();
                    foreach (var t in tVertices[i].vertIndices)
                    {
                        sb2.Append(t + ", ");
                    }

                    sb.AppendLine($"\t\tnew TrianglesData(new int[]{{{sb2.ToString()}}}),");
                }
                sb.AppendLine("\t};");
            }
            sb.AppendLine("}");
            
            using (var sw = new StreamWriter(Application.dataPath + "/MarchingCubesLookupTable.cs"))
            {
                sw.Write(sb.ToString());
            }
            
            AssetDatabase.Refresh();
        }

        private void AddNewPattern(string indexMaskString, string trianglesString)
        {
            var indexMask = uint.Parse(indexMaskString);
            var vertices = trianglesString.Split(',').Select(int.Parse).ToArray();

            for (uint flipXi = 0; flipXi <= 1; flipXi++)
            {
                var flipX = flipXi == 1;

                if (flipX)
                {
                    indexMask = flipIndexMaskX(indexMask);
                    FlipVerticesX(vertices);
                }
                
                for (var side = 1; side <= 6; side++)
                {
                    var even = side % 2 == 0;

                    for (var q = 0; q < 3; q++)
                    {
                        Apply(indexMask, vertices);
                        indexMask = even ? RotateY_cw(indexMask) : RotateY_ccw(indexMask);
                        TransformVertices(vertices, even ? rotVertY_cw : rotVertY_ccw);
                    }

                    Apply(indexMask, vertices);
                    indexMask = even ? RotateX_ccw(indexMask) : RotateX_cw(indexMask);
                    TransformVertices(vertices, even ? rotVertX_ccw: rotVertX_cw);
                }
            }
        }

        private static readonly int[] rotVertY_cw = {7, 9, 4, 1, 10, 2, 0, 8, 6, 11, 5, 3};
        private static readonly int[] rotVertY_ccw = {6, 3, 5, 11, 2, 10, 8, 0, 7, 1, 4, 9};
        private static readonly int[] rotVertX_cw = {2, 4, 10, 5, 9, 11, 3, 1, 0, 7, 8, 6};
        private static readonly int[] rotVertX_ccw = {8, 7, 0, 6, 1, 3, 11, 9, 10, 4, 2, 5};

        private static readonly int[] flipVertX = {0, 3, 2, 1, 5, 4, 7, 6, 8, 11, 10, 9};

        private static void FlipVerticesX(int[] verts)
        {
            TransformVertices(verts, flipVertX);
            Array.Reverse(verts);
        }
        
        private static void TransformVertices(int[] verts, int[] rotateMatrix)
        {
            for (var i = 0; i < verts.Length; i++)
            {
                verts[i] = rotateMatrix[verts[i]];
            }
        }

        private static uint RotateX_ccw(uint i)
        {
            i = swapBits(i, 4, 0);
            i = swapBits(i, 0, 2);
            i = swapBits(i, 2, 6);

            i = swapBits(i, 5, 1);
            i = swapBits(i, 1, 3);
            i = swapBits(i, 3, 7);

            return i;
        }

        private static uint RotateX_cw(uint i)
        {
            i = swapBits(i, 0, 4);
            i = swapBits(i, 4, 6);
            i = swapBits(i, 6, 2);
            
            i = swapBits(i, 1, 5);
            i = swapBits(i, 5, 7);
            i = swapBits(i, 7, 3);
            
            return i;
        }


        private static uint RotateY_cw(uint i)
        {
            i = swapBits(i, 1, 0);
            i = swapBits(i, 5, 1);
            i = swapBits(i, 4, 5);
            
            i = swapBits(i, 3, 2);
            i = swapBits(i, 7, 3);
            i = swapBits(i, 6, 7);
            
            return i;
        }

        private static uint RotateY_ccw(uint i)
        {
            i = swapBits(i, 1, 0);
            i = swapBits(i, 0, 4);
            i = swapBits(i, 4, 5);
            
            i = swapBits(i, 3, 2);
            i = swapBits(i, 2, 6);
            i = swapBits(i, 6, 7);
            
            return i;
        }

        private void Apply(uint indexMask, int[] triangles)
        {
            //Debug.Log(indexMask.ToString().PadLeft(2, ' ') + " – " + Convert.ToString(indexMask, 2).PadLeft(8, '0').Replace('0', '-'));
            t.vertices[(int) indexMask].vertIndices = triangles.ToArray();
        }

        private static uint flipIndexMaskX(uint indexMask)
        {
            var result = indexMask;
            
            result = swapBits(result, 0, 1);
            result = swapBits(result, 2, 3);
            result = swapBits(result, 4, 5);
            result = swapBits(result, 6, 7);
            
            return result;
        }

        private static uint swapBits(uint a, int bit1, int bit2)
        {
            var bit1Value = (a >> bit1) & 1;
            var bit2Value = (a >> bit2) & 1;

            a = (uint) ((a & ~(1 << bit1)) | (bit2Value << bit1));
            a = (uint) ((a & ~(1 << bit2)) | (bit1Value << bit2));

            return a;
        }
    }
}