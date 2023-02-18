using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public Character owner;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FootL()
    {
        owner.FootL();
    }

    public void FootR()
    {
        owner.FootR();
    }

    public void PlayerReached()
    {
        owner.PlayerReached();
    }

    public void AnimationEnd(string clipName)
    {
        owner.AnimationEnd(clipName);
    }

    public void ReadyThrowBottle()
    {
        owner.ReadyThrowBottle();
    }

    public virtual void AfterStealVegetable()
    {
        owner.AfterStealVegetable();
    }

    public void Hit()
    {

    }
}
