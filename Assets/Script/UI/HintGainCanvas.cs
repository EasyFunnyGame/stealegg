using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HintGainCanvas : BaseCanvas
{
    public Button btn_sure;

    public Button btn_cancel;

    // Start is called before the first frame update
    void Start()
    {
        btn_sure.onClick.AddListener(onWatchVideoHandler);
        btn_cancel.onClick.AddListener(onCloseCanvasHandler);
    }

    void onCloseCanvasHandler()
    {
        Game.Instance.hintGainCanvas.Hide();
    }

    void onWatchVideoHandler()
    {
        Game.Instance.hintGainCanvas.Hide();
        Game.teaching = true;
        Game.clearTeaching = 0;
        Game.Instance.gameCanvas.Hide();
        SceneManager.LoadScene(Game.Instance.currentLevelName);
        Game.Instance.playing = false;
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
