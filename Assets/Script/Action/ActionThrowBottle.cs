﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionThrowBottle : ActionBase
{
    public Transform bottleParent;
    public Vector3 bottleStartPosition;
    public Quaternion bottleStartRotation;

    public Vector3[] linePointList;

    public Transform bottle;

    public int segmentIndex;

    public string targetTileName;

    public ActionThrowBottle(Player player, string targetTile, Vector3 targetPosition) : base(player, ActionType.ThrowBottle)
    {
        player.m_animator.SetBool("bottle_select", false);

        targetTileName = targetTile;
        bottle = player.bottle.transform;
        bottleParent = bottle.transform.parent;
        bottleStartPosition = bottle.transform.localPosition;
        bottleStartRotation = bottle.transform.localRotation;
        bottle.transform.parent = null;

        var direction = (targetPosition - bottle.transform.position).normalized;
        var distance = Vector3.Distance(bottle.transform.position, targetPosition);
        var middlePoint = direction * distance / 2 + bottle.transform.position;

        middlePoint.y = 2.5f;
        
        var wayPoints = new List<Vector3>();
        wayPoints.Add(bottle.transform.position);
        wayPoints.Add(middlePoint);
        wayPoints.Add(targetPosition);
        segmentIndex = 0;
        linePointList = BezierUtils.GetBeizerPointList(150, wayPoints);
        player.bottleCount--;
    }

    private Player player
    {
        get
        {
            return character as Player;
        }
    }

    public override bool CheckComplete()
    {
        if (segmentIndex > linePointList.Length - 2)
        {
            Game.Instance.BottleThorwed(targetTileName);
            return true;
        }
        return false;
    }

    public override void Run()
    {
        bottle.transform.position = Vector3.Lerp(linePointList[segmentIndex], linePointList[segmentIndex + 1], 1);

        segmentIndex++;
        
        base.Run();
    }
}
