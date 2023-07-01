using UnityEngine;

public class ActionSteal : ActionBase
{
    private float actionDuration = 1;
    private GraffItem graffItem;

    public Quaternion targetRotation;

    public bool completed = false;

    public void ActionComplete()
    {
        completed = true;
        
      

        
    }

    public ActionSteal(Player player, GraffItem item) : base(player, ActionType.Steal)
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
        if (!completed && actionDuration < 0)
        {
            graffItem.picked = true;
            graffItem.gameObject.SetActive(false);
            graffItem.icon.gameObject.SetActive(false);

            if(Game.Instance && Game.Instance.draw_able)
            {
                Game.Instance.translateCanvas.Show();
                Game.Instance.translateCanvas.SetAfterTranslate("steal");
                Game.Instance.playing = false;
            }
           
            graffItem.gameObject.SetActive(false);
        }
        
        return completed;
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
