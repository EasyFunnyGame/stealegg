using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingCanvas : MonoBehaviour
{
    public Button btn_clz;

    private void Awake()
    {
        btn_clz.onClick.AddListener(this.Hide);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void Hide()
    {
        Game.Instance.settingCanvas.gameObject.SetActive(false);
    }
}
