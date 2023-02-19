using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionFoundPlayer : ActionBase
{
    private float actionDuration = 0.5f;

    public ActionFoundPlayer(Enemy character) : base(character, ActionType.FoundPlayer)
    {
        actionDuration = 0;
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
