using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Static,
    Distracted,
    Patrol,
    Sentinel
}

public static class EnemyName
{
    public const string Enemy_Distracted = "Enemy_Distracted";
    public const string Enemy_Patrol = "Enemy_Patrol";
    public const string Enemy_Sentinel = "Enemy_Sentinel";
    public const string Enemy_Static = "Enemy_Static"; 
}

public class Enemy : Character
{
    [SerializeField]
    public EnemyType enemyType;

    public Animator animator;

    public static int count;

    public bool tracingPlayer = false;

    public ActionBase currentAction = null;

    public Tile foundPlayerTile = null;

    public Tile hearSoundTile = null;

    public Tile originalTile = null;
    

    public override void ResetDirection()
    {
        base.ResetDirection();
        UpdateRouteMark();
    }

    public void Alert(string tileName)
    {
        Debug.Log("警觉" + tileName);
        var targetTile = gridManager.GetTileByName(tileName); 
        if(targetTile!=null)
        {
            hearSoundTile = targetTile;
            // tracingTile = targetTile;
            // selected_tile_s = tracingTile;
            // gridManager.find_paths_realtime(this, tracingTile);
            // UpdateMoves(targetTile);
            // UpdateTargetDirection(targetTile);
            animator.Play("Enemy_Alert");
        }
    }


    public virtual void CheckAction()
    {
        // Debug.Log("检查行为");
        var caught = TryCatchPlayer();
        if (caught)
        {
            currentAction = new ActionCatchPlayer(this,ActionType.CatchPlayer);
            return;
        }
        

        if (hearSoundTile != null)
        {
            originalTile = null;
            currentAction = new ActionEnemyMove(this, ActionType.EnemyMove, hearSoundTile);
            return;
        }

        if (foundPlayerTile != null)
        {
            originalTile = null;
            currentAction = new ActionEnemyMove(this, ActionType.EnemyMove, foundPlayerTile);
            return;
        }

        if (originalTile != null)
        {
            currentAction = new ActionEnemyMove(this, ActionType.EnemyMove, originalTile);
            return;
        }

        if ( tile_s.name != originalCoord.name )
        {
            originalTile = gridManager.GetTileByName(originalCoord.name);
            if(originalTile!=null)
            {
                FindPathRealTime(originalTile);
                if(direction == targetDirection)
                {
                    currentAction = new ActionEnemyMove(this, ActionType.EnemyMove, originalTile);
                }
                else
                {
                    currentAction = new ActionTurnDirection(this, ActionType.EnemyMove, targetDirection);
                }
                for (int x = 0; x < gridManager.db_tiles.Count; x++)
                    gridManager.db_tiles[x].db_path_lowest.Clear(); //Clear all previous lowest paths for this char//
            }
            return;
        }

        var trace = TryFoundPlayer();
        if (trace)
        {
            currentAction = new ActionFoundPlayer(this, ActionType.FoundPlayer);
            return;
        }
    }

    public List<Transform> redLines = new List<Transform>();

    public List<MeshRenderer> redNodes = new List<MeshRenderer>();

    public void RedLineByName(string nodeName1, string nodeName2)
    {
        var line = boardManager.FindLine(nodeName1, nodeName2);

        if (line != null)
        {
            var lineTr = line.transform;
            var copyLine = Instantiate(lineTr);
            copyLine.transform.position = lineTr.position;
            copyLine.transform.Translate(new Vector3(0, 0.001f, 0));
            copyLine.transform.rotation = lineTr.rotation;
            redLines.Add(copyLine);
            copyLine.GetChild(0).GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/RouteRed");
        }
    }

    public void RedNodeByName(string nodeName)
    {
        var node = boardManager.FindNode(nodeName);

        if (node != null)
        {
            var nodeTr = node.targetIcon;
            var copyNode = Instantiate(nodeTr.GetComponent<MeshRenderer>());
            redNodes.Add(copyNode);
            copyNode.transform.position = nodeTr.transform.position;
            copyNode.transform.Translate(new Vector3(0, 0.001f, 0));
            copyNode.transform.rotation = nodeTr.transform.rotation;
            copyNode.material = Resources.Load<Material>("Material/RouteRed");
        }
    }

    public override void Reached()
    {
        base.Reached();

        UpdateRouteMark();

        if (foundPlayerTile != null || hearSoundTile != null)
        {
            animator.CrossFade("Enemy_Alert", 0.1f);
        }
        else
        {
            animator.CrossFade("Player_Idle", 0.1f);
        }
        

        if (Player.Instance.tile_s)
        {
            Player.Instance.CheckBottle();
            Player.Instance.CheckWhistle();
        }

        // 
        var catchPlayer = TryCatchPlayer();
        if(catchPlayer)
        {
            currentAction = new ActionCatchPlayer(this, ActionType.CatchPlayer);
            return;
        }
        var foundPlayer = TryFoundPlayer();
        if (foundPlayer)
        {
            currentAction = new ActionFoundPlayer(this, ActionType.FoundPlayer);
            return;
        }


        Debug.Log("敌人到达路径点:"+tile_s.name);
    }

    public virtual void UpdateRouteMark()
    {

    }

    public virtual bool TryCatchPlayer()
    {
        var xOffset = 0;
        var zOffset = 0;
        if (direction == Direction.Up)
        {
            zOffset = 1;
        }
        else if (direction == Direction.Down)
        {
            zOffset = -1;
        }
        else if (direction == Direction.Right)
        {
            xOffset = 1;
        }
        else if (direction == Direction.Left)
        {
            xOffset = -1;
        }
        var catchNodeX = coord.x + xOffset;
        var catchNodeZ = coord.z + zOffset;
        var catchTileName = string.Format("{0}_{1}", catchNodeX, catchNodeZ);
        var catchTile = gridManager.GetTileByName(catchTileName);
        //Debug.Log("抓捕:"+ catchTileName + " 主角位置:" + Player.Instance.coord.name);
        if (catchTile != null && Player.Instance.coord.name == catchTileName)
        {
            Game.Instance.status = GameStatus.FAIL;
            return true;
        }
        return false;
    }

    public virtual bool TryFoundPlayer()
    {
        var xOffset = 0;
        var zOffset = 0;
        if (direction == Direction.Up)
        {
            zOffset = 1;
        }
        else if (direction == Direction.Down)
        {
            zOffset = -1;
        }
        else if (direction == Direction.Right)
        {
            xOffset = 1;
        }
        else if (direction == Direction.Left)
        {
            xOffset = -1;
        }
        var catchNodeX = coord.x + xOffset * 2;
        var catchNodeZ = coord.z + zOffset * 2;
        var next1NodeName = string.Format("{0}_{1}", catchNodeX, catchNodeZ);
        if (Player.Instance.tile_s && Player.Instance.tile_s.name == next1NodeName)
        {
            var targetTile = gridManager.GetTileByName(next1NodeName);
            if (targetTile != null)
            {
                foundPlayerTile = targetTile;
                hearSoundTile = null;
                originalTile = null;
                return true;
            }
        }
        return false;
    }

    public override void StartMove()
    {
        animator.CrossFade("Player_Sprint", 0.1f);
    }


}
