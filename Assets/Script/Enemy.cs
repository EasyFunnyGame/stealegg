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

    public GridTile foundPlayerTile = null;

    public GridTile hearSoundTile = null;

    public GridTile originalTile = null;

    public IconsAboveEnemy icons;

    public Transform headPoint;

    public Animator enemyMove;

    public GameObject route;

    public Transform routeLine;

    public Transform routeArrow;

    public string routeNode1Name;

    public string routeNode2Name;

    public List<MeshRenderer> redNodes = new List<MeshRenderer>();

    public bool sawPlayer = false;

    public override void Start()
    {
        base.Start();
        Reached();
        OnReachedOriginal();
        DisapearTraceTarget();
    }

    public override void ResetDirection()
    {
        base.ResetDirection();
        UpdateRouteMark();
    }


    public void Alert(string tileName)
    {
        var catchPlayer = TryCatchPlayer();
        if(catchPlayer)
        {
            return;
        }
        var targetTile = gridManager.GetTileByName(tileName); 
        if(targetTile!=null)
        {
            m_animator.Play("Enemy_Alert");
            if (hearSoundTile && hearSoundTile.name == targetTile.name)
            {   // 原地吹哨
                currentAction = new ActionEnemyMove(this, hearSoundTile);
                return;
            }
            
            var canSeePlayer = Game.Instance.player.CanReach(currentTile.name);
            if (canSeePlayer)
            {
                sawPlayer = true;
                hearSoundTile = targetTile;
                ShowTraceTarget(targetTile);
                currentAction = new ActionTurnDirection(this, Utils.DirectionTo(currentTile, Game.Instance.player.currentTile, direction));
            }
            else
            // 如果追踪方向和当前方向相同  直接行进
            if(foundPlayerTile)
            {
                currentAction = new ActionEnemyMove(this, foundPlayerTile);
                return;
            }
            else if (!sawPlayer)
            {
                ShowFound();
                hearSoundTile = targetTile;
                ShowTraceTarget(targetTile);
                // 判断寻路的方向  
                FindPathRealTime(targetTile);
                UpdateTargetDirection(nextTile);
                if (targetDirection != direction)
                {
                    currentAction = new ActionTurnDirection(this, targetDirection);
                }
            }
            else if(sawPlayer)
            {
                currentAction = new ActionEnemyMove(this, hearSoundTile);
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

        if(foundPlayerTile == null)
        {
            TryFoundPlayer();
            if(foundPlayerTile != null)
            {
                if(hearSoundTile==null)
                {
                    currentAction = new ActionFoundPlayer(this);
                }
                else
                {
                    currentAction = new ActionEnemyMove(this, hearSoundTile);
                }
                return;
            }
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

        //var foundPlayer = TryFoundPlayer();
        //if (foundPlayer)
        //{
        //    currentAction = new ActionFoundPlayer(this, ActionType.FoundPlayer);
        //    return;
        //}

        if (originalTile != null)
        {
            currentAction = new ActionEnemyMove(this, originalTile);
            return;
        }

        ReturnOriginal(true);
    }


    public void ReturnOriginal(bool needAction)
    {
        targetDirection = originalDirection;
        if (currentTile.name != originalCoord.name)
        {
            originalTile = gridManager.GetTileByName(originalCoord.name);
            if (originalTile != null)
            {
                FindPathRealTime(originalTile);
                ShowBackToOriginal();
                if (needAction)
                {
                    if (direction == targetDirection)
                    {
                        //currentAction = new ActionEnemyMove(this, originalTile);
                    }
                    else
                    {
                        currentAction = new ActionTurnDirection(this, targetDirection);
                    }
                }
            }
        }
        else if(direction != targetDirection)
        {
            currentAction = new ActionTurnDirection(this, this.originalDirection);
            ShowBackToOriginal();
        }
        else
        {
            OnReachedOriginal();
        }
    }

    public void RedNodeByName(string nodeName)
    {
        var node = boardManager.FindNode(nodeName);

        if (node != null)
        {
            var nodeTr = node.targetIcon;
            var copyNode = Instantiate(nodeTr.GetComponent<MeshRenderer>());
            copyNode.name = nodeName;
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
            currentAction = new ActionFoundPlayer(this);
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

        Debug.Log("敌人到达路径点:"+ currentTile.name);
    }

    public virtual void UpdateRouteMark()
    {

    }

    public virtual bool TryCatchPlayer()
    {
        if( foundPlayerTile == null )
        {
            return false;
        }
        if (Game.Instance.player == null || Game.Instance.player.currentTile == null) return false;
        var canSeePlayer = Game.Instance.player.CanReach(currentTile.name);
        var targetDirection = Utils.DirectionTo(currentTile, Game.Instance.player.currentTile, direction);
        if(targetDirection == direction)
        {
            if (foundPlayerTile != null && canSeePlayer)
            {
                m_animator.CrossFade("Enemy_Caught", 0.1f);
                Game.Instance.FailGame();
                return true;
            }
            var canReach = CanReach(Game.Instance.player.currentTile.name);
            if (canReach)
            {
                m_animator.CrossFade("Enemy_Caught", 0.1f);
                Game.Instance.FailGame();
                return true;
            }
        }
        return false;;
    }

    public virtual bool PlayerWalkIntoSight()
    {
        if (Game.Instance.player == null || Game.Instance.player.currentTile == null)
        {
            return false;
        }
        var canReach = false;
        // 情况1
        canReach = Game.Instance.player.CanReach(currentTile.name);
        var foundPlayerDirection = Utils.DirectionTo(currentTile, Game.Instance.player.currentTile, direction);
        if (canReach && foundPlayerDirection == direction)
        {
            var targetTile = gridManager.GetTileByName(Game.Instance.player.currentTile.name);
            if (targetTile != null)
            {
                foundPlayerTile = targetTile;
                ShowTraceTarget(targetTile);
                ShowFound();
                hearSoundTile = null;
                originalTile = null;
                Debug.Log("主角闯进视野:" + targetTile.name);
                return true;
            }
        }
        return false;
    }

    public virtual bool TryFoundPlayer()
    {
        if (Game.Instance.result == GameResult.FAIL) return false;
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
        var foundNodeX = coord.x + xOffset;
        var foundNodeZ = coord.z + zOffset;
        var next1NodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);

        var foundPlayer = false;

        var canReach = false;
        
        if (!Game.Instance.player || !Game.Instance.player.currentTile)
        {
            return false;
        }
        // 情况1
        canReach = Game.Instance.player.CanReach(currentTile.name);
        if (Game.Instance.player && Game.Instance.player.currentTile)// 主角可能未加载
        {
            var foundPlayerDirection = Utils.DirectionTo(currentTile, Game.Instance.player.currentTile, direction);
            if (canReach && foundPlayerDirection == direction)
            {
                foundPlayer = true;
            }
        }
        // 情况2
        if (foundPlayer == false)
        {
            canReach = CanReach(next1NodeName);// Game.Instance.player.CanReach(currentTile.name);
            if (canReach)
            {
                if (Game.Instance.player.currentTile && Game.Instance.player.currentTile.name == next1NodeName)
                {
                    foundPlayer = true;
                }
            }
        }
        
        // 情况3
        if ( foundPlayer == false )
        {
            foundNodeX += xOffset;
            foundNodeZ += zOffset;
            next1NodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
            canReach = CanReach(next1NodeName,2);
            if (canReach&& Game.Instance.player.currentTile && Game.Instance.player.currentTile.name == next1NodeName)
            {
                foundPlayer = true;
            }
        }

        if (foundPlayer)
        {
            var targetTile = gridManager.GetTileByName(next1NodeName);
            if (targetTile != null)
            {
                foundPlayerTile = targetTile;
                ShowTraceTarget(targetTile);
                ShowFound();
                originalTile = null;
                Debug.Log("开始追踪:" + targetTile.name);
                sawPlayer = true;
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
        if(body_looking)
        {
            route.SetActive(false);
        }
        else
        {
            if (!string.IsNullOrEmpty(routeNode1Name) && !string.IsNullOrEmpty(routeNode2Name))
            {
                route.SetActive(true);
                var node1 = boardManager.FindNode(routeNode1Name);
                var node2 = boardManager.FindNode(routeNode2Name);
                if (node1==null && node2 ==null)
                {
                    route.SetActive(false);
                }
                else
                {
                    var endNode = node2 ? node2 : node1;
                    var distance = Vector3.Distance(transform.position, endNode.transform.position);
                    routeLine.localScale = new Vector3(1.1f, 1, distance * 40);
                    routeArrow.localPosition = new Vector3(0, 0, distance);

                    for (var index = 0; index < redNodes.Count; index++)
                    {
                        if (redNodes[index].name == currentTile.name)
                        {
                            distance = Vector3.Distance(transform.position, redNodes[index].transform.position);
                            if (distance > 0.1f)
                            {
                                redNodes[index].gameObject.SetActive(false);
                            }
                        }
                    }
                    //Debug.Log("线路终点:" + endNode.name + " 距离:" + distance);
                }
            }
        }
        
    }

    public virtual void OnReachedOriginal()
    {

    }


    public void ShowNotFound()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(true);
        DisapearTraceTarget();
    }

    public void ShowFound()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(true);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
    }

    public void ShowBackToOriginal()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(true);
        icons.wenhao.gameObject.SetActive(false);
    }

    public void ShowSleep()
    {
        icons.shuijiao.gameObject.SetActive(true);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
    }

    public void ShowTraceTarget(GridTile tile)
    {
        enemyMove.transform.parent = null;
        enemyMove.gameObject.SetActive(true);
        enemyMove.Play("Movement_Animation");
        enemyMove.transform.transform.position = tile.transform.position;
    }

    public void DisapearTraceTarget()
    {
        enemyMove.transform.parent = transform;
        enemyMove.gameObject.SetActive(false);
    }
}
