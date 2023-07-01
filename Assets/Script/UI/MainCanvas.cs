using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainCanvas : BaseCanvas
{
    public Button btn_start;
    public Button btn_setting;
    public Image bg;

    void Awake()
    {
        btn_start.onClick.AddListener(StartGame);
        btn_setting.onClick.AddListener(ShowSettingCanvas);
        UiUtils.Adaptive(bg, GetComponent<RectTransform>());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartGame()
    {
        Game.Instance?.mainCanvas.Hide();
        Game.Instance?.chapterCanvas.Show();
        AudioPlay.Instance?.PlayClick();
    }

    void ShowSettingCanvas()
    {
        Game.Instance?.settingCanvas.Show();
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
