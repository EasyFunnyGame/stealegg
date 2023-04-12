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
public class Coord
{
    public int x;

    public int z;

    public float height;

    public string name;

    public Coord(){
        x = int.MinValue;
        z = int.MinValue;
        height = int.MinValue;
    }


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

    public Coord(int x, int z , float height)
    {
        this.x = Mathf.RoundToInt(x);
        this.z = Mathf.RoundToInt(z);
        this.height = height;
        this.name = string.Format("{0}_{1}", x, z);
    }

    public bool Equals(Coord coord)
    {
        return x == coord.x && z == coord.z && Mathf.Abs(height - coord.height)<0.5f;
    }

    public bool EqualsIgnoreY(Coord coord)
    {
        return x == coord.x && z == coord.z;
    }

    public Coord Clone()
    {
        return new Coord(new Vector3(x, height, z));
    }
    
    public bool isMin
    {
        get
        {
            return x == int.MinValue && z == int.MinValue;
        }
    }

    public bool isMax
    {
        get
        {
            return x == int.MaxValue && z == int.MaxValue;
        }
    }

    public bool isLegal
    {
        get
        {
            return x != int.MinValue && z != int.MinValue && x != int.MaxValue && z != int.MaxValue;
        }
    }

    public void SetMax()
    {
        x = int.MaxValue;
        z = int.MaxValue;
    }

    public void SetMin()
    {
        x = int.MinValue;
        z = int.MinValue;
    }


    public static bool WithIn(Coord from , Coord to, int range)
    {
        if (!from.isLegal || !to.isLegal) return false;
        var distanceX = Math.Abs(from.x - to.x);
        var distanceZ = Math.Abs(from.z - to.z);
        return distanceX <= range && distanceZ <= range;

    }

    public static int MinDistance(Coord from, Coord to)
    {
        if (!from.isLegal || !to.isLegal) return int.MinValue;
        var distanceX = Math.Abs(from.x - to.x);
        var distanceZ = Math.Abs(from.z - to.z);
        var distance = Math.Min(distanceX, distanceZ);
        return distance;
    }

    public static int Distance(Coord from, Coord to)
    {
        if (!from.isLegal || !to.isLegal) return int.MinValue;
        var distanceX = Math.Abs(from.x - to.x);
        var distanceZ = Math.Abs(from.z - to.z);
        var distance = Math.Max(distanceX, distanceZ);
        return distance;
    }

    public static Coord TransformFromPosition(Vector3 position)
    {
        return new Coord(position);
    }


    public static bool inLine(Coord source, Coord target)
    {
        return (source.x == target.x || source.z == target.z) && (Math.Abs(source.height - target.height) < 0.1f);
    }
    
    public static readonly Coord Illegal = new Coord(int.MinValue, int.MinValue, int.MinValue);
}

