using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraffCanvas : BaseCanvas
{
    public Camera cam;

    public GameObject drawOn;

    public GameObject drawSetting;

    public Button btn_clz;

    private void Awake()
    {
        btn_clz.onClick.AddListener(onCloseGraffCanvasHandler);
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
        if(cam.gameObject.activeSelf==false)
        {
            cam.gameObject.SetActive(true);
            drawOn.gameObject.SetActive(true);
            drawSetting.gameObject.SetActive(true);
        }
    }

    protected override void OnHide()
    {
        if(Game.Instance.camera && Game.Instance.camera.gameObject.activeSelf==false)
        {
            Game.Instance.camera.gameObject.SetActive(true);
            drawOn.gameObject.SetActive(false);
            drawSetting.gameObject.SetActive(false);
            cam.gameObject.SetActive(false);
            Game.Instance.gameCanvas.Show();
        }
    }


}
