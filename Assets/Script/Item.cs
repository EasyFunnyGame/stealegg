﻿using UnityEngine;

public enum ItemType
{
    /** 玩家出生点 **/
    Start,

    /** 星星 **/
    Star,

    /** 钳子 **/
    Pincers,

    /** 水井盖 **/
    ManHoleCover,

    /** 瓶子 **/
    LureBottle,
   
    /** 草丛 **/
    Growth,

    /** 涂鸦 **/
    Graff,

    /** 出口 **/
    End,
}

public static class ItemName
{
    /** 玩家出生点 **/
    public const string Item_Start = "Item_Start";

    public const string Item_Star = "Item_Star";

    public const string Item_Pincers = "Item_Pincers";

    public const string Item_ManholeCover = "Item_ManholeCover";

    public const string Item_LureBottle = "Item_LureBottle";

    public const string item_Growth = "item_Growth";

    public const string Item_Graff = "Item_Graff";

    public const string Item_End = "Item_End";
}

public class Item : MonoBehaviour
{
    [SerializeField]
    public ItemType itemType;

    [SerializeField]
    public Coord coord;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
