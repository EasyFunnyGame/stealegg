using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIEffectCanvas : BaseCanvas
{

    public Image m_image;

    public Animation m_click_effect;

    public MousePassThrough ui_mouse_pass_through;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    protected override void OnShow()
    {
        base.OnShow();
        m_click_effect.gameObject.SetActive(false);
        ui_mouse_pass_through.OnPointerDownAction = PointerClick;
    }

    protected override void OnHide()
    {
        base.OnHide();
    }


    public void PointerClick(PointerEventData eventData)
    {
        var position = Input.mousePosition;
        m_click_effect.transform.position = position;
        m_click_effect.Stop();
        m_click_effect.Play();
        m_click_effect.gameObject.SetActive(true);
    }

    public void OnBeginDrag()
    {

    }

    public void OnEndDrag()
    {

    }
}
