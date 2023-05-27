
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public Character owner;

    public Item item;

    public void EnemySleep()
    {
        if (owner != null)
        {
            AudioPlay.Instance.Speep(owner as EnemyDistracted);
        }
    }

    public void EnemySleepOut()
    {
        AudioPlay.Instance.EnemySleepOut();
    }

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

    public void FootRight()
    {
        if (!owner) return;
        if(owner is Player)
        {
            AudioPlay.Instance.PlayerFootRight();
        }
        else
        {
            AudioPlay.Instance.EnemyFootRight(owner as Enemy);
        }
        
    }

    public void FootLeft()
    {
        if (!owner) return;
        if (owner is Player)
        {
            AudioPlay.Instance.PlayerFootLeft();
        }
        else
        {
            AudioPlay.Instance.EnemyFootLeft(owner as Enemy);
        }

    }

    public void PlayerWalkingExit()
    {
        AudioPlay.Instance.PlaySFX(12);
    }

    public void Pick()
    {
        owner?.Pick();
    }

    public void BlowWhitsle()
    {
        AudioPlay.Instance.PlayerBlowWhitsle();
    }

    public void Lure()
    {
        owner?.Lure();

    }

    public void StopLookAround()
    {
        owner?.StopLookAround();
    }
}
