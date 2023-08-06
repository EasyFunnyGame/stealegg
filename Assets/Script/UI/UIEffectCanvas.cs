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

    public TrailRenderer m_trail;

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
        ui_mouse_pass_through.OnPointerClickAction = PointerClick;
        ui_mouse_pass_through.OnBeginDragAction = OnBeginDrag;
        ui_mouse_pass_through.OnEndDragAction = OnEndDrag;
        ui_mouse_pass_through.OnDragAction = OnMouseDrag;
    }

    protected override void OnHide()
    {
        base.OnHide();
    }


    public void PointerClick(PointerEventData eventData)
    {
        if (draging) return;
        var position = Input.mousePosition;
        m_click_effect.transform.position = position;
        m_click_effect.Stop();
        m_click_effect.Play();
        m_click_effect.gameObject.SetActive(true);
    }

    bool draging = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        var position = Input.mousePosition;
        m_trail.transform.position = position;
        m_trail.Clear();
        draging = true;
        //m_trail.gameObject.SetActive(true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        draging = false;
        // m_trail.gameObject.SetActive(false);
    }

    public void OnMouseDrag(PointerEventData eventData)
    {
        var position = Input.mousePosition;
        m_trail.transform.position = position;
    }
}
