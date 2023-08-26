using UnityEngine;
using UnityEngine.UI;

public class LevelItem : MonoBehaviour  
{
    public static string SelectedLevel = "";

    public static float SelectedDelayEnter = 0.0f;

    public RawImage img_vedio;

    public RawImage img_center_locked;

    public RawImage img_center_unlocked;

    public RawImage bg;

    public int chapter;

    public int index;

    public RectTransform rectTransform;

    public Button btn_enter;

    public Text txt_level;

    private void Awake()
    {
        btn_enter.onClick.AddListener(onClickChapterLevelHandler);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(int chapter , int idx)
    {
        this.chapter = chapter;
        index = idx;
        txt_level.text = (level+1).ToString();
        bg.texture = Resources.Load<Texture>("UI/Sprite/guanka-" + (chapter + 1));
    }

    public int level
    {
        get { return chapter * 12 + index; }
    }

    public void SetLock()
    {
        img_center_unlocked.gameObject.SetActive(false);
        img_center_locked.gameObject.SetActive(true);
        img_vedio.gameObject.SetActive(false);
    }

    public void SetCurrent()
    {
        img_center_unlocked.gameObject.SetActive(true);
        img_center_locked.gameObject.SetActive(false);
        img_vedio.gameObject.SetActive(false);
    }

    public void SetUnlocked()
    {
        img_center_unlocked.gameObject.SetActive(true);
        img_center_locked.gameObject.SetActive(false);
        img_vedio.gameObject.SetActive(false);
    }

    public void SetUnlockable()
    {
        img_center_unlocked.gameObject.SetActive(false);
        img_center_locked.gameObject.SetActive(true);
        img_vedio.gameObject.SetActive(true);
    }

    void onClickChapterLevelHandler()
    {
        if(!string.IsNullOrEmpty(SelectedLevel))
        {
            return;
        }

        // 隐藏关卡限制
        var myStars = 0;
        for (var i = 0; i < 36; i++)
        {
            var levelStars = PlayerPrefs.GetInt(UserDataKey.Level_Stars + i.ToString());
            myStars += levelStars;
        }
        var starNeed = LevelUnLockConfig.LEVEL_UNLOCK_CONFIG.ContainsKey(this.level + 1) ? LevelUnLockConfig.LEVEL_UNLOCK_CONFIG[this.level + 1] : 0;
        if (myStars < starNeed)
        {
            Game.Instance?.msgCanvas.PopMessage("获得" + starNeed.ToString() + "星星可解锁此关卡");
            return;
        }


        // 24小时免费关卡
        var levelLimit = Game.Instance.isLevelLimit();
        // 24小时无限体力
        var tiliLimit = Game.Instance.isEnergyLimit();


        var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
        if( energy < 1 && tiliLimit)
        {
            Game.Instance?.energyGainCanvas.Show();
            return;
        }
        

        var sceneName = string.Format("{0}-{1}", chapter + 1, (index % 12) + 1);
        var level = PlayerPrefs.GetInt(UserDataKey.Level);
        if (this.level == level + 1 && levelLimit)
        {
            //Game.Instance?.msgCanvas.PopMessage("观看视频可直接试玩此关!");

            var canvas = Game.Instance?.watchVedioCanvas;
            if(canvas)
            {
                canvas.Show();
                canvas.SetMessage("试玩", "观看视频可试玩本关", PlayerLevel);
            }
            return;
        }
        if(this.level > level + 1 && levelLimit)
        {
            Game.Instance?.msgCanvas.PopMessage("请先通过" + this.level.ToString() + "关");
            return;
        }
        
        PlayerLevel();
        //Game.Instance?.effectCanvas.PointerClick(null);
    }


    private void PlayerLevel()
    {
        var sceneName = string.Format("{0}-{1}", chapter + 1, (index % 12) + 1);
        var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
        if(Game.Instance.isEnergyLimit())
        {
            PlayerPrefs.SetInt(UserDataKey.Energy, energy - 1);
            PlayerPrefs.Save();
        }

        SelectedLevel = sceneName;
        SelectedDelayEnter = 0;
    }
}
