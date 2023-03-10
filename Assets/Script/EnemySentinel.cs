using UnityEngine;
using System.Collections.Generic;
public class EnemySentinel : Enemy
{
    public bool turn;

    public List<Direction> sentinelDirections;

    public int indexTurn = 1;

    public bool willTurn = false;

    const string Up = "Up";

    const string Down = "Down";

    const string Left = "Left";

    const string Right = "Right";

    public bool watching = true;

    // 顺时针
    const string CW = "CW";

    // 逆时针
    const string CCW = "CCW";

    public override void ResetDirection()
    {
        base.ResetDirection();
        UpdateRouteMark();
    }
    public override void UpdateRouteMark()
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

        var distance = hearSoundTile == null ? 20 : 2;
        var foundNodeX = coord.x;
        var foundNodeZ = coord.z;
        while (distance >= 0)
        {
            var currentNodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
            foundNodeX = foundNodeX + xOffset;
            foundNodeZ = foundNodeZ + zOffset;
            var nextNodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
            routeNodeNames.Add(currentNodeName);
            RedNodeByName(currentNodeName);
            
            var linkLine = boardManager.FindLine(currentNodeName, nextNodeName);
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
            for (var i = 0; i < boardManager.enemies.Count; i++)
            {
                var enemy = boardManager.enemies[i];
                var enemyPosition = enemy.transform.position;
                var enemyCoordX = Mathf.RoundToInt(enemyPosition.x);
                var enemyCoordZ = Mathf.RoundToInt(enemyPosition.z);
                var enemyCoordName = string.Format("{0}_{1}", enemyCoordX, enemyCoordZ);
                if (enemyCoordName == nextNodeName)
                {
                    distance = -1;
                    break;
                }
            }
            distance--;
        }

        BoardNode endNode = boardManager.FindNode(routeNodeNames[routeNodeNames.Count - 1]);
        routeArrow.position = endNode.transform.position;
        routeArrow.rotation = transform.rotation;
        routeArrow.Rotate(new Vector3(0, 0, 180));
        routeArrow.parent = null;
    }


    public override void CheckAction()
    {
        if (currentAction != null) return;

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
            currentAction = new ActionEnemyMove(this, foundPlayerTile);
            return;
        }

        if (hearSoundTile != null)
        {
            originalTile = null;
            currentAction = new ActionEnemyMove(this, hearSoundTile);
            return;
        }

        if (!watching && originalTile == null)
        {
            ReturnOriginal(true);
            return;
        }

        if (originalTile != null)
        {
            currentAction = new ActionEnemyMove(this, originalTile);
            return;
        }

        Sentinel();
    }



    public void Sentinel()
    {
        if (sentinelDirections == null || sentinelDirections.Count < 1) return;
        if (!willTurn)
        {
            var currentDirectionIndex = sentinelDirections.IndexOf(direction);
            if (currentDirectionIndex == 0 )
            {
                indexTurn = 1;
            }
            else if(currentDirectionIndex == sentinelDirections.Count - 1)
            {
                indexTurn = -1;
            }

            if(indexTurn == 1)
            {
                ShowCCW();
            }
            else
            {
                ShowCW();
            }
            
            var tryTurnDirectionIndex = currentDirectionIndex + indexTurn;
            if(tryTurnDirectionIndex < 0)
            {
                tryTurnDirectionIndex = sentinelDirections.Count - 1;
            }
            else if(tryTurnDirectionIndex >= sentinelDirections.Count)
            {
                tryTurnDirectionIndex = 0;
            }
            var tryTurnDirection = sentinelDirections[tryTurnDirectionIndex];

            targetDirection = tryTurnDirection;
            willTurn = true;
        }
        else
        {
            // 执行转向动作
            currentAction = new ActionTurnDirection(this, targetDirection);
            AudioPlay.Instance.PlayWatchTurn();
            willTurn = false;
            HideSentinelTurn();
        }
    }

    public override bool TryFoundPlayer()
    {
        if (!Game.Instance.player || !Game.Instance.player.currentTile)
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
       
        var foundPlayer = false;
        var foundPlayerNode = "";

        var distance = hearSoundTile == null ? 10 : 2;
        var foundNodeX = coord.x;
        var foundNodeZ = coord.z;
        while (foundPlayer==false && distance > 0)
        {
            var currentNodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
            foundNodeX = foundNodeX + xOffset;
            foundNodeZ = foundNodeZ + zOffset;
            var nextNodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
            var linkLine = boardManager.FindLine(currentNodeName, nextNodeName);
            if (linkLine == null)
                break;
            if (linkLine.transform.childCount < 1 || (linkLine.transform.childCount > 0 && !linkLine.transform.GetChild(0).name.Contains("Visual")))
            {
                break;
            }
            if(player.currentTile.name == nextNodeName && !player.hidding)
            {
                foundPlayer = true;
                foundPlayerNode = nextNodeName;
                break;
            }
            for (var i = 0; i < boardManager.enemies.Count; i++)
            {
                var enemy = boardManager.enemies[i];
                var enemyPosition = enemy.transform.position;
                var enemyCoordX = Mathf.RoundToInt(enemyPosition.x);
                var enemyCoordZ = Mathf.RoundToInt(enemyPosition.z);
                var enemyCoordName = string.Format("{0}_{1}", enemyCoordX, enemyCoordZ);
                if (enemyCoordName == nextNodeName)
                {
                    distance = -1;
                    break;
                }
            }
            distance--;
        }

        if (foundPlayer)
        {
            hearSoundTile = null;
            var targetTile = gridManager.GetTileByName(foundPlayerNode);
            if (targetTile != null)
            {
                ShowTraceTarget(targetTile, foundPlayerTile == null,2);
                foundPlayerTile = targetTile;
                ShowFound();
                originalTile = null;
                //Debug.Log("开始追踪:" + targetTile.name);
                turnOnReached = true;
                patroling = false;
                watching = false;
                return true;
            }
        }
        return false;
    }

    public override bool LureBottle(string tileName)
    {
        var result = base.LureBottle(tileName);
        UpdateRouteMark();
        watching = false;
        return result;
    }

    public override bool LureSteal(string tileName)
    {
        var result = base.LureSteal(tileName);
        UpdateRouteMark();
        watching = false;
        return result;
    }

    public override bool LureWhistle(string tileName)
    {
        var result = base.LureWhistle(tileName);
        UpdateRouteMark();
        watching = false;
        return result;
    }

    public override void OnReachedOriginal()
    {
        base.OnReachedOriginal();
        HideSentinelTurn();
        watching = true;
        willTurn = false;
    }
}
