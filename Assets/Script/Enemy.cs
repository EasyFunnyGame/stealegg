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

    public bool sleeping = false;

    public override void Start()
    {
        base.Start();
        tr_body = transform;
        Reached();
        OnReachedOriginal();
        DisapearTraceTarget();
    }

    public override void ResetDirection()
    {
        base.ResetDirection();
        UpdateRouteMark();
    }

    public virtual void CheckAction()
    {
        if (currentAction != null) return;

        if (foundPlayerTile == null)
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
                    currentAction = new ActionTurnDirection(this, targetDirection);
                    //currentAction = new ActionEnemyMove(this, hearSoundTile);
                }
                return;
            }
        }
        else
        {
            if (CatchPlayer()) return;
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
                    if (direction != targetDirection)
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

        if (CatchPlayer()) return;

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
        for (var index = 0; index < redNodes.Count; index++)
        {
            DestroyImmediate(redNodes[index].gameObject);
        }
        redNodes.Clear();

        routeNode1Name = routeNode2Name = "";

        if (sleeping) return;

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

        var curNodeName = currentTile.gameObject.name;
        RedNodeByName(curNodeName);

        var next1CoordX = coord.x + xOffset;
        var next1CoordZ = coord.z + zOffset;
        var next1NodeName = string.Format("{0}_{1}", next1CoordX, next1CoordZ);
        var line1Name = boardManager.FindLine(curNodeName, next1NodeName);
        if(line1Name == null)
        {
            return;
        }
        
        routeNode1Name = next1NodeName;
        
        RedNodeByName(next1NodeName);

        var next2CoordX = next1CoordX + xOffset;
        var next2CoordZ = next1CoordZ + zOffset;
        var next2NodeName = string.Format("{0}_{1}", next2CoordX, next2CoordZ);
        line1Name = boardManager.FindLine(next1NodeName, next2NodeName);
        if (line1Name == null)
        {
            return;
        }
        routeNode2Name = next2NodeName;
        RedNodeByName(next2NodeName);
    }

    public virtual bool TryCatchPlayer()
    {
        if (Game.Instance.player == null || Game.Instance.player.currentTile == null) return false;
        var canSeePlayer = Game.Instance.player.CanReachInSteps(currentTile.name);
        var targetDirection = Utils.DirectionTo(currentTile, Game.Instance.player.currentTile, direction);
        if(targetDirection == direction)
        {
            if (foundPlayerTile != null && canSeePlayer)
            {
                m_animator.CrossFade("Enemy_Caught", 0.1f);
                Game.Instance.FailGame();
                return true;
            }
            var canReach = CanReachInSteps(Game.Instance.player.currentTile.name);
            if (canReach)
            {
                m_animator.CrossFade("Enemy_Caught", 0.1f);
                Game.Instance.FailGame();
                return true;
            }
        }
        return false;
    }

    public virtual bool PlayerWalkIntoSight()
    {
        if (Game.Instance.player == null || Game.Instance.player.currentTile == null)
        {
            return false;
        }
        var canReach = false;
        // 情况1
        canReach = Game.Instance.player.CanReachInSteps(currentTile.name);
        var foundPlayerDirection = Utils.DirectionTo(currentTile, Game.Instance.player.currentTile, direction);
        if (canReach && foundPlayerDirection == direction)
        {
            var targetTile = gridManager.GetTileByName(Game.Instance.player.currentTile.name);
            if (targetTile != null)
            {
                hearSoundTile = targetTile;
                turnOnReached = true;
                ShowTraceTarget(targetTile);
                ShowFound();
                originalTile = null;
                currentAction = new ActionTurnDirection(this, targetDirection);
                Debug.Log("主角闯进视野:" + targetTile.name);
                return true;
            }
        }
        return false;
    }

    public virtual bool TryFoundPlayer()
    {
        if (!Game.Instance.player || !Game.Instance.player.currentTile)
        {
            return false;
        }
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
        //var canReach = false;
        
        var player = Game.Instance.player;

        var foundPlayerNode = "";

        LinkLine linkLine;
        // 情况1
        if (foundPlayer == false)
        {
            linkLine = boardManager.FindLine(currentTile.name, next1NodeName);
            if (linkLine && Game.Instance.player.currentTile.name == next1NodeName)
            {
                foundPlayer = true;
                foundPlayerNode = next1NodeName;
            }
        }
        
        // 情况2
        foundNodeX += xOffset;
        foundNodeZ += zOffset;
        var next2NodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
        linkLine = boardManager.FindLine(next1NodeName, next2NodeName);
        if (linkLine && Game.Instance.player.currentTile.name == next2NodeName)
        {
            foundPlayer = true;
            foundPlayerNode = next2NodeName;
        }

        if (foundPlayer)
        {
            var targetTile = gridManager.GetTileByName(foundPlayerNode);
            if (targetTile != null)
            {
                foundPlayerTile = targetTile;
                ShowTraceTarget(targetTile);
                ShowFound();
                originalTile = null;
                Debug.Log("开始追踪:" + targetTile.name);
                turnOnReached = true;
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
            route.SetActive(!sleeping);
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

    public void LostTarget()
    {
        ShowNotFound();
        foundPlayerTile = null;
        hearSoundTile = null;

        targetTileName = "";
        turnOnReached = false;
    }

    // =======================================================================================

    public bool turnOnReached = false;

    public string targetTileName = "";

    public bool HearSound(string tileName)
    {
        if (turnOnReached) return false;
        return Goto(tileName);
    }

    public bool SawPlayer(string tileName)
    {
        turnOnReached = true;
        return Goto(tileName);
    }

    public string CheckNeighborGrid()
    {
        if (!Game.Instance.player || !Game.Instance.player.currentTile) return string.Empty;
        var upTileName = string.Format("{0}_{1}", coord.x, coord.z + 1);
        var downTileName = string.Format("{0}_{1}", coord.x, coord.z - 1);
        var leftTileName = string.Format("{0}_{1}", coord.x - 1, coord.z);
        var rightTileName = string.Format("{0}_{1}", coord.x + 1, coord.z);
        var player = Game.Instance.player;
        var tileName = player.currentTile.name;
        if (tileName == upTileName)
        {
            return upTileName;
        }
        else if (tileName == downTileName)
        {
            return downTileName;
        }
        else if (tileName == leftTileName)
        {
            return leftTileName;
        }
        else if (tileName == rightTileName)
        {
            return rightTileName;
        }
        return string.Empty;
    }

    public bool CatchPlayerOn(string tileName)
    {
        var player = Game.Instance.player;
        if (player == null || player.currentTile == null) return false;

        var targetDirection = Utils.DirectionTo(currentTile.name, tileName, direction);
        if (targetDirection == direction && player.CanReachInSteps(currentTile.name))
        {
            m_animator.CrossFade("Enemy_Caught", 0.1f);
            Game.Instance.FailGame();
            return true;
        }
        return false;
    }

    public bool CatchPlayer()
    {
        var player = Game.Instance.player;
        if (player == null || player.currentTile == null) return false;
        var targetDirection = Utils.DirectionTo(currentTile.name, player.currentTile.name, direction);
        if (targetDirection == direction && player.CanReachInSteps(currentTile.name))
        {
            m_animator.CrossFade("Enemy_Caught", 0.1f);
            Game.Instance.FailGame();
            return true;
        }
        return false;
    }

    public virtual bool LureWhistle(string tileName)
    {
        var playerTileName = CheckNeighborGrid();
        if(!string.IsNullOrEmpty(playerTileName) && CatchPlayerOn(playerTileName))
        {
            return true;
        }

        var targetTile = gridManager.GetTileByName(tileName);
        if (targetTile == null) return false;

        m_animator.Play("Enemy_Alert");
        // 原地吹哨、被敌人看见之后继续吹哨
        if ( (hearSoundTile && (hearSoundTile.name == tileName || turnOnReached )) || foundPlayerTile)
        {
            currentAction = new ActionEnemyMove(this, foundPlayerTile??hearSoundTile);
            return false;
        }

        var player = Game.Instance.player;
        var playerNeighbor = player.CanReachInSteps(currentTile.name);
        if (playerNeighbor && player.currentTile.name == tileName)
        {
            turnOnReached = true;
            UpdateTargetDirection(player.currentTile);
        }
        else
        {
            // 判断寻路的方向  
            FindPathRealTime(targetTile);
            UpdateTargetDirection(nextTile);
        }

        currentAction = new ActionTurnDirection(this, targetDirection);

        hearSoundTile = targetTile;
        ShowTraceTarget(targetTile);
        ShowFound();
        return true;
    }

    public virtual bool LureBottle(string tileName)
    {
        var playerTileName = CheckNeighborGrid();
        if (!string.IsNullOrEmpty(playerTileName) && CatchPlayerOn(playerTileName))
        {
            return true;
        }

        var targetTile = gridManager.GetTileByName(tileName);
        if (targetTile == null) return false;

        m_animator.Play("Enemy_Alert");
        // 原地吹哨、被敌人看见之后继续吹哨
        if ((hearSoundTile && (hearSoundTile.name == tileName || turnOnReached)) || foundPlayerTile)
        {
            currentAction = new ActionEnemyMove(this, hearSoundTile ?? foundPlayerTile);
            return false;
        }

        var player = Game.Instance.player;
        var playerNeighbor = player.CanReachInSteps(currentTile.name);
        if (playerNeighbor && player.currentTile.name == tileName)
        {
            turnOnReached = true;
            UpdateTargetDirection(player.currentTile);
        }
        else
        {
            // 判断寻路的方向  
            FindPathRealTime(targetTile);
            UpdateTargetDirection(nextTile);
        }

        currentAction = new ActionTurnDirection(this, targetDirection);

        hearSoundTile = targetTile;
        ShowTraceTarget(targetTile);
        ShowFound();
        return true;
    }

    public virtual bool LureSteal(string tileName)
    {
        var playerTileName = CheckNeighborGrid();
        if (!string.IsNullOrEmpty(playerTileName) && CatchPlayerOn(playerTileName))
        {
            return true;
        }

        var targetTile = gridManager.GetTileByName(tileName);
        if (targetTile == null) return false;

        m_animator.Play("Enemy_Alert");
        // 原地吹哨、被敌人看见之后继续吹哨
        if ((hearSoundTile && (hearSoundTile.name == tileName || turnOnReached)) || foundPlayerTile)
        {
            currentAction = new ActionEnemyMove(this, hearSoundTile ?? foundPlayerTile);
            return false;
        }

        var player = Game.Instance.player;
        var playerNeighbor = player.CanReachInSteps(currentTile.name);
        if (playerNeighbor && player.currentTile.name == tileName)
        {
            turnOnReached = true;
            UpdateTargetDirection(player.currentTile);
        }
        else
        {
            // 判断寻路的方向  
            FindPathRealTime(targetTile);
            UpdateTargetDirection(nextTile);
        }

        currentAction = new ActionTurnDirection(this, targetDirection);

        hearSoundTile = targetTile;
        ShowTraceTarget(targetTile);
        ShowFound();
        return true;
    }

    public void OnTurnEnd()
    {
        body_looking = false;
        if (foundPlayerTile == null)
        {
            TryFoundPlayer();
        }
        else
        {
            CatchPlayer();
        }
    }





}
