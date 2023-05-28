using UnityEngine;

public class ActionJumpManholeCover : ActionBase
{
    private float jumpInDelay = 0f;
    private float jumpOutDelay = 0f;


    private float jumpInCoverDelay = 0f;
    private float jumpOutCoverDelay = 0f;

    private ManholeCoverItem jumpOutCover;
    private ManholeCoverItem jumpInCover;
    public ActionJumpManholeCover(Player player, ManholeCoverItem item) : base(player, ActionType.ManHoleCover)
    {
        player.justJump = true;

        player.jumstJumpTileName = player.coord.name;

        jumpInDelay = 1.5f;

        jumpInCoverDelay = 0.1f;

        jumpOutCoverDelay = 1.0f;

        jumpOutCover = item;


        var manholeCover = player.boardManager.allItems[player.currentTile.name] as ManholeCoverItem;
        if(manholeCover && manholeCover.itemType == ItemType.ManHoleCover)
        {
            jumpInCover = manholeCover;
            player.m_animator.SetTrigger("jump_in");
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
            if (Game.teaching && Game.Instance.showingStep != null)
            {
                if (Game.Instance.showingStep.actionType == ActionType.ManHoleCover && Game.Instance.showingStep.tileName == jumpOutCover.coord.name)
                {
                    boardManager.steps.RemoveAt(0);
                    Game.Instance.ShowGuide();
                }
            }

            player.Reached();
            return true;
        }
        return false;
    }

    public override void Run()
    {
        if(jumpInCoverDelay>0)
        {
            jumpInCoverDelay -= Time.deltaTime;
            if(jumpInCoverDelay <=0)
            {
                jumpInCover.JumpIn();
            }
        }

        if (jumpOutCoverDelay > 0)
        {
            jumpOutCoverDelay -= Time.deltaTime;
            if (jumpOutCoverDelay <= 0)
            {
                jumpOutCover.JumpOut();
            }
        }

        if (jumpInDelay>0)
        {
            jumpInDelay -= Time.deltaTime;
            if (jumpInDelay < 0)
            {
                jumpOutDelay = 1.5f;
                player.m_animator.SetTrigger("jump_out");
                var boardManager = Game.Instance.boardManager;
                var tile = player.gridManager.GetTileByName(jumpOutCover.coord.name);
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
