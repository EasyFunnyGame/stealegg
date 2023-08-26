using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarCanvas : BaseCanvas
{
    public Button btn_clz;


    private void Awake()
    {
        btn_clz.onClick.AddListener(() => { Hide(); AudioPlay.Instance?.PlayClick(); });
    }



    protected override void OnShow()
    {

    }

    protected override void OnHide()
    {

    }
}
