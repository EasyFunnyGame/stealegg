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

    public static int count;

    public bool tracingPlayer = false;

    public GridTile foundPlayerTile = null;

    public GridTile hearSoundTile = null;

    public GridTile originalTile = null;

    public override void ResetDirection()
    {
        base.ResetDirection();
        UpdateRouteMark();
    }


    public void Alert(string tileName)
    {
        //Debug.Log("警觉" + tileName);
        var targetTile = gridManager.GetTileByName(tileName); 
        if(targetTile!=null)
        {
            m_animator.Play("Enemy_Alert");
            if (hearSoundTile && hearSoundTile.name == targetTile.name)
            {
                currentAction = new ActionEnemyMove(this, hearSoundTile);
                return;
            }
            hearSoundTile = targetTile;
            var canSeePlayer = Game.Instance.player.CanReach(currentTile.name);
            if (canSeePlayer)
            {
                currentAction = new ActionTurnDirection(this, Utils.DirectionTo(currentTile, Game.Instance.player.currentTile, direction));
                return;
            }
            // 如果追踪方向和当前方向相同  直接行进
            if(foundPlayerTile)
            {
                currentAction = new ActionEnemyMove(this, foundPlayerTile);
                return;
            }
            // 判断寻路的方向  
            FindPathRealTime(targetTile);
            UpdateTargetDirection(nextTile);
            if (targetDirection != direction)
            {
                currentAction = new ActionTurnDirection(this, targetDirection);
            }
        }
    }


    public virtual void CheckAction()
    {
        // Debug.Log("检查行为");
        var caught = TryCatchPlayer();
        if (caught)
        {
            return;
        }

        if (foundPlayerTile != null)
        {
            originalTile = null;
            currentAction = new ActionEnemyMove(this,  foundPlayerTile);
            return;
        }

        if (hearSoundTile != null)
        {
            originalTile = null;
            currentAction = new ActionEnemyMove(this, hearSoundTile);
            return;
        } 

        if (originalTile != null)
        {
            currentAction = new ActionEnemyMove(this, originalTile);
            return;
        }

        if (ReturnOriginal(true))
        {
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

    public bool ReturnOriginal(bool needAction)
    {
        if (currentTile.name != originalCoord.name)
        {
            originalTile = gridManager.GetTileByName(originalCoord.name);
            if (originalTile != null)
            {
                FindPathRealTime(originalTile);
                if (needAction)
                {
                    if (direction == targetDirection)
                    {
                        currentAction = new ActionEnemyMove(this, originalTile);
                    }
                    else
                    {
                        currentAction = new ActionTurnDirection(this, targetDirection);
                    }
                }
            }
            
            return true;
        }
        return false;
    }

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

        // 
        var catchPlayer = TryCatchPlayer();
        if (catchPlayer)
        {
            return;
        }
        var foundPlayer = TryFoundPlayer();
        if (foundPlayer)
        {
            currentAction = new ActionFoundPlayer(this, ActionType.FoundPlayer);
            return;
        }

        if (Game.Instance.result == GameResult.FAIL)
        {
            m_animator.CrossFade("Enemy_Caught", 0.1f);
        }
        else if (foundPlayerTile != null || hearSoundTile != null)
        {
            m_animator.CrossFade("Enemy_Alert", 0.1f);
        }
        else
        {
            m_animator.CrossFade("Player_Idle", 0.1f);
        }

        if (Game.Instance.player.currentTile)
        {
            Game.Instance.player.CheckBottle();
            Game.Instance.player.CheckWhistle();
        }
        Debug.Log("敌人到达路径点:"+ currentTile.name);
    }

    public virtual void UpdateRouteMark()
    {

    }

    public virtual bool TryCatchPlayer()
    {
        if (Game.Instance.player == null || Game.Instance.player.currentTile == null) return false;
        var canSeePlayer = Game.Instance.player.CanReach(currentTile.name);
        var targetDirection = Utils.DirectionTo(currentTile, Game.Instance.player.currentTile, direction);
        if(targetDirection == direction)
        {
            if (foundPlayerTile != null && canSeePlayer)
            {
                Game.Instance.FailGame();
                m_animator.CrossFade("Enemy_Caught", 0.1f);
                return true;
            }
            var canReach = CanReach(Game.Instance.player.currentTile.name);
            if (canReach)
            {
                Game.Instance.FailGame();
                m_animator.CrossFade("Enemy_Caught", 0.1f);
                return true;
            }
        }
        return false;;
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
        if (Game.Instance.player.currentTile && Game.Instance.player.currentTile.name == next1NodeName)
        {
            var targetTile = gridManager.GetTileByName(next1NodeName);
            if (targetTile != null)
            {
                foundPlayerTile = targetTile;
                hearSoundTile = null;
                originalTile = null;
                Debug.Log("开始追踪:" + targetTile.name);
                return true;
            }
        }
        return false;
    }

    public override void StartMove()
    {
        m_animator.CrossFade("Player_Sprint", 0.1f);
    }


    private void Update()
    {
        
    }

    public virtual void OnReachedOriginal()
    {

    }

}
