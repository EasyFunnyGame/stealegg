﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameCanvas : BaseCanvas
{
    public int index;

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

    public GameObject home;

    public GameObject playing;

    public Button btn_start;

    public ItemIconOnUI icon_graff;

    public ItemIconOnUI icon_star;

    public ItemIconOnUI icon_template_bottle;

    public ItemIconOnUI icon_template_pricers;

    public ItemIconOnUI icon_template_manholecover;

    public ItemIconOnUI icon_template_growth;

    public List<ItemIconOnUI> icon_bottles;

    public List<ItemIconOnUI> icon_pincers;

    public List<ItemIconOnUI> icon_manholecover;

    public List<ItemIconOnUI> icon_growth;

    public List<IconsAboveEnemy> icon_enemies;


    public Image distance_up;
    public Text txt_up;
    public Image distance_down;
    public Text txt_down;
    public Image distance_left;
    public Text txt_left;
    public Image distance_right;
    public Text txt_right;


    public Image playerPosition;


    private void Awake()
    {
        btn_add.onClick.AddListener(onClickShowEnergyGainCanvasHandler);
        btn_bottle.onClick.AddListener(onClickUseBottleHandler);
        btn_whistle.onClick.AddListener(onClickUseWhistleHandler);

        btn_home.onClick.AddListener(onClickBackToHomeHandler);
        btn_reStart.onClick.AddListener(onClickReStartLevelHandler);
        btn_pause.onClick.AddListener(onClickPasueGameHandler);

        btn_start.onClick.AddListener(onClickStartPlayingGameHandler);

        icon_graff.gameObject.SetActive(false);

        icon_star.gameObject.SetActive(false);

        icon_template_bottle.gameObject.SetActive(false);

        icon_template_pricers.gameObject.SetActive(false);

        icon_template_manholecover.gameObject.SetActive(false);

        icon_template_growth.gameObject.SetActive(false);
    }

    void onClickStartPlayingGameHandler()
    {
        home.gameObject.SetActive(false);
        playing.gameObject.SetActive(true);
        Game.Instance.playing = true;
    }

    private void onClickPasueGameHandler()
    {
        playing.gameObject.SetActive(false);
        home.gameObject.SetActive(true);
    }

    void onClickReStartLevelHandler()
    {
        Game.Instance.gameCanvas.Hide();
        SceneManager.LoadScene(Game.Instance.currentLevelName);
        Game.Instance.playing = false;
    }

    void onClickBackToHomeHandler()
    {
        Game.Instance.gameCanvas.Hide();
        SceneManager.LoadScene("Main");
        Game.Instance.playing = false;
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
        Game.Instance.UseWhistle();
    }

    private void onClickPauseGameHandler()
    {
        Game.Instance.UseBottle();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    Vector3 screenPoint = new Vector3();

    // Update is called once per frame
    void Update()
    {
        if (Game.Instance.player != null)
        {
            UiUtils.WorldToScreenPoint(Game.Instance.camera.m_camera, this, Game.Instance.player.transform.position, out screenPoint);
            playerPosition.rectTransform.anchoredPosition = screenPoint;
            Game.Instance.camera.UpdatePlayerPositionOnScreen(GetComponent<RectTransform>(), screenPoint, playerPosition);
        }

        if (icon_star.gameObject.activeSelf)
        {
            UiUtils.WorldToScreenPoint(Game.Instance.camera.m_camera, this, icon_star.item.GetIconPosition(),  out screenPoint);
            icon_star.rectTransform.anchoredPosition = screenPoint;
        }

        if (icon_graff.gameObject.activeSelf)
        {
            UiUtils.WorldToScreenPoint(Game.Instance.camera.m_camera, this, icon_graff.item.GetIconPosition(), out screenPoint);
            icon_graff.rectTransform.anchoredPosition = screenPoint;
        }
    }

    void LateUpdate()
    {
        if (!Game.Instance.camera) return;
        distance_up.rectTransform.sizeDelta = new Vector2(2, Math.Abs(Game.Instance.camera.playerPaddingUp));
        txt_up.text = Math.Abs(Game.Instance.camera.playerPaddingUp).ToString();

        distance_down.rectTransform.sizeDelta = new Vector2(2, Math.Abs(Game.Instance.camera.playerPaddingDown));
        txt_down.text = Math.Abs(Game.Instance.camera.playerPaddingDown).ToString();

        distance_left.rectTransform.sizeDelta = new Vector2(Math.Abs(Game.Instance.camera.playerPaddingLeft), 2);
        txt_left.text = Math.Abs(Game.Instance.camera.playerPaddingLeft).ToString();

        distance_right.rectTransform.sizeDelta = new Vector2(Math.Abs(Game.Instance.camera.playerPaddingRight), 2);
        txt_right.text = Math.Abs(Game.Instance.camera.playerPaddingRight).ToString();
    }


    protected override void OnShow()
    {
        RefreshEnergy();
        img_level.texture = Resources.Load<Texture>("UI/Sprite/Num/" + (index+1).ToString());

        playing.gameObject.SetActive(false);
        home.gameObject.SetActive(true);
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

    public void InitWithBoardManager(BoardManager boardManager)
    {
        for (var index = 0; index < boardManager.itemRoot.childCount; index++)
        {
            var itemTr = boardManager.itemRoot.GetChild(index);
            var item = itemTr.GetComponent<Item>();
            if (item == null)
            {
                Debug.Log(string.Format("未挂载脚本Item{0}", itemTr.name));
                continue;
            }
            switch (itemTr.name)
            {
                case ItemName.Item_Star:
                    icon_star.item = item;
                    item.icon = icon_star;
                    icon_star.gameObject.SetActive(true);
                    break;
                case ItemName.Item_Pincers:
                    //pincerses.Add(itemTr.GetComponent<Item>());
                    break;
                case ItemName.Item_ManholeCover:
                    //manholeCovers.Add(item);
                    break;
                case ItemName.Item_LureBottle:
                    //bottles.Add(item);
                    break;
                case ItemName.item_Growth:
                    //grouthes.Add(item);
                    break;
                case ItemName.Item_Graff:
                    icon_graff.item = item;
                    item.icon = icon_star;
                    icon_graff.gameObject.SetActive(true);
                    break;
                case ItemName.Item_End:
                    //end = item;
                    break;
                default:
                    Debug.LogError(string.Format("未处理未定义Item{0}", itemTr.name));
                    break;
            }
        }
    }
}
