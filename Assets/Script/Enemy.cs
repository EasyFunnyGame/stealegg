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

    //public Transform routeLine;

    public Transform routeArrow;

    public List<string> routeNodeNames;

    public List<MeshRenderer> redNodes = new List<MeshRenderer>();

    public List<LinkLine> redLines = new List<LinkLine>();

    public bool sleeping = false;

    public bool patroling = false;

    public bool waiting = false;

    public string walkingLineType;

    public int up = 0;

    public override void Start()
    {
        base.Start();
        lookAroundTime = Random.Range(5, 10);
        tr_body = transform;
        Reached();
        OnReachedOriginal();
        DisapearTraceTarget(false);
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

    public void RedLineByName(LinkLine line)
    {
        var copy = Instantiate(line);
        copy.name = line.name;
        copy.transform.position = line.transform.position;
        copy.transform.Translate(new Vector3(0, 0.001f, 0));
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
            var nodeTr = node.targetIcon;
            var copyNode = Instantiate(nodeTr.GetComponent<MeshRenderer>());
            copyNode.name = nodeName;
            copyNode.transform.position = nodeTr.transform.position;
            copyNode.transform.Translate(new Vector3(0, 0.001f, 0));
            copyNode.transform.rotation = nodeTr.transform.rotation;
            copyNode.material = Resources.Load<Material>("Material/RouteRed");
            redNodes.Add(copyNode);
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
        }
        m_animator.SetBool("moving", false);
        //Debug.Log("敌人到达路径点:"+ currentTile.name);
    }

    public virtual void UpdateRouteMark()
    {
        for (var index = 0; index < redNodes.Count; index++)
        {
            DestroyImmediate(redNodes[index].gameObject);
        }
        redNodes.Clear();

        for (var index = 0; index < redLines.Count; index++)
        {
            DestroyImmediate(redLines[index].gameObject);
        }
        redLines.Clear();

        routeNodeNames.Clear();

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

            if (linkLine.transform.childCount < 1 || (linkLine.transform.childCount > 0 && !linkLine.transform.GetChild(0).name.Contains("Visual")))
            {
                break;
            }
            distance--;
        }

        BoardNode endNode = boardManager.FindNode(routeNodeNames[routeNodeNames.Count - 1]);
        routeArrow.position = endNode.transform.position+new Vector3(0,0.02f,0);
        routeArrow.rotation = transform.rotation;
        routeArrow.Rotate(new Vector3(0, 0, 180));
        routeArrow.parent = null;
    }

    public virtual bool TryFoundPlayer()
    {
        if (!Game.Instance.player || !Game.Instance.player.currentTile)
        {
            return false;
        }
        if(Game.Instance.result == GameResult.FAIL || Game.Instance.result == GameResult.WIN)
        {
            return false;
        }
        var player = Game.Instance.player;
        if (Game.Instance.result == GameResult.FAIL) return false;
        if (sleeping) return false;
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
        
        var foundPlayerNode = "";

        LinkLine linkLine;
        // 情况1
        if (foundPlayer == false)
        {
            linkLine = boardManager.FindLine(currentTile.name, next1NodeName);
            if (linkLine)
            {
                var linkLineName = linkLine.transform.GetChild(0).name;
                var canReach = linkLineName.Contains("Visual");
                if (hearSoundTile == null && linkLine && canReach && player.currentTile.name == next1NodeName && !player.hidding)
                {
                    foundPlayer = true;
                    foundPlayerNode = next1NodeName;
                }
            }
            else
            {
                return false;
            }
        }
        
        // 情况2
        foundNodeX += xOffset;
        foundNodeZ += zOffset;
        var next2NodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
        linkLine = boardManager.FindLine(next1NodeName, next2NodeName);
        if (linkLine && Game.Instance.player.currentTile.name == next2NodeName && !player.hidding)
        {
            foundPlayer = true;
            foundPlayerNode = next2NodeName;
        }

        if (foundPlayer)
        {
            hearSoundTile = null;
            var targetTile = gridManager.GetTileByName(foundPlayerNode);
            if (targetTile != null)
            {
                ShowTraceTarget(targetTile , foundPlayerTile== null,2);
                foundPlayerTile = targetTile;
                ShowFound();
                originalTile = null;
                //Debug.Log("开始追踪:" + targetTile.name);
                turnOnReached = true;
                patroling = false;
                routeArrow.gameObject.SetActive(true);
                return true;
            }
        }
        return false;
    }

    public override void StartMove()
    {
        m_animator.SetBool("moving", true);
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
            idleType -= 0.025f;
            if (targetIdleType >= idleType)
            {
                idleType = targetIdleType;
            }
        }

        if (targetIdleType > idleType)
        {
            idleType += 0.025f;
            if (targetIdleType <= idleType)
            {
                idleType = targetIdleType;
            }
        }
        m_animator.SetFloat("idle_type", idleType);

        if(targetIdleType != 0f )
        {
            if (lookAroundTime > 0)
            {
                lookAroundTime -= Time.deltaTime;
                if (lookAroundTime <= 0)
                {

                    lookAroundTime = Random.Range(5,10);
                    var lookAroundType = Random.Range(0, 2);
                    m_animator.SetFloat("look_around_type", lookAroundType);
                    m_animator.SetTrigger("look_around");
                }
            }
        }
        
    }

    public virtual void OnReachedOriginal()
    {
        routeArrow.gameObject.SetActive(false);
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
        if (Game.Instance.result == GameResult.FAIL || Game.Instance.result == GameResult.WIN)
        {
            return false;
        }
        var player = Game.Instance.player;
        if (player == null || player.currentTile == null) return false;

        var targetDirection = Utils.DirectionTo(currentTile.name, tileName, direction);
        if (targetDirection == direction && player.CanReachInSteps(currentTile.name) && !player.hidding)
        {
            Game.Instance.FailGame();
            m_animator.SetBool("catch", true);
            AudioPlay.Instance.PlayCatch(this);
            return true;
        }
        return false;
    }

    public bool CatchPlayer()
    {
        if (Game.Instance.result == GameResult.FAIL || Game.Instance.result == GameResult.WIN)
        {
            return false;
        }
        var player = Game.Instance.player;
        if (player == null || player.currentTile == null) return false;
        var targetDirection = Utils.DirectionTo(currentTile.name, player.currentTile.name, direction);
        if (targetDirection == direction && player.CanReachInSteps(currentTile.name) && !player.hidding)
        {
            Game.Instance.FailGame();
            m_animator.SetBool("catch", true);
            AudioPlay.Instance.PlayCatch(this);
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

        patroling = false;
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
        ShowTraceTarget(targetTile, hearSoundTile == null,1);
        hearSoundTile = targetTile;
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


        patroling = false;
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
        ShowTraceTarget(targetTile, hearSoundTile== null,1);
        hearSoundTile = targetTile;
        
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

        patroling = false;
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
        ShowTraceTarget(targetTile, hearSoundTile == null,1);

        hearSoundTile = targetTile;
        
        ShowFound();
        return true;
    }

    public void OnTurnEnd()
    {
        body_looking = false;
        if (foundPlayerTile == null)
        {
            TryFoundPlayer();
            if (foundPlayerTile != null)
            {
                if (hearSoundTile == null)
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
        m_animator.SetBool("moving",false);
        UpdateRouteMark();
    }

    public virtual void ShowNotFound()
    {
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

    public virtual void HideSentinelTurn()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        icons.ccw.gameObject.SetActive(false);
        icons.cw.gameObject.SetActive(false);
    }


    public virtual void ShowTraceTarget(GridTile tile, bool playSound, int soundType)
    {
        enemyMove.transform.parent = null;
        enemyMove.gameObject.SetActive(true);
        enemyMove.Play("Movement_Animation");
        var node = boardManager.FindNode(tile.name);
        enemyMove.transform.transform.position = node.transform.position;
        if (soundType == 1 && playSound)
        {
            AudioPlay.Instance.PlayHeard();
        }
        if(soundType == 2 && playSound)
        {
            AudioPlay.Instance.PlayFound();
        }
    }

    public virtual void DisapearTraceTarget(bool play = true)
    {
        enemyMove.transform.parent = transform;
        enemyMove.gameObject.SetActive(false);
        if(play)
        {
            AudioPlay.Instance.PlayNotFound(this);
        }
        
    }

    public virtual void LostTarget()
    {
        ShowNotFound();
        foundPlayerTile = null;
        hearSoundTile = null;
        targetTileName = "";
        turnOnReached = false;
        targetIdleType = 1;
        m_animator.SetTrigger("not_found");

    }

    protected float idleType;
    protected float targetIdleType;
    protected float lookAroundTime = 0;

}
