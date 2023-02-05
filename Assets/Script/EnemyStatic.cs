using UnityEngine;

public class EnemyStatic : Enemy
{
    public GameObject question;
    public GameObject back;
    public GameObject exclamation;
    public GameObject sleep;

    private void Awake()
    {
        base.Awake();
        question.gameObject.SetActive(false);
        back.gameObject.SetActive(false);
        exclamation.gameObject.SetActive(false);
        sleep.gameObject.SetActive(false);
    }


    public override void UpdateRouteMark()
    {
        base.UpdateRouteMark();
        for(var index=  0; index < redNodes.Count; index++)
        {
            DestroyImmediate(redNodes[index].gameObject);
        }
        redNodes.Clear();

        for (var index = 0; index < redLines.Count; index++)
        {
            DestroyImmediate(redLines[index].gameObject);
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

        RedLineByName(curNodeName, next1NodeName);
        RedLineByName(next1NodeName, next2NodeName);

        RedNodeByName(curNodeName);
        RedNodeByName(next1NodeName);
        RedNodeByName(next2NodeName);
    }

    //public override ActionBase CheckAction()
    //{
    //    var action = base.CheckAction();

    //    return action;
    //}
}
