using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTurnDirection : ActionBase
{
    private Direction targetDirection;

    public ActionTurnDirection(Character character, Direction direciton) : base(character, ActionType.TurnDirection)
    {
        targetDirection = direciton;
        Utils.SetDirection(character, targetDirection);
    }

    public ActionTurnDirection(Character character,  string tileName) : base(character, ActionType.TurnDirection)
    {
        targetDirection = character.LookAt(tileName);
        Utils.SetDirection(character, targetDirection);
    }
    public Enemy enemy
    {
        get
        {
            return character as Enemy;
        }
    }
    public override bool CheckComplete()
    {
        if(character._direction == targetDirection)
        {
            enemy.Turned();
            return true;
        }
        return false;
    }

    public override void Run()
    {
        Vector3 tar_dir = character.db_moves[1].position - character.db_moves[0].position;
        Vector3 new_dir = Vector3.RotateTowards(character.tr_body.GetChild(0).forward, tar_dir, character.rotate_speed * Time.deltaTime / 2, 0f);
        new_dir.y = 0;
        character.tr_body.GetChild(0).transform.rotation = Quaternion.LookRotation(new_dir);

        var angle = Vector3.Angle(tar_dir, character.tr_body.GetChild(0).forward);
        if(angle < 1)
        {
            //character.transform.forward = tar_dir;
            character.body_looking = false;
            character.Turned();
        }
    }



}
