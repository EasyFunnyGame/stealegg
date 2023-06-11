using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowthItem : Item
{
    override protected void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.GetComponent<Character>())
        upper = true;
        velocity = Vector3.zero;
        if (other?.transform.parent?.name == "Player")
        {
            AudioPlay.Instance.HideInTree();
        }
    }

    override protected void OnTriggerExit(Collider other)
    {
        //if (other.gameObject.GetComponent<Character>())
        upper = false;
        velocity = Vector3.zero;
        if (other?.transform.parent?.name == "Player")
        {
            AudioPlay.Instance.WalkOutTree();
        }
    }
}
    