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

    private void Awake()
    {
        btn_home.onClick.AddListener(onClickReturnToHomeHandler);
        btn_nxtLevel.onClick.AddListener(onClickPlayNextLevelHandler);
        btn_replay.onClick.AddListener(onClickReplayThisLevelHandler);
        btn_share.onClick.AddListener(onClickShareHandler);
    }

    private void onClickReturnToHomeHandler()
    {
        SceneManager.LoadScene("Main");
        Game.Instance.endCanvas.Hide();
        AudioPlay.Instance.PlayClick();
        //throw new NotImplementedException();
    }

    private void onClickPlayNextLevelHandler()
    {
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
        Game.Instance.endCanvas.Hide();
        SceneManager.LoadScene(Game.Instance.currentLevelName);
        AudioPlay.Instance.PlayClick();
    }

    private void onClickShareHandler()
    {
        AudioPlay.Instance.PlayClick();
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
        if(Game.Instance.result == GameResult.WIN)
        {
            img_title.sprite = Resources.Load<Sprite>("UI/Sprite/ui-_0049");
            btn_nxtLevel.gameObject.SetActive(true);
            btn_replay.gameObject.SetActive(false);
            AudioPlay.Instance.PlayWin();
        }
        else if(Game.Instance.result == GameResult.FAIL)
        {
            img_title.sprite = Resources.Load<Sprite>("UI/Sprite/ui-_0048");
            btn_nxtLevel.gameObject.SetActive(false);
            btn_replay.gameObject.SetActive(true);
            AudioPlay.Instance.PlayFail();
        }
    }

    protected override void OnHide()
    {
    }
}
