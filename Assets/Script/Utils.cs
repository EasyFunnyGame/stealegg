using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static bool SameDirectionWithLookingAt(string from, string to, Direction comparedDirection)
    {
        var fromArr = from.Split('_');
        var fromX = int.Parse(fromArr[0]);
        var fromZ = int.Parse(fromArr[1]);

        var toArr = to.Split('_');

        var toX = int.Parse(toArr[0]);
        var toZ = int.Parse(toArr[1]);

        if (fromX == toX && (fromZ - toZ >= 1))
        {
            return comparedDirection == Direction.Down;
        }
        else if (fromX == toX && (fromZ - toZ <= -1))
        {
            return comparedDirection == Direction.Up;
        }
        else if (fromZ == toZ && (fromX - toX >= 1))
        {
            return comparedDirection == Direction.Left;
        }
        else if (fromZ == toZ && (fromX - toX <= -1))
        {
            return comparedDirection == Direction.Right;
        }

        return false;
    }

    public static Direction DirectionTo(string from, string to, Direction defaultDir)
    {
        var fromArr = from.Split('_');
        var fromX = int.Parse(fromArr[0]);
        var fromZ = int.Parse(fromArr[1]);

        var toArr = to.Split('_');

        var toX = int.Parse(toArr[0]);
        var toZ = int.Parse(toArr[1]);

        Direction dir = defaultDir;

        if (fromX == toX && (fromZ - toZ == 1))
        {
            return Direction.Down;
        }
        else if (fromX == toX && (fromZ - toZ == -1))
        {
            return Direction.Up;
        }
        else if (fromZ == toZ && (fromX - toX == 1))
        {
            return Direction.Left;
        }
        else if (fromZ == toZ && (fromX - toX == -1))
        {
            return Direction.Right;
        }
        return dir;
    }

    public static Direction DirectionToMultyGrid(string from, string to, Direction defaultDir)
    {
        var fromArr = from.Split('_');
        var fromX = int.Parse(fromArr[0]);
        var fromZ = int.Parse(fromArr[1]);

        var toArr = to.Split('_');

        var toX = int.Parse(toArr[0]);
        var toZ = int.Parse(toArr[1]);

        Direction dir = defaultDir;

        if (fromX == toX && (fromZ - toZ >= 1))
        {
            return Direction.Down;
        }
        else if (fromX == toX && (fromZ - toZ <= -1))
        {
            return Direction.Up;
        }
        else if (fromZ == toZ && (fromX - toX >= 1))
        {
            return Direction.Left;
        }
        else if (fromZ == toZ && (fromX - toX <= -1))
        {
            return Direction.Right;
        }
        return dir;
    }

    public static Direction DirectionTo(GridTile from , GridTile to, Direction defaultDir)
    {
        var fromArr = from.name.Split('_');
        var fromX = int.Parse(fromArr[0]);
        var fromZ = int.Parse(fromArr[1]);

        var toArr = to.name.Split('_');

        var toX = int.Parse(toArr[0]);
        var toZ = int.Parse(toArr[1]);

        Direction dir = defaultDir;

        if (fromX == toX && (fromZ - toZ == 1))
        {
            return Direction.Down;
        }
        else if (fromX == toX && (fromZ - toZ == -1))
        {
            return Direction.Up;
        }
        else if (fromZ == toZ && (fromX - toX == 1))
        {
            return Direction.Left;
        }
        else if (fromZ == toZ && (fromX - toX == -1))
        {
            return Direction.Right;
        }
        return dir;
    }

    public static void SetDirection(Character character, Direction targetDirection)
    {
        character.body_looking = true;
        character.db_moves[1].parent = null;
        if (targetDirection == Direction.Up)
        {
            character.db_moves[1].position = character.db_moves[0].position + new Vector3(0, 0, 1);
        }
        else if (targetDirection == Direction.Left)
        {
            character.db_moves[1].position = character.db_moves[0].position + new Vector3(-1, 0, 0);
        }
        else if (targetDirection == Direction.Right)
        {
            character.db_moves[1].position = character.db_moves[0].position + new Vector3(1, 0, 0);
        }
        else if (targetDirection == Direction.Down)
        {
            character.db_moves[1].position = character.db_moves[0].position + new Vector3(0, 0, -1);
        }
    }

    public static ActionBase CreatePlayerAction(ActionType actionType, GridTile tile)
    {
        if (!Game.Instance) return null;
        var player = Game.Instance.player;
        if (player == null) return null;

        if ( Game.teaching)
        {
            var rightStep = false;
            var steps = Game.Instance.boardManager.steps;
            if (steps.Count>0)
            {
                var currentStep = Game.Instance.boardManager.steps[0];
                if(currentStep.actionType == actionType)
                {
                    if(currentStep.tileName == tile.name)
                    {
                        rightStep = true;
                        Game.Instance?.boardManager.steps.RemoveAt(0);
                    }
                }
            }
            if(rightStep==false)
            {
                Game.Instance?.msgCanvas.PopMessage("请按照步骤进行");
                return null;
            }
        }
        Game.Instance?.HideGuide();
        switch(actionType)
        {
            case ActionType.PlayerMove:
                return new ActionPlayerMove(player, tile);
            case ActionType.Steal:
                return new ActionSteal(player, player.boardManager.allItems[player.currentTile.name] as GraffItem);
            case ActionType.ThrowBottle:
                return new ActionThrowBottle(player, tile.name);
            case ActionType.BlowWhistle:
                return new ActionBlowWhistle(player);
        }
        return null;
    }
}
