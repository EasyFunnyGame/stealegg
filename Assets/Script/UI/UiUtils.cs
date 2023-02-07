using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiUtils
{
    public static void Adaptive(Image image, RectTransform canvasRect)
    {
        float width = canvasRect.sizeDelta.x;
        float xyper = image.preferredWidth / image.preferredHeight;
        float height = width / xyper;
        image.rectTransform.sizeDelta = new Vector2(width, height);
    }

    public static void WorldToScreenPoint(Camera camera,BaseCanvas canvas, Vector3 position, out Vector3 point)
    {
        point = camera.WorldToScreenPoint(position);
        var canvasRt = canvas.GetComponent<RectTransform>();
        float resolutionRotioWidth = canvasRt.sizeDelta.x;
        float resolutionRotioHeight = canvasRt.sizeDelta.y;
        float widthRatio = resolutionRotioWidth / Screen.width;
        float heightRatio = resolutionRotioHeight / Screen.height;
        point.x *= widthRatio;
        point.y *= heightRatio;

        point.x -= resolutionRotioWidth / 2;
        point.y -= resolutionRotioHeight / 2;

        //Debug.Log("玩家位置在屏幕上的位置[" + playerPosOnScreen.x + "]  " + "[" + playerPosOnScreen.y + "]");
    }

}
