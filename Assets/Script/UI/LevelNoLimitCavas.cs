using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelNoLimitCavas : BaseCanvas
{
    public Button btn_sure;

    public Button btn_cancel;

    public Button btn_clz;

    public Text txt_current;

    public Text txt_total;
    void Start()
    {
        btn_sure.onClick.AddListener(onWatchVideoHandler);
        btn_cancel.onClick.AddListener(onCloseCanvasHandler);
        btn_clz.onClick.AddListener(onHideCanvasHandler);
    }

    void onHideCanvasHandler()
    {
        Hide();
        AudioPlay.Instance.PlayClick();
    }

    void onCloseCanvasHandler()
    {
        Hide();
        AudioPlay.Instance.PlayClick();
    }

    void onWatchVideoHandler()
    {
        if (!Game.Instance) return;

        if (!Game.Instance) return;
        var date = DateTime.Now.ToShortDateString();
        var cnt = PlayerPrefs.GetInt(UserDataKey.TodayFreeLevelCnt);
        PlayerPrefs.SetInt(UserDataKey.TodayFreeLevelCnt, cnt + 1);
        PlayerPrefs.SetString(UserDataKey.FreeLevelDay, date);
        PlayerPrefs.Save();
        Hide();
        AudioPlay.Instance.PlayClick();
    }
    // Update is called once per frame
    void Update()
    {

    }
    protected override void OnShow()
    {
        var date = DateTime.Now.ToShortDateString();
        var saveDate = PlayerPrefs.GetString(UserDataKey.FreeLevelDay);
        if (date != saveDate)
        {
            PlayerPrefs.SetInt(UserDataKey.TodayFreeLevelCnt, 0);
        }
        var cnt = PlayerPrefs.GetInt(UserDataKey.TodayFreeLevelCnt);
        txt_current.text = cnt.ToString();
        txt_total.text = "3";
    }

    protected override void OnHide()
    {

    }
}
