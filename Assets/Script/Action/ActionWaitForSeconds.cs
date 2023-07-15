using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionWaitForSeconds : ActionBase
{
    float timer = 0;

    public ActionWaitForSeconds(Character character, float seconds) : base(character, ActionType.WaitForSeconds)
    {
        timer = seconds;
    }

    public override bool CheckComplete()
    {
        if (timer<=0)
        {
            return true;
        }
        return false;
    }

    public override void Run()
    {
        timer -= Time.deltaTime;
    }
}
