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

    [SerializeField]
    public ShowUpCharacters words;

    public List<Character>characters = new List<Character>();

    private Dictionary<int, List<Vector3>> m_positions = new Dictionary<int, List<Vector3>>();
    public List<Vector3> getPositions(int count)
    {
        return m_positions[count];
    }

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

        var list = new List<Vector3>();
        list.Add(transform.position);
        m_positions[1] = list;

        list = new List<Vector3>();
        list.Add(transform.position + Vector3.right * 0.251f);
        list.Add(transform.position - Vector3.right * 0.252f);
        m_positions[2] = list;

        list = new List<Vector3>();
        list.Add(transform.position + Vector3.right * 0.25f + Vector3.forward * 0.25f);
        list.Add(transform.position - Vector3.right * 0.25f + Vector3.forward * 0.25f);
        list.Add(transform.position - Vector3.forward * 0.25f);
        m_positions[3] = list;

        list = new List<Vector3>();
        list.Add(transform.position + Vector3.right * 0.25f + Vector3.forward * 0.25f);
        list.Add(transform.position - Vector3.right * 0.25f + Vector3.forward * 0.25f);
        list.Add(transform.position + Vector3.right * 0.25f - Vector3.forward * 0.25f);
        list.Add(transform.position - Vector3.right * 0.25f - Vector3.forward * 0.25f);
        m_positions[4] = list;
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
        //if (characters.Count > 1)
        //{
        //    // 位置排序

        //    var poses = new List<Vector3>();
        //    foreach (var kvp in positions)
        //    {
        //        poses.Add(kvp.Value);
        //    }
        //    for (var index = 0; index < characters.Count; index++)
        //    {
        //        var shortest = float.MaxValue;
        //        Vector3 nearstPosition = Vector3.zero;
        //        var nearestIndex = -1;
        //        for (var jndex = 0; jndex < poses.Count; jndex++)
        //        {
        //            var distance = Vector3.Distance(characters[index].transform.GetChild(0).position, poses[jndex]);
        //            if (distance < shortest)
        //            {
        //                shortest = distance;
        //                nearstPosition = poses[jndex];
        //                nearestIndex = jndex;
        //            }
        //        }
        //        if (nearestIndex != -1)
        //        {
        //            poses.RemoveAt(nearestIndex);
        //            positions[characters[index].Uid] = nearstPosition;
        //        }
        //    }

        var count = characters.Count;
        if (count < 1) return;
        for (var index = 0; index < characters.Count; index++)
        {
            var enemy = characters[index];
            if (positions.ContainsKey(enemy.Uid))
            {
                var position = positions[enemy.Uid];
                var child = enemy.transform.GetChild(0);
                child.position = Vector3.MoveTowards(child.position, position, 3 * Time.deltaTime);
            }
        }
        //}
        //else if (characters.Count == 1)
        //{
        //    var enemy = characters[0];
        //    var child = enemy.transform.GetChild(0);
        //    child.localPosition = Vector3.MoveTowards(child.localPosition, Vector3.zero, 2 * Time.deltaTime);
        //}
    }

    public void OnTriggerEnter(Collider other)
    {
        var enemy = other.transform.parent?.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (characters.IndexOf(enemy) == -1)
            {
                if( characters.Count < 2 )
                {
                    characters.Add(enemy);
                }
                else
                {
                    var chars = new List<Character>();
                    chars.Add(enemy);
                    for(var index = 0; index < characters.Count; index ++)
                    {
                        chars.Add(characters[index]);
                    }
                    characters = chars;
                }
            }
            RrefreshEnemyPosition();
            //Debug.Log("位置:" + gameObject.name + " 敌人数量:" + characters.Count);
        }

        var player = other.transform.parent?.GetComponent<Player>();
        if(player!= null)
        {
            if(words != null)
            {
                words.Show();
            }
        }

    }
    public void OnTriggerExit(Collider other)
    {
        var enemy = other.transform.parent?.GetComponent<Enemy>();
        if (enemy != null)
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


    private int characterCount = 0;
    
    public void SetCharacterCount(int count)
    {
        //characterCount = count;
    }

    public void AddCharacter(Enemy enemy)
    { 
        //if (characters.IndexOf(enemy) != -1) return;
        //characters.Add(enemy);
        //var poses = m_positions[characterCount];
        //for( var index = 0; index < poses.Count; index++ )
        //{
        //    var position = poses[index];
        //    if(enemy.bodyPositionOffset.Equals(position))
        //    {
        //         positions[enemy.Uid] = position;
        //    }
        //}
    }

    public void ClearCharacters()
    {
        //characters.Clear();
        //characterCount = 0;
    }

    Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>();
    //// List<Vector3> positions = new List<Vector3>();
    void RrefreshEnemyPosition()
    {
        positions.Clear();
        if(characters.Count<1)
        {
            return;
        }
        var poses = m_positions[characters.Count];
        var occupied = new List<Vector3>();
        for (var index = 0; index < characters.Count; index++)
        {
            var dis = float.MaxValue;
            var selectedPosition = Vector3.zero;
            var enemy = characters[index] as Enemy;
            enemy.moveDistance = 0;
            for (var posIndex = 0; posIndex < poses.Count; posIndex++)
            {
                var pos = poses[posIndex];
                if (occupied.IndexOf(pos) != -1)
                {
                    continue;
                }
                var testDis = Vector3.Distance(enemy.tr_body.GetChild(0).position, pos);
                if (testDis < dis )
                {
                    dis = testDis;
                    selectedPosition = pos;
                }
            }
            if (!selectedPosition.Equals(Vector3.zero))
            {
                positions[enemy.Uid] = selectedPosition;
                enemy.bodyPositionOffset = selectedPosition;
                enemy.moveDistance = dis;
                occupied.Add(selectedPosition);
                // enemy.db_moves[0].transform.position = selectedPosition;
            }
        }
        for (var index = 0; index < characters.Count; index++)
        {
            var enemy = characters[index] as Enemy;
            if (enemy != null)
            {
                if (enemy.moveDistance > 1.1f)
                {
                    //Debug.Log("出现换位出现换位出现换位" + enemy.moveDistance + "  enemy uid:" + enemy.Uid);
                    //Debug.Break();
                    var farestPosition = positions[enemy.Uid];
                    Enemy nearestEnemy = null;
                    var nearestDistance = float.MaxValue;
                    for(var d = 0; d < characters.Count; d++)
                    {
                        var targetEnemy = characters[d] as Enemy;
                        if (targetEnemy.Uid == enemy.Uid) continue;
                        if(nearestDistance > targetEnemy.moveDistance)
                        {
                            nearestEnemy = targetEnemy;
                            nearestDistance = targetEnemy.moveDistance;
                        }
                    }
                    if(nearestEnemy != null)
                    {
                        var nearestPosition = positions[nearestEnemy.Uid];

                        positions[nearestEnemy.Uid] = farestPosition;
                        positions[enemy.Uid] = nearestPosition;

                        // Debug.Log("修正换位");
                    }
                    break;
                }
                //Debug.Log("敌人的偏移量:" + "  " + enemy.Uid + "  " + (enemy.bodyPositionOffset - this.transform.position));
                //Debug.Log("敌人的移动的距离:" + "  " + enemy.Uid + "  " + enemy.moveDistance);
            }
        }
    }


    public void Red()
    {
        var targetIconRenderer = targetIcon.GetComponent<MeshRenderer>();
        targetIconRenderer.transform.localScale = new Vector3(Enemy.RED_SCALE, 1, Enemy.RED_SCALE);
        //targetIconRenderer.transform.position = node.transform.position;
        targetIconRenderer.transform.Translate(new Vector3(0,0.012f,0));
        targetIconRenderer.material = Resources.Load<Material>("Material/RouteRed");
        contour.gameObject.SetActive(false);
        sphereCollider.gameObject.SetActive(false);
    }
}
