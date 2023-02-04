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
        Game.Instance.mainCanvas.Hide();
        Game.Instance.chapterCanvas.Show();
    }

    void ShowSettingCanvas()
    {
        Game.Instance.settingCanvas.gameObject.SetActive(true);
    }

    protected override void OnShow()
    {
    }

    protected override void OnHide()
    {
    }
}
