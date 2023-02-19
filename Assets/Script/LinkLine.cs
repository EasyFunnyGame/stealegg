using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkLine : MonoBehaviour
{
    public string node1;

    public string node2;

    public bool through = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool LinkingNode(string nodeName1, string nodeName2)
    {
        return (node1 == nodeName1 && node2 == nodeName2) || (node2 == nodeName1 && node1 == nodeName2);
    }
}
