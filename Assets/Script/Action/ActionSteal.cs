using UnityEngine;

public class ActionSteal : ActionBase
{
    private float actionDuration = 1;
    public ActionSteal(Player player) : base(player, ActionType.Steal)
    {
        player.m_animator.SetTrigger("graff");
        player.PlayStealEffect();
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

            var targetArray = playerTileName.Split('_');
            var x = int.Parse(targetArray[0]);
            var z = int.Parse(targetArray[1]);
            
            foreach (var enemy in boardManager.enemies)
            {
                var coord = enemy.coord;
                var distanceFromX = Mathf.Abs(x - coord.x);
                var distanceFromZ = Mathf.Abs(z - coord.z);
                if (distanceFromX <= 2 && distanceFromZ <= 2)
                {
                    enemy.LureSteal(playerTileName);
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
