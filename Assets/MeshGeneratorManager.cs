using System.Collections;
using UnityEngine;

public class MeshGeneratorManager : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private Camera camera;
    
    [SerializeField] private MarchingCubesMeshGeneratorAsync generator;

    private IEnumerator Start()
    {
        for (var i = 0; i < 10; i++)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.N));
            GenerateChunk(new Vector3Int(i, 0, 0));

            var startPos = camera.transform.position;
            var finalPos = camera.transform.position + new Vector3(15, 0, 0);
            const float time = 1f;
            for (float t = 0; t <= 1; t += Time.deltaTime / time)
            {
                camera.transform.position = Vector3.LerpUnclamped(startPos, finalPos, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }
        }
    }

    public void GenerateChunk(Vector3Int cell)
    {
        var go = new GameObject("Chunk " + cell);
        go.transform.SetParent(this.transform);
        
        var offset = Vector3.Scale(cell, generator.Resolution);
        go.transform.localPosition = offset;
        go.transform.localScale = Vector3.one;
        generator.Offset = offset;
        
        var meshFilter = go.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        meshFilter.sharedMesh = mesh;

        var renderer = go.AddComponent<MeshRenderer>();
        renderer.material = material;

        generator.GenerateMesh(mesh);
    }
}