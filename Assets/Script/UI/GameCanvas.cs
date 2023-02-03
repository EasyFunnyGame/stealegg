using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : BaseCanvas
{
    public RawImage img_level;

    public RawImage img_energy;

    public Button btn_add;

    public Button btn_home;

    public Button btn_reStart;

    public Button btn_hint;

    public Button btn_bottle;

    public Button btn_bottle_disable;

    public Button btn_whistle;

    public Button btn_whistle_disable;

    public Button btn_pause;

    public int index;

    private void Awake()
    {
        btn_add.onClick.AddListener(onClickShowEnergyGainCanvasHandler);
        btn_bottle.onClick.AddListener(onClickUseBottleHandler);
        btn_whistle.onClick.AddListener(onClickUseWhistleHandler);
        btn_pause.onClick.AddListener(onClickPauseGameHandler);
    }

    private void onClickShowEnergyGainCanvasHandler()
    {
        Game.Instance.energyGainCanvas.Show();
    }

    private void onClickUseBottleHandler()
    {

    }

    private void onClickUseWhistleHandler()
    {

    }

    private void onClickPauseGameHandler()
    {

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
        RefreshEnergy();
        img_level.texture = Resources.Load<Texture>("UI/Sprite/Num/" + (index+1).ToString());
    }

    protected override void OnHide()
    {

    }

    public void RefreshEnergy()
    {
        img_energy.texture = Resources.Load<Texture>("UI/Sprite/Num/" + Game.Instance.energy);
    }

    public void DisableWhistle()
    {
        btn_whistle.gameObject.SetActive(false);
        btn_whistle_disable.gameObject.SetActive(true);
    }

    public void EnableWhistle()
    {
        btn_whistle.gameObject.SetActive(true);
        btn_whistle_disable.gameObject.SetActive(false);
    }

    public void DisableBottle()
    {
        btn_bottle.gameObject.SetActive(false);
        btn_bottle_disable.gameObject.SetActive(true);
    }

    public void EnableBottle()
    {
        btn_bottle.gameObject.SetActive(true);
        btn_bottle_disable.gameObject.SetActive(false);
    }
}
