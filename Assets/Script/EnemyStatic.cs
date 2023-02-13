using UnityEngine;

public class EnemyStatic : Enemy
{
    public override void UpdateRouteMark()
    {
        base.UpdateRouteMark();
        for(var index=  0; index < redNodes.Count; index++)
        {
            DestroyImmediate(redNodes[index].gameObject);
        }
        redNodes.Clear();

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

        var curNodeName = currentTile.gameObject.name;

        var next1CoordX = coord.x + xOffset;
        var next1CoordZ = coord.z + zOffset;
        var next1NodeName = string.Format("{0}_{1}", next1CoordX, next1CoordZ);

        var next2CoordX = next1CoordX + xOffset;
        var next2CoordZ = next1CoordZ + zOffset;
        var next2NodeName = string.Format("{0}_{1}", next2CoordX, next2CoordZ);

        routeNode1Name = next1NodeName;
        routeNode2Name = next2NodeName;

        RedNodeByName(curNodeName);
        RedNodeByName(next1NodeName);
        RedNodeByName(next2NodeName);
    }

    public override void OnReachedOriginal()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        m_animator.Play("Player_Idle");
    }
}
