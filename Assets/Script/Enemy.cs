using System.Collections.Generic;
using UnityEngine;
public enum EnemyType
{
    Static,
    Distracted,
    Patrol,
    Sentinel
}

public enum CheckPlayerResult
{
    None,
    Catch,
    Found,
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

    // 是否实时更新主角追踪点
    public bool updateCoordPlayer = false;

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
    public List<BoardNode> redNodes = new List<BoardNode>();

    //
    public List<LinkLine> redLines = new List<LinkLine>();

    //
    public bool sleeping = false;

    //
    public bool patroling = false;

    // 
    public bool watching = true;

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
        routeArrow.transform.parent = route.transform.parent;
    }

    public Vector3 getHeadPointPosition()
    {
        return this.headPoint.position + new Vector3(0,0.25f,0);
    }

    // 主角定位点更新
    public void UpdateTracingPlayerTile()
    {
        var player = Game.Instance.player;
        if(player.justThroughNet)
        {
            if(coordPlayer.isLegal)
            {
                coordPlayer.SetTurnBack();
            }
        }
        // tobe filled
    }

    public virtual void CheckAction()
    {
        if (currentAction != null) return;

        if(!coordTracing.isLegal)
        {
            if (CheckPlayer() == CheckPlayerResult.Found)
            {
                checkRange = 3;
                UpdateRouteMark();
                Debug.Log("发现敌人,更新监测点");
                return;
            }
        }

        var result = CheckPlayer();
        if(result == CheckPlayerResult.Catch)
        {
            return;
        }
        if(result == CheckPlayerResult.None)
        {
            var coordLure = boardManager.coordLure;
            var rangeLure = boardManager.rangeLure;
            var player = Game.Instance.player;
            if (boardManager.coordLure.isLegal && coordPlayer.isMin && Coord.Distance(coordLure, coord) <= rangeLure)
            {
                sleeping = false;
                watching = false;
                patroling = false;
                checkRange = 3;
                UpdateRouteMark();
                if (CheckPlayer() == CheckPlayerResult.Found)// 睡觉敌人更新视野之后尝试发现敌人，如果发现了敌人此回合结束
                {
                    return;
                }

                originalTile = null;
                coordTracing = coordLure.Clone();
                ShowTraceTarget(coordLure.name);
                ShowFound();

                var playerSteps = player.StepsReach(coord.name);
                if ( Coord.inLine(coordLure,coord) && playerSteps <= rangeLure)// 敌人、引诱点共线 并且路线能通行
                {
                    // 直接赋值玩家追踪点，不在转向后检查主角，否则会直接抓捕
                    if (Coord.inLine(coordLure,player.coord) )// 玩家、引诱点共线 
                    {
                        var dir1 = Utils.DirectionToMultyGrid(coord.name, player.coord.name, _direction);
                        var dir2 = Utils.DirectionToMultyGrid(coord.name, coordLure.name, _direction);
                        if(dir2 == dir1 && _direction != dir1)// 同乡
                        {
                            playerSteps = player.StepsReach(coord.name);
                            var enemySteps = StepsReach(player.coord.name);

                            coordTracing = player.coord.Clone();
                            ShowTraceTarget(player.coord.name);
                            coordPlayer = player.coord.Clone();
                            stepsAfterFoundPlayer = 0;
                            if (playerSteps == enemySteps)
                            {
                                // 敌人能直达
                            }
                            else
                            {
                                // 敌人不能直达 吹口哨时回头望
                                coordPlayer.SetTurnBack();
                            }
                        }
                    }
                    // 这个转向要在后面，否则转向不对
                    currentAction = new ActionTurnDirection(this, coordLure.name, false);
                }
                else
                {
                    // 不共线转向寻路下一点
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
                        Debug.LogWarning("在敌人不可达的点吹口哨！！！");
                        LostTarget();
                    }
                }
                // 更新声波点 回合停止
                if (!coordLureMe.isLegal || !coordLureMe.Equals(coordLure))
                {
                    coordLureMe = coordLure.Clone();
                    return;
                }
            }
            else if( boardManager.growthLure.isLegal )
            {
                var growthLure = boardManager.growthLure;
                // var distanceToGrowthTile = Coord.Distance(coord, boardManager.growthLure) == 1;
                var lookingAtGrowthTile = boardManager.allItems.ContainsKey(front.name) && boardManager.allItems[front.name]?.itemType == ItemType.Growth;
                if(lookingAtGrowthTile )
                {
                    var lureTile = gridManager.GetTileByName(growthLure.name);
                    if (lureTile == null)
                    {
                        LostTarget();
                        return;
                    }
                    var success = FindPathRealTime(lureTile);
                    if (success)
                    {
                        
                    }

                    coordTracing = boardManager.growthLure.Clone();
                    ShowTraceTarget(player.coord.name);
                    ShowFound();

                    //else
                    //{
                    //    Debug.LogWarning("在敌人不可达的出逃树林！！！");
                    //    LostTarget();
                    //}
                    return;
                }
            }
        }

        //
        if (coordTracing.isLegal)
        {
            sleeping = false;
            patroling = false;
            watching = false;

            if(updateCoordPlayer)
            {
                UpdateTracingPlayerTile();
            }

            var tile = gridManager.GetTileByName(coordTracing.name);
            if (tile == null)
            {
                // 如果不在起点就返回起点。
                LostTarget();
                GoBack();
                return;
            }
            var success = FindPathRealTime(tile);
            if (!success)
            {
                var player = Game.Instance.player;
                // 如果主角在正前方而且 前方一格可达 则继续前进
                if (TryMoveFrontToWardPlayer())
                {
                    return;
                }
                // 寻路失败就返回起点。
                //Debug.LogWarning("寻路失败,寻路回到起点并转向");
                LostTarget();
                GoBack();
                return;
            }
            currentAction = new ActionEnemyMove(this, tile);
            return;
        }

        if(!GoBack())
        {
            DoDefaultAction();
        }
    }

    // 默认行为
    public virtual void DoDefaultAction()
    {

    }


    public virtual bool TryMoveFrontToWardPlayer()
    {
        var player = Game.Instance.player;
        // 如果主角在正前方而且 前方一格可达 则继续前进
        if (Coord.inLine(coord, player.coord))
        {
            var same = Utils.SameDirectionWithLookingAt(coord.name, player.coord.name, _direction);
            if (same)
            {
                var stepsPlayer = player.StepsReach(coord.name);
                if (stepsPlayer <= checkRange - 1)
                {
                    var stepFront = StepsReach(front.name);
                    if (stepFront == 1)
                    {
                        var frontTile = gridManager.GetTileByName(front.name);
                        currentAction = new ActionEnemyMove(this, frontTile);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // 回去原点  是否完结此回合
    public virtual bool GoBack()
    {
        if (coord.name != originalCoord.name || _direction != originalDirection)
        {
            if (coord.name != originalCoord.name)
            {
                if (originalTile == null)
                {
                    originalTile = gridManager.GetTileByName(originalCoord.name);
                    FindPathRealTime(originalTile);
                    currentAction = new ActionTurnDirection(this, nextTile.name, true);
                    ShowBackToOriginal();
                    return true;
                }
                else
                {
                    currentAction = new ActionEnemyMove(this, originalTile);
                    return true;
                }
            }
            else
            {
                ShowBackToOriginal();
                targetDirection = originalDirection;
                currentAction = new ActionTurnDirection(this, originalDirection, true);
                return true;
            }
        }
        else 
        {
            ReachedOriginal();
            return false;
        }
    }

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

            var isBlocked = false;
            // 是否有敌人阻挡
            for (var idx = 0; idx < boardManager.enemies.Count; idx++)
            {
                var enemy = boardManager.enemies[idx];
                if (enemy.gameObject.name == gameObject.name)
                {
                    continue;
                }
                if (enemy.coord.name == routeCoordName)
                {
                    isBlocked = true;
                    break;
                }
            }

            // 是否有树林阻挡
            foreach (var kvp in boardManager.allItems)
            {
                var item = kvp.Value;
                if (item == null) continue;
                if (item.itemType == ItemType.Growth && item.coord.name == routeCoordName)
                {
                    isBlocked = true;
                    break;
                }
            }

            if (isBlocked)
            {
                break;
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
        UpdateRouteRedLine();
        UpdateAnimatorParams();
    }

    protected virtual void UpdateRouteRedLine()
    {
        route.SetActive(!sleeping && !patroling && redNodes.Count > 1);

        if( (coordTracing.isLegal || originalTile != null) && redNodes.Count > 1 && !body_looking)
        {
            routeArrow.gameObject.SetActive(true);
            var lastRedNode = redNodes[redNodes.Count - 1];
            routeArrow.transform.position = lastRedNode.transform.position;

            routeArrow.transform.rotation = transform.GetChild(0).transform.rotation;
            routeArrow.transform.Rotate(new Vector3(0, 0, 180));

        }
        else
        {
            routeArrow.gameObject.SetActive(false);
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

    public CheckPlayerResult CheckPlayer()
    {
        var caught = TryCatch();
        if(caught)
        {
            return CheckPlayerResult.Catch;
        }
        var found = TryFound();
        if(found)
        {
            return CheckPlayerResult.Found;
        }
        return CheckPlayerResult.None;
    }

    public bool TryCatch()
    {
        if (Game.Instance.result == GameResult.WIN) return false;
        if (!coordTracing.isLegal) return false;
        if (sleeping) return false;
        var player = Game.Instance.player;
        //
        var playerSteps = player.StepsReach(coord.name);

        if (player != null && !player.isHidding && player.coord.Equals(front) && playerSteps == 1)
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
            if(coordRed.Equals(foundPlayerCoord) && !player.isHidding)
            {
                var targetTile = gridManager.GetTileByName(coordRed.name);
                if (targetTile != null)
                {
                    coordTracing = foundPlayerCoord.Clone();
                    coordPlayer = foundPlayerCoord.Clone();

                    updateCoordPlayer = true;

                    var playerSteps = player.StepsReach(coord.name);
                    var enemySteps = StepsReach(player.coord.name);
                    if (playerSteps == enemySteps)
                    {
                        // 敌人能直达
                    }
                    else
                    {
                        // 敌人不能直达
                        //coordPlayer.SetTurnBack();
                    }
                    if( patroling && enemySteps >= 3)
                    {
                        coordPlayer.SetNoTurn();// 望远镜敌人看见远处敌人到达追踪点后不转向
                    }
                    else if(enemySteps == 2)
                    {
                        updateCoordPlayer = false;
                        // coordPlayer.SetTurnBack();
                    }
                    else
                    {

                    }

                    ShowTraceTarget(coordRed.name);
                    ShowFound();
                    originalTile = null;
                    patroling = false;
                    watching = false;
                    lookAroundTime = 9;
                    stepsAfterFoundPlayer = 0;
                    return true;
                }
                break;
            }
        }
        return false;
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
        coordTracing.SetNoTurn();
        coordPlayer.SetNoTurn();
        coordLureMe.SetNoTurn();
        stepsAfterFoundPlayer = -1;
    }

    protected float idleType;
    protected float targetIdleType;
    protected float lookAroundTime = 0;

    public int stepsAfterFoundPlayer = -1;

}
