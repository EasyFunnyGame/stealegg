using UnityEngine;
using UnityEngine.UI;

public class LevelItem : MonoBehaviour
{
    public RawImage img_vedio;

    public RawImage img_center_locked;

    public RawImage img_center_unlocked;

    public int chapter;

    public int index;

    public RectTransform rectTransform;

    public Button btn_enter;

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
        var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
        if( energy < 5 )
        {
            Game.Instance.energyGainCanvas.Show();
            return;
        }
        var sceneName = string.Format("{0}-{1}", chapter + 1, (index % 12) + 1);
        var level = PlayerPrefs.GetInt("Level");
        if (level == level + 1)
        {
            Game.Instance.msgCanvas.PopMessage("观看视频可直接试玩此关!");
            return;
        }
        if(level > level + 1)
        {
            Game.Instance.msgCanvas.PopMessage("请先通过前一关!");
            return;
        }
        PlayerPrefs.SetInt(UserDataKey.Energy, energy - 5);

        Game.Instance.StartGame(sceneName);
    }

}
