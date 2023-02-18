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

    public bool upper = false;

    public Vector3 upperPosition = new Vector3(-0.06f,0.6f,0);

    public Vector3 lowerPosition = new Vector3();

    public Transform iconPosition;

    public GameObject debug_sphere;

    public bool picked = false;

    public ItemIconOnUI icon;

    private void Awake()
    {
        HideDebugSphere();
        upper = false ;
    }

    // Start is called before the first frame update
    void Start()
    {
    }


    public bool Picked(Player player)
    {
        Debug.Log(string.Format("拾取道具:{0}",gameObject.name));
        var delete = false;
        switch(itemType)
        {
            case ItemType.Star:
                picked = true;
                gameObject.SetActive(false);
                icon.gameObject.SetActive(false);
                Game.Instance.energy += 5;
                delete = true;
                break;

            case ItemType.LureBottle:
                picked = true;
                gameObject.SetActive(false);
                player.bottleCount++;
                player.m_animator.Play("Player_PickUp");
                delete = true;
                break;

            case ItemType.Pincers:
                break;

            case ItemType.End:
                Game.Instance.ReachEnd();
                break;

            case ItemType.Graff:
                // Game.Instance.Graff();
                break;
        }
        return delete;
    }

    public void HideDebugSphere()
    {
        if(debug_sphere)
        {
            debug_sphere.SetActive(false);
        }
    }

    public float MoveSmoothTime = 0.2f;
    public Vector3 velocity = new Vector3();
    // Update is called once per frame
    void Update()
    {
        if(!iconPosition )
        {
            return;
        }
        
        if (upper)
        {
            iconPosition.position = Vector3.SmoothDamp(iconPosition.position, transform.position + upperPosition, ref velocity, MoveSmoothTime);
        }
        else
        {
            iconPosition.position = Vector3.SmoothDamp(iconPosition.position, transform.position + lowerPosition, ref velocity, MoveSmoothTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.GetComponent<Character>())
            upper = true;
        velocity = Vector3.zero;
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.gameObject.GetComponent<Character>())
            upper = false;
        velocity = Vector3.zero;
    }
    
    public Vector3 GetIconPosition()
    {
        if (!iconPosition)
        {
            return transform.position;
        }
        return iconPosition.position;
    }

}
