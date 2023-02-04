using UnityEngine;

public class ActionCatchPlayer : ActionBase
{
    private float actionDuration = 1;

    public ActionCatchPlayer(Enemy character, ActionType actionType) : base(character, actionType)
    {
        actionDuration = 1;
        character.animator.Play("Enemy_Caught");
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
