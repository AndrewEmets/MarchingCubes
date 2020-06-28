using System;
using MarchingCubes.Entities;
using UnityEngine;

namespace MarchingCubes.Generator
{
    public class VerticesCache
    {
        private struct nullable<T>
        {
            public bool hasValue;
            public T value;

            public nullable(T value)
            {
                hasValue = true;
                this.value = value;
            }
        }

        private struct Cell
        {
            public nullable<float> sdfValue;
            public (nullable<int>, nullable<int>, nullable<int>) verticesIndex; // in vertices list 0,1,7
        }

        private Cell[,,] cells;

        public VerticesCache()
        {

        }

        public void Validate(Vec3Int size)
        {
            if (cells == null || cells.GetLength(0) != size.x + 1 || cells.GetLength(1) != size.y + 1 ||
                cells.GetLength(2) != size.z + 1)
            {
                cells = new Cell[size.x + 1, size.y + 1, size.z + 1];
            }
        }

        public void Clear()
        {
            for (int i = 0, length0 = cells.GetLength(0); i < length0; i++)
                for (int j = 0, length1 = cells.GetLength(1); j < length1; j++)
                    for (int k = 0, length2 = cells.GetLength(2); k < length2; k++)
                    {
                        cells[i, j, k] = default;
                    }
        }

        #region SDF

        public bool TryGetSdf(Vec3Int cellIndex, out float sdf)
        {
            var data = cells[cellIndex.x, cellIndex.y, cellIndex.z].sdfValue;

            var dataHasValue = data.hasValue;

            sdf = dataHasValue ? data.value : default;
            return dataHasValue;
        }

        public void SetSdf(Vec3Int cellIndex, float value)
        {
            cells[cellIndex.x, cellIndex.y, cellIndex.z].sdfValue = new nullable<float>(value);
        }

        #endregion

        #region Vertices

        private static readonly (int, Vec3Int)[] VertexModificator = {
        /* 0 */ (0, Vec3Int.zero),
        /* 1 */ (1, Vec3Int.zero),
        /* 2 */ (0, Vec3Int.up),
        /* 3 */ (1, Vec3Int.right),
        /* 4 */ (2, Vec3Int.up),
        /* 5 */ (2, Vec3Int.up + Vec3Int.right),
        /* 6 */ (2, Vec3Int.right),
        /* 7 */ (2, Vec3Int.zero),
        /* 8 */ (0, Vec3Int.forward),
        /* 9 */ (1, Vec3Int.forward),
        /*10 */ (0, Vec3Int.forward + Vec3Int.up),
        /*11 */ (1, Vec3Int.forward + Vec3Int.right)
    };

        public bool TryGetVertexIndexInList(Vec3Int cellIndex, int vertexIndex, out int result)
        {
            var (newIndex, cellOffset) = VertexModificator[vertexIndex];
            vertexIndex = newIndex;
            cellIndex += cellOffset;

            nullable<int> data;
            switch (vertexIndex)
            {
                case 0:
                    data = cells[cellIndex.x, cellIndex.y, cellIndex.z].verticesIndex.Item1;
                    break;
                case 1:
                    data = cells[cellIndex.x, cellIndex.y, cellIndex.z].verticesIndex.Item2;
                    break;
                case 2:
                    data = cells[cellIndex.x, cellIndex.y, cellIndex.z].verticesIndex.Item3;
                    break;

                default:
                    throw new Exception("Wrong index");
            }

            var dataHasValue = data.hasValue;
            result = dataHasValue ? data.value : default;
            return dataHasValue;
        }

        public void SetVertexIndex(Vec3Int cellIndex, int vertexIndexInCube, int vertexIndexInList)
        {
            var (newIndex, cellOffset) = VertexModificator[vertexIndexInCube];
            vertexIndexInCube = newIndex;
            cellIndex += cellOffset;

            switch (vertexIndexInCube)
            {
                case 0:
                    cells[cellIndex.x, cellIndex.y, cellIndex.z].verticesIndex.Item1 = new nullable<int>(vertexIndexInList);
                    break;
                case 1:
                    cells[cellIndex.x, cellIndex.y, cellIndex.z].verticesIndex.Item2 = new nullable<int>(vertexIndexInList);
                    break;
                case 2:
                    cells[cellIndex.x, cellIndex.y, cellIndex.z].verticesIndex.Item3 = new nullable<int>(vertexIndexInList);
                    break;

                default:
                    throw new Exception("Wrong index");
            }
        }

        #endregion
    }
}