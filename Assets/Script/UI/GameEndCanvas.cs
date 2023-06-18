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
        Game.Instance.mainCanvas.Show();
        Game.Instance.resLoaded = false;
        Game.Instance.endCanvas.Hide();
        AudioPlay.Instance.PlayClick();
        Game.Instance.playing = false;
        //throw new NotImplementedException();
    }

    private void onClickPlayNextLevelHandler()
    {
        var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
        if(energy <= 0)
        {
            Game.Instance.energyGainCanvas.Show();
            return;
        }

        PlayerPrefs.SetInt(UserDataKey.Energy, energy - 1);
        PlayerPrefs.Save();


        Game.Instance.playing = false;
        Game.Instance.endCanvas.Hide();
        var playingLevel = Game.Instance.playingLevel+1;
        
        if(playingLevel>Game.MAX_LEVEL)
        {
            playingLevel = 0;
        }
        var playingChapter = Mathf.FloorToInt(playingLevel/12) + 1;
        var playingIndex = playingLevel % 12 + 1;
        if (playingLevel % 12 == 0)
        {
            playingIndex = 1;
        }
        
        var nextLevelName = string.Format("{0}-{1}", playingChapter, playingIndex);
        Game.Instance.PlayLevel(nextLevelName);
        AudioPlay.Instance.PlayClick();

    }

    private void onClickReplayThisLevelHandler()
    {
        var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
        if (energy <= 0)
        {
            Game.Instance.energyGainCanvas.Show();
            return;
        }

        PlayerPrefs.SetInt(UserDataKey.Energy, energy - 1);
        PlayerPrefs.Save();

        Game.Instance.playing = false;
        Game.Instance.endCanvas.Hide();
        Game.restart = true;
        SceneManager.LoadScene(Game.Instance.currentLevelName);
        Game.Instance.resLoaded = false;
        AudioPlay.Instance.PlayClick();

    }

    private void onClickShareHandler()
    {
        AudioPlay.Instance.PlayClick();
    }

    
    protected override void OnShow()
    {
        

        var drawGameObject = GameObject.Find("Draw");
        if(drawGameObject != null)
        {
            //Debug.Log("找到 Draw");
            var renderCamera = drawGameObject.transform.Find("render_camera");
            renderCamera.gameObject.SetActive(Game.Instance.result == GameResult.WIN);
        }

        if (Game.Instance.result == GameResult.WIN)
        {
            Game.Instance.player.m_animator.SetBool("moving", false);
            Game.Instance.player.gameObject.SetActive(false);

            img_title.sprite = Resources.Load<Sprite>("UI/Sprite/ui-_0049");
            btn_nxtLevel.gameObject.SetActive(true);
            btn_replay.gameObject.SetActive(false);
            AudioPlay.Instance.PlayWin();

            winTxture.gameObject.SetActive(true);
            failTexture.gameObject.SetActive(false);
        }
        else if(Game.Instance.result == GameResult.FAIL)
        {
            var draw_able = GameObject.FindObjectOfType<FreeDraw.Drawable>();
            draw_able?.gameObject.SetActive(false);
            img_title.sprite = Resources.Load<Sprite>("UI/Sprite/ui-_0048");
            btn_nxtLevel.gameObject.SetActive(false);
            btn_replay.gameObject.SetActive(true);
            AudioPlay.Instance.PlayFail();

            winTxture.gameObject.SetActive(false);
            failTexture.gameObject.SetActive(true);
        }
        AudioPlay.Instance.PlaySnapShot();
    }

    protected override void OnHide()
    {

    }
}
