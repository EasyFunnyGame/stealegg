using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChapterCanvas : BaseCanvas, IPointerDownHandler, IPointerUpHandler
{
    public Image bg;

    public RawImage locate;

    public List<LevelItem> levelItems;

    public RawImage img_chapter;

    public Button btn_left;

    public Button btn_right;

    public Button btn_clz;

    public int chapter;

    private void Awake()
    {
        UiUtils.Adaptive(bg, GetComponent<RectTransform>());

        btn_left.onClick.AddListener(onClickPreChapterHandler);
        btn_right.onClick.AddListener(onClickNxtChapterHandler);
        btn_clz.onClick.AddListener(onClickCloseChapterHandler);
    }

    int downX;

    int upX;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Down");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Up");
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
        var level = PlayerPrefs.GetInt(UserDataKey.Level);
        chapter = (level - level % 12) / 12;
        ShowChapter(chapter);
    }

    protected override void OnHide()
    {

    }

    void ShowChapter(int chapter)
    {
        locate.gameObject.SetActive(false);
        var level = PlayerPrefs.GetInt(UserDataKey.Level);
        //level = 5;
        var start = chapter * 12;
        levelItems.ForEach((LevelItem levelItem) => {
          
            levelItem.SetData(chapter, start);
            if(levelItem.level > level)
            {
                if(levelItem.level == level + 1)
                {
                    levelItem.SetUnlockable();
                }
                else
                {
                    levelItem.SetLock();
                }
            }
            else if(levelItem.level == level)
            {
                levelItem.SetCurrent();
                locate.rectTransform.anchoredPosition = levelItem.rectTransform.anchoredPosition;
                locate.gameObject.SetActive(true);
            }
            else if(levelItem.level < level)
            {
                levelItem.SetUnlocked();
            }
            start++;
        });

        if(chapter == 0)
        {
            btn_left.enabled = false;
            btn_right.enabled = true;
        }
        else if(chapter == 2)
        {
            btn_left.enabled = true;
            btn_right.enabled = false;
        }
        else
        {
            btn_left.enabled = true;
            btn_right.enabled = true;
        }

        img_chapter.texture = Resources.Load<Texture>("UI/Sprite/Num/"+(chapter+1).ToString());
    }

    void onClickPreChapterHandler()
    {
        if (chapter - 1 < 0) return;
        ShowChapter(--chapter);
    }

    void onClickNxtChapterHandler()
    {
        if (chapter + 1 > 3) return;
        ShowChapter(++chapter);
    }

    void onClickCloseChapterHandler()
    {
        Game.Instance.chapterCanvas.Hide();
        Game.Instance.mainCanvas.Show();
    }

}
