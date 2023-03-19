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


        btn_changeColor.onClick.AddListener(ShowColorPlateHandler);
        btn_thickness.onClick.AddListener(ShowLineThicknessHandler);

        btn_black.onClick.AddListener(() => { ChangeColorHandler("black"); AudioPlay.Instance.PlayClick(); });
        btn_gray.onClick.AddListener(() => { ChangeColorHandler("gray"); AudioPlay.Instance.PlayClick(); });
        btn_white.onClick.AddListener(() => { ChangeColorHandler("white"); AudioPlay.Instance.PlayClick(); });

        btn_yellow.onClick.AddListener(() => { ChangeColorHandler("yellow"); AudioPlay.Instance.PlayClick(); });
        btn_orange.onClick.AddListener(() => { ChangeColorHandler("orange"); AudioPlay.Instance.PlayClick(); });
        btn_red.onClick.AddListener(() => { ChangeColorHandler("red"); AudioPlay.Instance.PlayClick(); });

        btn_purple.onClick.AddListener(() => { ChangeColorHandler("purple"); AudioPlay.Instance.PlayClick(); });
        btn_blue.onClick.AddListener(() => { ChangeColorHandler("blue"); AudioPlay.Instance.PlayClick(); });
        btn_green.onClick.AddListener(() => { ChangeColorHandler("green"); AudioPlay.Instance.PlayClick(); });


        btn_thick.onClick.AddListener(() => { ChangThicknessHandler(10); AudioPlay.Instance.PlayClick(); });
        btn_normal.onClick.AddListener(() => { ChangThicknessHandler(5); AudioPlay.Instance.PlayClick(); });
        btn_slime.onClick.AddListener(() => { ChangThicknessHandler(2); AudioPlay.Instance.PlayClick(); });

        //ChangeColorHandler("transparent");
        //ChangThicknessHandler(50);
        //AudioPlay.Instance.PlayClick();
        btn_eraser.onClick.AddListener(() => {
            var drawable = GameObject.Find("Drawable").GetComponent<FreeDraw.Drawable>();
            drawable.ResetCanvas();
             });

        btn_complete.onClick.AddListener(onCloseGraffCanvasHandler);
    }

    public void HideDefaultImage()
    {
        img_default.gameObject.SetActive(false);
        
    }

    void ChangThicknessHandler(float thickness)
    {
        var setting = GameObject.Find("DrawingSettings").GetComponent<FreeDraw.DrawingSettings>();
        setting.SetMarkerWidth(thickness);
        rect_thickNessPlate.gameObject.SetActive(false);
    }

    void ChangeColorHandler(string color)
    {
        var setting = GameObject.Find("DrawingSettings").GetComponent<FreeDraw.DrawingSettings>();
        switch(color)
        {
            case "black":
                setting.SetMarkerColour(new Color(0, 0, 0, 1));
                break;
            case "gray":
                setting.SetMarkerColour(new Color(25f / 255, 4f / 255, 222f / 255, 1));
                break;
            case "white":
                setting.SetMarkerColour(new Color(3 / 255f, 226 / 255f, 240 / 255f, 1));
                break;
            case "yellow":
                setting.SetMarkerColour(new Color(247f / 255, 188f / 255, 0f / 255, 1));
                break;
            case "orange":
                setting.SetMarkerColour(new Color(225f / 255, 80f / 255, 33f / 255, 1));
                break;
            case "red":
                setting.SetMarkerColour(new Color(196f / 255, 0f / 255, 0f / 255, 1));
                break;
            case "purple":
                setting.SetMarkerColour(new Color(147f / 255, 89f / 255, 198f / 255, 1));
                break;
            case "blue":
                setting.SetMarkerColour(new Color(24f / 255, 117f / 255, 178f / 255, 1));
                break;
            case "green":
                setting.SetMarkerColour(new Color(45f / 255, 124f / 255, 81f / 255, 1));
                break;
            case "transparent":
                setting.SetMarkerColour(new Color(255f / 255, 255f / 255, 255f / 255, 0));
                break;
        }
        if(color != "transparent")
        {
            img_nowColor.sprite = Resources.Load<Sprite>("UI/Sprite/color_" + color);
        }
        
        rect_colorPlate.gameObject.SetActive(false);
    }

    void ShowLineThicknessHandler()
    {
        rect_colorPlate.gameObject.SetActive(false);
        rect_thickNessPlate.gameObject.SetActive(!rect_thickNessPlate.gameObject.activeSelf);
        AudioPlay.Instance.PlayClick();
    }

    void ShowColorPlateHandler()
    {
        rect_thickNessPlate.gameObject.SetActive(false);
        rect_colorPlate.gameObject.SetActive(!rect_colorPlate.gameObject.activeSelf);
        AudioPlay.Instance.PlayClick();
    }


    void onCloseGraffCanvasHandler()
    {
        Game.Instance.translateCanvas.Show();
        Game.Instance.translateCanvas.SetAfterTranslate("main");
        //Game.Instance.playing = false;
        //Game.Instance.graffCanvas.Hide(); 
        //Game.Instance.gameCanvas.Show();
        Game.Instance.gameCanvas.playing.gameObject.SetActive(true);
        Game.Instance.gameCanvas.home.gameObject.SetActive(false);
        AudioPlay.Instance.PlayClick();
    }
    protected override void OnShow()
    {
        img_default.gameObject.SetActive(true); 

        if (Game.Instance.camera != null)
            Game.Instance.camera.gameObject.SetActive(false);
        if (Game.Instance.draw_able != null)
            Game.Instance.draw_able.gameObject.SetActive(true);
        if (Game.Instance.draw_setting != null)
        {
            Game.Instance.draw_setting.gameObject.SetActive(true);
            var setting = GameObject.Find("DrawingSettings").GetComponent<FreeDraw.DrawingSettings>();
            setting.SetMarkerColour(new Color(0, 0, 0, 1));
            setting.SetMarkerWidth(2);
        }
        if (Game.Instance.draw_camera != null)
            Game.Instance.draw_camera.gameObject.SetActive(true);
    }

    protected override void OnHide()
    {
        

        if (Game.Instance.camera!=null)
            Game.Instance.camera?.gameObject.SetActive(true);
        //if (Game.Instance.draw_able != null)
        //    Game.Instance.draw_able?.gameObject.SetActive(false);
        if (Game.Instance.draw_setting != null)
            Game.Instance.draw_setting?.gameObject.SetActive(false);
        if (Game.Instance.draw_camera != null)
            Game.Instance.draw_camera?.gameObject.SetActive(false);
    }
}
