using UnityEngine;

public class ActionSteal : ActionBase
{
    private float actionDuration = 1;
    private Item graffItem;

    public Quaternion targetRotation;

    public ActionSteal(Player player, Item item) : base(player, ActionType.Steal)
    {
        graffItem = item;
        
        targetRotation = player.transform.rotation;
        var draw = GameObject.Find("Draw");
        if(draw)
        {
            targetRotation = draw.transform.rotation;
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
            graffItem.picked = true;
            graffItem.gameObject.SetActive(false);
            graffItem.icon.gameObject.SetActive(false);

            if(Game.Instance.draw_able)
            {
                Game.Instance.translateCanvas.Show();
                Game.Instance.translateCanvas.SetAfterTranslate("steal");
                Game.Instance.playing = false;
            }
            else
            {
                player.PlayStealEffect(player.transform.position);

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
            }
            graffItem.gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    public override void Run()
    {
        if (!player.tr_body.transform.rotation.Equals(targetRotation))
        {
            var playerRotation = player.tr_body.transform.rotation;
            player.tr_body.transform.rotation = Quaternion.RotateTowards(playerRotation, targetRotation, 10);
            if (player.tr_body.transform.rotation.Equals(targetRotation))
            {
                player.m_animator.SetTrigger("pick");
            }
            return;
        }

        actionDuration -= Time.deltaTime;
        base.Run();
    }
}
