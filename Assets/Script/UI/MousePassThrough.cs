using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MousePassThrough : MonoBehaviour, IPointerDownHandler , IPointerUpHandler , IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Action<PointerEventData> OnBeginDragAction;

    public Action<PointerEventData> OnEndDragAction;

    public Action<PointerEventData> OnDragAction;

    public Action<PointerEventData> OnPointerDownAction;

    public Action<PointerEventData> OnPointerUpAction;

    public Action<PointerEventData> OnPointerClickAction;


    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragAction?.Invoke(eventData);
        Psss(eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragAction?.Invoke(eventData);
        Psss(eventData, ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragAction?.Invoke(eventData);
        Psss(eventData, ExecuteEvents.endDragHandler);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerClickAction?.Invoke(eventData);
        Psss(eventData, ExecuteEvents.pointerClickHandler);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownAction?.Invoke(eventData);
        Psss(eventData, ExecuteEvents.pointerDownHandler);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Psss(eventData, ExecuteEvents.pointerUpHandler);
    }

    private bool hasPassedEvent = false;
    public void Psss<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
            where T : IEventSystemHandler
    {
        if (hasPassedEvent) return;
        hasPassedEvent = true;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;

        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            if (result.gameObject == null)
            {
                continue;
            }
            if (current != result.gameObject)
            {
                //if (passMultiTimes)
                //{
                //    ExecuteEvents.Execute(result.gameObject, data, function);
                //}
                //else
                //{
                    if (ExecuteEvents.Execute(result.gameObject, data, function))
                    {
                        break;
                    }
                //}
            }
        }
        results.Clear();
        hasPassedEvent = false;
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
