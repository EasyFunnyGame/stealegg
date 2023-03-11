
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public Character owner;

    public Item item;

    public void ManHoleCoverOpen()
    {
        if (item == null) return;
        var manHoleCover = item as ManholeCoverItem;
        if(manHoleCover == null) return;
        manHoleCover.OpenSound();
    }

    public void ManHoleCoverClose()
    {
        if (item == null) return;
        var manHoleCover = item as ManholeCoverItem;
        if (manHoleCover == null) return;
        manHoleCover.CloseSound();
    }

    public void AnimationEnd(string clipName)
    {
        owner.AnimationEnd(clipName);
    }
    public void PlayerMoveEnd()
    {
        owner?.PlayerReached();
    }

    public void PlayerFootRight()
    {
        if (!owner) return;
        if(owner is Player)
        {
            AudioPlay.Instance.PlayerFootRight();
        }
        else if(owner is EnemyStatic)
        {

        }
        else if (owner is EnemyDistracted)
        {

        }
        else if (owner is EnemyPatrol)
        {

        }
        else if (owner is EnemySentinel)
        {

        }
    }

    public void PlayerFootLeft()
    {
        if (!owner) return;
        if (owner is Player)
        {
            AudioPlay.Instance.PlayerFootLeft();
        }
        else if (owner is EnemyStatic)
        {

        }
        else if (owner is EnemyDistracted)
        {

        }
        else if (owner is EnemyPatrol)
        {

        }
        else if (owner is EnemySentinel)
        {

        }
    }

    public void PlayerWalkingExit()
    {
        AudioPlay.Instance.PlaySFX(12);
    }

    public void Pick()
    {
        AudioPlay.Instance.PlayPickSfx();
    }

    public void BlowWhitsle()
    {
        AudioPlay.Instance.PlayerBlowWhitsle();
    }
}
