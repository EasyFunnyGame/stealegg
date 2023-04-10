using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPincersCut : ActionBase
{
    private float actionDuration = 1;

    private PincersItem pincers;

    bool cutted = false;
    public Quaternion targetRotation;
    public ActionPincersCut(Player player, PincersItem item) : base(player, ActionType.PincersCut)
    {
        pincers = item;
        cutted = false;
        targetRotation = Quaternion.LookRotation(item.wireNetMesh.transform.position - item.transform.position);
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
           
            return true;
        }
        return false;
    }

    public override void Run()
    {

        if (!player.tr_body.transform.rotation.Equals(targetRotation))
        {
            var playerRotation = player.tr_body.transform.rotation;
            player.tr_body.transform.rotation = Quaternion.RotateTowards(playerRotation, targetRotation, 5);
            if (player.tr_body.transform.rotation.Equals(targetRotation))
            {
                if(!cutted)
                {
                    cutted = true;
                    Cut();
                }
            }
            return;
        }

        actionDuration -= Time.deltaTime;
        base.Run();
    }

    void Cut()
    {
        player.m_animator.SetTrigger("cut");
        AudioPlay.Instance.PlayPrincersCut();
        var boardManager = Game.Instance.boardManager;
        var nodes = boardManager.FindNodesAround(player.currentTile.name, 2);
        foreach (var kvp in nodes)
        {
            for (var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                if (enemy.coord.name == kvp.Key)
                {
                    //enemy.LureWhistle(player.currentTile.name);
                    enemy.ShowTraceTarget(player.currentTile);
                }
            }
        }
        pincers.Cut();
        player.PlayeWhitsleEffect(player.transform.position);
        pincers.picked = true;
        pincers.gameObject.SetActive(false);
        pincers.icon.gameObject.SetActive(false);
    }
}
