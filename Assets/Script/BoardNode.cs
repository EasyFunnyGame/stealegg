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
    public SphereCollider sphereCollider;

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

    private void Awake()
    {
        var baseSquare = transform.GetChild(0);
        sphereCollider = baseSquare.transform.Find("Square_Sphere").GetComponent<SphereCollider>();
        var trigger = sphereCollider.GetComponent<BoardNodeTrigger>();
        if (trigger == null)
        {
            trigger = sphereCollider.gameObject.AddComponent<BoardNodeTrigger>();
        }
        trigger.node = this;
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

    public void OnTriggerEnter(Collider other)
    {
        var enemy = other.transform.parent?.GetComponent<Enemy>();
        if(enemy != null)
        {
            if(characters.IndexOf(enemy)==-1)
            {
                characters.Add(enemy);
            }
            Debug.Log("位置:" + gameObject.name + " 敌人数量:" + characters.Count);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        var enemy = other.transform.parent?.GetComponent<Enemy>();
        if(enemy != null)
        {
            
            var index = characters.IndexOf(enemy);
            if (index != -1)
            {
                characters.RemoveAt(index);
            }

            Debug.Log("位置:"+ gameObject.name + " 敌人数量:" + characters.Count);
        }
        
    }
}
