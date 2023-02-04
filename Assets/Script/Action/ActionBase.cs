using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ActionBase
{
    public Character character;

    public ActionType actionType;

    public ActionBase(Character character, ActionType actionType)
    {
        this.character = character;
        this.actionType = actionType;
    }

    public virtual void Run()
    {
    }

    public virtual bool CheckComplete()
    {
        return true;
    }

}
