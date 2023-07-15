using UnityEngine;
using System.Collections.Generic;
public class EnemySentinel : Enemy
{
    public bool turn;

    public List<Direction> sentinelDirections;

    public int indexTurn = 1;

    public bool willTurn = false;

    public Direction willWatchDirection = Direction.Down;

    const string Up = "Up";

    const string Down = "Down";

    const string Left = "Left";

    const string Right = "Right";

    // 顺时针
    const string CW = "CW";

    // 逆时针
    const string CCW = "CCW";

    bool showCCW = false;

    bool showCW = false;

    public override void Start()
    {
        this.checkRange = 10;
        this.watching = true;
        this.sleeping = false;
        this.patroling = false;
        base.Start();
        willWatchDirection = targetDirection;
    }

    public override void DoDefaultAction()
    {
        base.DoDefaultAction();
        if (sentinelDirections == null ) return;// || sentinelDirections.Count < 1 
        if (!willTurn)
        {
            var currentDirectionIndex = sentinelDirections.IndexOf(direction);
            if (currentDirectionIndex == 0)
            {
                indexTurn = 1;
            }
            else if (currentDirectionIndex == sentinelDirections.Count - 1)
            {
                indexTurn = -1;
            }
          

            var tryTurnDirectionIndex = currentDirectionIndex + indexTurn;
            if (tryTurnDirectionIndex < 0)
            {
                tryTurnDirectionIndex = sentinelDirections.Count - 1;
            }
            else if (tryTurnDirectionIndex >= sentinelDirections.Count)
            {
                tryTurnDirectionIndex = 0;
            }

            var tryTurnDirection = sentinelDirections[tryTurnDirectionIndex];
            targetDirection = tryTurnDirection;
            willWatchDirection = tryTurnDirection;
            willTurn = true;

            var angleIndex = targetDirection - _direction;
            if (angleIndex < -1)
            {
                angleIndex += 4;
            }
            if(angleIndex > 1)
            {
                angleIndex -= 4;
            }
            //Debug.Log("angle index :" + angleIndex + " targetDirection: " + targetDirection + " currentDirection: " + _direction);
            if (angleIndex == -1)
            {
                ShowCCW();
                showCCW = true;
                showCW = false;
            }
            else
            {
                ShowCW();
                showCCW = false;
                showCW = true;
            }
            currentAction = new ActionWaitForSeconds(this, 1f);
        }
        else
        {
            // 执行转向动作
            currentAction = new ActionTurnDirection(this, willWatchDirection, true);
            AudioPlay.Instance?.PlayWatchTurn();
            willTurn = false;
            HideSentinelTurn();
            showCCW = false;
            showCW = false;
        }
    }

    public override void ReachedOriginal()
    {
        base.ReachedOriginal();
        HideSentinelTurn();
        watching = true;
        checkRange = 10;

        if (targetDirection != willWatchDirection)
        {
            targetDirection = willWatchDirection;
        }
        if (showCW)
        {
            ShowCW();
        }
        else if(showCCW)
        {
            ShowCCW();
        }
    }


    public override void Turned()
    {
        base.Turned();
        UpdateRouteMark();
        //Debug.Log("转向完毕更新检测点");

        if (coord.name == originalCoord.name && _direction == originalDirection && !coordTracing.isLegal)
        {
            ReachedOriginal();
        }

        if( watching )
        {
            originalDirection = _direction;
        }
    }


    // 回去原点  是否完结此回合
    //public override bool GoBack()
    //{
    //    if (coord.name != originalCoord.name || _direction != originalDirection)
    //    {
    //        if (coord.name != originalCoord.name)
    //        {
    //            if (originalTile == null)
    //            {
    //                originalTile = gridManager.GetTileByName(originalCoord.name);
    //                FindPathRealTime(originalTile, null,false);
    //                currentAction = new ActionTurnDirection(this, nextTile.name, true);
    //                ShowBackToOriginal();
    //                return true;
    //            }
    //            else
    //            {
    //                var useFastest = ifGoBackUseFastestWay();
    //                currentAction = new ActionEnemyMove(this, originalTile,useFastest );
    //                return true;
    //            }
    //        }
    //        else
    //        {
    //            ShowBackToOriginal();
    //            targetDirection = originalDirection;
    //            currentAction = new ActionTurnDirection(this, originalDirection, true);
    //            return true;
    //        }
    //    }
    //    else
    //    {
    //        ReachedOriginal();
    //        return false;
    //    }
    //}
}
