using System.Collections.Generic;
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

    private List<Character> m_characters = new List<Character>();

    protected virtual void Awake()
    {
        var x = int.Parse(transform.position.x.ToString());
        var z = int.Parse(transform.position.z.ToString());
        coord = new Coord(x,z,transform.position.y);
        HideDebugSphere();
        upper = false ;
    }

    public void Init()
    {
        var x = int.Parse(transform.position.x.ToString());
        var z = int.Parse(transform.position.z.ToString());
        coord = new Coord(x, z, transform.position.y);
        HideDebugSphere();
        upper = false;
    }

    public bool Picked(Player player)
    {
        //Debug.Log(string.Format("拾取道具:{0}",gameObject.name));
        var delete = false;
        switch(itemType)
        {
            case ItemType.Star:
                picked = true;
                gameObject.SetActive(false);
                icon.gameObject.SetActive(false);

                Game.Instance.gainEergy = 5;
                delete = true;
                AudioPlay.Instance.PlayStarGain();
                break;

            case ItemType.LureBottle:
                
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
            iconPosition.position = Vector3.SmoothDamp(iconPosition.position, transform.position + upperPosition + new Vector3(0, enemyHightOffset, 0), ref velocity, MoveSmoothTime);
        }
        else
        {
            iconPosition.position = Vector3.SmoothDamp(iconPosition.position, transform.position + lowerPosition, ref velocity, MoveSmoothTime);
        }
    }

    private float enemyHightOffset = 0.0f;

    protected virtual void OnTriggerEnter(Collider other)
    {


        upper = true;

        velocity = Vector3.zero;


        var character = other.transform.parent.GetComponent<Character>();
        if(character is EnemyStatic)
        {
            enemyHightOffset = 0.5f;
        }
        else if(character is Player)
        {
            enemyHightOffset = 0.0f;
        }
        else
        {
            enemyHightOffset = 0.3f;
        }
        //else if(character is EnemyDistracted)
        //{
        //    enemyHightOffset = 0.12f;
        //}
        //else if(character is EnemyPatrol)
        //{
        //    enemyHightOffset = 0.12f;
        //}
        //else if(character is EnemySentinel)
        //{
        //    enemyHightOffset = 0.12f;
        //}
        //else if(character is Player)
        //{
        //    enemyHightOffset = 0.12f;
        //}
        
        if(character)
        {
            var exist = false;
            for(var index = 0; index < m_characters.Count; index++)
            {
                var cha = m_characters[index];
                if(cha.Uid == character.Uid)
                {
                    exist = true;
                    break;
                }
            }
            if(!exist)
            {
                m_characters.Add(character);
            }
        }

        if (itemType == ItemType.LureBottle)
        {
            if (character is Player)
            {
                if (Game.Instance.boardManager.bottleCount <= 0)
                {
                    Game.Instance.player.m_animator.SetTrigger("pick");
                    picked = true;
                }
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        var character = other.transform.parent.GetComponent<Character>();
        if (character)
        {
            var exist = false;
            for (var index = 0; index < m_characters.Count; index++)
            {
                var cha = m_characters[index];
                if (cha.Uid == character.Uid)
                {
                    exist = true;
                    break;
                }
            }
            if (exist)
            {
                m_characters.Remove(character);
            }
        }

        if (m_characters.Count > 0)
        {
            for(var index = 0; index < m_characters.Count; index++)
            {
                var cha = m_characters[index];
                if(cha is EnemyStatic)
                {
                    enemyHightOffset = 0.5f;
                }
                else
                {
                    enemyHightOffset = 0.3f;
                }
            }
        }
        else
        {
            upper = false;
            velocity = Vector3.zero;
            enemyHightOffset = 0.0f;
        }

        //if (other.gameObject.GetComponent<Character>())
       
    }
    
    public Vector3 GetIconPosition()
    {
        if (!iconPosition)
        {
            return transform.position + new Vector3(0, 0.25f, 0);
        }
        return iconPosition.position + new Vector3(0, 0.25f, 0);
    }

}
