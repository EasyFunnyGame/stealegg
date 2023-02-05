using System;

using UnityEngine;

public enum Direction
{
    Up,// 0
    Right,// 90
    Down,// 180
    Left,// 270
}

[Serializable]
public struct Coord
{
    public int x;

    public int z;

    public float height;

    public string name;

    public Coord(string tileName, float tileHeight)
    {
        name = tileName;
        height = tileHeight;
        var coordArr = tileName.Split('_');
        x = int.Parse(coordArr[0]);
        z = int.Parse(coordArr[1]);
    }

    public Coord(Vector3 position)
    {
        this.x = Mathf.RoundToInt(position.x);
        this.z = Mathf.RoundToInt(position.z);
        this.height = position.y;
        this.name = string.Format("{0}_{1}",x,z);
    }

    public bool Equals(Coord coord)
    {
        return x == coord.x && z == coord.z && height == coord.height;
    }

    public Coord Clone()
    {
        return new Coord(new Vector3(x, height, z));
    }

    //static Coord ParseByName(string name)
    //{
    //    var coordArr = name.Split('_');
    //    if (coordArr.Length != 2)
    //        throw new Exception(string.Format("Can Not Parse Coord By Name {0},0",name));
    //    var x = -1;

    //    var z = -1;

    //    var succees = int.TryParse(coordArr[0],out x);
    //    if(!succees)
    //    {
    //        throw new Exception(string.Format("Can Not Parse Coord By Name {0}, x", name));
    //    }
    //    succees = int.TryParse(coordArr[1], out z);
    //    if (!succees)
    //    {
    //        throw new Exception(string.Format("Can Not Parse Coord By Name {0}, z", name));
    //    }

    //    return new Coord();
    //}
}
