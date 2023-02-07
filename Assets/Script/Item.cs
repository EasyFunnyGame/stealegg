using UnityEngine;

public enum ItemType
{
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

    public Transform iconLower;

    public Transform iconUpper;

    public Transform iconPosition;

    public GameObject debug_sphere;

    private void Awake()
    {
        HideDebugSphere();
        targetIconPosition = iconLower;
    }

    // Start is called before the first frame update
    void Start()
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
                break;

            case ItemType.Graff:
                Game.Instance.graffable = true;
                break;
        }
       
    }

    public void HideDebugSphere()
    {
     
        if(debug_sphere)
        {
            debug_sphere.SetActive(false);
        }
    }

    public float MoveSmoothTime = 0.1f;
    // Update is called once per frame
    void Update()
    {
        if(!iconUpper || !iconLower || !iconPosition )
        {
            return;
        }
        Vector3 velocity = Vector3.zero;
        if (targetIconPosition == iconUpper)
        {
            targetIconPosition.position = Vector3.SmoothDamp(iconUpper.position, iconLower.position, ref velocity, MoveSmoothTime);
        }
        else
        {
            targetIconPosition.position = Vector3.SmoothDamp(iconLower.position, iconUpper.position, ref velocity, MoveSmoothTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!iconUpper || !iconLower || !iconPosition)
        {
            return;
        }
        targetIconPosition = iconUpper;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!iconUpper || !iconLower || !iconPosition)
        {
            return;
        }
        targetIconPosition = iconLower;
    }

    private Transform targetIconPosition;
    
    public Vector3 GetIconPosition()
    {
        if (!iconUpper || !iconLower || !iconPosition)
        {
            return transform.position;
        }
        return iconPosition.position;
    }
}
