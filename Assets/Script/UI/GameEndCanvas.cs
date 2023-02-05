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
        //throw new NotImplementedException();
    }

    private void onClickPlayNextLevelHandler()
    {
        Game.Instance.endCanvas.Hide();
        SceneManager.LoadScene("1-1");
    }

    private void onClickReplayThisLevelHandler()
    {
        Game.Instance.endCanvas.Hide();
        SceneManager.LoadScene("1-1");
    }

    private void onClickShareHandler()
    {
        throw new NotImplementedException();
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
        if(Game.Instance.status == GameStatus.WIN)
        {
            img_title.sprite = Resources.Load<Sprite>("UI/Sprite/ui-_0049");
            btn_nxtLevel.gameObject.SetActive(true);
            btn_replay.gameObject.SetActive(false);
        }
        else if(Game.Instance.status == GameStatus.FAIL)
        {
            img_title.sprite = Resources.Load<Sprite>("UI/Sprite/ui-_0048");
            btn_nxtLevel.gameObject.SetActive(false);
            btn_replay.gameObject.SetActive(true);
        }
    }

    protected override void OnHide()
    {
    }
}
