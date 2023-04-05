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

public enum ReachTurnTo
{
    None = 0,
    PlayerToEnemy = 1,
    EnemyToPlayer = 2,
}

public class Enemy : Character
{
    [SerializeField]
    public EnemyType enemyType;

    public static int count;

    public Coord coordTracing;

    // 玩家当前所在坐标
    public Coord coodPlayer;

    // 玩家上一个坐标
    public Coord coodPlayerLastRound;

    public GridTile originalTile = null;

    public IconsAboveEnemy icons;

    public Transform headPoint;

    public Animator enemyMove;

    public GameObject route;

    public Transform routeArrow;

    public List<string> routeNodeNames;

    public List<BoardNode> redNodes = new List<BoardNode>();

    public List<LinkLine> redLines = new List<LinkLine>();

    public bool sleeping = false;

    public bool patroling = false;

    public string walkingLineType;

    public int up = 0;

    public static float RED_SCALE = 1.5f;

    public ReachTurnTo turnOnReachDirection = ReachTurnTo.None;

    public string tracingTileName;

    public override void Start()
    {
        base.Start();
        lookAroundTime = 9;
        tr_body = transform;
        Reached();
        OnReachedOriginal();
        DisapearTraceTarget(false);
    }

    public Vector3 getHeadPointPosition()
    {
        return this.headPoint.position + new Vector3(0,0.25f,0);
    }

    public void TracingPlayerRefresh()
    {
        if (!coordTracing.isLegal || !coodPlayer.isLegal)
        {
            return;
        }
        var coord = Game.Instance.player.coord;
        

    }

    public virtual void CheckAction()
    {
        if (currentAction != null) return;


        //if (hearSoundTile != null)
        //{
        //    var player = Game.Instance.player;
        //    var playerTileName = player.currentTile.name;
        //    var playerCanReachInOneStep = player.CanReachInSteps(currentTile.name);
        //    var dir = Utils.DirectionTo(currentTile.name, playerTileName, direction);
        //    if (playerCanReachInOneStep && dir == direction)
        //    {
        //        var catched = TryCatch();
        //        if (catched)
        //        {
        //            return;
        //        }
        //    }
        //}

        //if (foundPlayerTile == null )
        //{
        //    TryFound();
        //    if(foundPlayerTile != null)
        //    {
        //        if(hearSoundTile != null)
        //        {
        //            currentAction = new ActionEnemyMove(this,foundPlayerTile);
        //        }
        //        return;
        //    }
        //}
        //else
        //{
        //    TryFound();
        //    if (TryCatch()) return;
        //}


        //if (foundPlayerTile != null)
        //{
        //    originalTile = null;
        //    currentAction = new ActionEnemyMove(this,  foundPlayerTile);
        //    return;
        //}

        //if (growthTile != null)
        //{
        //    hearSoundTile = growthTile;
        //    growthTile = null;
        //    return;
        //}

        //if (hearSoundTile != null)
        //{
        //    originalTile = null;
        //    currentAction = new ActionEnemyMove(this, hearSoundTile);
        //    return;
        //}

        //if (originalTile != null )
        //{
        //    currentAction = new ActionEnemyMove(this, originalTile);
        //    return;
        //}

        //if(Game.Instance.result == GameResult.NONE)
        //{
        //    ReturnOriginal(true);
        //}
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
            OnReachedOriginal();
        }
        enemyMove.gameObject.SetActive(false);
    }

    public void RedLineByName(LinkLine line)
    {
        var copy = Instantiate(line);
        copy.name = line.name;
        copy.transform.position = line.transform.position;
        var scale = line.transform.localScale;
        copy.transform.localScale = new Vector3(RED_SCALE, scale.y, scale.z);
        copy.transform.position = new Vector3(line.transform.position.x, 0.006f+line.transform.position.y, line.transform.position.z);
        copy.transform.rotation = line.transform.rotation;
        var renderer = copy.transform.GetChild(0).GetComponent<MeshRenderer>();
        renderer.material = Resources.Load<Material>("Material/RouteRed");
        redLines.Add(copy);
    }

    public void RedNodeByName(string nodeName)
    {
        var node = boardManager.FindNode(nodeName);

        if (node != null)
        {
            var copyNode = Instantiate<BoardNode>(node);
            copyNode.name = nodeName;
            copyNode.Red();
            redNodes.Add(copyNode);
        }
    }

    public override void Reached()
    {
        
        base.Reached();
        lookAroundTime = 9;
        UpdateRouteMark();

        //if(hearSoundTile != null)
        //{
        //    var catchPlayer = TryCatch();
        //    if (catchPlayer)
        //        return;
        //}

        //if (foundPlayerTile == null)
        //{
        //    TryFound();
        //    if (foundPlayerTile != null)
        //    {
        //        return;
        //    }
        //}
        //else
        //{
        //    TryFound();
        //    TryCatch();
        //}
        m_animator.SetBool("moving", false);
        //Debug.Log("敌人到达路径点:"+ currentTile.name);
    }

    public virtual void UpdateRouteMark()
    {
        for (var index = 0; index < redNodes.Count; index++)
        {
            redNodes[index].gameObject.gameObject.SetActive(false);
            Destroy(redNodes[index].gameObject);
        }
        redNodes.Clear();

        for (var index = 0; index < redLines.Count; index++)
        {
            redLines[index].gameObject.gameObject.SetActive(false);
            Destroy(redLines[index].gameObject);
        }
        redLines.Clear();

        routeNodeNames.Clear();

        if (sleeping)
        {
            RedNodeByName(coord.name);
            return;
        }

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
        var distance = 2;
        var foundNodeX = coord.x;
        var foundNodeZ = coord.z;

        while (distance >= 0)
        {
            var currentNodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
            foundNodeX = foundNodeX + xOffset;
            foundNodeZ = foundNodeZ + zOffset;
            var nextNodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
            var linkLine = boardManager.FindLine(currentNodeName, nextNodeName);
            
            routeNodeNames.Add(currentNodeName);
            RedNodeByName(currentNodeName);

            if (linkLine == null)
                break;

            if (distance > 0)
            {
                RedLineByName(linkLine);
            }
            if(linkLine.transform.childCount < 1)
            {
                break;
            }
            if (linkLine.transform.childCount > 0  )
            {
                if(!linkLine.transform.GetChild(0).name.Contains("Visual"))
                {
                    break;
                }
            }
            distance--;
        }

        BoardNode endNode = boardManager.FindNode(routeNodeNames[routeNodeNames.Count - 1]);
        routeArrow.position = endNode.transform.position+new Vector3(0,0.025f,0);
        routeArrow.rotation = transform.GetChild(0).rotation;
        routeArrow.Rotate(new Vector3(0, 0, 180));
        routeArrow.parent = null;
    }

    


    public override void StartMove()
    {
        m_animator.SetBool("moving", true);
        if(redNodes?.Count>0)
        {
            redNodes[0].transform.position = redNodes[redNodes.Count - 1].transform.position;
        }
    }

    protected virtual void Update()
    {
        if(body_looking)
        {
            route.SetActive(false);
        }
        else
        {
            UpdateRouteRedLine();
            if(this is EnemyPatrol)
            {
                routeArrow.gameObject.SetActive(true);
            }
        }
        UpdateAnimatorParams();
    }

    protected virtual void UpdateRouteRedLine()
    {
        route.SetActive(!sleeping);
        if(routeNodeNames.Count<=0)
        {
            route.SetActive(false);
        }
    }

    private void UpdateAnimatorParams()
    {
        if (targetIdleType < idleType)
        {
            idleType -= (0.1f * Time.deltaTime * 60);
            if (targetIdleType >= idleType)
            {
                idleType = targetIdleType;
            }
        }

        if (targetIdleType > idleType)
        {
            idleType += (0.1f * Time.deltaTime * 60);
            if (targetIdleType <= idleType)
            {
                idleType = targetIdleType;
            }
        }
        m_animator.SetFloat("idle_type", idleType);

        if(targetIdleType != 0f || this is EnemyPatrol)
        {
            if (lookAroundTime > 0)
            {
                lookAroundTime -= Time.deltaTime;
                if (lookAroundTime <= 0)
                {

                    lookAroundTime = 9;
                    var lookAroundType = coordTracing.isLegal ? 1 : 0;
                    m_animator.SetFloat("look_around_type", lookAroundType);
                    m_animator.SetTrigger("look_around");
                }
            }
        }
        
    }

    public virtual void OnReachedOriginal()
    {
        routeArrow.gameObject.SetActive(false);
        lureByBottle = lureByWhistle = lureBySteal = false;
    }

    // =======================================================================================


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

    public bool CheckPlayer()
    {
        return !TryCatch() && TryFound();
    }

    public bool TryCatch()
    {
        if (!coordTracing.isLegal) return false;
        if (sleeping) return false;
        var player = Game.Instance.player;
        if (player != null && !player.isHidding && player.coord.Equals(front))
        {
            return true;
        }
        return false;


        //var targetDirection = Utils.DirectionTo(currentTile.name, player.currentTile.name, direction);
        //if (targetDirection == direction && player.CanReachInSteps(currentTile.name) && !player.hidding)
        //{
        //    Game.Instance.FailGame(this);
        //    m_animator.SetBool("catch", true);
        //    AudioPlay.Instance.PlayCatch(this);
        //    ShowCatch();
        //    foundPlayerTile = hearSoundTile = null;
        //    m_animator.SetBool("moving", false);
        //    return true;
        //}
        //return false;
    }

    public virtual bool TryFound()
    {
        var player = Game.Instance.player;
        if (player == null) return false;
        var coordPlayer = player.coord;
        if (!coordPlayer.isLegal) return false;
        for(var index = 0; index < this.redNodes.Count; index++)
        {
            var coordRed = this.redNodes[index].coord;
            if(coordRed.Equals(coordPlayer))
            {
                var targetTile = gridManager.GetTileByName(coordRed.name);
                if (targetTile != null)
                {
                    coordTracing = coordPlayer.Clone();
                    ShowTraceTarget(targetTile);
                    ShowFound();
                    originalTile = null;
                    //Debug.Log("开始追踪:" + targetTile.name);
                    //if (canReach)
                    //{
                    //    turnOnReachDirection = ReachTurnTo.EnemyToPlayer;
                    //}
                    //else
                    //{
                    //    turnOnReachDirection = ReachTurnTo.PlayerToEnemy;
                    //}

                    patroling = false;
                    routeArrow.gameObject.SetActive(true);
                    lookAroundTime = 9;
                    UpdateRouteMark();
                    return true;
                }

                break;
            }
        }

        return false;


        //return false;
        //if (!Game.Instance.player || !Game.Instance.player.currentTile)
        //{
        //    return false;
        //}
        //if (Game.Instance.result == GameResult.FAIL || Game.Instance.result == GameResult.WIN)
        //{
        //    return false;
        //}
        //var player = Game.Instance.player;
        //if (Game.Instance.result == GameResult.FAIL) return false;
        //if (sleeping) return false;
        //var xOffset = 0;
        //var zOffset = 0;
        //if (direction == Direction.Up)
        //{
        //    zOffset = 1;
        //}
        //else if (direction == Direction.Down)
        //{
        //    zOffset = -1;
        //}
        //else if (direction == Direction.Right)
        //{
        //    xOffset = 1;
        //}
        //else if (direction == Direction.Left)
        //{
        //    xOffset = -1;
        //}
        //var foundNodeX = coord.x + xOffset;
        //var foundNodeZ = coord.z + zOffset;
        //var next1NodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);

        //var foundPlayer = false;

        //var foundPlayerNode = "";

        //var canReach = false;

        //LinkLine linkLine;
        //// 第一点
        //if (foundPlayer == false)
        //{
        //    linkLine = boardManager.FindLine(currentTile.name, next1NodeName);
        //    if (linkLine)
        //    {
        //        if (linkLine && player.currentTile.name == next1NodeName && !player.hidding)// hearSoundTile == null &&
        //        {
        //            foundPlayer = true;
        //            foundPlayerNode = next1NodeName;
        //            canReach = CanReachInSteps(next1NodeName, 1);

        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //// 第二点
        //foundNodeX += xOffset;
        //foundNodeZ += zOffset;
        //var next2NodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
        //linkLine = boardManager.FindLine(next1NodeName, next2NodeName);
        //if (linkLine && player.currentTile.name == next2NodeName && !player.hidding)
        //{
        //    foundPlayer = true;
        //    foundPlayerNode = next2NodeName;
        //    canReach = CanReachInSteps(next2NodeName, 2);
        //    if (!canReach)
        //    {
        //        var playerFindPathSuccess = player.CanReachInSteps(currentTile.name, 2);
        //        if (playerFindPathSuccess)
        //        {
        //            var canReachLastNode = CanReachInSteps(next1NodeName, 1);
        //            if (canReachLastNode)
        //            {
        //                foundPlayerNode = next1NodeName;
        //            }
        //        }
        //    }
        //}

        //if (foundPlayer)
        //{
        //    var targetTile = gridManager.GetTileByName(foundPlayerNode);
        //    if (targetTile != null)
        //    {
        //        foundPlayerTile = targetTile;
        //        ShowTraceTarget(targetTile);
        //        ShowFound();
        //        originalTile = null;
        //        //Debug.Log("开始追踪:" + targetTile.name);
        //        if (canReach)
        //        {
        //            turnOnReachDirection = ReachTurnTo.EnemyToPlayer;
        //        }
        //        else
        //        {
        //            turnOnReachDirection = ReachTurnTo.PlayerToEnemy;
        //        }

        //        patroling = false;
        //        routeArrow.gameObject.SetActive(true);
        //        lookAroundTime = 9;
        //        UpdateRouteMark();
        //        return true;
        //    }
        //}
        //return false;
    }


    public GridTile growthTile = null;
    public virtual void LureGrowth(string tileName)
    {
        var targetTile = gridManager.GetTileByName(tileName);
        if (targetTile == null) return ;
        ShowTraceTarget(targetTile);
        growthTile = targetTile;
        ShowFound();
       
    }

    public bool lureByWhistle = false;

    public virtual bool LureWhistle(string tileName)
    {
        AudioPlay.Instance.StopSleepSound();
        var player = Game.Instance.player;
        var playerNeighbor = player.CanReachInSteps(currentTile.name);
        var playerTileName = CheckNeighborGrid();
        var targetTile = gridManager.GetTileByName(tileName);


        if (!string.IsNullOrEmpty(playerTileName) && TryCatch())
        {
            return true;
        }

        targetTile = gridManager.GetTileByName(tileName);
        if (targetTile == null) return false;

        sleeping = false;
        patroling = false;
        // 原地吹哨、被敌人看见之后继续吹哨
        //if ( (hearSoundTile && hearSoundTile.name == tileName) || foundPlayerTile)
        //{
        //    currentAction = new ActionEnemyMove(this, foundPlayerTile??hearSoundTile);
        //    return false;
        //}

        if (playerNeighbor && player.currentTile.name == tileName)
        {
            // turnOnReached = true;
            UpdateTargetDirection(player.currentTile);
        }
        else
        {
            // 判断寻路的方向  
            var success = FindPathRealTime(targetTile);
            if (!success)
            {
                //hearSoundTile = foundPlayerTile = null;
                ShowNotFound();
                ReturnOriginal(true);
                return false;
            }
            UpdateTargetDirection(nextTile);
        }
        //if(direction != targetDirection)
        {
            currentAction = new ActionTurnDirection(this, targetDirection);
        }
        
        // ShowTraceTarget(targetTile, hearSoundTile == null,1);
        //hearSoundTile = targetTile;
        lureByWhistle = true;
        ShowFound();
        return true;
    }

    public bool lureByBottle = false;
    public virtual bool LureBottle(string tileName)
    {
        AudioPlay.Instance.StopSleepSound();
        var player = Game.Instance.player;
        var playerNeighbor = player.CanReachInSteps(currentTile.name);
        var playerTileName = CheckNeighborGrid();
        var targetTile = gridManager.GetTileByName(tileName);

        if (!string.IsNullOrEmpty(playerTileName) && TryCatch())
        {
            return true;
        }

         targetTile = gridManager.GetTileByName(tileName);
        if (targetTile == null) return false;

        sleeping = false;
        patroling = false;
        // 原地吹哨、被敌人看见之后继续吹哨
        if ((hearSoundTile && hearSoundTile.name == tileName) || foundPlayerTile)
        {
            currentAction = new ActionEnemyMove(this, hearSoundTile ?? foundPlayerTile);
            return false;
        }

        if (playerNeighbor && player.currentTile.name == tileName)
        {
            UpdateTargetDirection(player.currentTile);
        }
        else
        {
            // 判断寻路的方向  
            var success = FindPathRealTime(targetTile);
            if (!success)
            {
                hearSoundTile = foundPlayerTile = null;
                ShowNotFound();
                ReturnOriginal(true);
                return false;
            }
            UpdateTargetDirection(nextTile);
        }
        //if (direction != targetDirection)
        {
            currentAction = new ActionTurnDirection(this, targetDirection);
        }
        //ShowTraceTarget(targetTile, hearSoundTile== null,1);
        hearSoundTile = targetTile;
        lureByBottle = true;
        ShowFound();
        return true;
    }
    public bool lureBySteal = false;
    public virtual bool LureSteal(string tileName)
    {
        AudioPlay.Instance.StopSleepSound();
        var player = Game.Instance.player;
        var playerNeighbor = player.CanReachInSteps(currentTile.name);
        var playerTileName = CheckNeighborGrid();
        var targetTile = gridManager.GetTileByName(tileName);
       
        if (!string.IsNullOrEmpty(playerTileName) && TryCatch())
        {
            return true;
        }

        
        if (targetTile == null) return false;

        sleeping = false;
        patroling = false;
        // 原地吹哨、被敌人看见之后继续吹哨
        if ((hearSoundTile && hearSoundTile.name == tileName) || foundPlayerTile)
        {
            currentAction = new ActionEnemyMove(this, hearSoundTile ?? foundPlayerTile);
            return false;
        }

        if (playerNeighbor && player.currentTile.name == tileName)
        {
            UpdateTargetDirection(player.currentTile);
        }
        else
        {
            // 判断寻路的方向  
            var success = FindPathRealTime(targetTile);
            if(!success)
            {
                hearSoundTile = foundPlayerTile = null;
                ShowNotFound();
                ReturnOriginal(true);
                return false;
            }
            UpdateTargetDirection(nextTile);
        }

        //if (direction != targetDirection)
        {
            currentAction = new ActionTurnDirection(this, targetDirection);
        }
        ShowTraceTarget(targetTile);
        hearSoundTile = targetTile;
        lureBySteal = true;
        idleType = 0.5f;
        ShowFound();
        return true;
    }

    public void OnTurnEnd()
    {
        body_looking = false;

        if (foundPlayerTile == null)
        {
            TryFound();
            if (foundPlayerTile != null)
            {
                return;
            }
        }
        else
        {
            TryFound();
            if (TryCatch()) return;
        }
        m_animator.SetBool("moving",false);
        UpdateRouteMark();
    }

    public virtual void ShowNotFound()
    {
        if(Game.Instance.result!=GameResult.NONE)
        {
            ShowCatch();
            return;
        }
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(true);
        icons.ccw.gameObject.SetActive(false);
        icons.cw.gameObject.SetActive(false);
        DisapearTraceTarget();
    }

    public virtual void ShowFound()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(true);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        icons.ccw.gameObject.SetActive(false);
        icons.cw.gameObject.SetActive(false);
        targetIdleType = 0.5f;
        routeArrow.gameObject.SetActive(true);
        m_animator.SetBool("moving",false);
        AudioPlay.Instance.PlayEnemyAlert(this);
    }

    public virtual void ShowBackToOriginal()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(true);
        icons.wenhao.gameObject.SetActive(false);
        icons.ccw.gameObject.SetActive(false);
        icons.cw.gameObject.SetActive(false);
        targetIdleType = 1;
    }

    public virtual void ShowSleep()
    {
        icons.shuijiao.gameObject.SetActive(true);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        icons.ccw.gameObject.SetActive(false);
        icons.cw.gameObject.SetActive(false);
    }

    public virtual void ShowCCW()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        icons.ccw.gameObject.SetActive(true);
        icons.cw.gameObject.SetActive(false);
    }

    public virtual void ShowCW()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        icons.ccw.gameObject.SetActive(false);
        icons.cw.gameObject.SetActive(true);
    }

    public virtual void ShowCatch()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        icons.ccw.gameObject.SetActive(false);
        icons.cw.gameObject.SetActive(false);
    }

    public virtual void HideSentinelTurn()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        icons.ccw.gameObject.SetActive(false);
        icons.cw.gameObject.SetActive(false);
    }


    public virtual void ShowTraceTarget(GridTile tile)
    {
        tracingTileName = tile.name;

        var node = boardManager.FindNode(tile.name);

        var enemies = boardManager.enemies;

        var targetingTiles = new List<string>();

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            var targetingTileName = enemy.tracingTileName;
            if (!string.IsNullOrEmpty(targetingTileName) && targetingTileName == tile.name)
            {
                var enemyMove = enemy.enemyMove;
                var index = targetingTiles.IndexOf(targetingTileName);
                if(index == -1)
                {
                    enemyMove.transform.parent = null;
                    enemyMove.gameObject.SetActive(true);
                    enemyMove.Play("Movement_Animation");
                    enemyMove.transform.transform.position = node.transform.position;
                    targetingTiles.Add(targetingTileName);
                }
                else
                {
                    enemyMove.gameObject.SetActive(false);
                }
            }
        }
        m_animator.SetTrigger("stop_looking");
    }

    public virtual void DisapearTraceTarget(bool play = true)
    {
        enemyMove.transform.parent = transform;
        enemyMove.gameObject.SetActive(false);
        if(play)
        {
            AudioPlay.Instance.PlayNotFound(this);
        }
        tracingTileName = "";
    }

    public virtual void LostTarget()
    {
        ShowNotFound();
        foundPlayerTile = null;
        hearSoundTile = null;
        targetIdleType = 1;
        lureByBottle = lureByWhistle = lureBySteal = false;

    }

    protected float idleType;
    protected float targetIdleType;
    protected float lookAroundTime = 0;

}
