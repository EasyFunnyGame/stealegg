using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManholeCoverItem : Item
{
    public List<ManholeCoverItem> links;

    public Animator m_animator;

    public void JumpOut( )
    {
        Debug.Log("跳出来");
    }

}
