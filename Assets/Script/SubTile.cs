using System;


[Serializable]
public class SubTile
{
    public bool blocked;
    public GridTile tile_s;
    public int num;
}


public enum efind_path { once_per_turn, max_tiles, on_click, on_hover }