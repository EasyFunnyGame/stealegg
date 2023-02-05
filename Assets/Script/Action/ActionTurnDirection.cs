using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTurnDirection : ActionBase
{
    private Direction targetDirection;
    public ActionTurnDirection(Character character,  Direction direction) : base(character, ActionType.TurnDirection)
    {
        targetDirection = direction;
        Utils.SetDirection(character, targetDirection);
    }

    public override bool CheckComplete()
    {
        if(character.direction == targetDirection)
        {
            return true;
        }
        return false;
    }
    public override void Run()
    {
        Vector3 tar_dir = character.db_moves[1].position - character.tr_body.position;
        Vector3 new_dir = Vector3.RotateTowards(character.tr_body.forward, tar_dir, character.rotate_speed * Time.deltaTime / 2, 0f);
        new_dir.y = 0;
        character.tr_body.transform.rotation = Quaternion.LookRotation(new_dir);

        var angle = Vector3.Angle(tar_dir, character.tr_body.forward);
        if(angle < 1)
        {
            character.ResetDirection();
            character.body_looking = false;
        }
    }



}
