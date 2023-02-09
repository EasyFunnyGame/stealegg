using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionFoundPlayer : ActionBase
{
    private float actionDuration = 1;

    public ActionFoundPlayer(Enemy character, ActionType actionType) : base(character, actionType)
    {
        actionDuration = 1;
        character.m_animator.Play("Enemy_Alert");
    }

    public override bool CheckComplete()
    {
        if(actionDuration < 0)
        {
            return true;
        }
        return false;
    }

    public override void Run()
    {
        actionDuration -= Time.deltaTime;
        base.Run();
    }
}
