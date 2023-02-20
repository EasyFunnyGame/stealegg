using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : Enemy
{
    private List<string> patrolPoints = new List<string>();
    private int patrolPointIndex = 0;

    public override void Start()
    {
        base.Start();
    }

    public override void OnReachedOriginal()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        patroling = true;
        UpdateRouteMark();
    }

    string routeNode3Name;
    string routeNode4Name;

    public override void UpdateRouteMark()
    {
        var points = new List<string>();

        for (var index = 0; index < redNodes.Count; index++)
        {
            DestroyImmediate(redNodes[index].gameObject);
        }
        redNodes.Clear();

        routeNode1Name = routeNode2Name = routeNode3Name = routeNode4Name = "";

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
        points.Add(curNodeName);
        // 1
        var next1CoordX = coord.x + xOffset;
        var next1CoordZ = coord.z + zOffset;
        var next1NodeName = string.Format("{0}_{1}", next1CoordX, next1CoordZ);
        var lineName = boardManager.FindLine(curNodeName, next1NodeName);
        if (lineName != null)
        {
            routeNode1Name = next1NodeName;
            RedNodeByName(next1NodeName);
            points.Add(next1NodeName);
        }
        
        // 2
        var next2CoordX = next1CoordX + xOffset;
        var next2CoordZ = next1CoordZ + zOffset;
        var next2NodeName = string.Format("{0}_{1}", next2CoordX, next2CoordZ);
        lineName = boardManager.FindLine(next1NodeName, next2NodeName);
        if (lineName != null)
        {
            routeNode2Name = next2NodeName;
            RedNodeByName(next2NodeName);
            points.Add(next2NodeName);
        }
        
        // 3
        var next3CoordX = next2CoordX + xOffset;
        var next3CoordZ = next2CoordZ + zOffset;
        var next3NodeName = string.Format("{0}_{1}", next3CoordX, next3CoordZ);
        lineName = boardManager.FindLine(next2NodeName, next3NodeName);
        if (lineName != null)
        {
            routeNode3Name = next3NodeName;
            RedNodeByName(next3NodeName);
            points.Add(next3NodeName);
        }
        
        // 4
        var next4CoordX = next3CoordX + xOffset;
        var next4CoordZ = next3CoordZ + zOffset;
        var next4NodeName = string.Format("{0}_{1}", next4CoordX, next4CoordZ);
        lineName = boardManager.FindLine(next3NodeName, next4NodeName);
        if (lineName != null)
        {
            routeNode4Name = next4NodeName;
            RedNodeByName(next4NodeName);
            points.Add(next4NodeName);
        }
        
        if(patrolPoints.Count == 0)
        {
            patrolPoints = points;
            patrolPointIndex = patrolPoints.IndexOf(currentTile.name);
        }
    }

    protected override void UpdateRouteRedLine()
    {
        if(patroling == false)
        {
            base.UpdateRouteRedLine();
        }
        else
        {
            var node1 = boardManager.FindNode(routeNode1Name);
            var node2 = boardManager.FindNode(routeNode2Name);
            var node3 = boardManager.FindNode(routeNode3Name);
            var node4 = boardManager.FindNode(routeNode4Name);
            if (node1 == null && node2 == null && node3 == null && node4 == null )
            {
                route.SetActive(false);
            }
            else
            {
                route.SetActive(true);
                BoardNode endNode = null;
                if(node1 != null)
                {
                    endNode = node1;
                }
                if (node2 != null)
                {
                    endNode = node2;
                }
                if (node3 != null)
                {
                    endNode = node3;
                }
                if (node4 != null)
                {
                    endNode = node4;
                }
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

        if (originalTile != null)
        {
            currentAction = new ActionEnemyMove(this, originalTile);
            return;
        }

        Patrol();
    }

    int patrolDirection = 1;
    void Patrol()
    {
        patroling = true;
        var nextIndex = patrolPointIndex + patrolDirection;

       if(patrolPoints.Count == nextIndex)
        {
            patrolDirection = -1;
        }
        else if(nextIndex < 0)
        {
            patrolDirection = 1;
        }
        else if (patrolPoints.Count < nextIndex)
        {
            patrolDirection = -1;
        }
        //else if(nextIndex < 0)
        //{
        //    patrolDirection = 1;
        //}
        
        nextIndex = patrolPointIndex + patrolDirection;

        patrolPointIndex = nextIndex;

        var tileName = patrolPoints[nextIndex];
        var tile = gridManager.GetTileByName(tileName);
        if(tile!=null)
        {
            currentAction = new ActionEnemyMove(this, tile);
            
        }
        Debug.Log("下一个巡逻点" + tileName);
    }

    public override void Reached()
    {
        base.Reached();
        if(patroling)
        {
            originalCoord = coord.Clone();
            originalDirection = direction;
        }
    }
}
 