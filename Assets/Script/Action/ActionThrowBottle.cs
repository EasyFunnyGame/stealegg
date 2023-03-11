using System.Collections;
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

    public Vector3 _targetPositon;

    public ActionThrowBottle(Player player, string targetTile) : base(player, ActionType.ThrowBottle)
    {
        player.m_animator.SetInteger("bottle", 1);
        var boardNode = player.boardManager.FindNode(targetTile);
        _targetPositon = boardNode.transform.position;
        targetTileName = targetTile;
        bottle = player.bottle.transform;
        bottleParent = bottle.transform.parent;
        bottleStartPosition = bottle.transform.localPosition;
        bottleStartRotation = bottle.transform.localRotation;
        bottle.transform.parent = null;

        var direction = (_targetPositon - bottle.transform.position).normalized;
        var distance = Vector3.Distance(bottle.transform.position, _targetPositon);
        var middlePoint = direction * distance / 2 + bottle.transform.position;

        middlePoint.y = 3.5f;
        
        var wayPoints = new List<Vector3>();
        wayPoints.Add(bottle.transform.position);
        wayPoints.Add(middlePoint);
        wayPoints.Add(_targetPositon);
        segmentIndex = 0;
        linePointList = BezierUtils.GetBeizerPointList(100, wayPoints);
        player.bottleCount--;
        AudioPlay.Instance.PlayerThrowBottle();
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
            bottle.parent = bottleParent;
            bottle.localPosition = bottleStartPosition;
            bottle.localRotation = bottleStartRotation;
            bottle.gameObject.SetActive(false);
            player.PlayBottleEffect(_targetPositon);
            Game.Instance.BottleThorwed(targetTileName);
            AudioPlay.Instance.PlayerBottleGrounded();
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
