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

    // 敌人数量
    public static int count;

    // 追踪的目标点
    public Coord _coordTracing = new Coord();

    public Coord coordTracing
    {
        get
        {
            return _coordTracing;
        }
        set
        {
            _coordTracing = value;
        }
    }

    // 玩家当前所在坐标
    public Coord coordPlayer = null;

    // 原点Tile
    public GridTile originalTile = null;

    // 敌人头顶的icon
    public IconsAboveEnemy icons;

    //
    public Transform headPoint;

    //
    public Animator enemyMove;

    //
    public GameObject route;

    //
    public Transform routeArrow;

    //
    public List<string> routeNodeNames;

    //
    public List<BoardNode> redNodes = new List<BoardNode>();

    //
    public List<LinkLine> redLines = new List<LinkLine>();

    //
    public bool sleeping = false;

    //
    public bool patroling = false;

    //
    public string walkingLineType;

    //
    public int up = 0;

    //
    public static float RED_SCALE = 1.5f;

    //
    public ReachTurnTo turnOnReachDirection = ReachTurnTo.None;

    //
    public string tracingTileName;

    // 红点范围
    public int checkRange = 0;

    public Coord coordLureMe = new Coord();

    public override void Start()
    {
        lookAroundTime = 9;
        tr_body = transform;
        base.Start();
        Reached();
        DisapearTraceTarget(false);
    }

    public Vector3 getHeadPointPosition()
    {
        return this.headPoint.position + new Vector3(0,0.25f,0);
    }

    // 主角定位点更新
    public void UpdateTracingPlayerTile()
    {
        var player = Game.Instance.player;
        //if(stepsAfterFoundPlayer>2)
        //{
        //    // 追踪步数超过5则丢失玩家;
        //    if(coordPlayer.isLegal)
        //    {
        //        coordPlayer.SetMax();
        //    }
        //}
    }

    public virtual void CheckAction()
    {
        if (currentAction != null) return;

        if(!coordTracing.isLegal)
        {
            if (CheckPlayer())
            {
                UpdateRouteMark();
                Debug.Log("发现敌人,更新监测点");
                return;
            }
        }

        var notCatchNorFound = CheckPlayer();
        if(!notCatchNorFound)
        {
            if (boardManager.coordLure.isLegal)
            {
                var coordLure = boardManager.coordLure;
                var rangeLure = boardManager.rangeLure;
                if (Coord.Distance(coordLure, coord) <= rangeLure)
                {
                    checkRange = 3;
                    UpdateRouteMark();
                    if (CheckPlayer())// 睡觉敌人更新视野之后尝试发现敌人，如果发现了敌人此回合结束
                    {
                        return;
                    }

                    originalTile = null;
                    coordTracing = coordLure.Clone();
                    ShowTraceTarget(coordLure.name);
                    ShowFound();

                    var player = Game.Instance.player;
                    coordTracing = coordLure.Clone();
                    if (Coord.inLine(coordLure, coord) && player.CanReachInSteps(coord.name, rangeLure))
                    {
                        currentAction = new ActionTurnDirection(this, coordLure.name, false);
                        // 不用在 TurnEnd 再执行找人抓人行为,此处直接赋值 coordPlayer
                        // 如果誘惑點 和 主角  共綫 共享;
                        if (Coord.inLine(coordLure, player.coord) &&
                            Coord.inLine(coord, player.coord) && 
                            player.CanReachInSteps(coordLure.name, rangeLure) &&
                            player.CanReachInSteps(coord.name, checkRange - 1))
                        {
                            coordTracing = player.coord.Clone();
                            ShowTraceTarget(player.coord.name);
                            coordPlayer = player.coord.Clone();
                            coordPlayer.SetMax();
                            stepsAfterFoundPlayer = 0;
                        }
                    }
                    else
                    {
                        var lureTile = gridManager.GetTileByName(coordLure.name);
                        var success = FindPathRealTime(lureTile);
                        if (success)
                        {
                            LookAt(nextTile.name);
                            if (_direction != targetDirection)
                            {
                                currentAction = new ActionTurnDirection(this, nextTile.name, true);
                            }
                        }
                        else
                        {
                            // 寻路失败就返回起点。

                            // 如果主角在正前方而且 前方一格可达 则继续前进
                        }
                    }


                    if (!coordLureMe.isLegal || !coordLureMe.Equals(coordLure))
                    {
                        coordLureMe = coordLure.Clone();
                        return;
                    }
                }
            }
        }
        

        //
        if (coordTracing.isLegal)
        {
            sleeping = false;
            patroling = false;
            
            UpdateTracingPlayerTile();

            var tile = gridManager.GetTileByName(coordTracing.name);
            if (tile == null)
            {
                // 如果不在起点就返回起点。
                // 
                return;
            }
            var success = FindPathRealTime(tile);
            if (!success)
            {
                // 寻路失败就返回起点。

                // 如果主角在正前方而且 前方一格可达 则继续前进

                return;
            }
            currentAction = new ActionEnemyMove(this, tile);
            return;
        }


        if (coord.name != originalCoord.name || _direction != originalDirection)
        {
            if(coord.name != originalCoord.name)
            {
                if (originalTile == null)
                {
                    originalTile = gridManager.GetTileByName(originalCoord.name);
                    FindPathRealTime(originalTile);
                    currentAction = new ActionTurnDirection(this, nextTile.name,true);
                    ShowBackToOriginal();
                }
                else
                {
                    currentAction = new ActionEnemyMove(this, originalTile);
                }
            }
            else
            {
                ShowBackToOriginal();
                targetDirection = originalDirection;
                currentAction = new ActionTurnDirection(this, originalDirection,true);
            }
        }


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


    //public void ReturnOriginal(bool needAction)
    //{
    //    targetDirection = originalDirection;
    //    if (currentTile.name != originalCoord.name)
    //    {
    //        originalTile = gridManager.GetTileByName(originalCoord.name);
    //        if (originalTile != null)
    //        {
    //            FindPathRealTime(originalTile);
    //            ShowBackToOriginal();
    //            if (needAction)
    //            {
    //                if (direction != targetDirection)
    //                {
    //                    currentAction = new ActionTurnDirection(this, targetDirection);
    //                }
    //            }
    //        }
    //    }
    //    else if(direction != targetDirection)
    //    {
    //        currentAction = new ActionTurnDirection(this, this.originalDirection);
    //        ReachedOriginal();
    //    }
    //    enemyMove.gameObject.SetActive(false);
    //}

    public void RedLine(LinkLine line)
    {
        var copy = Instantiate(line);
        copy.name = line.name;
        copy.transform.position = line.transform.position;
        var scale = line.transform.localScale;
        copy.transform.localScale = new Vector3(RED_SCALE, scale.y, scale.z);
        copy.transform.position = new Vector3(line.transform.position.x, 0.006f + line.transform.position.y, line.transform.position.z);
        copy.transform.rotation = line.transform.rotation;
        var renderer = copy.transform.GetChild(0).GetComponent<MeshRenderer>();
        renderer.material = Resources.Load<Material>("Material/RouteRed");
        redLines.Add(copy);
    }

    public void RedNode(BoardNode node)
    {
        var copyNode = Instantiate<BoardNode>(node);
        copyNode.name = node.gameObject.name;
        copyNode.Red();
        redNodes.Add(copyNode);
    }



    public virtual void ShowNotFound()
    {
        if (Game.Instance.result != GameResult.NONE)
        {
            ClearAllIconsAboveHead();
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

    public override void Turned()
    {
        base.Turned();
        UpdateRouteMark();
        Debug.Log("转向完毕更新检测点");

        if(coord.name == originalCoord.name && _direction == originalDirection && !coordTracing.isLegal)
        {
            ReachedOriginal();
        }
    }

    public override void Reached()
    {
        base.Reached();
        lookAroundTime = 9;
        m_animator.SetBool("moving", false);
        UpdateRouteMark();
        Debug.Log("到达更新监测点");
        Debug.Log(gameObject.name + " 到达点" + coord.name);
        if (originalCoord.Equals(coord)  && !coordTracing.isLegal) // && _direction == originalDirection
        {
            ReachedOriginal();
        }
    }

    public virtual void UpdateRouteMark()
    {
        for (var index = 0; index < redNodes.Count; index++)
        {
            redNodes[index].gameObject.SetActive(false);
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

        var coordDirection = GetDirectionCoord();

        var lastCoordName = "";
        var routeCoordName = "";
        var coordX = coord.x;
        var coordZ = coord.z;
        for(var index = 0; index < checkRange; index++)
        {
            routeCoordName = string.Format("{0}_{1}", coordX, coordZ);

            var node = boardManager.FindNode(routeCoordName);

            if (node == null) break;

            if(index > 0 )
            {
                var linkLine = boardManager.FindLine(lastCoordName, routeCoordName);
                if (linkLine==null || !linkLine.through)
                {
                    break;
                }
            }

            RedNode(node);

            if(!string.IsNullOrEmpty(lastCoordName))
            {
                var linkLine = boardManager.FindLine(lastCoordName, routeCoordName);
                if(linkLine)
                {
                    RedLine(linkLine);
                }
            }

            coordX += coordDirection.x;

            coordZ += coordDirection.z;

            lastCoordName = routeCoordName;
        }
        
    }

    public override void StartMove()
    {
        m_animator.SetBool("moving", true);
        // 起步后,隐藏脚下的点
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

    public virtual void ReachedOriginal()
    {
        Debug.Log(gameObject.name + " 回到原点");
        routeArrow.gameObject.SetActive(false);
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
        var caught = TryCatch();
        var found = !caught && TryFound();
        return caught || found;
    }

    public bool TryCatch()
    {
        if (!coordTracing.isLegal) return false;
        if (sleeping) return false;
        var player = Game.Instance.player;
        if (player != null && !player.isHidding && player.coord.Equals(front))
        {
            AudioPlay.Instance.PlayCatch(this);
            m_animator.SetBool("catch", true);
            m_animator.SetBool("moving", false);
            Game.Instance.FailGame(this);
            ClearAllIconsAboveHead();
            return true;
        }
        return false;
    }

    public virtual bool TryFound()
    {
        var player = Game.Instance.player;
        if (player == null) return false;
        var foundPlayerCoord = player.coord;
        if (!foundPlayerCoord.isLegal) return false;
        for(var index = 0; index < this.redNodes.Count; index++)
        {
            var coordRed = this.redNodes[index].coord;
            if(coordRed.Equals(foundPlayerCoord))
            {
                var targetTile = gridManager.GetTileByName(coordRed.name);
                if (targetTile != null)
                {
                    coordTracing = foundPlayerCoord.Clone();
                    coordPlayer = foundPlayerCoord.Clone();
                    if(index == 1)
                    {
                        coordPlayer.SetMax();
                    }
                    ShowTraceTarget(coordRed.name);
                    ShowFound();
                    originalTile = null;
                    patroling = false;
                    routeArrow.gameObject.SetActive(true);
                    lookAroundTime = 9;
                    stepsAfterFoundPlayer = 0;
                    return true;
                }
                break;
            }
        }
        return false;
    }


    public GridTile growthTile = null;
    public virtual void LureGrowth(string tileName)
    {
        var targetTile = gridManager.GetTileByName(tileName);
        if (targetTile == null) return ;
        ShowTraceTarget(tileName);
        growthTile = targetTile;
        ShowFound();
    }


    //public virtual void LureWhistle(string tileName)
    //{
    //    AudioPlay.Instance.StopSleepSound();
    //    var player = Game.Instance.player;
    //    var playerNeighbor = player.CanReachInSteps(currentTile.name);
    //    var playerTileName = CheckNeighborGrid();
    //    var targetTile = gridManager.GetTileByName(tileName);

    //    targetTile = gridManager.GetTileByName(tileName);
    //    if (targetTile == null) return false;

    //    sleeping = false;
    //    patroling = false;
    //    // 原地吹哨、被敌人看见之后继续吹哨
    //    //if ( (hearSoundTile && hearSoundTile.name == tileName) || foundPlayerTile)
    //    //{
    //    //    currentAction = new ActionEnemyMove(this, foundPlayerTile??hearSoundTile);
    //    //    return false;
    //    //}

    //    if (playerNeighbor && player.currentTile.name == tileName)
    //    {
    //        // turnOnReached = true;
    //        UpdateTargetDirection(player.currentTile);
    //    }
    //    else
    //    {
    //        // 判断寻路的方向  
    //        var success = FindPathRealTime(targetTile);
    //        if (!success)
    //        {
    //            //hearSoundTile = foundPlayerTile = null;
    //            ShowNotFound();
    //            ReturnOriginal(true);
    //            return false;
    //        }
    //        UpdateTargetDirection(nextTile);
    //    }

    //    //if(direction != targetDirection)
    //    {
    //        currentAction = new ActionTurnDirection(this, targetDirection);
    //    }

    //    // ShowTraceTarget(targetTile, hearSoundTile == null,1);
    //    //hearSoundTile = targetTile;
    //    lureByWhistle = true;
    //    ShowFound();
    //    return true;
    //}

    //public virtual bool LureSteal(string tileName)
    //{
    //    AudioPlay.Instance.StopSleepSound();
    //    var player = Game.Instance.player;
    //    var playerNeighbor = player.CanReachInSteps(currentTile.name);
    //    var playerTileName = CheckNeighborGrid();
    //    var targetTile = gridManager.GetTileByName(tileName);
       
    //    if (!string.IsNullOrEmpty(playerTileName) && TryCatch())
    //    {
    //        return true;
    //    }

        
    //    if (targetTile == null) return false;

    //    sleeping = false;
    //    patroling = false;
    //    // 原地吹哨、被敌人看见之后继续吹哨
    //    //if ((hearSoundTile && hearSoundTile.name == tileName) || foundPlayerTile)
    //    //{
    //    //    currentAction = new ActionEnemyMove(this, hearSoundTile ?? foundPlayerTile);
    //    //    return false;
    //    //}

    //    if (playerNeighbor && player.currentTile.name == tileName)
    //    {
    //        LookAt(player.currentTile.name);
    //    }
    //    else
    //    {
    //        // 判断寻路的方向  
    //        var success = FindPathRealTime(targetTile);
    //        if(!success)
    //        {
    //            ShowNotFound();
    //            // ReturnOriginal(true);
    //            return false;
    //        }
    //        LookAt(nextTile.name);
    //    }

    //    //if (direction != targetDirection)
    //    {
    //        currentAction = new ActionTurnDirection(this, targetDirection);
    //    }
    //    ShowTraceTarget(tileName);
    //    idleType = 0.5f;
    //    ShowFound();
    //    return true;
    //}

    

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

    public virtual void ClearAllIconsAboveHead()
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


    public virtual void ShowTraceTarget(string nodeName)
    {
        tracingTileName = nodeName;

        var node = boardManager.FindNode(nodeName);

        var enemies = boardManager.enemies;

        var targetingTiles = new List<string>();

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            var targetingTileName = enemy.tracingTileName;
            if (!string.IsNullOrEmpty(targetingTileName) && targetingTileName == nodeName)
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
        targetIdleType = 1;
        coordTracing.SetMin();
        coordPlayer.SetMin();
        coordLureMe.SetMin();
        stepsAfterFoundPlayer = -1;

    }

    protected float idleType;
    protected float targetIdleType;
    protected float lookAroundTime = 0;

    public int stepsAfterFoundPlayer = -1;

}
