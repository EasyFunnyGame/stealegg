using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardNode : MonoBehaviour
{
    [SerializeField]
    public Transform targetIcon;

    [SerializeField]
    public Transform contour;

    [SerializeField]
    public float height;

    [SerializeField]
    public Coord coord;

    public void initWithTransform(Transform node )
    {
        var baseSquare = node.GetChild(0);
        targetIcon = baseSquare.transform.Find("TargetIcon");
        contour = baseSquare.transform.Find("contour");
        height = float.Parse(targetIcon.transform.position.y.ToString("#0.0"));

        coord = new Coord(transform.position);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
