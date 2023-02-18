using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionGraff : ActionBase
{
    private float actionDuration = 1;
    public ActionGraff(Player player) : base(player, ActionType.Steal)
    {
        
    }
    public override bool CheckComplete()
    {
        if (actionDuration < 0)
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
