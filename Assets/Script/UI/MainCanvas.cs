
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainCanvas : MonoBehaviour
{
    public Button btn_start;
    public Button btn_setting;
    public Image img_bg;

    private void Awake()
    {
        btn_start.onClick.AddListener(StartGame);
        btn_setting.onClick.AddListener(ShowSettingCanvas);
        UiUtils.Adaptive(img_bg, GetComponent<RectTransform>());
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
        var level = PlayerPrefs.GetInt("Level");
        var chapter = (level - level % 12) / 12;
        level = level % 12;
        var sceneName = string.Format("{0}-{1}", chapter + 1, level + 1);
        SceneManager.LoadScene(sceneName);
        Game.Instance.mainCanvas.gameObject.SetActive(false);
    }

    void ShowSettingCanvas()
    {
        Game.Instance.settingCanvas.gameObject.SetActive(true);
    }
}
