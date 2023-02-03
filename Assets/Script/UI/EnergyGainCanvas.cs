using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyGainCanvas : BaseCanvas
{
    public Button btn_cancel;

    public Button btn_watch;

    public Button btn_clz;

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
        var gain = 5;

        Game.Instance.energy += gain;
        btn_watch.enabled = false;
        Game.Instance.energyGainCanvas.Hide();
        Game.Instance.msgCanvas.PopMessage("获得" + gain + "点体力");
        Game.Instance.gameCanvas.RefreshEnergy();
    }

    void onClickCloseCanvasHandler()
    {
        Game.Instance.energyGainCanvas.Hide();
    }
}
