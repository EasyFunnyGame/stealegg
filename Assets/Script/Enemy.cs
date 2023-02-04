using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Static,
    Distracted,
    Patrol,
    Sentinel
}

public static class EnemyName
{
    public const string Enemy_Distracted = "Enemy_Distracted";
    public const string Enemy_Patrol = "Enemy_Patrol";
    public const string Enemy_Sentinel = "Enemy_Sentinel";
    public const string Enemy_Static = "Enemy_Static"; 
}

public class Enemy : Character
{
    [SerializeField]
    public EnemyType enemyType;

    public Tile tracingTile = null;

    public Animator animator;


    public static int count;
    

    // Start is called before the first frame update

    void Awake()
    {
        base.Awake ();
    }

    void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    public override void Update()
    {
        if (Game.Instance.status != GameStatus.PLAYING) return;

        if(targetDirection != direction && tracingTile != null)
        {
            //Debug.Log("转向目标方向" + targetDirection + " -- -- " + direction);
            Vector3 tar_dir = tracingTile.transform.position - tr_body.position;

            if (targetDirection == Direction.Up)
            {
                tar_dir = new Vector3(0,0,1);
            }
            else if (targetDirection == Direction.Down)
            {
                tar_dir = new Vector3(0, 0, -1);
            }
            else if (targetDirection == Direction.Left)
            {
                tar_dir = new Vector3(-1, 0, 0);
            }
            else if (targetDirection == Direction.Right)
            {
                tar_dir = new Vector3(1, 0, 0);
            }

            Vector3 new_dir = Vector3.RotateTowards(tr_body.forward, tar_dir, rotate_speed * Time.deltaTime / 2, 0f);
            new_dir.y = 0;
            tr_body.transform.rotation = Quaternion.LookRotation(new_dir);

            var angle = Vector3.Angle(tar_dir, tr_body.forward);
            if (angle <= 1)
            {
                ResetDirection();
                body_looking = false;
            }
            return;
        }
        if (Game.Instance.turn != Turn.ENEMY) return;


        if (selected_tile_s != null && !moving && tile_s != selected_tile_s && selected_tile_s != null)
        {
            if (selected_tile_s.db_path_lowest.Count > 0)
                move_tile(selected_tile_s);
            else
                print("no valid tile selected");
        }

        if (body_looking)
        {
            Vector3 tar_dir = db_moves[1].position - tr_body.position;
            Vector3 new_dir = Vector3.RotateTowards(tr_body.forward, tar_dir, rotate_speed * Time.deltaTime / 2, 0f);
            new_dir.y = 0;
            tr_body.transform.rotation = Quaternion.LookRotation(new_dir);

            var angle = Vector3.Angle(tar_dir, tr_body.forward);
            if (angle <= 1)
            {
                ResetDirection();
                body_looking = false;
                OnDirectionRested();
                StartMove();
            }

            return;
        }


        if (moving)
        {
            float step = move_speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, db_moves[0].position, step);
            var tdist = Vector3.Distance(tr_body.position, db_moves[0].position);
            if (tdist < 0.001f)
            {
                //tile_s.db_chars.Remove(this);
                tile_s = tar_tile_s.db_path_lowest[num_tile];
                //tile_s.db_chars.Add(this);
                if (moving_tiles && num_tile < tar_tile_s.db_path_lowest.Count - 1)
                {
                    num_tile++;
                    var tpos = tar_tile_s.db_path_lowest[num_tile].transform.position;
                    if (big) //Large chars//
                    {
                        tpos = new Vector3(0, 0, 0);
                        tpos += tar_tile_s.db_path_lowest[num_tile].transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[2].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
                        tpos /= 4; //Takes up 4 tiles//
                    }
                    tpos.y = transform.position.y;
                    db_moves[0].position = tpos;
                    nextTile = tar_tile_s.db_path_lowest[num_tile];
                    db_moves[1].position = tpos;
                }
                else
                {
                    db_moves[4].gameObject.SetActive(false);
                    moving = false;
                    moving_tiles = false;
                }
                Reached();
            }
        }
    }


    public override void OnDirectionRested()
    {
        base.OnDirectionRested();
    }

    public void Alert(string tileName)
    {
        Debug.Log("警觉" + tileName);
        var targetTile = gridManager.GetTileByName(tileName); 
        if(targetTile!=null)
        {
            //ClearPath();
            tracingTile = targetTile;
            selected_tile_s = tracingTile;
            gridManager.find_paths_realtime(this, tracingTile);
            hasAction = true;
            UpdateMoves(targetTile);
            //UpdateTargetDirection(targetTile);
            animator.Play("Enemy_Alert");
        }
    }

    public virtual void CheckAction()
    {
        //Debug.Log("检查行为");
        if(tracingTile!=null )
        {
            if(tile_s != tracingTile)
            {
                hasAction = true;
            }
            else
            {
                if (tracingTile.name != originalCoord.name)
                {
                    tracingTile = gridManager.GetTileByName(originalCoord.name);
                    if(tracingTile!=null)
                    {
                        selected_tile_s = tracingTile;
                        gridManager.find_paths_realtime(this, tracingTile);
                        UpdateMoves(tracingTile);
                        hasAction = true;
                    }
                }
                //else
                //{
                //    targetDirection = originalDirection;
                //}
            }
        }
    }

    public List<Transform> redLines = new List<Transform>();

    public List<MeshRenderer> redNodes = new List<MeshRenderer>();

    public void RedLineByName(string nodeName1, string nodeName2)
    {
        var line = boardManager.FindLine(nodeName1, nodeName2);

        if (line != null)
        {
            var lineTr = line.transform;
            var copyLine = Instantiate(lineTr);
            copyLine.transform.position = lineTr.position;
            copyLine.transform.Translate(new Vector3(0, 0.001f, 0));
            copyLine.transform.rotation = lineTr.rotation;
            redLines.Add(copyLine);
            copyLine.GetChild(0).GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/RouteRed");
        }
    }

    public void RedNodeByName(string nodeName)
    {
        var node = boardManager.FindNode(nodeName);

        if (node != null)
        {
            var nodeTr = node.targetIcon;
            var copyNode = Instantiate(nodeTr.GetComponent<MeshRenderer>());
            redNodes.Add(copyNode);
            copyNode.transform.position = nodeTr.transform.position;
            copyNode.transform.Translate(new Vector3(0, 0.001f, 0));
            copyNode.transform.rotation = nodeTr.transform.rotation;
            copyNode.material = Resources.Load<Material>("Material/RouteRed");
        }
    }

    protected override void OnReached()
    {
        base.OnReached();

        if(Player.Instance.tile_s)
        {
            Player.Instance.CheckBottle();
            Player.Instance.CheckWhistle();
        }
        

        // 回到起点
        if (originalCoord.name == coord.name && tracingTile.name == coord.name)
        {
            targetDirection = originalDirection;
        }
    }

    public virtual void UpdateRouteMark()
    {

    }

    public virtual void TryCatch()
    {

    }

    public virtual void TryTrace()
    {

    }

}
