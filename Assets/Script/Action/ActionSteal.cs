using UnityEngine;

public class ActionSteal : ActionBase
{
    private float actionDuration = 2;
    private Item graffItem;
    public ActionSteal(Player player, Item item) : base(player, ActionType.Steal)
    {
        graffItem = item;
        player.m_animator.SetTrigger("pick");
        player.PlayStealEffect(player.transform.position);
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

            graffItem.picked = true;
            graffItem.gameObject.SetActive(false);
            graffItem.icon.gameObject.SetActive(false);

            Game.Instance.graffCanvas.Show();
            Game.Instance.gameCanvas.Hide();
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
