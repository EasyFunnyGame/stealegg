using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{

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
}
