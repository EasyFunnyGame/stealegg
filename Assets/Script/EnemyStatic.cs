using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatic : Enemy
{
    public Animator animator;


    public List<MeshRenderer> redLines = new List<MeshRenderer>();

    public List<MeshRenderer> redNodes = new List<MeshRenderer>();

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    protected override void OnReached()
    {
        base.OnReached();
        UpdateRouteMark();
        hasAction = false;
        //Debug.Log("Eeney Static Guard Nodes " + linkLine1.name + "   " + linkLine2.name);
        //Debug.Log("Eeney Static Guard Nodes " + node1.name + "   " + node2.name);
    }


    protected override void OnStartMove()
    {
        base.OnStartMove();
    }

    public override void OnDirectionRested()
    {
        base.OnDirectionRested();
        UpdateRouteMark();
        TryCatch();
    }

    public override void TryCatch()
    {
        base.TryCatch();
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
        else if (direction == Direction.Left)
        {
            xOffset = -1;
        }
        var next1CoordX = coord.x + xOffset;
        var next1CoordZ = coord.z + zOffset;
        var next1NodeName = string.Format("{0}_{1}", next1CoordX, next1CoordZ);
        if(Player.Instance.tile_s.name == next1NodeName)
        {
            Game.Instance.status = GameStatus.FAIL;
        }
    }

    public override void TryTrace()
    {
        base.TryTrace();
    }

    public override void UpdateRouteMark()
    {
        base.UpdateRouteMark();


        for(var index=  0; index < redNodes.Count; index++)
        {
            Destroy(redNodes[index].gameObject);
        }
        redNodes.Clear();

        for (var index = 0; index < redLines.Count; index++)
        {
            Destroy(redLines[index].gameObject);
        }
        redLines.Clear();

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
        else if (direction == Direction.Left)
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

        if (linkLine1 != null)
        {
            var lineTr = linkLine1.transform.GetChild(0);
            var copyLine = Instantiate(lineTr.GetComponent<MeshRenderer>());
            copyLine.transform.position = lineTr.position;
            copyLine.transform.rotation = lineTr.rotation;
            redLines.Add(copyLine);
            copyLine.material = Resources.Load<Material>("Material/UI_Red_Mat");
        }

        var linkLine2 = boardManager.FindLine(next1NodeName, next2NodeName);

        if (linkLine2 != null)
        {
            var lineTr = linkLine2.transform.GetChild(0);
            var copyLine = Instantiate(lineTr.GetComponent<MeshRenderer>());
            copyLine.transform.position = lineTr.position;
            copyLine.transform.rotation = lineTr.rotation;
            redLines.Add(copyLine);
            copyLine.material = Resources.Load<Material>("Material/UI_Red_Mat");
        }

        var node0 = boardManager.FindNode(curNodeName);

        if (node0 != null)
        {
            var nodeTr = node0.targetIcon;
            var copyNode = Instantiate(nodeTr.GetComponent<MeshRenderer>());
            copyNode.transform.position = nodeTr.transform.position;
            copyNode.transform.rotation = nodeTr.transform.rotation;
            redNodes.Add(copyNode);
            copyNode.material = Resources.Load<Material>("Material/UI_Red_Mat");
        }

        var node1 = boardManager.FindNode(next1NodeName);

        if (node1 != null)
        {
            var nodeTr = node1.targetIcon;
            var copyNode = Instantiate(nodeTr.GetComponent<MeshRenderer>());
            copyNode.transform.position = nodeTr.transform.position;
            copyNode.transform.rotation = nodeTr.transform.rotation;
            redNodes.Add(copyNode);
            copyNode.material = Resources.Load<Material>("Material/UI_Red_Mat");
        }

        var node2 = boardManager.FindNode(next2NodeName);

        if (node2 != null)
        {
            var nodeTr = node2.targetIcon;
            var copyNode = Instantiate(nodeTr.GetComponent<MeshRenderer>());
            redNodes.Add(copyNode);
            copyNode.transform.position = nodeTr.transform.position;
            copyNode.transform.rotation = nodeTr.transform.rotation;
            copyNode.material = Resources.Load<Material>("Material/UI_Red_Mat");
        }
    }
}
