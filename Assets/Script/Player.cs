using UnityEngine;
public class Player : Character
{
    public Animator animator;

    public static Player Instance;

    public int bottleCount;

    void Awake()
    {
        Instance = this;
        //big = true;
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (Game.Instance.turn != Turn.PLAYER) return;
        if (selected_tile_s != null && !moving && tile_s != selected_tile_s && selected_tile_s != null)
        {
            if (selected_tile_s.db_path_lowest.Count > 1)
            {
                ClearPath();
            }
        }
        base.Update();
    }

    override protected void OnReached()
    {
        base.OnReached();
        boardManager.PickItem(tile_s.name,this);
        animator.CrossFade("Player_Idle",0.1f);
        //Debug.Log(string.Format("{0}到达{1}", gameObject.name, tile_s.gameObject.name));

        CheckWhistle();
        CheckBottle();

        hasAction = false;
        Game.Instance.CheckPlayerTurnEnd();
    }

    override protected void OnStartMove()
    {
        base.OnStartMove();
        animator.CrossFade("Player_Run",0.1f);
        hasAction = true;

        Game.Instance.gameCanvas.DisableWhistle();
        Game.Instance.gameCanvas.DisableBottle();
        //Debug.Log(string.Format("{0}开始行走{1}", gameObject.name, tar_tile_s.name));
    }

    public void CheckWhistle()
    {
        Game.Instance.gameCanvas.DisableWhistle();
        var nodes = boardManager.FindNodesAround(tile_s.name, 2);
        var stop = false;
        foreach( var kvp in nodes)
        {
            for(var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                if(enemy.coord.Equals(kvp.Value.coord))
                {
                    Game.Instance.gameCanvas.EnableWhistle();
                    stop = true;
                    break;
                }
            }
            if(stop)
            {
                break;
            }
        }
    }

    public void CheckBottle()
    {
        if( bottleCount > 0 )
        {
            Game.Instance.gameCanvas.EnableBottle();
        }
        else
        {
            Game.Instance.gameCanvas.DisableBottle();
        }
    }

    public override void FootL()
    {
        base.FootL();
    }

    public override void FootR()
    {
        base.FootR();
    }
}
