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

    protected override void OnReached()
    {
        base.OnReached();

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
        else if (direction == Direction.Right)
        {
            xOffset = 1;
        }
        else if(direction == Direction.Left)
        {
            xOffset = -1;
        }

        var curNodeName = tile_s.gameObject.name;

        var next1CoordX = coord.x + xOffset;
        var next1CoordZ = coord.z + zOffset;
        var next1NodeName = string.Format("{0}_{1}", next1CoordX, next1CoordZ);

        var next2CoordX = next1CoordX + xOffset;
        var next2CoordZ = next1CoordZ + zOffset;
        var next2NodeName = string.Format("{0}_{1}", next2CoordX, next2CoordZ);

        var linkLine1 = boardManager.FindLine(curNodeName, next1NodeName);

        boardManager.RedLine(linkLine1);

        var linkLine2 = boardManager.FindLine(next1NodeName, next2NodeName);

        boardManager.RedLine(linkLine2);

        var node1 = boardManager.FindNode(next1NodeName);

        boardManager.RedNode(node1);

        var node2 = boardManager.FindNode(next2NodeName);

        boardManager.RedNode(node2);

        UpdateGuardNode();

        //Debug.Log("Eeney Static Guard Nodes " + linkLine1.name + "   " + linkLine2.name);
        //Debug.Log("Eeney Static Guard Nodes " + node1.name + "   " + node2.name);
    }

    protected override void OnStartMove()
    {
        base.OnStartMove();
    }

    private void UpdateGuardNode()
    {

    }
}
