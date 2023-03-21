using UnityEngine;

public class ActionJumpManholeCover : ActionBase
{
    private float jumpInDelay = 0f;
    private float jumpOutDelay = 0f;
    private ManholeCoverItem manholecover;
    public ActionJumpManholeCover(Player player, ManholeCoverItem item) : base(player, ActionType.ManHoleCover)
    {
        jumpInDelay = 0.8f;
        manholecover = item;
        var manholeCover = player.boardManager.allItems[player.currentTile.name];
        if(manholeCover && manholeCover.itemType == ItemType.ManHoleCover)
        {
            (manholeCover as ManholeCoverItem).JumpIn();
            player.m_animator.SetBool("jumping", true);
        }
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
        if (jumpInDelay <= 0 && jumpOutDelay<= 0)
        {
            var boardManager = Game.Instance.boardManager;
            //var tile = player.gridManager.GetTileByName(manholecover.coord.name);
            //if(tile)
            //{
            //    player.currentTile = tile;
            //    player.transform.position = tile.transform.position;
            //}
            if (Game.teaching && Game.Instance.showingStep != null)
            {
                if (Game.Instance.showingStep.actionType == ActionType.ManHoleCover && Game.Instance.showingStep.tileName == manholecover.coord.name)
                {
                    boardManager.steps.RemoveAt(0);
                    Game.Instance.ShowGuide();
                }
            }
            return true;
        }
        return false;
    }

    public override void Run()
    {
        if(jumpInDelay>0)
        {
            jumpInDelay -= Time.deltaTime;
            if (jumpInDelay < 0)
            {
                jumpOutDelay = 2f;
                manholecover.JumpOut();
                player.m_animator.SetBool("jumping", false);
                var boardManager = Game.Instance.boardManager;
                var tile = player.gridManager.GetTileByName(manholecover.coord.name);
                if (tile)
                {
                    player.currentTile = tile;
                    player.transform.position = tile.transform.position;
                }
            }
        }

        if(jumpOutDelay>0)
        {
            jumpOutDelay -= Time.deltaTime;
        }
        
        
        base.Run();
    }



}
