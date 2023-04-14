using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : Enemy
{
    public int patrolDistance = 2;

    public List<Coord> edgeCoords = new List<Coord>();

    public GridTile patrolTile;

    private Transform routeLine;

    public override void Start()
    {
        routeLine = route.transform.Find("Route");
        this.checkRange = 3;
        this.sleeping = false;
        this.patroling = true;
        InitEdgeTiles();
        base.Start();
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
                    if (lineType.name.Contains("Doted") || !lineType.name.Contains("Visual"))
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

        
        if(direction == Direction.Up)
        {
            zOffset = 1;
        }
        else if(direction == Direction.Down)
        {
            zOffset = -1;
        }
        else if (direction == Direction.Left)
        {
            xOffset = -1;
        }
        else if (direction == Direction.Right)
        {
            xOffset = 1;
        }
        for(var index = 0; index < edgeCoords.Count; index++)
        {
            var edge = edgeCoords[index];
            
            var xOff = edge.x - coord.x;
            if (xOff!=0)
            {
                var xOffModel = Mathf.Abs(xOff) / xOff;
                if (xOffModel == xOffset )
                {
                    originalCoord = edge;
                }
            }
            else
            {
                var zOff = edge.z - coord.z;
                if(zOff!=0)
                {
                    var zOffModel = Mathf.Abs(zOff) / zOff;
                    if ( zOffModel == zOffset)
                    {
                        originalCoord = edge;
                        for (var a = 0; a < edgeCoords.Count; a++)
                        {
                            var another = edgeCoords[a];
                            if (!another.EqualsIgnoreY(edge))
                            {
                                originalDirection = Utils.DirectionToMultyGrid( edge.name, another.name, direction);
                                break;
                            }
                        }
                        
                    }
                }
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        if ( routeLine != null)
        {
            routeLine.gameObject.SetActive(patroling);
            if ( redNodes.Count > 0)
            {
                var endPosition = transform.localPosition + transform.GetChild(0).forward * 2;
                var x = Mathf.CeilToInt(endPosition.x);
                var z = Mathf.CeilToInt(endPosition.z);
                var length = 0f;
                // Debug.Log("最终点" + x + " " + z);
                var endNode = boardManager.FindNode(string.Format("{0}_{1}", x, z));
                if (endPosition.x < 0 || endPosition.z < 0)
                {
                    endNode = null;
                }
                if(endNode)
                {
                    length = 80;
                    routeLine.transform.localScale = new Vector3(1.2f, 1, 80);
                    routeLine.transform.position = new Vector3(transform.position.x, 0.016f + endNode.transform.position.y, transform.position.z);
                }
                else
                {
                    var finalNode = redNodes[redNodes.Count - 1];
                    var distance = Vector3.Distance(transform.position, finalNode.transform.localPosition);
                    length = distance * 40;
                }
                routeLine.transform.localScale = new Vector3(RED_SCALE, 1, length);
                routeLine.transform.rotation = transform.GetChild(0).transform.rotation;
                routeArrow.transform.position = transform.localPosition + transform.GetChild(0).forward * length / 40;
                //routeArrow.transform.position = new Vector3(routeArrow.transform.position.x,0.006f+ endNode.transform.localPosition.y, routeArrow.transform.position.z);
                routeArrow.transform.rotation = transform.GetChild(0).transform.rotation;
                routeArrow.transform.Rotate(new Vector3(0, 0, 180));
                
            }
            if(!patroling )
            {
                if (redNodes.Count > 0)
                {
                    var lastNode = redNodes[redNodes.Count - 1];
                    routeLine.transform.rotation = transform.GetChild(0).transform.rotation;
                    routeArrow.transform.position = new Vector3(lastNode.transform.position.x, 0.006f+ lastNode.transform.position.y, lastNode.transform.position.z);
                    routeArrow.transform.rotation = transform.GetChild(0).transform.rotation;
                    routeArrow.transform.Rotate(new Vector3(0, 0, 180));
                }
            }
        }
        routeArrow.gameObject.SetActive(!body_looking);
    }

    //public override void StartMove()
    //{
    //    base.StartMove();
    //    //if (redNodes.Count > 0)
    //    //{
    //    //    var firstNode = redNodes[0];
    //    //    Destroy(firstNode);
    //    //    redNodes.RemoveAt(0);
    //    //}
    //}

    //public override void UpdateRouteMark()
    //{
    //    for (var index = 0; index < redNodes.Count; index++)
    //    {
    //        DestroyImmediate(redNodes[index].gameObject);
    //    }
    //    redNodes.Clear();

    //    for (var index = 0; index < redLines.Count; index++)
    //    {
    //        DestroyImmediate(redLines[index].gameObject);
    //    }
    //    redLines.Clear();

    //    routeNodeNames.Clear();

    //    if (sleeping) return;

    //    var xOffset = 0;

    //    var zOffset = 0;

    //    if (direction == Direction.Up)
    //    {
    //        zOffset = 1;
    //    }
    //    else if (direction == Direction.Down)
    //    {
    //        zOffset = -1;
    //    }
    //    else if (direction == Direction.Right)
    //    {
    //        xOffset = 1;
    //    }
    //    else if (direction == Direction.Left)
    //    {
    //        xOffset = -1;
    //    }

    //    var distance = patrolDistance;
    //    var foundNodeX = coord.x;
    //    var foundNodeZ = coord.z;
    //    while ( distance >= 0)
    //    {
    //        var currentNodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
    //        foundNodeX = foundNodeX + xOffset;
    //        foundNodeZ = foundNodeZ + zOffset;
    //        var nextNodeName = string.Format("{0}_{1}", foundNodeX, foundNodeZ);
    //        routeNodeNames.Add(currentNodeName);
    //        RedNodeByName(currentNodeName);
            
    //        var linkLine = boardManager.FindLine(currentNodeName, nextNodeName);
    //        if (linkLine == null)
    //            break;

    //        if (distance > 0 && !patroling)
    //        {
    //            RedLineByName(linkLine);
    //        }

    //        if (linkLine.transform.childCount<1)
    //        {
    //            break;
    //        }
    //        var lineType = linkLine.transform.GetChild(0);
    //        if (lineType.name.Contains("Doted") || !lineType.name.Contains("Visual"))
    //        {
    //            break;
    //        }
    //        distance--;
    //    }
    //    BoardNode endNode = boardManager.FindNode(routeNodeNames[routeNodeNames.Count - 1]);
    //    routeArrow.position = endNode.transform.position + new Vector3(0, 0.006f+ endNode.transform.position.y, 0);
    //    routeArrow.rotation = transform.rotation;
    //    routeArrow.Rotate(new Vector3(0, 0, 180));
    //    routeArrow.parent = null;
    //}

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
                if(lineType.name.Contains("Doted") || !lineType.name.Contains("Visual") )
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
                for (var index = 0; index < edgeCoords.Count; index++)
                {
                    if (edgeCoords[index].name != originalCoord.name)
                    {
                        originalDirection = Utils.DirectionTo(originalCoord.name, edgeCoords[index].name,direction);
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
                    if (lineName.Contains("Visual"))
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

    public override void ReachedOriginal()
    {
        base.ReachedOriginal();
        this.checkRange = 3;
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        //if (hearSoundTile == null && foundPlayerTile == null)
        //{
        //    patroling = true;
        //}
        UpdateNextPatrolPoint();
        UpdateRouteMark();
    }

    public override void CheckAction()
    {
        if (currentAction != null) return;

        if (growthTile != null)
        {
            growthTile = null;
            return;
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
        //    TryFoundPlayer();
        //    if (foundPlayerTile != null)
        //    {
        //        if (hearSoundTile != null)
        //        {
        //            currentAction = new ActionEnemyMove(this, foundPlayerTile);
        //        }
        //        return;
        //    }
        //}
        //else
        //{
        //    TryFoundPlayer();
        //    if (TryCatch()) return;
        //}

        //if (foundPlayerTile != null)
        //{
        //    originalTile = null;
        //    currentAction = new ActionEnemyMove(this, foundPlayerTile);
        //    return;
        //}


        //if (hearSoundTile != null)
        //{
        //    originalTile = null;
        //    currentAction = new ActionEnemyMove(this, hearSoundTile);
        //    return;
        //}

        if(!patroling && originalTile == null)
        {
            // ReturnOriginal(true);
            return;
        }
        if (originalTile != null)
        {
            currentAction = new ActionEnemyMove(this, originalTile);
            return;
        }
        if (patrolTile!=null)
        {
            currentAction = new ActionEnemyMove(this, patrolTile);
        }
    }
}
 