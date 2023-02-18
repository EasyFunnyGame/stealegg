using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBlowWhistle : ActionBase
{
    private float actionDuration = 1;
    public ActionBlowWhistle(Player player) : base(player, ActionType.BlowWhistle)
    {
        player.PlayWhitsle();
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
