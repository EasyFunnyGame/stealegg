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

    public static float ARROW_HIEGHT = 0.025f;

    // 追踪的目标点
    public Coord _coordTracing = new Coord();
    private float m_traceRound = int.MinValue;
    public Coord coordTracing
    {
        get
        {
            return _coordTracing;
        }
        set
        {
            if(!value.isLegal)
            {
                m_traceRound = int.MinValue;
            }
            if(!_coordTracing.isLegal &&  value.isLegal)
            {
                m_traceRound = Game.Instance.enemyRound;
            }
            _coordTracing = value;
        }
    }

    // 玩家当前所在坐标
    public Coord coordPlayer = null;

    public bool sawPlayer = false;

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

    // 红点范围
    public int checkRange = 0;

    public Coord coordLureMe = new Coord();

    public Vector3 bodyPositionOffset = Vector3.zero;

    public float moveDistance = 0;

    public override void Start()
    {
        lookAroundTime = 9;
        tr_body = transform;
        base.Start();
        Reached();
        DisapearTraceTarget(false);
        routeArrow.transform.parent = route.transform.parent;
        Game.Instance.AddMoves(enemyMove);
    }

    public Vector3 getHeadPointPosition()
    {
        return this.headPoint.position + new Vector3(0,0.25f,0);
    }

    // 主角定位点更新
    public void UpdateTracingPlayerTile()
    {
        var player = Game.Instance.player;
        if (player.justJump)
        {
            var distance = Coord.Distance(coord, Game.Instance.player.lastCoord);
            if(distance>1)
            {
                coordPlayer.SetTurnBack();
            }
            else
            {
                coordPlayer.SetNoTurn();
            }
        }
        else
        {
            coordPlayer = player.coord.Clone();
        }

        updateCoordPlayer = false;
    }

    public virtual void CheckAction()
    {
        if (currentAction != null) return;
        var player = Game.Instance.player;
        // var nextTileName = "";
        if (!coordTracing.isLegal)
        {
            // 2-3-15
            if (CheckPlayer() == CheckPlayerResult.Found)
            {
                
                UpdateRouteMark();
                // Debug.Log("发现敌人,更新监测点");
                return;
            }
        }
        else
        {
            //nextTileName = nextTile?.name;
            // 主角躲进树林，清空追踪点
            //if(player.isHidding && sawPlayer)
            //{
                //sawPlayer = false;
                //coordTracing.SetNoTurn();
                //coordPlayer.SetNoTurn();
            //}
        }

        var result = CheckPlayer();
        if(result == CheckPlayerResult.Catch)
        {
            return;
        }

        // 玩家是否在视线范围内
        //var isPlayerInSight = false;
        //for(var index = 0; index < redNodes.Count; index++)
        //{
        //    if (redNodes[index].name == player.coord.name)
        //    {
        //        isPlayerInSight = true;
        //        break;
        //    }
        //}

        if (!sawPlayer )// 优先追击看见敌人的点,同样是声音引起的追击点才进行点更新
        {
            var tracing = coordTracing.isLegal;

            var coordLure = boardManager.coordLure;

            var rangeLure = boardManager.rangeLure;
            
            if (boardManager.coordLure.isLegal && Coord.Distance(coordLure, coord) <= rangeLure)// coordPlayer.isMin &&
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
                coordPlayer.SetNoTurn();
                m_animator.SetBool("look_around", false);
                Game.Instance.UpdateMoves();
                //ShowTraceTarget(coordLure.name);
                ShowFound();

                var playerSteps = player.ReachInStepsFrom(coordLure.name,coord.name);   //player.StepsReach(coord.name);
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
                            
                            if(enemySteps <= rangeLure)
                            {
                                coordTracing = player.coord.Clone();
                                Game.Instance.UpdateMoves();
                                //ShowTraceTarget(player.coord.name);
                                coordPlayer = player.coord.Clone();
                                stepsAfterFoundPlayer = 0;
                                updateCoordPlayer = true;
                                if (playerSteps == enemySteps)
                                {
                                    // 敌人能直达
                                }
                                else
                                {
                                    // 敌人不能直达 吹口哨时回头望
                                }
                            }
                            
                        }
                    }
                    // 这个转向要在后面，否则转向不对
                    var lureTile = gridManager.GetTileByName(coordLure.name);
                    FindPathRealTime(lureTile, null, true);
                    LookAt(coordLure.name);
                    if (_direction != targetDirection)
                    {
                        currentAction = new ActionTurnDirection(this, coordLure.name, true);
                    }
                }
                else
                {
                    // 不共线转向寻路下一点
                    var lureTile = gridManager.GetTileByName(coordLure.name);
                    // var useFastestWay = IfEnemyUseFastestWay();
                    var success = FindPathRealTime(lureTile, null, true);
                    if (success)
                    {
                        var lookAtNextTile = HearSoudLookAt();
                        LookAt(lookAtNextTile);
                        if (_direction != targetDirection)
                        {
                            currentAction = new ActionTurnDirection(this, lookAtNextTile, true);
                        }
                    }
                    else
                    {
                        // 寻路失败就返回起点。
                        // 如果主角在正前方而且 前方一格可达 则继续前进
                        Debug.LogWarning("在敌人不可达的点吹口哨！！！");
                        LostTarget();
                        // 2-5-13, 第二章第三关
                        GoBack();
                    }
                }

                // 如果看见了敌人而且正在追击,听到声音则继续追击
                if (sawPlayer)
                {
                    if (_direction != targetDirection)
                    {
                        currentAction = new ActionTurnDirection(this, nextTile.name, true);
                    }
                    else
                    // 同向则直接行进
                    {
                        currentAction = new ActionEnemyMove(this, gridManager.GetTileByName(coordTracing.name), true);
                    }
                }

                // 更新声波点 
                if (!coordLureMe.isLegal || !coordLureMe.Equals(coordLure))
                {
                    coordLureMe = coordLure.Clone();
                    return;
                }
            }
            // else 
        }

        // 2-3-17  2-3-15
        if (Game.Instance != null && boardManager.growthLure.isLegal)
        {
            var growthLure = boardManager.growthLure;
            var targetName = "";
            var body = transform.GetChild(0);
            if (body != null)
            {
                var eulerY = body.rotation.eulerAngles.y;
                while (eulerY < 0)
                {
                    eulerY += 360;
                }
                while (eulerY >= 360)
                {
                    eulerY -= 360;
                }
                var zOffset = 0;
                var xOffset = 0;
                if (Mathf.Abs(eulerY - 0) < 10)// up
                {
                    zOffset = 1;
                }
                else if (Mathf.Abs(eulerY - 180) < 10)// down
                {
                    zOffset = -1;
                }
                else if (Mathf.Abs(eulerY - 270) < 10)// left 270
                {
                    xOffset = -1;
                }
                else if (Mathf.Abs(eulerY - 90) < 10)// right
                {
                    xOffset = 1;
                }
                var checkCoord = coord.Clone();
                for (var index = 0; index < checkRange - 2 ; index++)
                {
                    checkCoord.x += xOffset;
                    checkCoord.z += zOffset;

                    var isObstructByOther = false;
                    for(var jndex = 0; jndex < Game.Instance.boardManager.enemies.Count; jndex++)
                    {
                        var otherEnemy = Game.Instance.boardManager.enemies[jndex];
                        if (otherEnemy == this) continue;
                        if(otherEnemy.coord.x == checkCoord.x && otherEnemy.coord.z == checkCoord.z)
                        {
                            isObstructByOther = true;
                        }
                    }

                    if(isObstructByOther)
                    {
                        break;
                    }

                    targetName = new Coord(checkCoord.x + xOffset, checkCoord.z + zOffset, transform.position.y).name;
                    var lookingAtGrowthTile = boardManager.allItems.ContainsKey(targetName) && boardManager.allItems[targetName]?.itemType == ItemType.Growth && (targetName == player.lastTileName || targetName == player.coord.name);

                    if (lookingAtGrowthTile)
                    {
                        var lureTile = gridManager.GetTileByName(growthLure.name);
                        if (lureTile == null)
                        {
                            LostTarget();
                            return;
                        }

                        var success = FindPathRealTime(lureTile, null, true);

                        var continueTrace = false;
                        if (coordTracing.isLegal)
                        {
                            continueTrace = true;
                        }

                        coordTracing = boardManager.growthLure.Clone();
                        coordPlayer.SetNoTurn();

                        ShowFound();
                        checkRange = 3;
                        UpdateRouteMark();
                        body_looking = false;// 修复 3-10-2 第三段，箭头消失bug
                        Game.Instance.UpdateMoves();

                        if (!success)
                        {
                            // 2-3-15
                            var distance = player.StepsReach(coord.name);
                            if (distance > 2)
                            {
                                LostTarget();
                                GoBack();
                            }
                        }
                        else
                        {
                            if (continueTrace)
                            {
                                var tile = gridManager.GetTileByName(coordTracing.name);
                                currentAction = new ActionEnemyMove(this, tile, true);
                            }
                            patroling = false;
                            // Debug.LogWarning("在敌人不可达的出逃树林！！！");
                            // 2-3-15 下一个回合才转身回去, 而且要保持警戒状态
                            // LostTarget();
                            // GoBack();
                        }
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

            var same = Utils.SameDirectionWithLookingAt(coord.name, Game.Instance.player.coord.name, _direction);
            var coordDistance = Coord.Distance(coordPlayer, coord);
            var stepFront = StepsReach(front.name);
            // 距离主角为2，而且前面一步可抵达，并且为敌人回合，并且敌人朝向主角
            if (coordDistance == 2 && same && stepFront == 1 && Game.Instance.enemyTurn)
            {
                tile = gridManager.GetTileByName(front.name);
            }

            var success = FindPathRealTime(tile, null, true);
            if (!success)
            {
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
            currentAction = new ActionEnemyMove(this, tile,true);
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
                        currentAction = new ActionEnemyMove(this, frontTile,true);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    string HearSoudLookAt()
    {
        if (Game.Instance == null) return nextTile.name;
        var currentLevelName = Game.Instance.currentLevelName;
        if (currentLevelName == "3-12")
        {
            if (this is EnemyDistracted)
            {
                if(coord.name == "4_2")
                return "3_2";
            }
        }

        return nextTile.name;
    }

    bool IfEnemyUseFastestWay()
    {
        if (Game.Instance == null) return true;
        var currentLevelName = Game.Instance.currentLevelName;
        #region
        if(currentLevelName == "3-12")
        {
            if(this is EnemyDistracted)
            {
                return false;
            }
        }
        #endregion

        return true;
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
                    var assignGoBackTile = getAssignGoBackTileName();
                    originalTile = gridManager.GetTileByName(originalCoord.name);
                    
                    
                    FindPathRealTime(assignGoBackTile, null, false);

                    currentAction = new ActionTurnDirection(this, nextTile.name, true);
                    ShowBackToOriginal();
                    return true;
                }
                else
                {
                    var goBackTile = getAssignGoBackTileName();
                    var useFastest = ifGoBackUseFastestWay();
                    if (goBackTile)
                    {
                        currentAction = new ActionEnemyMove(this, goBackTile, useFastest);
                    }
                    else
                    {
                        currentAction = new ActionEnemyMove(this, originalTile, useFastest);
                    }
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


    public GridTile assignNextTile;

    public string assignOriginalTileName;

    /// <summary>
    /// 返回原点时是否用最快捷路径
    /// </summary>
    /// <returns></returns>
    protected bool ifGoBackUseFastestWay()
    {
        // 2-7-16
        var currentLevelName = Game.Instance.currentLevelName;
        if (currentLevelName == "2-7")
        {
            var enemyPatrol = this as EnemyPatrol;
            if (coord.name == "2_1")
            {
                for (var index = 0; index < enemyPatrol.patrolNodes.Count; index++)
                {
                    if (enemyPatrol.patrolNodes[index].name == "4_3")
                    {
                        return true;
                    }
                }
            }

            if (coord.name == "2_3")
            {
                for (var index = 0; index < enemyPatrol.patrolNodes.Count; index++)
                {
                    if (enemyPatrol.patrolNodes[index].name == "4_1")
                    {
                        return true;
                    }
                }
            }
        }

        #region

        else if(currentLevelName == "3-11")
        {
            if(this is EnemyPatrol )
            {
                if(coord.name == "1_3")
                {
                    return true;
                }
            }
            if (this is EnemyStatic)
            {
                if (coord.name == "3_0")
                {
                    return true;
                }
            }
            if(this is EnemyDistracted)
            {
                return true;
            }
            if (this is EnemyStatic)
            {
                return true;
            }
        }
        #endregion
        return false;
    }

    GridTile getAssignGoBackTileName()
    {
        if(string.IsNullOrEmpty(assignOriginalTileName))
        {
            var assignedTurnBackTile = "";
            var currentLevelName = Game.Instance.currentLevelName;
            if(currentLevelName == "1-1")
            {

            }
            #region

            else if(currentLevelName == "1-10")
            {
                if(this is EnemyStatic)
                {
                    if(coord.name == "1_4"|| coord.name == "1_0")
                    {
                        assignedTurnBackTile = lastTileName;
                    }
                }
            }

            #endregion

            #region 2-7
            else if (currentLevelName == "2-7")
            {
                var enemyPatrol = this as EnemyPatrol;
                if (coord.name == "2_2")
                {
                    for (var index = 0; index < enemyPatrol.patrolNodes.Count; index++)
                    {
                        if (enemyPatrol.patrolNodes[index].name == "4_1")
                        {
                            assignedTurnBackTile = "2_1";
                            originalCoord = new Coord(4, 1, 0.0f);
                            originalDirection = Direction.Left;
                            assignOriginalTileName = "4_1";
                        }
                        if (enemyPatrol.patrolNodes[index].name == "4_3")
                        {
                            assignedTurnBackTile = "2_3";
                            originalCoord = new Coord(4, 3, 0.0f);
                            originalDirection = Direction.Left;
                            assignOriginalTileName = "4_3";
                        }
                    }
                }
                else
                if (coord.name == "3_2" )
                {
                    for (var index = 0; index < enemyPatrol.patrolNodes.Count; index++)
                    {
                        if (enemyPatrol.patrolNodes[index].name == "1_1")
                        {
                            assignedTurnBackTile = "3_1";
                            originalCoord = new Coord(1, 1, 0.0f);
                            originalDirection = Direction.Right;
                            assignOriginalTileName = "1_1";
                        }
                        if (enemyPatrol.patrolNodes[index].name == "1_3")
                        {
                            assignedTurnBackTile = "3_3";
                            originalCoord = new Coord(1, 3, 0.0f);
                            originalDirection = Direction.Right;
                            assignOriginalTileName = "1_3";
                        }
                    }
                }
                else if (coord.name == "3_1")
                {
                    for (var index = 0; index < enemyPatrol.patrolNodes.Count; index++)
                    {
                        if (enemyPatrol.patrolNodes[index].name == "1_1")
                        {
                            assignedTurnBackTile = "2_1";
                            originalCoord = new Coord(1, 1, 0.0f);
                            originalDirection = Direction.Right;
                            assignOriginalTileName = "1_1";
                        }
                        // 2-7-16
                        if (enemyPatrol.patrolNodes[index].name == "1_3")
                        {
                            assignedTurnBackTile = "1_1";
                            originalCoord = new Coord(1, 3, 0.0f);
                            originalDirection = Direction.Right;
                            assignOriginalTileName = "1_3";
                        }
                    }
                }
                else if(coord.name == "3_3")
                {
                    for (var index = 0; index < enemyPatrol.patrolNodes.Count; index++)
                    {
                        if (enemyPatrol.patrolNodes[index].name == "1_3")
                        {
                            assignedTurnBackTile = "2_3";
                            originalCoord = new Coord(1, 3, 0.0f);
                            originalDirection = Direction.Right;
                            assignOriginalTileName = "1_3";
                        }
                        // 2-7-16
                        if (enemyPatrol.patrolNodes[index].name == "1_1")
                        {
                            assignedTurnBackTile = "2_3";
                            originalCoord = new Coord(1, 1, 0.0f);
                            originalDirection = Direction.Right;
                            assignOriginalTileName = "1_1";
                        }
                    }
                }
            }
            #endregion

            #region 2-10
            else if (currentLevelName == "2-10")
            {
                if (gameObject.name.Contains("Enemy_Distracted"))
                {
                    if (coord.name == "2_4")
                    {
                        assignedTurnBackTile = "3_3";
                    }
                }
            }
            #endregion

            #region 2-12
            else if (currentLevelName == "2-12")
            {
                if (this is EnemyPatrol)
                {
                    if (coord.name == "2_2")
                    {
                        var enemyPatrol = this as EnemyPatrol;
                        for (var index = 0; index < enemyPatrol.patrolNodes.Count; index++)
                        {
                            if (enemyPatrol.patrolNodes[index].name == "3_0")
                            {
                                assignedTurnBackTile = "2_1";
                                originalCoord = new Coord(3, 0, 0.0f);
                                originalDirection = Direction.Up;
                                assignOriginalTileName = "3_0";
                            }
                        }
                    }
                    if (coord.name == "5_1")
                    {
                        var enemyPatrol = this as EnemyPatrol;
                        for (var index = 0; index < enemyPatrol.patrolNodes.Count; index++)
                        {
                            if (enemyPatrol.patrolNodes[index].name == "3_0")
                            {
                                assignedTurnBackTile = "4_2";
                                originalCoord = new Coord(3, 0, 0.0f);
                                originalDirection = Direction.Up;
                                assignOriginalTileName = "3_0";
                            }
                        }
                    }
                }
                if (gameObject.name.Contains("Enemy_Static"))
                {
                    if (coord.name == "2_2")
                    {
                        assignedTurnBackTile = "2_1";
                    }
                }
            }
            #endregion

            #region 2-4
            else if (currentLevelName == "2-4")
            {
                if (coord.name == "0_3")
                {
                    assignedTurnBackTile = "1_0";
                    originalCoord = new Coord(1, 0, 0.0f);
                    originalDirection = Direction.Up;
                    assignOriginalTileName = "1_0";
                }
                if (coord.name == "2_2")
                {
                    assignedTurnBackTile = "1_4";
                    originalCoord = new Coord(1, 4, 0.0f);
                    originalDirection = Direction.Down;
                    assignOriginalTileName = "1_4";
                }
            }
            #endregion

            #region 3-4
            else if (currentLevelName == "3-4")
            {
                if(coord.name == "1_2")
                {
                    if(originalCoord.name == "5_3")
                    {
                        assignedTurnBackTile = "1_3";
                    }
                }
            }
            #endregion

            #region 3-6
            else if(currentLevelName == "3-6")
            {
                if (this is EnemySentinel && (coord.name == "3_1" || coord.name == "3_3"))
                {
                    assignedTurnBackTile = lastTileName;
                }
                if(this is EnemyStatic && coord.name == "4_1")
                {
                    assignedTurnBackTile = lastTileName;
                }
            }
            #endregion

            #region 3-8
            else if (currentLevelName == "3-8")
            {
                if (this is EnemyStatic && coord.name == "1_3")
                {
                    assignedTurnBackTile = lastTileName;
                }
            }
            #endregion

            #region 3-9
            else if (currentLevelName == "3-9")
            {
                if (this is EnemyDistracted && coord.name == "2_0")
                {
                    assignedTurnBackTile = "1_2";
                }
            }
            #endregion

            #region 3-10
            else if (currentLevelName == "3-10")
            {
                if (this is EnemyPatrol && coord.name == "3_3")
                {
                    //assignedTurnBackTile = "4_1";
                    originalCoord = new Coord(2, 0, 0.0f);
                    originalDirection = Direction.Right;
                    assignOriginalTileName = "2_0";
                }
                if(this is EnemyPatrol && coord.name == "1_2")
                {
                    assignedTurnBackTile = "1_3";
                    originalCoord = new Coord(2, 0, 0.0f);
                    originalDirection = Direction.Right;
                    assignOriginalTileName = "2_0";
                }
            }
            #endregion

            #region 3-11
            else if (currentLevelName == "3-11")
            {
                if (this is EnemyPatrol )
                {
                    if (coord.name == "1_3")
                    {
                        assignedTurnBackTile = "2_3";
                        originalCoord = new Coord(3, 0, 0.0f);
                        originalDirection = Direction.Up;
                        assignOriginalTileName = "3_0";
                    }
                    else if(coord.name == "4_2" ||
                            coord.name == "3_3" ||
                            coord.name == "1_2" ||
                            coord.name == "4_3" ||
                            coord.name == "3_2" ||
                            coord.name == "2_2" ||
                            coord.name == "2_3")
                    {
                        originalCoord = new Coord(3, 0, 0.0f);
                        originalDirection = Direction.Up;
                        assignOriginalTileName = "3_0";
                    }
                   
                }
                if(this is EnemyStatic)
                {
                    if(coord.name == "3_0")
                    {
                        assignedTurnBackTile = "2_0";
                    }
                }
                if (this is EnemyDistracted)
                {
                    if (coord.name == "3_3")
                    {
                        assignedTurnBackTile = "4_3";
                    }
                }
            }
            #endregion


            #region
            else if(currentLevelName == "3-12")
            {
                if(this is EnemySentinel)
                {
                    if(coord.name == "")
                    {

                    }
                }
            }
            #endregion

            #region
            else if (currentLevelName == "3-12")
            {
                if(this is EnemySentinel)
                {
                    assignedTurnBackTile = lastTileName;
                }

            }
            #endregion
            var assignTile = gridManager.GetTileByName(assignedTurnBackTile);
            if (assignTile)
            {
                assignNextTile = assignTile;
                return assignTile;
            }
        }
        
        return gridManager.GetTileByName(originalCoord.name);
    }

    public void RedLine(LinkLine line)
    {
        var copy = Instantiate(line);
        copy.name = line.name;
        copy.transform.position = line.transform.position;
        var scale = line.transform.localScale;
        copy.transform.localScale = new Vector3(RED_SCALE, scale.y, scale.z);
        copy.transform.position = new Vector3(line.transform.position.x, 0.001f + line.transform.position.y, line.transform.position.z);
        copy.transform.rotation = line.transform.rotation;

        var childCount = copy.transform.childCount;
        for(var index = 0; index < childCount; index++)
        {
            var renderer = copy.transform.GetChild(index).GetComponent<MeshRenderer>();
            renderer.material = Resources.Load<Material>("Material/RouteRed");
        }
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
        //Debug.Log("转向完毕更新检测点");

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
        m_animator.SetBool("look_around", false);
        UpdateRouteMark();
        //Debug.Log("到达更新监测点");
        //Debug.Log(gameObject.name + " 到达点" + coord.name);
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
                if ( linkLine == null)// || !linkLine.through || linkLine.name == "Hor_Fenced_Visual"// 未间断的铁网路线也要显示
                {
                    break;
                }
            }

            var blockByHeight = false; 
            // 视线会被中间的高台挡住。参考第二章第十关；
            for (var x = 1; x < index; x++)
            {
                var middleNode = redNodes[x];
                var middleNodeY = middleNode.transform.position.y;
                if (middleNodeY - transform.position.y > 0.2f && middleNodeY - node.transform.position.y > 0.2f)
                {
                    // return false;
                    blockByHeight = true;
                    break;
                }
            }
            if (blockByHeight)
            {
                break;
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
                var exactPos = string.Format("{0}_{1}", Mathf.RoundToInt(enemy.transform.position.x), Mathf.RoundToInt(enemy.transform.position.z));
                if (exactPos == routeCoordName && coord.name != routeCoordName)
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
            routeArrow.transform.position = lastRedNode.transform.position+ new Vector3(0,Enemy.ARROW_HIEGHT, 0);

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


        if( this is EnemySentinel )
        {
            if (lookAroundTime > 0)
            {
                lookAroundTime -= Time.deltaTime;
                if (lookAroundTime <= 0)
                {
                    lookAroundTime = 9;

                    if (coordTracing.isLegal)
                    {
                        m_animator.SetFloat("look_around_type", 0.5f);
                    }
                    else
                    {
                        m_animator.SetFloat("look_around_type", 0);
                    }
                    
                    m_animator.SetBool("look_around", true);
                }
            }
        }
        else if ((targetIdleType != 0f || this is EnemyPatrol))
        {
            if (lookAroundTime > 0)
            {
                lookAroundTime -= Time.deltaTime;
                if (lookAroundTime <= 0)
                {
                    lookAroundTime = 9;
                    m_animator.SetFloat("look_around_type", coordTracing.isLegal ? 1 : 0);
                    m_animator.SetBool("look_around", true);
                }
            }
        }
        // Debug.Log("站立状态:" + idleType);
    }

    public virtual void ReachedOriginal()
    {
        //Debug.Log(gameObject.name + " 回到原点");
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
        if (sleeping) return CheckPlayerResult.None;
        var caught = TryCatch();
        if(caught)
        {
            return CheckPlayerResult.Catch;
        }
        var found = TryFound();
        if(found)
        {
            checkRange = 3;
            return CheckPlayerResult.Found;
        }
        return CheckPlayerResult.None;
    }

    public bool TryCatch()
    {
        if (Game.Instance.result == GameResult.WIN) return false;
        if (sleeping) return false;
        if(m_traceRound == Game.Instance.enemyRound)
        {
            return false;
        }
        if (!coordTracing.isLegal) return false;
        
        var player = Game.Instance.player;
        //
        var playerSteps = player.StepsReach(coord.name);

        if (player != null && !player.isHidding && player.coord.Equals(front) && playerSteps == 1)
        {
            AudioPlay.Instance?.PlayCatch(this);
            m_animator.SetBool("catch", true);
            m_animator.SetBool("moving", false);
            m_animator.Play("Cath");
            // 抓到了仍然需要更新定位框
            coordTracing = player.coord.Clone();
            Game.Instance.UpdateMoves();

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

        if (!player.coord.isLegal) return false;

        for(var index = 0; index < redNodes.Count; index++)
        {
            var coordRed = redNodes[index].coord;
            if(coordRed.Equals(player.coord) && !player.isHidding)
            {
               
                var targetTile = gridManager.GetTileByName(coordRed.name);
                if (targetTile != null)
                {
                    // 视线会被中间的高台挡住。参考第二章第十关；
                    for (var x = 1; x < index; x++)
                    {
                        var middleNode = redNodes[x];
                        var middleNodeY = middleNode.transform.position.y;
                        if (middleNodeY - transform.position.y > 0.2f && middleNodeY - player.transform.position.y > 0.2f )
                        {
                            return false;
                        }
                    }

                    coordTracing = player.coord.Clone();
                    coordPlayer = player.coord.Clone();
                    sawPlayer = true;
                    updateCoordPlayer = true;

                    var enemySteps = StepsReach(player.coord.name);

                    if (watching && enemySteps > 2)
                    {
                        coordPlayer.SetNoTurn();// 望远镜敌人看见远处敌人到达追踪点后不转向
                        updateCoordPlayer = false;
                    }

                    Game.Instance?.UpdateMoves();

                    m_animator.SetBool("look_around", false);
                   

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

        var isLookingAround = m_animator.GetBool("look_around");
        if(isLookingAround)
        {
            idleType = 0.5f;
            m_animator.SetFloat("idle_type", targetIdleType);
        }
        m_animator.SetBool("look_around", false);
        AudioPlay.Instance?.PlayEnemyAlert(this);
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


    //public virtual void ShowTraceTarget(string nodeName)
    //{
    //    m_animator.SetTrigger("stop_looking");
    //}

    public virtual void DisapearTraceTarget(bool play = true)
    {
        //enemyMove.transform.parent = transform;
        //enemyMove.gameObject.SetActive(false);
        if(play)
        {
            AudioPlay.Instance?.PlayNotFound(this);
        }
    }

    public override void StopLookAround()
    {
        base.StopLookAround();
        m_animator.SetBool("look_around", false);
    }

    public virtual void LostTarget()
    {
        ShowNotFound();
        targetIdleType = 1;
        coordTracing.SetNoTurn();
        coordPlayer.SetNoTurn();
        coordLureMe.SetNoTurn();
        stepsAfterFoundPlayer = -1;
        sawPlayer = false;
        assignOriginalTileName = "";
    }

    protected float idleType;
    public float targetIdleType;
    public float lookAroundTime = 0;

    public int stepsAfterFoundPlayer = -1;

}
