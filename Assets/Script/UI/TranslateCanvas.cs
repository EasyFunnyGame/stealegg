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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(toBlack)
        {
            var alpha = black.color.a;
            if(alpha<1)
            {
                alpha += 0.05f;
                black.color = new Color(0, 0, 0, alpha);
                if(alpha >=1)
                {
                    onComplete();
                    //toWhite = true;
                    toBlack = false;
                }
            }
        }
        //if(toWhite)
        //{
        //    var alpha = black.color.a;
        //    if (alpha > 0)
        //    {
        //        alpha -= 0.1f;
        //        black.color = new Color(0, 0, 0, alpha);
        //        if (alpha <= 0)
        //        {
        //            onComplete();
        //        }
        //    }
        //}
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


    public void onComplete()
    {
        if (afterTranslate == "steal")
        {
            Game.Instance.graffCanvas.Show();
            Game.Instance.gameCanvas.Hide();
        }
        Hide();
    }
}
