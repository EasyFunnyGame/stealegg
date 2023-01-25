using System;

using UnityEngine;

[Serializable]
public struct Coord
{
    public int x;

    public int z;

    public float height;

    public string name;

    public Coord(Vector3 position)
    {
        this.x = Mathf.FloorToInt(position.x);
        this.z = Mathf.FloorToInt(position.z);
        this.height = position.y;
        this.name = string.Format("{0}_{1}",x,z);
    }
   
}
