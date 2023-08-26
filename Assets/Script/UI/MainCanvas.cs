using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainCanvas : BaseCanvas

{
    public Button btn_start;

    public Button btn_setting;

    public Image bg;

    public Button btn_how_to_play;

    public Button btn_star;

    public Button btn_no_limit_tili;

    public Button btn_play_any_level;


    void Awake()
    {
        btn_start.onClick.AddListener(StartGame);
        btn_setting.onClick.AddListener(ShowSettingCanvas);

        btn_how_to_play.onClick.AddListener(ShowHowToPlayThisGameHandler);
        btn_star.onClick.AddListener(ShowStarInGameStatementHandler);
        btn_no_limit_tili.onClick.AddListener(WatchVedioNoLimitTilyHandler);
        btn_play_any_level.onClick.AddListener(WathVedioPlayAnyLevelHandler);

        UiUtils.Adaptive(bg, GetComponent<RectTransform>());
    }

    void ShowHowToPlayThisGameHandler()
    {if (!Game.Instance.startButtonNoCover())
        {
            return;
        }
        Game.Instance?.howToPlayCanvas.Show();
        AudioPlay.Instance?.PlayClick();
    }

    void ShowStarInGameStatementHandler()
    {if (!Game.Instance.startButtonNoCover())
        {
            return;
        }
        Game.Instance?.starCanvas.Show();
        AudioPlay.Instance?.PlayClick();
    }

    void WatchVedioNoLimitTilyHandler()
    {
        if(!Game.Instance.startButtonNoCover())
        {
            return; 
        }
        Game.Instance?.tiliNoLimitCanvas.Show();
        AudioPlay.Instance?.PlayClick();
    }

    void WathVedioPlayAnyLevelHandler()
    {
        if (!Game.Instance.startButtonNoCover())
        {
            return;
        }
        Game.Instance?.levelNoLimitCavas.Show();
        AudioPlay.Instance?.PlayClick();
    }

    void ShowSettingCanvas()
    {if (!Game.Instance.startButtonNoCover())
        {
            return;
        }
        Game.Instance?.settingCanvas.Show();
        AudioPlay.Instance?.PlayClick();
    }

    void StartGame()
    {
        if(!Game.Instance)
        {
            return;
        }
        if(!Game.Instance.startButtonNoCover())
        {
            return;
        }

        Game.Instance.mainCanvas.Hide();
        Game.Instance.chapterCanvas.Show();
        AudioPlay.Instance?.PlayClick();
    }

    

    protected override void OnShow()
    {
        AudioPlay.Instance?.PlayMain();
    }

    protected override void OnHide()
    {
    }
}
