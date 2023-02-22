using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PincersItem : Item
{
    public LinkLine linkline;

    public MeshRenderer wireNetMesh;

    private void Awake()
    {
        if(linkline!=null)
        {
            linkline.through = false;
        }
    }

    public void Cut()
    {
        linkline.through = true;

        var dotLinePrefab = Resources.Load("Prefab/Hor_Doted_Visual");
        Instantiate(dotLinePrefab, linkline.transform);

        var originalLine = linkline.transform.GetChild(0);
        originalLine?.gameObject.SetActive(false);

        var texture = Resources.Load<Texture>("Texture/grillage_03_cutted");
        wireNetMesh?.material.SetTexture("_MainTex", texture);

    }

}
