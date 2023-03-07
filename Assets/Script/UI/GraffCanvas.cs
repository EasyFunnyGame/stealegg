﻿using System.Collections;
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


        btn_changeColor.onClick.AddListener(ShowColorPlateHandler);
        btn_thickness.onClick.AddListener(ShowLineThicknessHandler);

        btn_black.onClick.AddListener(() => { ChangeColorHandler("black"); });
        btn_gray.onClick.AddListener(() => { ChangeColorHandler("gray"); });
        btn_white.onClick.AddListener(() => { ChangeColorHandler("white"); });

        btn_yellow.onClick.AddListener(() => { ChangeColorHandler("yellow"); });
        btn_orange.onClick.AddListener(() => { ChangeColorHandler("orange"); });
        btn_red.onClick.AddListener(() => { ChangeColorHandler("red"); });

        btn_purple.onClick.AddListener(() => { ChangeColorHandler("purple"); });
        btn_blue.onClick.AddListener(() => { ChangeColorHandler("blue"); });
        btn_green.onClick.AddListener(() => { ChangeColorHandler("green"); });


        btn_thick.onClick.AddListener(() => { ChangThicknessHandler(10); });
        btn_normal.onClick.AddListener(() => { ChangThicknessHandler(5); });
        btn_slime.onClick.AddListener(() => { ChangThicknessHandler(2); });

        btn_eraser.onClick.AddListener(() => { ChangeColorHandler("transparent"); });

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
                setting.SetMarkerColour(new Color(142f / 255, 142f / 255, 142f / 255, 1));
                break;
            case "white":
                setting.SetMarkerColour(new Color(0.8f, 0.8f, 0.8f, 1));
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
    }

    void ShowColorPlateHandler()
    {
        rect_thickNessPlate.gameObject.SetActive(false);
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
        Game.Instance.graffCanvas.Hide(); Game.Instance.gameCanvas.Show();
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
