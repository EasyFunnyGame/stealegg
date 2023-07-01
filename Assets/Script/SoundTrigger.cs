using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        None,
        ThroughWireNet,
        ThroughCuttedWireNet,
    }

    public TriggerType type;

     protected void OnTriggerEnter(Collider other)
    {
        if (other?.transform.parent?.name != "Player") return;
        switch (type)
        {
            case TriggerType.ThroughWireNet:
                AudioPlay.Instance?.ThroughWireNet();
                break;

            case TriggerType.ThroughCuttedWireNet:
                AudioPlay.Instance?.ThroughCuttedWireNet();
                break;
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        //if (other?.transform.parent?.name != "Player") return;
    }
}
