using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraffItem : Item
{

    public GameObject sceneGameObject;

    protected override void Awake()
    {
        base.Awake();
        sceneGameObject = GameObject.Find("Target");
    }
}
