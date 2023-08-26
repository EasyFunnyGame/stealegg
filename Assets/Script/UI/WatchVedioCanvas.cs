using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WatchVedioCanvas : BaseCanvas
{
    public Button btn_cancel;

    public Button btn_watch;

    public Button btn_clz;

    public TextMeshProUGUI txt_title;

    public TextMeshProUGUI txt_content;

    private Action m_callBack;

    private void Awake()
    {
        btn_cancel.onClick.AddListener(onClickCloseCanvasHandler);
        btn_clz.onClick.AddListener(onClickCloseCanvasHandler);
        btn_watch.onClick.AddListener(onClickWatchVedioHandler);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnShow()
    {
        btn_watch.enabled = true;

    }

    protected override void OnHide()
    {

    }

    void onClickWatchVedioHandler()
    {
        m_callBack?.Invoke();
        AudioPlay.Instance?.PlayClick();
        Game.Instance?.watchVedioCanvas.Hide();
    }

    void onClickCloseCanvasHandler()
    {
        Game.Instance?.watchVedioCanvas.Hide();
        AudioPlay.Instance?.PlayClick();
    }

    public void SetMessage(string title, string content, Action callBack)
    {
        txt_title.text = title;
        txt_content.text = content;
        m_callBack = callBack;

    }
}
