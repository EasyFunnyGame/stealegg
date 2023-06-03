using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HintGainCanvas : BaseCanvas
{
    public Button btn_sure;

    public Button btn_cancel;

    public Button btn_clz;

    // Start is called before the first frame update
    void Start()
    {
        btn_sure.onClick.AddListener(onWatchVideoHandler);
        btn_cancel.onClick.AddListener(onCloseCanvasHandler);
        btn_clz.onClick.AddListener(onHideCanvasHandler);
    }

    void onHideCanvasHandler()
    {
        Game.Instance.hintGainCanvas.Hide();
    }

    void onCloseCanvasHandler()
    {
        
        AudioPlay.Instance.PlayClick();
    }

    void onWatchVideoHandler()
    {
        Game.Instance.hintGainCanvas.Hide();
        Game.teaching = true;
        Game.clearTeaching = 0;
        Game.Instance.gameCanvas.Hide();
        SceneManager.LoadScene(Game.Instance.currentLevelName);
        Game.Instance.resLoaded = false;
        Game.Instance.playing = false;
        AudioPlay.Instance.PlayClick();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    protected override void OnShow()
    {
    }

    protected override void OnHide()
    {

    }
}
