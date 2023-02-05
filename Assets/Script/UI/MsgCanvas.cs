using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MsgCanvas : BaseCanvas
{
    public GameObject content;

    public Image img_bg;

    public TMPro.TextMeshProUGUI txt_msg;

    public float hideCd = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(content.activeSelf)
        {
            hideCd -= Time.deltaTime;
            if(hideCd<=0)
            {
                content.gameObject.SetActive(false);
            }
            img_bg.rectTransform.sizeDelta = new Vector2(txt_msg.preferredWidth + 20,50);
        }
    }

    protected override void OnShow()
    {

    }

    protected override void OnHide()
    {
    }

    public void PopMessage(string msg)
    {
        hideCd = 2;
        content.gameObject.SetActive(true);
        txt_msg.text = msg;
    }
}
