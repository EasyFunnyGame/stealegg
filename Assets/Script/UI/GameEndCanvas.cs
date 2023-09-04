using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEndCanvas : BaseCanvas
{
    public Button btn_home;

    public Button btn_nxtLevel;

    public Button btn_share;

    public Button btn_replay;

    public Image img_title;

    public RawImage winTxture;

    public RawImage failTexture;

    public Image img_star1;

    public Image img_star2;

    public Image img_star3;

    private void Awake()
    {
        btn_home.onClick.AddListener(onClickReturnToHomeHandler);
        btn_nxtLevel.onClick.AddListener(onClickPlayNextLevelHandler);
        btn_replay.onClick.AddListener(onClickReplayThisLevelHandler);
        btn_share.onClick.AddListener(onClickShareHandler);
    }

    private void onClickReturnToHomeHandler()
    {
        //SceneManager.LoadScene("Main");
        if(Game.Instance)
        {
            Game.Instance.mainCanvas.Show();
            Game.Instance.resLoaded = false;
            Game.Instance.endCanvas.Hide();
            Game.Instance.playing = false;
        }
       
        AudioPlay.Instance?.PlayClick();
       
        //throw new NotImplementedException();
    }

    private void onClickPlayNextLevelHandler()
    {
        // 24小时免费关卡
        var levelLimit = Game.Instance.isLevelLimit();
        // 24小时无限体力
        var tiliLimit = Game.Instance.isEnergyLimit();

        var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
        if(energy <= 0 && tiliLimit)
        {
            Game.Instance?.energyGainCanvas.Show();
            return;
        }

        
        // 隐藏关卡限制
        var myStars = 0;
        for (var i = 0; i < 36; i++)
        {
            var levelStars = PlayerPrefs.GetInt(UserDataKey.Level_Stars + i.ToString());
            myStars += levelStars;
        }

        var nextLevel = Game.Instance.playingLevel + 1;
        var starNeed = LevelUnLockConfig.LEVEL_UNLOCK_CONFIG.ContainsKey(nextLevel + 1) ? LevelUnLockConfig.LEVEL_UNLOCK_CONFIG[nextLevel + 1] : 0;
        if (myStars < starNeed )
        {
            Game.Instance?.msgCanvas.PopMessage("获得" + starNeed.ToString() + "星星可解锁此关卡");
            return;
        }

        if (tiliLimit)
        {
            PlayerPrefs.SetInt(UserDataKey.Energy, energy - 1);
            PlayerPrefs.Save();
        }


        if (Game.Instance)
        {
            Game.Instance.playing = false;
            Game.Instance.endCanvas.Hide();
            var playingLevel = Game.Instance.playingLevel + 1;

            if (playingLevel > Game.MAX_LEVEL)
            {
                playingLevel = 0;
            }
            var playingChapter = Mathf.FloorToInt(playingLevel / 12) + 1;
            var playingIndex = playingLevel % 12 + 1;
            if (playingLevel % 12 == 0)
            {
                playingIndex = 1;
            }

            var nextLevelName = string.Format("{0}-{1}", playingChapter, playingIndex);
            Game.Instance?.PlayLevel(nextLevelName);
        }
        AudioPlay.Instance?.PlayClick();

    }

    private void onClickReplayThisLevelHandler()
    {
        // 24小时无限体力
        var tiliLimit = Game.Instance.isEnergyLimit();

        var energy = PlayerPrefs.GetInt(UserDataKey.Energy); 
        if (energy <= 0 && tiliLimit)
        {
            Game.Instance?.energyGainCanvas.Show();
            return;
        }

        if(tiliLimit)
        {
            PlayerPrefs.SetInt(UserDataKey.Energy, energy - 1);
            PlayerPrefs.Save();
        }

        if(Game.Instance)
        {
            Game.Instance.playing = false;
            Game.Instance.endCanvas.Hide();
            Game.restart = true;
            Game.Instance.resLoaded = false;
        }
        AudioPlay.Instance?.PlayClick();
        SceneManager.LoadScene(Game.Instance?.currentLevelName);
    }

    private void onClickShareHandler()
    {
        AudioPlay.Instance?.PlayClick();
    }

    
    protected override void OnShow()
    {
        var drawGameObject = GameObject.Find("Draw");
        if(drawGameObject != null)
        {
            //Debug.Log("找到 Draw");
            var renderCamera = drawGameObject.transform.Find("render_camera");
            renderCamera.gameObject.SetActive(Game.Instance?.result == GameResult.WIN);
        }

        if (Game.Instance?.result == GameResult.WIN)
        {
            Game.Instance?.player.m_animator.SetBool("moving", false);
            Game.Instance?.player.gameObject.SetActive(false);

            img_title.sprite = Resources.Load<Sprite>("UI/Sprite/ui-_0049");
            btn_nxtLevel.gameObject.SetActive(true);
            btn_replay.gameObject.SetActive(false);
            AudioPlay.Instance?.PlayWin();

            winTxture.gameObject.SetActive(true);
            failTexture.gameObject.SetActive(false);

            var star = 1;

            img_star1.gameObject.SetActive(true);
            img_star2.gameObject.SetActive(false);
            img_star3.gameObject.SetActive(false);
            if (Game.Instance.gainStar)
            {
                star++;
            }
            if(Game.Instance.neverFound)
            {
                star++;
            }

            if(star ==2)
            {
                img_star2.gameObject.SetActive(true);
                img_star3.gameObject.SetActive(false);
            }
            else if(star==3)
            {
                img_star2.gameObject.SetActive(true);
                img_star3.gameObject.SetActive(true);
            }

            var key = UserDataKey.Level_Stars + Game.Instance.playingLevel.ToString();
            var haveStars = PlayerPrefs.GetInt(key);
            if(star > haveStars)
            {
                PlayerPrefs.SetInt(key, star);
                PlayerPrefs.Save();
            }


            var level = PlayerPrefs.GetInt(UserDataKey.Level);
            if (Game.Instance.playingLevel >= level)
            {
                PlayerPrefs.SetInt(UserDataKey.Level, Game.Instance.playingLevel + 1);
                PlayerPrefs.Save();
            }
        }
        else if(Game.Instance?.result == GameResult.FAIL)
        {
            var draw_able = GameObject.FindObjectOfType<FreeDraw.Drawable>();
            draw_able?.gameObject.SetActive(false);
            img_title.sprite = Resources.Load<Sprite>("UI/Sprite/ui-_0048");
            btn_nxtLevel.gameObject.SetActive(false);
            btn_replay.gameObject.SetActive(true);
            AudioPlay.Instance?.PlayFail();

            winTxture.gameObject.SetActive(false);
            failTexture.gameObject.SetActive(true);

            img_star1.gameObject.SetActive(false);
            img_star2.gameObject.SetActive(false);
            img_star3.gameObject.SetActive(false);
        }
        AudioPlay.Instance?.PlaySnapShot();
        AudioPlay.Instance?.StopBackGroundMisic();
    }

    protected override void OnHide()
    {

    }
}
