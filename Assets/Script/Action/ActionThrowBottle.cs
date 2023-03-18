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

    public Quaternion targetRotation;

    public ActionThrowBottle(Player player, string targetTile) : base(player, ActionType.ThrowBottle)
    {
        var boardNode = player.boardManager.FindNode(targetTile);
        _targetPositon = boardNode.transform.position;
        targetTileName = targetTile;

        targetRotation = Quaternion.LookRotation(boardNode.transform.position - player.transform.position);

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

    void Throw()
    {
        player.m_animator.SetInteger("bottle", 1);
        
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
        linePointList = BezierUtils.GetBeizerPointList(50, wayPoints);
        once = false;
    }

    private bool once = false;
    private float delayTime = 1;
    public override bool CheckComplete()
    {
        if(linePointList==null)return false;
        if (linePointList.Length <=0) return false;
        if (segmentIndex > linePointList.Length - 2)
        {
            if(!once)
            {
                once = true;
                delayTime = 0;
                bottle.parent = bottleParent;
                bottle.localPosition = bottleStartPosition;
                bottle.localRotation = bottleStartRotation;
                bottle.gameObject.SetActive(false);
                player.PlayBottleEffect(_targetPositon);
                var targetArray = targetTileName.Split('_');
                var x = int.Parse(targetArray[0]);
                var z = int.Parse(targetArray[1]);
                var targetTile = player.gridManager.GetTileByName(targetTileName);
                foreach (var enemy in player.boardManager.enemies)
                {
                    var coord = enemy.coord;
                    var distanceFromX = Mathf.Abs(x - coord.x);
                    var distanceFromZ = Mathf.Abs(z - coord.z);
                    if (distanceFromX <= 2 && distanceFromZ <= 2)
                    {
                        enemy.ShowTraceTarget(targetTile, enemy.hearSoundTile == null, 1);
                    }
                }
                AudioPlay.Instance.PlayerBottleGrounded();
            }
            delayTime += Time.deltaTime;
            if (once && delayTime >= 0.75f)
            {
                Game.Instance.BottleThorwed(targetTileName);
                return true;
            }
           
        }
        return false;
    }
    
    public override void Run()
    {

        if (!player.tr_body.transform.rotation.Equals(targetRotation))
        {
            var playerRotation = player.tr_body.transform.rotation;
            player.tr_body.transform.rotation = Quaternion.RotateTowards(playerRotation, targetRotation, 10);
            if (player.tr_body.transform.rotation.Equals(targetRotation))
            {
                Throw();
            }
            return;
        }

        if (segmentIndex >= 0 && segmentIndex < linePointList.Length - 1) 
        {
            bottle.transform.position = Vector3.Lerp(linePointList[segmentIndex], linePointList[segmentIndex + 1], 1);

            segmentIndex++;
        }
        base.Run();
    }
}
