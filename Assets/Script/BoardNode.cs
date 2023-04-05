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

        if (characters.Count > 1)
        {
            var count = characters.Count;
            for(var index = 0; index < characters.Count; index++)
            {
                var enemy = characters[index];
                if (positions.ContainsKey(enemy.Uid))
                {
                    var position = positions[enemy.Uid];
                    var child = enemy.transform.GetChild(0);
                    child.position = Vector3.MoveTowards(child.position, position, 2 * Time.deltaTime);
                }
            }
        }
        else if(characters.Count == 1)
        {
            var enemy = characters[0];
            var child = enemy.transform.GetChild(0);
            child.localPosition = Vector3.MoveTowards(child.localPosition, Vector3.zero, 2 * Time.deltaTime);
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
            RrefreshEnemyPosition();
            //Debug.Log("位置:" + gameObject.name + " 敌人数量:" + characters.Count);
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
            RrefreshEnemyPosition();
            //Debug.Log("位置:"+ gameObject.name + " 敌人数量:" + characters.Count);
        }
    }

    Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>();

    void RrefreshEnemyPosition()
    {
        positions.Clear();
        var position = transform.position;
        Enemy enemy = null;
        if (characters.Count == 2)
        {
            enemy = (characters[0] as Enemy);
            if (enemy != null)
            {
                positions.Add(enemy.Uid, position + transform.right * 0.25f);
            }
            enemy = (characters[1] as Enemy);
            if (enemy != null)
            {
                positions.Add(enemy.Uid, position - transform.right * 0.25f);
            }
        }
        else if (characters.Count == 3)
        {
            enemy = (characters[0] as Enemy);
            if (enemy != null)
            {
                positions.Add(enemy.Uid, position + transform.right * 0.25f + transform.forward * 0.25f);
            }
            enemy = (characters[1] as Enemy);
            if (enemy != null)
            {
                positions.Add(enemy.Uid, position - transform.right * 0.25f + transform.forward * 0.25f);
            }
            enemy = (characters[2] as Enemy);
            if (enemy != null)
            {
                positions.Add(enemy.Uid, position - transform.forward * 0.25f);
            }
        }
        else if (characters.Count > 3)
        {
            enemy = (characters[0] as Enemy);
            if (enemy != null)
            {
                positions.Add(enemy.Uid, position + transform.right * 0.25f + transform.forward * 0.25f);
            }
            enemy = (characters[1] as Enemy);
            if (enemy != null)
            {
                positions.Add(enemy.Uid, position - transform.right * 0.25f + transform.forward * 0.25f);
            }
            enemy = (characters[2] as Enemy);
            if (enemy != null)
            {
                positions.Add(enemy.Uid, position + transform.right * 0.25f - transform.forward * 0.25f);
            }
            enemy = (characters[3] as Enemy);
            if (enemy != null)
            {
                positions.Add(enemy.Uid, position - transform.right * 0.25f - transform.forward * 0.25f);
            }
        }
    }

    public void Red()
    {
        var targetIconRenderer = targetIcon.GetComponent<MeshRenderer>();
        targetIconRenderer.transform.localScale = new Vector3(RED_SCALE, 1, RED_SCALE);
        targetIconRenderer.transform.position = node.transform.position;
        targetIconRenderer.Translate(new Vector3(0,0.012f,0));
        targetIconRenderer.material = Resources.Load<Material>("Material/RouteRed");
        contour.gameObject.SetActive(false);
        sphereCollider.gameObject.SetActive(false);
    }
}
