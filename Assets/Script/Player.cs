using UnityEngine;
public class Player : Character
{
    public int bottleCount;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    override public void Reached()
    {
        base.Reached();
        CheckWhistle();
        CheckBottle();
        boardManager.PickItem(currentTile.name,this);
        m_animator.CrossFade("Player_Idle",0.1f);
        //Debug.Log(string.Format("{0}到达{1}", gameObject.name, tile_s.gameObject.name));
    }

    override public void StartMove()
    {
        base.StartMove();
        m_animator.CrossFade("Player_Sprint", 0.1f);

        Game.Instance.gameCanvas.DisableWhistle();
        Game.Instance.gameCanvas.DisableBottle();
        //Debug.Log(string.Format("{0}开始行走{1}", gameObject.name, tar_tile_s.name));
    }

    public override void FootL()
    {
        base.FootL();
    }

    public override void FootR()
    {
        base.FootR();
    }


    public void CheckWhistle()
    {
        Game.Instance.gameCanvas.DisableWhistle();
        var nodes = boardManager.FindNodesAround(currentTile.name, 2);
        var stop = false;
        foreach (var kvp in nodes)
        {
            for (var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                if (enemy.coord.Equals(kvp.Value.coord))
                {
                    Game.Instance.gameCanvas.EnableWhistle();
                    stop = true;
                    break;
                }
            }
            if (stop)
            {
                break;
            }
        }
    }

    public void CheckBottle()
    {
        if (bottleCount > 0)
        {
            Game.Instance.gameCanvas.EnableBottle();
        }
        else
        {
            Game.Instance.gameCanvas.DisableBottle();
        }
    }
}
