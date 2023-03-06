using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraffCanvas : BaseCanvas
{
    public Button btn_clz;

    public Button btn_changeColor;

    public Image img_nowColor;
    public Button btn_black;
    public Button btn_gray;
    public Button btn_white;
    public Button btn_yellow;
    public Button btn_orange;
    public Button btn_red;
    public Button btn_purple;
    public Button btn_blue;
    public Button btn_green;
    public RectTransform rect_colorPlate;
    public RectTransform img_default;
    public Button btn_eraser;
    public Button btn_thickness;
    public RectTransform rect_thickNessPlate;

    public Button btn_thick;
    public Button btn_normal;
    public Button btn_slime;

    public Button btn_complete;


    private void Awake()
    {
        Game.Instance.camera.gameObject.SetActive(false);

        rect_colorPlate.gameObject.SetActive(false);
        rect_thickNessPlate.gameObject.SetActive(false);

        btn_clz.onClick.AddListener(onCloseGraffCanvasHandler);;
        img_nowColor.sprite = Resources.Load<Sprite>(string.Format("UI/Sprite/color_{0}","black"));

        btn_changeColor.onClick.AddListener(this.ShowColorPlateHandler);
    }


    void ShowColorPlateHandler()
    {
        rect_colorPlate.gameObject.SetActive(!rect_colorPlate.gameObject.activeSelf);

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onCloseGraffCanvasHandler()
    {
        Game.Instance.graffCanvas.Hide();
    }
    protected override void OnShow()
    {
        Game.Instance.camera.gameObject.SetActive(false);
        Game.Instance.draw_able.gameObject.SetActive(true);
        Game.Instance.draw_setting.gameObject.SetActive(true);
        Game.Instance.draw_camera.gameObject.SetActive(true);
    }

    protected override void OnHide()
    {
        if(Game.Instance.camera!=null)
            Game.Instance.camera?.gameObject.SetActive(true);
        //if (Game.Instance.draw_able != null)
        //    Game.Instance.draw_able?.gameObject.SetActive(false);
        if (Game.Instance.draw_setting != null)
            Game.Instance.draw_setting?.gameObject.SetActive(false);
        if (Game.Instance.draw_camera != null)
            Game.Instance.draw_camera?.gameObject.SetActive(false);
    }
}
