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

    public List<Character>characters = new List<Character>();

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

    float counturScale = 1;
    // Update is called once per frame
    void Update()
    {

        if (contour.gameObject.activeSelf)
        {
            counturScale = Mathf.Abs(Mathf.Sin(Time.time * 2))*0.05f;
            contour.transform.localScale = new Vector3(0.15f + counturScale, 0.15f + counturScale, 0.15f + counturScale);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter Board Node:" + other.gameObject.name);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit Board Node:" + other.gameObject.name);
    }
}
