using UnityEngine;
using System.Collections.Generic;
public class EnemySentinel : Enemy
{
    public bool turn;

    public List<Direction> sentinelDirections;

    public int indexTurn = 1;

    public bool willTurn = false;

    const string Up = "Up";

    const string Down = "Down";

    const string Left = "Left";

    const string Right = "Right";

    public bool watching = true;

    // 顺时针
    const string CW = "CW";

    // 逆时针
    const string CCW = "CCW";

    public override void Start()
    {
        this.checkRange = 10;
        this.sleeping = false;
        this.patroling = false;
        base.Start();
    }

    public override void DoDefaultAction()
    {
        base.DoDefaultAction();
        if (sentinelDirections == null || sentinelDirections.Count < 1) return;
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

            if (indexTurn == 1)
            {
                ShowCCW();
            }
            else
            {
                ShowCW();
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
            willTurn = true;
        }
        else
        {
            // 执行转向动作
            currentAction = new ActionTurnDirection(this, targetDirection, true);
            AudioPlay.Instance.PlayWatchTurn();
            willTurn = false;
            HideSentinelTurn();
        }
    }

    public override void ReachedOriginal()
    {
        base.ReachedOriginal();
        HideSentinelTurn();
        watching = true;
        willTurn = false;
        this.checkRange = 10;
    }
}
