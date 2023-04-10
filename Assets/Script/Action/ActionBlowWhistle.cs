using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBlowWhistle : ActionBase
{
    private float actionDuration = 1;
    public ActionBlowWhistle(Player player) : base(player, ActionType.BlowWhistle)
    {
        player.PlayWhitsle();
        var boardManager = Game.Instance.boardManager;
        var nodes = boardManager.FindNodesAround(player.currentTile.name, 2);
        var targetTile = player.currentTile;
        foreach (var kvp in nodes)
        {
            for (var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                if (enemy.coord.name == kvp.Key)
                {
                    enemy.ShowTraceTarget(targetTile);
                }
            }
        }
    }

    private Player player
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
            Game.Instance.boardManager.coordWhitsle = player.coord.Clone();
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
