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

                var playerTileName = Game.Instance.player.currentTile.name;
                var item = Game.Instance.boardManager.allItems[playerTileName];
                if (item.itemType == ItemType.Graff)
                {
                    var boardManager = Game.Instance.boardManager;

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

                var items = player.boardManager.allItems;
                foreach (var kvp in items)
                {
                    var endItem = kvp.Value;
                    if (endItem.itemType == ItemType.End)
                    {
                        var notActive = endItem.transform.Find("Exit_not_active");
                        var active = endItem.transform.Find("Exit_active");
                        notActive.gameObject.SetActive(false);
                        active.gameObject.SetActive(true);
                        break;
                    }
                }

            }
        }
        Hide();
    }
}
