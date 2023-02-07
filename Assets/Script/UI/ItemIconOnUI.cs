using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIconOnUI : IconOnUI 
{
    public Item item;

    public RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
