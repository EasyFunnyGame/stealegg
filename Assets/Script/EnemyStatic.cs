using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatic : Enemy
{
    
    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    protected override void OnReached()
    {
        base.OnReached();

        Debug.Log("敌人到达路径点");

        UpdateRouteMark();
        hasAction = false;
        animator.CrossFade("Player_Idle", 0.1f);
        //Debug.Log("Eeney Static Guard Nodes " + linkLine1.name + "   " + linkLine2.name);
        //Debug.Log("Eeney Static Guard Nodes " + node1.name + "   " + node2.name);
    }


    protected override void OnStartMove()
    {
        base.OnStartMove();
        animator.CrossFade("Player_Sprint", 0.1f);
    }

    public override void OnDirectionRested()
    {
        base.OnDirectionRested();
        UpdateRouteMark();
        TryCatch();
        TryTrace();
    }

    public override void TryCatch()
    {
        base.TryCatch();
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
        var catchNodeX = coord.x + xOffset;
        var catchNodeZ = coord.z + zOffset;
        var catchTileName = string.Format("{0}_{1}", catchNodeX, catchNodeZ);
        var catchTile = gridManager.GetTileByName(catchTileName);
        //Debug.Log("抓捕:"+ catchTileName + " 主角位置:" + Player.Instance.coord.name);
        if(catchTile != null && Player.Instance.coord.name == catchTileName)
        {
            Game.Instance.status = GameStatus.FAIL;
        }
    }

    public override void TryTrace()
    {
        base.TryTrace();
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
        var catchNodeX = coord.x + xOffset * 2;
        var catchNodeZ = coord.z + zOffset * 2;
        var next1NodeName = string.Format("{0}_{1}", catchNodeX, catchNodeZ);
        if (Player.Instance.tile_s && Player.Instance.tile_s.name == next1NodeName)
        {
            var targetTile = gridManager.GetTileByName(next1NodeName);
            if (targetTile != null)
            {
                tracingTile = targetTile;
                selected_tile_s = tracingTile;
                gridManager.find_paths_realtime(this, tracingTile);
                hasAction = true;
                targetDirection = direction;
                UpdateMoves(targetTile);
                //Debug.Log("追踪敌人"+targetTile.name);
            }
        }
    }

    public override void UpdateRouteMark()
    {
        base.UpdateRouteMark();


        for(var index=  0; index < redNodes.Count; index++)
        {
            DestroyImmediate(redNodes[index].gameObject);
        }
        redNodes.Clear();

        for (var index = 0; index < redLines.Count; index++)
        {
            DestroyImmediate(redLines[index].gameObject);
        }
        redLines.Clear();

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

        var curNodeName = tile_s.gameObject.name;

        var next1CoordX = coord.x + xOffset;
        var next1CoordZ = coord.z + zOffset;
        var next1NodeName = string.Format("{0}_{1}", next1CoordX, next1CoordZ);

        var next2CoordX = next1CoordX + xOffset;
        var next2CoordZ = next1CoordZ + zOffset;
        var next2NodeName = string.Format("{0}_{1}", next2CoordX, next2CoordZ);
        RedLineByName(curNodeName, next1NodeName);
        RedLineByName(next1NodeName, next2NodeName);
        RedNodeByName(curNodeName);
        RedNodeByName(next1NodeName);
        RedNodeByName(next2NodeName);
    }

    
}
