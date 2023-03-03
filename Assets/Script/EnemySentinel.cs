using UnityEngine;

public class EnemySentinel : Enemy
{
    public bool turn;

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
            distance--;
            var linkLine = boardManager.FindLine(currentNodeName, nextNodeName);
            if (linkLine == null)
                break;
            if (linkLine.transform.childCount < 1 || (linkLine.transform.childCount > 0 && !linkLine.transform.GetChild(0).name.Contains("Visual")))
            {
                break;
            }
            for(var i = 0; i < boardManager.enemies.Count; i++)
            {
                var enemy = boardManager.enemies[i];
                var enemyPosition = enemy.transform.position;
                var enemyCoordX = Mathf.RoundToInt(enemyPosition.x);
                var enemyCoordZ = Mathf.RoundToInt(enemyPosition.z);
                var enemyCoordName = string.Format("{0}_{1}", enemyCoordX, enemyCoordZ);
                if(enemyCoordName == nextNodeName)
                {
                    distance = -1;
                    break;
                }
            }
        }
    }
    //public override void CheckAction()
    //{
    //    if (currentAction != null) return;

    //    if (foundPlayerTile == null)
    //    {
    //        TryFoundPlayer();
    //        if (foundPlayerTile != null)
    //        {
    //            if (hearSoundTile == null)
    //            {
    //                currentAction = new ActionFoundPlayer(this);
    //            }
    //            else
    //            {
    //                currentAction = new ActionTurnDirection(this, targetDirection);
    //            }
    //            return;
    //        }
    //    }
    //    else
    //    {
    //        if (CatchPlayer()) return;
    //    }

    //    if (foundPlayerTile != null)
    //    {
    //        originalTile = null;
    //        currentAction = new ActionEnemyMove(this, foundPlayerTile);
    //        return;
    //    }

    //    if (hearSoundTile != null)
    //    {
    //        originalTile = null;
    //        currentAction = new ActionEnemyMove(this, hearSoundTile);
    //        return;
    //    }

    //    if (!patroling && originalTile == null)
    //    {
    //        ReturnOriginal(true);
    //        return;
    //    }
    //    if (originalTile != null)
    //    {
    //        currentAction = new ActionEnemyMove(this, originalTile);
    //        return;
    //    }
        
    //}

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

        var distance = hearSoundTile == null ? 4 : 2;
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
                foundPlayerTile = targetTile;
                ShowTraceTarget(targetTile);
                ShowFound();
                originalTile = null;
                //Debug.Log("开始追踪:" + targetTile.name);
                turnOnReached = true;
                patroling = false;
                return true;
            }
        }
        return false;
    }

    public override bool LureBottle(string tileName)
    {
        var result = base.LureBottle(tileName);
        UpdateRouteMark();
        return result;
    }

    public override bool LureSteal(string tileName)
    {
        var result = base.LureSteal(tileName);
        UpdateRouteMark();
        return result;
    }

    public override bool LureWhistle(string tileName)
    {
        var result = base.LureWhistle(tileName);
        UpdateRouteMark();
        return result;
    }
}
