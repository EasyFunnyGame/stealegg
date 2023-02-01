using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvas : MonoBehaviour
{
    public Button btn_start;
    public Button btn_setting;

    private void Awake()
    {
        btn_start.onClick.AddListener(StartGame);
        btn_setting.onClick.AddListener(ShowSettingCanvas);
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
        Debug.Log("start game");
        var level = PlayerPrefs.GetInt("Level");
    }

    void ShowSettingCanvas()
    {
        Debug.Log("show setting canvas");
    }
}
