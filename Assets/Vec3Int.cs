using System;
using UnityEngine;

[Serializable]
public struct Vec3Int
{
    public int x, y, z;

    public Vec3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vec3Int operator +(Vec3Int a, Vec3Int b)
    {
        return new Vec3Int(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    
    public static implicit operator Vector3(Vec3Int v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static Vec3Int zero = new Vec3Int(0, 0, 0);
    public static Vec3Int right = new Vec3Int(1, 0, 0);
    public static Vec3Int up = new Vec3Int(0, 1, 0);
    public static Vec3Int forward = new Vec3Int(0, 0, 1);
}