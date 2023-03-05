using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconsAboveEnemy : IconOnUI
{
    public Enemy enemy;

    public GameObject tanhao;
    public GameObject wenhao;
    public GameObject fanhui;
    public GameObject shuijiao;
    public GameObject ccw;
    public GameObject cw;


    private void Awake()
    {
        tanhao.gameObject.SetActive(false);
        wenhao.gameObject.SetActive(false);
        fanhui.gameObject.SetActive(false);
        shuijiao.gameObject.SetActive(false);
        ccw.gameObject.SetActive(false);
        cw.gameObject.SetActive(false);
    }
}
