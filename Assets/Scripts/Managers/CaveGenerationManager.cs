using System;
using System.Collections;
using System.Collections.Generic;
using MarchingCubes.Generator;
using UnityEngine;
using static UnityEngine.GameObject;
using Random = UnityEngine.Random;

public class CaveGenerationManager : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject chunk_prefab;
    [SerializeField] private GameObject light_prefab;
    [SerializeField] private float step;
    [SerializeField] private float offset;
    [SerializeField] private float scale;
    [SerializeField] private int octaves;

    [SerializeField] private Transform target;
    
    private MarchingCubesMeshGenerator generator;

    private Vector3Int? currentCell;

    private readonly Dictionary<Vector3Int, GameObject> chunks = new Dictionary<Vector3Int, GameObject>();

    readonly Queue<Vector3Int> generationQueue = new Queue<Vector3Int>();
    
    private void Awake()
    {
        generator = new MarchingCubesMeshGenerator();
    }

    [SerializeField] private int extent = 3;
    
    private void Start()
    {
        StartCoroutine(StartRemoving());

        /*
        for (var i = -extent; i <= extent; i++)
        for (var j = -extent; j <= extent; j++)
        for (var k = -extent; k <= extent; k++)
        {
            
            var c = new Vector3Int(i, j, k);
            generationQueue.Enqueue(c);
        }
        */

        IEnumerator StartRemoving()
        {
            var chunksToRemove = new List<Vector3Int>();
            
            while (true)
            {
                chunksToRemove.Clear();
                yield return new WaitForSeconds(10f);

                var pos = target.position - Vector3.one * 0.5f;

                foreach (var chunk in chunks)
                {
                    var delta = pos - chunk.Key;
                    var length = Math.Abs(delta.x) + Math.Abs(delta.y) + Math.Abs(delta.z);

                    if (length > 4)
                    {
                        chunksToRemove.Add(chunk.Key);
                    }
                }

                foreach (var i in chunksToRemove)
                {
                    Destroy(chunks[i].gameObject);
                    chunks.Remove(i);
                }
            }
        }
    }

    private void Update()
    {
        var currentTargetCell = getTargetCell(target.position);

        if (!currentCell.HasValue || currentTargetCell != currentCell.Value)
        {
            const int generationExtent = 1;
            for (var i = -generationExtent; i <= generationExtent; i++)
            for (var j = -generationExtent; j <= generationExtent; j++)
            for (var k = -generationExtent; k <= generationExtent; k++)
            {
                var c = currentTargetCell + new Vector3Int(i, j, k);
                generationQueue.Enqueue(c);
            }
            
            currentCell = currentTargetCell;
        }

        if (generationQueue.Count != 0)
        {
            if (!generator.isBusy)
            {
                Generate(generationQueue.Dequeue());
            }
        }
    }

    Vector3Int getTargetCell(Vector3 targetPosition)
    {
        var pos = targetPosition;
        var result = new Vector3Int((int) Math.Floor(pos.x), (int) Math.Floor(pos.y), (int) Math.Floor(pos.z));

        return result;
    }

    private void Generate(Vector3Int cell)
    {
        if (chunks.ContainsKey(cell))
        {
            return;
        }
        
        var gameObject = Instantiate(chunk_prefab, parent);
        gameObject.name += " - " + cell;
        gameObject.transform.localPosition = cell;
        gameObject.transform.localScale = Vector3.one;

        var mesh = new Mesh();
        
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        
        var bounds = new Bounds(cell + Vector3.one * 0.5f, Vector3.one);
        
        generator.GenerateMesh(mesh, GetSdf, step, bounds, cell);

        if (false)
        {
            var light = Instantiate(light_prefab);

            var randomPosition = new Vector3(Random.value, Random.value, Random.value);

            var normal = getNormal(randomPosition);
            var field = GetSdf(randomPosition);
            randomPosition += normal * (-field + 0.025f);

            light.transform.position = cell + randomPosition;

            light.transform.SetParent(gameObject.transform);

            var material = light.transform.Find("Sphere").GetComponent<MeshRenderer>().material;
            var color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
            material.SetColor("_EmissionColor", color * 2f);
            light.GetComponent<Light>().color = color;
        }
        
        chunks.Add(cell, gameObject);
    }
    
    const float h = 0.01f;
    private readonly object calculateNormalLocker = new object();
    Vector3 getNormal(Vector3 p) // for function f(p)
    {
        var sum = Vector3.zero;

        //Parallel.For(0, 4, Iterate);
        for (int i = 0; i < 4; i++)
        {
            Iterate(i);
        }

        var normal = sum.normalized;
        return normal;

        void Iterate(int i)
        {
            var res = vArray[i] * GetSdf(p + hArray[i]);
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


    private float GetSdf(Vector3 pos)
    {
        var n = Perlin.Fbm(pos * scale, octaves) - offset;

        return n;
    }
}
