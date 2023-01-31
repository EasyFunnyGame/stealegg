using System;

using UnityEngine;

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
        this.x = int.Parse(coordArr[0]);
        this.z = int.Parse(coordArr[1]);
    }

    public Coord(Vector3 position)
    {
        this.x = Mathf.FloorToInt(position.x);
        this.z = Mathf.FloorToInt(position.z);
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
