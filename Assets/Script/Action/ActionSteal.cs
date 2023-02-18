using UnityEngine;

public class ActionSteal : ActionBase
{
    private float actionDuration = 1;
    public ActionSteal(Player player) : base(player, ActionType.Steal)
    {
        player.m_animator.SetTrigger("graff");
        player.PlayeWhitsleEffect();
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
            var playerTileName = player.currentTile.name;
            for (var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                var targetTile = enemy.gridManager.GetTileByName(playerTileName);
                if (targetTile)
                {
                    enemy.LureSteal(player.currentTile.name);
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
