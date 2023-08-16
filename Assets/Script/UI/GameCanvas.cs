using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameCanvas : BaseCanvas
{
    public int level;

    public Text txt_level;

    public Text txt_energy;

    public Button btn_add;

    public Button btn_home;

    public Button btn_reStart;

    public Button btn_hint;

    public Button btn_bottle;

    public Button btn_bottle_disable;

    public Button btn_bottle_cancel;

    public Button btn_whistle;

    public Button btn_whistle_disable;

    public Button btn_pause;

    public Button btn_graff;

    public GameObject home;

    public GameObject playing;

    public Button btn_start;

    public ItemIconOnUI icon_graff;

    public ItemIconOnUI icon_star;

    public ItemIconOnUI icon_template_bottle_template;

    public ItemIconOnUI icon_template_pricers_template;

    public ItemIconOnUI icon_template_manholecover_template;

    public ItemIconOnUI icon_template_growth_template;

    public IconsAboveEnemy icon_enemy_template;

    public CanvasGroup whitsleGroup;

    public CanvasGroup bottleGroup;

    public List<ItemIconOnUI> icon_bottles = new List<ItemIconOnUI>();

    public List<ItemIconOnUI> icon_pincers =  new List<ItemIconOnUI>();

    public List<ItemIconOnUI> icon_manholecover = new List<ItemIconOnUI>();

    public List<ItemIconOnUI> icon_growth = new List<ItemIconOnUI>();

    public List<IconsAboveEnemy> icon_enemies = new List<IconsAboveEnemy>();

    public Image distance_up;
    public Text txt_up;
    public Image distance_down;
    public Text txt_down;
    public Image distance_left;
    public Text txt_left;
    public Image distance_right;
    public Text txt_right;

    public Image playerPosition;

    public Image playerPos;

    Vector3 screenPoint = new Vector3();

    float iconScale = 1f;

    float updateTime = 0;

    private void Awake()
    {
        btn_add.onClick.AddListener(onClickShowEnergyGainCanvasHandler);

        btn_bottle.onClick.AddListener(onClickUseBottleHandler);

        btn_bottle_cancel.onClick.AddListener(CancelBottleThrow);

        btn_whistle.onClick.AddListener(onClickUseWhistleHandler);

        btn_home.onClick.AddListener(onClickBackToHomeHandler);

        btn_reStart.onClick.AddListener(onClickReStartLevelHandler);

        btn_pause.onClick.AddListener(onClickPasueGameHandler);

        btn_start.onClick.AddListener(onClickStartPlayingGameHandler);

        btn_graff.onClick.AddListener(onClickGraffHandler);

        btn_hint.onClick.AddListener(onClickHintHandler);

        icon_graff.gameObject.SetActive(false);

        icon_star.gameObject.SetActive(false);

        icon_template_bottle_template.gameObject.SetActive(false);

        icon_template_pricers_template.gameObject.SetActive(false);

        icon_template_manholecover_template.gameObject.SetActive(false);

        icon_template_growth_template.gameObject.SetActive(false);

        icon_enemy_template.gameObject.SetActive(false);
    }

    void onClickHintHandler()
    {
        Game.Instance.hintGainCanvas.Show();
        AudioPlay.Instance?.PlayClick();
    }

    void onClickGraffHandler()
    {
        var tileName = Game.Instance.player.currentTile.name;
        var allItems = Game.Instance.boardManager.allItems;
        
        if(allItems.ContainsKey(tileName))
        {
            var graffItem = allItems[tileName];
            if (graffItem!=null && graffItem.itemType == ItemType.Graff)
            {
                Game.Instance.Steal();
            }
        }
    }

    public void onClickStartPlayingGameHandler()
    {
        home.gameObject.SetActive(false);
        playing.gameObject.SetActive(true);
        Game.Instance.playing = true;
        AudioPlay.Instance?.PlayClick();
    }

    private void onClickPasueGameHandler()
    {
        if(Game.Instance.result != GameResult.NONE)
        {
            return;
        }

        playing.gameObject.SetActive(false);
        home.gameObject.SetActive(true);
        AudioPlay.Instance?.PlayClick();
    }

    void onClickReStartLevelHandler()
    {
        var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
        if(energy<1)
        {
            Game.Instance.msgCanvas.PopMessage("体力不足");
            Game.Instance.energyGainCanvas.Show();
            return;
        }
        PlayerPrefs.SetInt(UserDataKey.Energy, energy - 1);
        PlayerPrefs.Save();
        Game.Instance.gameCanvas.Hide();
        SceneManager.LoadScene(Game.Instance.currentLevelName);
        Game.Instance.resLoaded = false;
        Game.Instance.playing = false;
        AudioPlay.Instance?.PlayClick();
    }

    void onClickBackToHomeHandler()
    {
        Game.Instance.gameCanvas.Hide();
        //SceneManager.LoadScene("Main");
        Game.Instance.mainCanvas.Show();
        Game.Instance.resLoaded = false;
        Game.Instance.playing = false;
        AudioPlay.Instance?.PlayClick();
    }

    private void onClickShowEnergyGainCanvasHandler()
    {
        Game.Instance.energyGainCanvas.Show();
        AudioPlay.Instance?.PlayClick();
    }

    private void onClickUseBottleHandler()
    {
        if (buttonClickCd > 0) return;
        buttonClickCd = 1.0f;
        var teachingStep = Game.Instance.showingStep;
        if (Game.teaching && teachingStep != null)
        {
            if (teachingStep.actionType != ActionType.ThrowBottle)
            {
                Game.Instance.msgCanvas.PopMessage("请按照步骤进行");
                return;
            }
        }
        if (Game.Instance.enemyActionRunning || Game.Instance.player.currentAction!=null)
        {
            return;
        }

        Game.Instance.BottleSelectTarget();
        btn_bottle_cancel.gameObject.SetActive(true);
        btn_bottle.gameObject.SetActive(false);
    }


    void CancelBottleThrow()
    {
        if (buttonClickCd > 0) return;
        buttonClickCd = 1.5f;
        var teachingStep = Game.Instance.showingStep;
        if (Game.teaching && teachingStep != null)
        {
            if (teachingStep.actionType == ActionType.ThrowBottle && teachingStep?.tileName != "")
            {
                Game.Instance.msgCanvas.PopMessage("请按照步骤进行");
                return;
            }
        }
        Game.Instance.CancelBottleSelectTarget();
        btn_bottle_cancel.gameObject.SetActive(false);
        btn_bottle.gameObject.SetActive(true);
        AudioPlay.Instance?.PlayClick();
    }

    private void onClickUseWhistleHandler()
    {
        if (Game.Instance.bottleSelectingTarget) return;
        if (Game.Instance.player.moving) return;
        //if (Game.Instance.player.body_looking) return;
        if (Game.Instance.player.currentAction != null) return;
        for(int i = 0; i < Game.Instance.boardManager.enemies.Count; i++)
        {
            if (Game.Instance.boardManager.enemies[i].currentAction!=null)
            {
                return;
            }
        }
        if (buttonClickCd > 0) return;
        buttonClickCd = 1.0f;
        var teachingStep = Game.Instance.showingStep;
        if ( Game.teaching && teachingStep !=null  )
        {
            if(teachingStep.actionType != ActionType.BlowWhistle)
            {
                Game.Instance.msgCanvas.PopMessage("请按照步骤进行");
                return;
            }
        }

        if (Game.Instance.enemyActionRunning || Game.Instance.player.currentAction != null)
        {
            return;
        }
        Game.Instance.BlowWhistle();
    }

    private void onClickPauseGameHandler()
    {
        Game.Instance.UseBottle();
    }

    float buttonClickCd = 1.0f;

    // Update is called once per frame
    void Update()
    {
        if(buttonClickCd>0)
        {
            buttonClickCd -= Time.deltaTime;
        }
        if(!Game.Instance)
        {
            return;
        }
        //if (Game.Instance.player != null)
        //{
        //    UiUtils.WorldToScreenPoint(Game.Instance.camera.m_camera, this, Game.Instance.player.transform.position, out screenPoint);
        //    playerPosition.rectTransform.anchoredPosition = screenPoint;
        //    Game.Instance.camera.UpdatePlayerPositionOnScreen(GetComponent<RectTransform>(), screenPoint, playerPosition);
        //}

        if (icon_star.gameObject.activeSelf)
        {
            UiUtils.WorldToScreenPoint(Game.Instance.camera.cam, this, icon_star.item.GetIconPosition(),  out screenPoint);
            icon_star.rectTransform.anchoredPosition = screenPoint;
        }

        if (icon_graff.gameObject.activeSelf)
        {
            UiUtils.WorldToScreenPoint(Game.Instance.camera.cam, this, icon_graff.item.GetIconPosition(), out screenPoint);
            icon_graff.rectTransform.anchoredPosition = screenPoint;
        }

        for (var index = 0; index < icon_enemies.Count; index++)
        {
            var icon = icon_enemies[index];
            UiUtils.WorldToScreenPoint(Game.Instance.camera.cam, this, icon.enemy.getHeadPointPosition(), out screenPoint);
            icon.rectTransform.anchoredPosition = screenPoint;
        }

        for (var index = 0; index < icon_bottles.Count; index++)
        {
            var icon = icon_bottles[index];
            if (icon.gameObject.activeSelf)
            {
                UiUtils.WorldToScreenPoint(Game.Instance.camera.cam, this, icon.item.GetIconPosition(), out screenPoint);
                icon.rectTransform.anchoredPosition = screenPoint;
            }
        }

        for (var index = 0; index < icon_pincers.Count; index++)
        {
            var icon = icon_pincers[index];
            if (icon.gameObject.activeSelf)
            {
                UiUtils.WorldToScreenPoint(Game.Instance.camera.cam, this, icon.item.GetIconPosition(), out screenPoint);
                icon.rectTransform.anchoredPosition = screenPoint;
            }
        }

        // 水井盖图标显示，能用的才显示出来
        // 敌人移动中不显示
        // 主角进行中不显示
        // 酒瓶正在选点不显示
        var playerActing = Game.Instance.player.currentAction != null;
        var enemyActing = false;
        
        for(var index = 0; index < Game.Instance.boardManager.enemies.Count; index++)
        {
            if (Game.Instance.boardManager.enemies[index].currentAction != null)
            {
                enemyActing = true;
                break;
            }
        }

        var showManHoleCoverIcon = false;
        var playerTileName = Game.Instance.player?.currentTile?.name;
        if(!string.IsNullOrEmpty(playerTileName))
        {
            var items = Game.Instance.boardManager.allItems;
            if(items.ContainsKey(playerTileName))
            {
                var item = items[playerTileName];
                if(item?.itemType == ItemType.ManHoleCover)
                {
                    showManHoleCoverIcon = !enemyActing && !playerActing && !Game.Instance.bottleSelectingTarget;
                }
            }
        }
        
        for (var index = 0; index < icon_manholecover.Count; index++)
        {
            var icon = icon_manholecover[index];
            var item = icon.item;
            var iconShow = true;
            if(item.coord.name == playerTileName)
            {
                iconShow = false;
            }
            foreach(var enemy in Game.Instance.boardManager.enemies)
            {
                if(enemy.coord.name == item.coord.name)
                {
                    iconShow = false;
                    break;
                }
            }
            icon.gameObject.SetActive(iconShow && showManHoleCoverIcon);

            if (icon.gameObject.activeSelf )
            {
                UiUtils.WorldToScreenPoint(Game.Instance.camera.cam, this, icon.item.GetIconPosition(), out screenPoint);
                icon.rectTransform.anchoredPosition = screenPoint;
            }
        }

        for (var index = 0; index < icon_growth.Count; index++)
        {
            var icon = icon_growth[index];
            icon.gameObject.SetActive(playerTileName == icon.item.coord.name);
            if (icon.gameObject.activeSelf)
            {
                UiUtils.WorldToScreenPoint(Game.Instance.camera.cam, this, icon.item.GetIconPosition(), out screenPoint);
                icon.rectTransform.anchoredPosition = screenPoint;
            }
        }

        if(guidingWhitsle)
        {
            updateTime += (Time.deltaTime * 5);
            iconScale = 1 + 0.1f * Mathf.Sin(updateTime);
            btn_whistle.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
        }

        if(guidingBottle)
        {
            updateTime += (Time.deltaTime * 5);
            iconScale = 1 + 0.1f * Mathf.Sin(updateTime);
            btn_bottle.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
        }


        if(Input.touchCount == 2)
        {
            if(!twoFingers)
            {
                twoFingers = true;
                lstOnePos = Input.GetTouch(0).position;
                lstTwoPos = Input.GetTouch(1).position;
            }
            Vector2 onePos = Input.GetTouch(0).position;
            Vector2 twoPos = Input.GetTouch(1).position;

            float lastDis = Vector2.Distance(lstOnePos, lstTwoPos);
            float curDis = Vector2.Distance(onePos, twoPos);
            

            if(Game.Instance?.camera)
            {
                var cam = Game.Instance.camera;
                if (curDis > lastDis)
                {
                    cam.multiTarget.customPadding -= 0.05f;
                    // cam.rtsCam.TwoFingerZoomDirection = -1;
                }
                else
                {
                    cam.multiTarget.customPadding += 0.05f;
                    // cam.rtsCam.TwoFingerZoomDirection = +1;
                }
            }
        }
        else
        {
            twoFingers = false;
            if(Game.Instance?.camera)
            {
                // Game.Instance.camera.rtsCam.TwoFingerZoomDirection = 0;
            }
        }
    }

    Vector2 lstOnePos;
    Vector2 lstTwoPos;
    bool twoFingers = false;


    void LateUpdate()
    {
        if (!Game.Instance) return;
        if (!Game.Instance.camera) return;
        //distance_up.rectTransform.sizeDelta = new Vector2(2, Math.Abs(Game.Instance.camera.playerPaddingUp));
        //txt_up.text = Math.Abs(Game.Instance.camera.playerPaddingUp).ToString();

        //distance_down.rectTransform.sizeDelta = new Vector2(2, Math.Abs(Game.Instance.camera.playerPaddingDown));
        //txt_down.text = Math.Abs(Game.Instance.camera.playerPaddingDown).ToString();

        //distance_left.rectTransform.sizeDelta = new Vector2(Math.Abs(Game.Instance.camera.playerPaddingLeft), 2);
        //txt_left.text = Math.Abs(Game.Instance.camera.playerPaddingLeft).ToString();

        //distance_right.rectTransform.sizeDelta = new Vector2(Math.Abs(Game.Instance.camera.playerPaddingRight), 2);
        //txt_right.text = Math.Abs(Game.Instance.camera.playerPaddingRight).ToString();
    }

    protected override void OnShow()
    {
        RefreshEnergy();
        txt_level.text =level.ToString();

        playing.gameObject.SetActive(false);
        home.gameObject.SetActive(true);
        btn_bottle_cancel.gameObject.SetActive(false);
        playerPosition.gameObject.SetActive(false);
    }

    protected override void OnHide()
    {

    }

    public void RefreshEnergy()
    {
        var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
        
        txt_energy.text = energy.ToString();
    }

    public void DisableWhistle()
    {
        btn_whistle_disable.gameObject.SetActive(true);
        if (!btn_whistle.gameObject.activeSelf) return;
        btn_whistle.gameObject.SetActive(false);
    }

    public void EnableWhistle()
    {
        btn_whistle_disable.gameObject.SetActive(false);
        if (btn_whistle.gameObject.activeSelf) return;
        btn_whistle.gameObject.SetActive(true);
    }

    public void DisableBottle()
    {
        btn_bottle_disable.gameObject.SetActive(true);
        btn_bottle_cancel.gameObject.SetActive(false);
        if (!btn_bottle.gameObject.activeSelf) return;
        btn_bottle.gameObject.SetActive(false);
    }

    public void EnableBottle()
    {
        btn_bottle_disable.gameObject.SetActive(false);
        btn_bottle_cancel.gameObject.SetActive(false);
        if (btn_bottle.gameObject.activeSelf) return;
        btn_bottle.gameObject.SetActive(true);
    }

    public void InitWithBoardManager(BoardManager boardManager)
    {
        ClearIcons();
        InitItemIcons(boardManager);
        InitEnemyIcons(boardManager);
    }

    public void OnClickPricersHandler()
    {
        if (buttonClickCd > 0) return;
        buttonClickCd = 1.0f;
        var teachingStep = Game.Instance.showingStep;
        if (Game.teaching && teachingStep != null)
        {
            if (teachingStep.actionType != ActionType.PincersCut)
            {
                Game.Instance.msgCanvas.PopMessage("请按照步骤进行");
                return;
            }
        }
        if (Game.Instance.player == null || Game.Instance.player.currentTile == null)
        {
            return;
        }
        var player = Game.Instance.player;
        var tileName = player.currentTile.name;
        var allItems = Game.Instance.boardManager.allItems;
        if(!allItems.ContainsKey(tileName))
        {
            return;
        }
        var pincersItem = allItems[tileName];
        if (pincersItem == null || pincersItem.itemType != ItemType.Pincers)
        {
            return;
        }

        if(Game.Instance.enemyActionRunning || Game.Instance.player.currentAction != null)
        {
            return;
        }    

        Game.Instance.CutBarbedWire(pincersItem as PincersItem);
    }

    // 跳进下水道
    void OnClickManholeCoverIconHandler(ItemIconOnUI itemIcon)
    {
        if (buttonClickCd > 0) return;
        buttonClickCd = 1.0f;
        var teachingStep = Game.Instance.showingStep;
        if (Game.teaching && teachingStep != null)
        {
            if (teachingStep.actionType != ActionType.ManHoleCover || teachingStep.tileName != itemIcon.item.coord.name)
            {
                Game.Instance.msgCanvas.PopMessage("请按照步骤进行");
                return;
            }
        }
        if (Game.Instance.player == null || Game.Instance.player.currentTile == null)
        {
            return;
        }
        var player = Game.Instance.player;
        var tileName = player.currentTile.name;
        var allItems = Game.Instance.boardManager.allItems;
        if (!allItems.ContainsKey(tileName))
        {
            return;
        }
        var manHoleCover = allItems[tileName];
        if (manHoleCover == null || manHoleCover.itemType != ItemType.ManHoleCover)
        {
            return;
        }
        if(itemIcon.item == null || itemIcon.item.itemType != ItemType.ManHoleCover)
        {
            return;
        }
        if(itemIcon.item.transform.position == manHoleCover.transform.position)
        {
            return;
        }
        foreach(var enemy in Game.Instance.boardManager.enemies)
        {
            if(enemy.coord.name == itemIcon.item.coord.name)
            {
                return;
            }
        }

        if (Game.Instance.enemyActionRunning || Game.Instance.player.currentAction != null)
        {
            return;
        }
        Game.Instance.JumpIntoManholeCover(itemIcon.item as ManholeCoverItem);
    }

    // 跳过回合
    void OnClickGrowthHandler(GrowthItem item)
    {
        if (buttonClickCd > 0) return;
        buttonClickCd = 1.0f;
        var teachingStep = Game.Instance.showingStep;
        if (Game.teaching && teachingStep != null)
        {
            if (teachingStep.actionType != ActionType.TurnDirection)
            {
                Game.Instance.msgCanvas.PopMessage("请按照步骤进行");
                return;
            }
        }
        if (Game.Instance.player == null || Game.Instance.player.currentTile == null)
        {
            return;
        }
        var player = Game.Instance.player;
        var tileName = player.currentTile.name;
        var allItems = Game.Instance.boardManager.allItems;
        if (!allItems.ContainsKey(tileName))
        {
            return;
        }
        var pincersItem = allItems[tileName];
        if (pincersItem == null || pincersItem.itemType != ItemType.Growth)
        {
            return;
        }
        if (Game.Instance.enemyActionRunning || Game.Instance.player.currentAction != null)
        {
            return;
        }
        Game.Instance.SkipPlayerTurn();
    }

    void InitItemIcons(BoardManager boardManager)
    {
        // 物品图标
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
                    var pincersIcon = Instantiate(icon_template_pricers_template, icon_template_pricers_template.transform.parent);
                    pincersIcon.gameObject.SetActive(true);
                    icon_pincers.Add(pincersIcon);
                    pincersIcon.item = item;
                    item.icon = pincersIcon;
                    pincersIcon.button.onClick.AddListener(OnClickPricersHandler);
                    break;
                case ItemName.Item_ManholeCover:
                    var manCoverIcon = Instantiate(icon_template_manholecover_template, icon_template_manholecover_template.transform.parent);
                    manCoverIcon.gameObject.SetActive(true);
                    icon_manholecover.Add(manCoverIcon);
                    manCoverIcon.item = item;
                    item.icon = manCoverIcon;
                    manCoverIcon.button.onClick.AddListener(delegate() {
                        OnClickManholeCoverIconHandler(manCoverIcon);
                    });
                    break;
                case ItemName.Item_LureBottle:
                    var bottleIcon = Instantiate(icon_template_bottle_template, icon_template_bottle_template.transform.parent);
                    bottleIcon.gameObject.SetActive(true);
                    icon_bottles.Add(bottleIcon);
                    bottleIcon.item = item;
                    item.icon = bottleIcon;
                    break;
                case ItemName.item_Growth:

                    var growthIcon = Instantiate(icon_template_growth_template, icon_template_growth_template.transform.parent);
                    growthIcon.gameObject.SetActive(true);
                    icon_growth.Add(growthIcon);
                    growthIcon.item = item;
                    item.icon = growthIcon;
                    growthIcon.button.onClick.AddListener(delegate () {
                        OnClickGrowthHandler(item as GrowthItem);
                    });

                    break;
                case ItemName.Item_Graff:
                    icon_graff.item = item;
                    item.icon = icon_graff;
                    icon_graff.gameObject.SetActive(true);
                    break;
                case ItemName.Item_End:
                    break;
                default:
                    Debug.LogError(string.Format("未处理未定义Item{0}", itemTr.name));
                    break;
            }
        }
        icon_graff.transform.SetAsLastSibling();
    }

    void InitEnemyIcons(BoardManager boardManager)
    {
        for(var index = 0; index < boardManager.enemies.Count; index ++)
        {
            var enemy = boardManager.enemies[index];
            var enemyIcon = Instantiate(icon_enemy_template, icon_enemy_template.transform.parent);
            enemyIcon.enemy = enemy;
            enemy.icons = enemyIcon;
            icon_enemies.Add(enemyIcon);
            enemyIcon.gameObject.SetActive(true);
        }
    }

    void ClearIcons()
    {
        for(var index = 0; index < icon_enemies.Count; index++)
        {
            DestroyImmediate(icon_enemies[index].gameObject);
        }
        icon_enemies.Clear();
        for (var index = 0; index < icon_bottles.Count; index++)
        {
            DestroyImmediate(icon_bottles[index].gameObject);
        }
        icon_bottles.Clear();

        for (var index = 0; index < icon_pincers.Count; index++)
        {
            DestroyImmediate(icon_pincers[index].gameObject);
        }
        icon_pincers.Clear();

        for (var index = 0; index < icon_manholecover.Count; index++)
        {
            DestroyImmediate(icon_manholecover[index].gameObject);
        }
        icon_manholecover.Clear();

        for (var index = 0; index < icon_growth.Count; index++)
        {
            DestroyImmediate(icon_growth[index].gameObject);
        }
        icon_growth.Clear();
    }

    bool guidingWhitsle = false;

    bool guidingBottle = false;

    public void ShowBottleGuide()
    {
        guidingBottle = true;
        guidingWhitsle = false;
        updateTime = 0;
        iconScale = 1;
        btn_whistle.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
        btn_bottle.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
    }

    public void HideWhitsleAndBottleGuides()
    {
        guidingBottle = false;
        guidingWhitsle = false;

        updateTime = 0;
        iconScale = 1;
        btn_whistle.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
        btn_bottle.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
    }

    public void ShowWhitsleGuide()
    {
        guidingBottle = false;
        guidingWhitsle = true;
        updateTime = 0;
        iconScale = 1;
        btn_whistle.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
        btn_bottle.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
    }

    public void ShowStealGuide()
    {
        icon_graff.ShowGuide();
    }

    public void HideItemGuides()
    {
        icon_graff.HideGuide();
        for (var index = 0; index < icon_pincers.Count; index++)
        {
            icon_pincers[index].HideGuide();
        }
        for (var index = 0; index < icon_manholecover.Count; index++)
        {
            icon_manholecover[index].HideGuide();
        }
        for (var index = 0; index < icon_growth.Count; index++)
        {
            icon_growth[index].HideGuide();
        }
    }

    public void ShowPincersGuide(string tileName)
    {
        for(var index = 0; index < icon_pincers.Count; index++)
        {
            if (icon_pincers[index]?.item?.coord.name == tileName)
            {
                icon_pincers[index].ShowGuide();
            }
        }
    }

    public void ShowManHoleCoverGuide(string tileName)
    {
        for (var index = 0; index < icon_manholecover.Count; index++)
        {
            if (icon_manholecover[index]?.item?.coord.name == tileName)
            {
                icon_manholecover[index].ShowGuide();
            }
        }
    }

    public void ShowSkipTurnGuide(string tileName)
    {
        for (var index = 0; index < icon_growth.Count; index++)
        {
            if (icon_growth[index]?.item?.coord.name == tileName)
            {
                icon_growth[index].ShowGuide();
            }
        }
    }


    Vector3 beginPosition;

    float beginDragTime;

    public void BeginDrag()
    {
        if (Input.touchCount > 1)
        {
            return;
        }
        beginPosition = Input.mousePosition;
        beginDragTime = Time.time;
    }


    public void EndDrag()
    {
        var endDragTime = Time.time;
        if(endDragTime - beginDragTime > 2.0f)
        {
            //Debug.Log("滑动时间太长");
            return;
        }
        if (Game.Instance.result != GameResult.NONE) return;
        if (Input.touchCount > 1)
        {
            //Debug.Log("EndDrag 多点触控" + Input.touchCount);
            return;
        }
       
        if (Game.Instance.pausing) return;
        var player = Game.Instance.player;
        if (player.currentAction != null)
        {
            return;
        }


        var sceneMoveDelta = Input.mousePosition - beginPosition;

        var direction = Game.Instance.camera.transform.InverseTransformPoint(sceneMoveDelta).normalized;


        //var thresHold = Mathf.Abs(direction.x) - Mathf.Abs(direction.z);

        //if (Mathf.Abs(thresHold) < 0.1f)
        //{
        //    return;
        //}

        //direction.x = thresHold > 0 ? Mathf.Round(direction.x) : 0;

        //direction.z = thresHold < 0 ? Mathf.Round(direction.z) : 0;


        var targetOffsetX = 0;
        var targetOffsetZ = 0;

        //if (direction.x == 1 && direction.z == 0)
        //{
        //    targetOffsetX = 1;
        //    //Debug.Log("滑动方向右");
        //}
        //else if (direction.x == -1 && direction.z == 0)
        //{
        //    targetOffsetX = -1;
        //    //Debug.Log("滑动方向左");
        //}
        //else if (direction.z == 1 && direction.x == 0)
        //{
        //    targetOffsetZ = -1;
        //    //Debug.Log("滑动方向上");
        //}
        //else if (direction.z == -1 && direction.x == 0)
        //{
        //    targetOffsetZ = 1;
        //    //Debug.Log("滑动方向下");
        //}

        var thresHoldScreen = 150.0f;
        if (sceneMoveDelta.x > thresHoldScreen && sceneMoveDelta.y > thresHoldScreen)
        {
            targetOffsetX = 1;
            //Debug.Log("滑动方向右");
        }
        else if (sceneMoveDelta.x < -thresHoldScreen && sceneMoveDelta.y < -thresHoldScreen)
        {
            targetOffsetX = -1;
            //Debug.Log("滑动方向左");
        }
        else if (sceneMoveDelta.x < -thresHoldScreen && sceneMoveDelta.y > thresHoldScreen)
        {
            targetOffsetZ = 1;
            //Debug.Log("滑动方向上");
        }
        else if (sceneMoveDelta.x > thresHoldScreen && sceneMoveDelta.y < -thresHoldScreen)
        {
            targetOffsetZ = -1;
            //Debug.Log("滑动方向下");
        }

        var targetCoordX = player.coord.x + targetOffsetX;
        var targetCoordZ = player.coord.z + targetOffsetZ;

        var targetTileName = string.Format("{0}_{1}", targetCoordX, targetCoordZ);

        var targetTile = player.gridManager.GetTileByName(targetTileName);
        if (targetTileName == null)
        {
            AudioPlay.Instance?.ClickUnWalkable();
            return;
        }

        for (var i = 0; i < player.boardManager.enemies.Count; i++)
        {
            var enemy = player.boardManager.enemies[i];
            if (enemy.currentAction != null)
            {
                return;
            }
            if (enemy.coord.name == targetTileName)
            {
                AudioPlay.Instance?.ClickUnWalkable();
                return;
            }
        }
        var linkLine = player.boardManager.FindLine(player.currentTile.name, targetTileName);
        if (linkLine == null || linkLine.through == false)
        {
            //AudioPlay.Instance?.ClickUnWalkable();
            return;
        }
        if (player.moving || player.currentTile != targetTile)
        {
            player.currentAction = Utils.CreatePlayerAction(ActionType.PlayerMove, targetTile);
            //Debug.Log("主角行为====移动");
        }
    }

    Vector3 downPosition;

    public void PointerDown()
    {
        downPosition = Input.mousePosition;
    }

    public void PointerUp()
    {
        var upPosition = Input.mousePosition;
        var distance = Vector3.Distance(upPosition, downPosition);
        if (distance < 10 )
        {
            Game.Instance.ClickGameBoard();
        }
    }


    
}
