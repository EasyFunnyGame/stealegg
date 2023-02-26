using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : Enemy
{
    //private List<string> patrolPoints = new List<string>();
    //private int patrolPointIndex = 0;

    public List<Coord> edgeCoords = new List<Coord>();

    public GridTile patrolTile;

    public override void Start()
    {
        base.Start();
        InitEdgeTiles();
    }

    void InitEdgeTiles()
    {
        edgeCoords.Add(coord.Clone());
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

        GridTile anotherEdgeTile = currentTile;

        var coordX = coord.x;
        var coordZ = coord.z;


        var findEdge = false;

        while(!findEdge)
        {
            var currentTileName = string.Format("{0}_{1}", coordX, coordZ);

            var nextPatrolTileName = string.Format("{0}_{1}", coordX + xOffset, coordZ + zOffset);

            var node = boardManager.FindNode(nextPatrolTileName);

            if (node == null)
            {
                findEdge = true;
            }
            var linkLine = boardManager.FindLine(currentTileName, nextPatrolTileName);
            if (linkLine)
            {
                var lineType = linkLine.transform.GetChild(0);
                if (lineType)
                {
                    if (!lineType.name.Contains("Normal"))
                    {
                        findEdge = true;

                    }
                }
            }
            if(findEdge)
            {
                anotherEdgeTile = gridManager.GetTileByName(currentTileName);
                if(anotherEdgeTile)
                {
                    edgeCoords.Add(new Coord(anotherEdgeTile.name, anotherEdgeTile.transform.position.y));
                }
            }
            coordX = coordX + xOffset;
            coordZ = coordZ + zOffset;
        }
        
    }

    public override void UpdateRouteMark()
    {
        for (var index = 0; index < redNodes.Count; index++)
        {
            DestroyImmediate(redNodes[index].gameObject);
        }
        redNodes.Clear();

        routeNode1Name = routeNode2Name = "";// routeNode3Name = routeNode4Name = 

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
        // 1
        var next1CoordX = coord.x + xOffset;
        var next1CoordZ = coord.z + zOffset;
        var next1NodeName = string.Format("{0}_{1}", next1CoordX, next1CoordZ);
        var lineName = boardManager.FindLine(curNodeName, next1NodeName);
        if (lineName != null)
        {
            routeNode1Name = next1NodeName;
            RedNodeByName(next1NodeName);
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
        }
    }

    public bool needTurn()
    {
        var reachEdge = false;
        var nextOffsetX = 0;
        var nextOffSetZ = 0;
        if (direction == Direction.Up)
        {
            nextOffSetZ = 1;
        }
        else if (direction == Direction.Down)
        {
            nextOffSetZ = -1;
        }
        else if (direction == Direction.Left)
        {
            nextOffsetX = -1;
        }
        else if (direction == Direction.Right)
        {
            nextOffsetX = 1;
        }
        var position = transform.position;
        var curentIndex = string.Format("{0}_{1}", Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        var nextPatrolTileName = string.Format("{0}_{1}", Mathf.RoundToInt(position.x) + nextOffsetX, Mathf.RoundToInt(position.z) + nextOffSetZ);
        var node = boardManager.FindNode(nextPatrolTileName);
        if(node==null)
        {
            reachEdge = true;
        }
        var linkLine = boardManager.FindLine(curentIndex, nextPatrolTileName);
        if (linkLine)
        {
            var lineType = linkLine.transform.GetChild(0);
            if (lineType)
            {
                if(!lineType.name.Contains("Normal"))
                {
                    reachEdge = true;
                }
            }
        }
        if (reachEdge)
        {
            var rechEdgeDirection = Utils.DirectionTo(currentTile, gridManager.GetTileByName(lastTileName), direction);
            if (direction != rechEdgeDirection)
            {
                targetDirection = rechEdgeDirection;
                
                
                originalDirection = targetDirection;
                for(var index = 0; index < edgeCoords.Count; index++)
                {
                    if(edgeCoords[index].name != curentIndex)
                    {
                        originalCoord = edgeCoords[index].Clone();
                        break;
                    }
                }
                return true;
            }
        }
        return false;
    }

    void UpdateNextPatrolPoint()
    {
        if (patroling)
        {
            var nextOffsetX = 0;
            var nextOffSetZ = 0;
            if (direction == Direction.Up)
            {
                nextOffSetZ = 1;
            }
            else if (direction == Direction.Down)
            {
                nextOffSetZ = -1;
            }
            else if (direction == Direction.Left)
            {
                nextOffsetX = -1;
            }
            else if (direction == Direction.Right)
            {
                nextOffsetX = 1;
            }
            var nextPatrolTileName = string.Format("{0}_{1}", coord.x + nextOffsetX, coord.z + nextOffSetZ);
            var node = boardManager.FindNode(nextPatrolTileName);

            if (node)
            {
                var linkLine = boardManager.FindLine(coord.name, nextPatrolTileName);
                if (linkLine)
                {
                    var lineType = linkLine.transform.GetChild(0);
                    var lineName = lineType.name;
                    if (lineName.Contains("Normal"))
                    {
                        var tile = gridManager.GetTileByName(nextPatrolTileName);
                        if (tile != null)
                        {
                            patrolTile = tile;
                        }
                    }
                }
            }
        }
    }

    public override void Reached()
    {
        base.Reached();
        UpdateNextPatrolPoint();

    }

    public override void OnReachedOriginal()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        if (hearSoundTile == null && foundPlayerTile == null)
        {
            patroling = true;
        }
        UpdateNextPatrolPoint();
        UpdateRouteMark();
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
            if (node1 == null && node2 == null  )
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

        if(!patroling && originalTile == null)
        {
            ReturnOriginal(true);
            return;
        }
        if (originalTile != null)
        {
            currentAction = new ActionEnemyMove(this, originalTile);
            return;
        }
        if(patrolTile!=null)
        {
            currentAction = new ActionEnemyMove(this, patrolTile);
        }
    }
}
 