using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardNodeTrigger : MonoBehaviour
{
    public BoardNode node;
    
    public void OnTriggerEnter(Collider other)
    {
        node.OnTriggerEnter(other);
    }

    public void OnTriggerExit(Collider other)
    {
        node.OnTriggerExit(other);
    }
}
