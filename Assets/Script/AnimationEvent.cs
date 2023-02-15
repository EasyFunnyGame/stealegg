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

    public virtual void FootR()
    {
        owner.FootR();
    }

    public virtual void PlayerReached()
    {
        owner.PlayerReached();
    }

    public virtual void PlayerWhitsleEnd()
    {
        owner.PlayerWhitsleEnd();
    }
}
