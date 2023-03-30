using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PincersItem : Item
{
    public LinkLine linkline;

    public MeshRenderer wireNetMesh;

    override protected void Awake()
    {
        base.Awake();
        if (linkline!=null)
        {
            linkline.through = false;
        }
        wireNetMesh.transform.parent = null;
    }

    public void Cut()
    {
        linkline.through = true;
        linkline.traversability = TraversabilityOptions.Fenced;

        var dotLinePrefab = Resources.Load("Prefab/Hor_Doted_Visual");
        var copy = Instantiate(dotLinePrefab, linkline.transform);
        copy.name = "Hor_Doted_Visual";
        var originalLine = linkline.transform.GetChild(0);
        originalLine?.gameObject.SetActive(false);

        var texture = Resources.Load<Texture>("Texture/grillage_03_cutted");
        wireNetMesh?.material.SetTexture("_MainTex", texture);

    }

}
