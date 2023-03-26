using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemIconOnUI : IconOnUI 
{
    public Item item;

    public Image icon;
    // Start is called before the first frame update
    void Start()
    {
        if(transform.childCount>1)
        {
            icon = transform.GetChild(1).GetComponent<Image>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(teaching)
        {
            updateTime += (Time.deltaTime * 2.5f);
            iconScale = 1 + 0.25f * Mathf.Sin(updateTime);
            transform.localScale = new Vector3(iconScale, iconScale, iconScale);
        }
    }
    float iconScale = 1f;
    float updateTime = 0;
    public virtual void ShowGuide()
    {
        iconScale = 1;
        updateTime = 0;
        teaching = true;
        icon.transform.localScale = new Vector3(1, 1, 1);
    }
    bool teaching = false;
    public virtual void HideGuide()
    {
        iconScale = 1;
        teaching = false;
        icon.transform.localScale = new Vector3(1, 1, 1);
    }
}
