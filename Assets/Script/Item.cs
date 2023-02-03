using UnityEngine;

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

    public void Picked(Player player)
    {
        Debug.Log(string.Format("拾取道具:{0}",gameObject.name));

        switch(itemType)
        {
            case ItemType.Star:
                gameObject.SetActive(false);
                break;

            case ItemType.LureBottle:
                gameObject.SetActive(false);
                player.bottleCount++;
                break;

            case ItemType.Pincers:
                break;

            case ItemType.End:
                Game.Instance.status = GameStatus.WIN;
                break;
        }
       
    }
}
