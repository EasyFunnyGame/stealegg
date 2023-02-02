using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDistracted : Enemy
{
    public Animator animator;
    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    override protected void OnReached()
    {
        base.OnReached();
    }

    override protected void OnStartMove()
    {
        base.OnStartMove();
    }
}
