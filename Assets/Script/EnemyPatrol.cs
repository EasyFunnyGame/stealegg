using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : Enemy
{
    public List<Coord> edgeCoords = new List<Coord>();
    // 巡逻点 
    public List<BoardNode> patrolNodes;     

    public GridTile patrolTile;

    private Transform routeLine;

    public override void Start()
    {
        routeLine = route.transform.Find("Route");
        this.checkRange = 3;
        this.sleeping = false;
        this.patroling = true;
        base.Start();
        UpdateFartest();
    }

    protected override void Update()
    {
        base.Update();
        if ( routeLine != null)
        {
            routeLine.gameObject.SetActive(patroling);
            if ( redNodes.Count > 0)
            {
                var distance = Vector3.Distance(transform.position, patrolEnd.transform.position) * 40;
                distance = Mathf.Min(distance,80);


                routeLine.transform.localScale = new Vector3(RED_SCALE, 1, distance);
                routeLine.transform.rotation = transform.GetChild(0).transform.rotation;
                routeLine.transform.position = transform.position;
                routeLine.transform.Translate(new Vector3(0, 0.0115f, 0));

                routeArrow.transform.position = transform.localPosition + transform.GetChild(0).forward * distance / 40;
                routeArrow.transform.rotation = transform.GetChild(0).transform.rotation;
                routeArrow.transform.Rotate(new Vector3(0, 0, 180));
                
            }
            if(!patroling )
            {
                route.gameObject.SetActive(false);
                if (redNodes.Count > 0)
                {
                    var lastNode = redNodes[redNodes.Count - 1];
                    routeLine.transform.rotation = transform.GetChild(0).transform.rotation;
                    routeArrow.transform.position = new Vector3(lastNode.transform.position.x, 0.006f+ lastNode.transform.position.y, lastNode.transform.position.z);
                    routeArrow.transform.rotation = transform.GetChild(0).transform.rotation;
                    routeArrow.transform.Rotate(new Vector3(0, 0, 180));
                }
            }
            else
            {
                route.gameObject.SetActive(!body_looking);
                for (var index = 0; index < redLines.Count; index++)
                {
                    redLines[index].gameObject.SetActive(false);
                }
            }
        }
        routeArrow.gameObject.SetActive(!body_looking);
    }

    public void ResetOriginal()
    {
        originalCoord = coord.Clone();
        originalDirection = _direction;
    }

    public override void DoDefaultAction()
    {
        Debug.Log( this.gameObject.name +  "进行默认行为");
        this.patroling = true;

        var nextPatrol = this.front;
        var index = -1;
        for(var i = 0; i < this.patrolNodes.Count; i++)
        {
            if(this.patrolNodes[i].gameObject.name == nextPatrol.name)
            {
                index = i;
                break;
            }
        }

        if(index == -1)
        {
            Debug.LogError(this.gameObject.name + "巡逻点设置错误,请重新检查敌人巡逻点设置");
            return;
        }
        var steps = StepsReach(nextPatrol.name);
        if(steps!=1)
        {
            Debug.LogError(this.gameObject.name + "巡逻点设置错误,此点不可达 " + nextTile.name + " ,请重新检查敌人巡逻点设置");
            return;
        }
        var tile = gridManager.GetTileByName(nextPatrol.name);
        if (tile == null)
        {
            Debug.LogError(this.gameObject.name + "巡逻点不存在" + nextTile.name + " ,请重新检查地图路径点设置");
            return;
        }
        currentAction = new ActionEnemyMove(this, tile);
    }


    public Direction TurnDirection()
    {
        var nextPatrol = front;
        
        
        var index = -1;
        for (var i = 0; i < this.patrolNodes.Count; i++)
        {
            if (this.patrolNodes[i].gameObject.name == nextPatrol.name)
            {
                index = i;
                break;
            }
        }
        var steps = StepsReach(nextPatrol.name);
        if( steps <=0 )
        {
            index = -1;
        }
        if (index == -1)
        {
            if( _direction == Direction.Left )
            {
                return Direction.Right;
            }
            else if (_direction == Direction.Right)
            {
                return Direction.Left;
            }
            else if (_direction == Direction.Up)
            {
                return Direction.Down;
            }
            else// if (_direction == Direction.Down)
            {
                return Direction.Up;
            }
        }
        else
        {
            return _direction;
        }
    }

    public override void ReachedOriginal()
    {
        base.ReachedOriginal();
        this.patroling = true;
        this.checkRange = 3;
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        UpdateRouteMark();
    }

    public override void Reached()
    {
        base.Reached();
        if (patroling)
        {
            UpdateFartest();
            Debug.Log("巡逻敌人到达点111");
        }
    }

    public override void Turned()
    {
        base.Turned();
        if( patroling )
        {
            UpdateFartest();
            Debug.Log("巡逻敌人到达点222");
        }
    }

    BoardNode patrolEnd;

    void UpdateFartest()
    {
        var offsetX = 0;

        var offsetZ = 0;

        if(_direction == Direction.Left)
        {
            offsetX = -1;
        }
        else if (_direction == Direction.Right)
        {
            offsetX = 1;
        }
        else if (_direction == Direction.Up)
        {
            offsetZ = 1;
        }
        else
        {
            offsetZ = -1;
        }

        var coord_x = coord.x + offsetX;
        var coord_z = coord.z + offsetZ;
        for(var i =  0; i < 3;  i++)
        {
            var name = string.Format("{0}_{1}", coord_x, coord_z);
            var boardNode = boardManager.FindNode(name);
            if(boardNode?.name == name )
            {
                patrolEnd = boardNode;
                Debug.Log("最远巡逻路径点" + name);
                coord_x += offsetX;
                coord_z += offsetZ;
            }
        }
    }

    public override void LostTarget()
    {
        base.LostTarget();
        var one = patrolNodes[0];
        var another = patrolNodes[patrolNodes.Count - 1];
        var stepsToOne = StepsReach(one.gameObject.name);
        var stepsToAnother = StepsReach(another.gameObject.name);


        if(stepsToOne < stepsToAnother )
        {
            originalCoord = one.coord.Clone();
            originalDirection = Utils.DirectionToMultyGrid(one.coord.name, another.coord.name,_direction);
        }
        else 
        {
            originalCoord = another.coord.Clone();
            originalDirection = Utils.DirectionToMultyGrid(another.coord.name, one.coord.name, _direction);
        }

    }
}
 