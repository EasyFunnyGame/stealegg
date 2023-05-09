using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TranslateCanvas : BaseCanvas
{
    public Image black;

    public bool toBlack = false;

    public bool toWhite = false;

    public string afterTranslate;

    float speed = 0.02f;

    // Update is called once per frame
    void Update()
    {
        if(toBlack)
        {
            var alpha = black.color.a;
            if(alpha<1)
            {
                alpha += speed;
                black.color = new Color(0, 0, 0, alpha);
                if(alpha >=1)
                {
                    onMiddle();
                    black.color = new Color(0, 0, 0, 1);
                    toWhite = true;
                    toBlack = false;
                }
            }
        }
        if(toWhite)
        {
            var alpha = black.color.a;
            if (alpha > 0)
            {
                alpha -= speed;
                black.color = new Color(0, 0, 0, alpha);
                if (alpha <= 0)
                {
                    black.color = new Color(0, 0, 0, 0);
                    onComplete();
                    toWhite = false;
                    toBlack = false;
                }
            }
        }
    }

    protected override void OnShow()
    {
        base.OnShow();
        toBlack = true;
        black.color = new Color(0, 0, 0, 0);
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    public void SetAfterTranslate(string callback)
    {
        afterTranslate = callback;
    }

    public void onMiddle()
    {
        if (afterTranslate == "steal")
        {
            Game.Instance.graffCanvas.Show();
            Game.Instance.gameCanvas.Hide();
        }
        else if (afterTranslate == "main")
        {
            Game.Instance.graffCanvas.Hide();
            Game.Instance.gameCanvas.Show();
            Game.Instance.gameCanvas.home.gameObject.SetActive(false);
            Game.Instance.gameCanvas.playing.gameObject.SetActive(true);

            Player player = Game.Instance.player;
            var stealAction = player.currentAction;
            if(stealAction.actionType == ActionType.Steal)
            {
                (stealAction as ActionSteal).ActionComplete();
            }
        }
    }


    public void onComplete()
    {
        if (afterTranslate == "steal")
        {
            
        }
        else if(afterTranslate == "main")
        {
            
            Game.Instance.playing = true;
            var player = Game.Instance.player;
            
            if (player != null)
            {
                player.PlayStealEffect(player.transform.position);
                var targetArray = player.coord.name.Split('_');
                var x = int.Parse(targetArray[0]);
                var z = int.Parse(targetArray[1]);
                foreach (var enemy in player.boardManager.enemies)
                {
                    var coord = enemy.coord;
                    var distanceFromX = Mathf.Abs(x - coord.x);
                    var distanceFromZ = Mathf.Abs(z - coord.z);
                    if (distanceFromX <= 2 && distanceFromZ <= 2)
                    {
                        Game.Instance.UpdateMoves(player.coord.name);
                        break;
                        //enemy.ShowTraceTarget(targetTileName);
                    }
                }
                player.justSteal = true;
            }
        }
        Hide();
    }
}
