using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatic : Enemy
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

    new protected void OnReached()
    {
        base.OnReached();
        var direction = this.direction;

        var xOffset = 0;
        var zOffset = 0;

        if (direction == Direction.Up)
        {
            zOffset = 1;
        }
        else if (direction == Direction.Down)
        {
            zOffset = -1;
        }
        else if (Direction.Left == Direction.Right)
        {
            xOffset = 1;
        }
        else if(Direction.Right == Direction.Left)
        {
            xOffset = -1;
        }

        var coordX = this.coord.x;
        var coordZ = this.coord.z;

        var nextCoordX = this.coord.x + xOffset;
        var nextCoordZ = this.coord.z + zOffset;
        var nextNodeName = string.Format("{0}_{1}", nextCoordX, nextCoordZ);

    }

    new protected void OnStartMove()
    {
        base.OnStartMove();
    }
}
