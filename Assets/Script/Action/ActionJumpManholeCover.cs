using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionJumpManholeCover : ActionBase
{
    private float actionDuration = 1;
    private ManholeCoverItem manholecover;
    public ActionJumpManholeCover(Player player, ManholeCoverItem item) : base(player, ActionType.ManHoleCover)
    {
        manholecover = item;
    }
    public Player player
    {
        get
        {
            return character as Player;
        }
    }
    public override bool CheckComplete()
    {
        if (actionDuration < 0)
        {
            var boardManager = Game.Instance.boardManager;
            var nodes = boardManager.FindNodesAround(player.currentTile.name, 2);
            foreach (var kvp in nodes)
            {
                for (var index = 0; index < boardManager.enemies.Count; index++)
                {
                    var enemy = boardManager.enemies[index];
                    if (enemy.coord.name == kvp.Key)
                    {
                        enemy.LureWhistle(player.currentTile.name);
                    }
                }
            }
            manholecover.JumpOut();
            var tile = player.gridManager.GetTileByName(manholecover.coord.name);
            if(tile)
            {
                player.currentTile = tile;
                player.transform.position = tile.transform.position;
            }
            return true;
        }
        return false;
    }

    public override void Run()
    {
        actionDuration -= Time.deltaTime;
        base.Run();
    }

}
