
using UnityEngine;

public class Player : character
{
    public Level level;
    //public PathFinder finder;

    public static Player instance;

    void Awake()
    {
        Player.instance = this;
        //if(finder == null)
        //{
        //    finder = gameObject.GetComponent<PathFinder>() ?? gameObject.AddComponent<PathFinder>();
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }

    override protected void Reached()
    {
        base.Reached();
        level.boardMgr.PickItem(tile_s.name,this);
    }
}
