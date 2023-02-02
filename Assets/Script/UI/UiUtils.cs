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
}
