using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Character : MonoBehaviour
{
    public GridManager gridManager;
    public bool big;
    public bool body_looking;
    public bool moving;
    public bool moving_tiles;
    public float move_speed = 1f;
    public float rotate_speed = 1f;
    public Color col;
    public Transform tr_body;
    public Tile tile_s;
    public Tile tar_tile_s;
    public Tile selected_tile_s;
    public Tile nextTile;
    public List<Transform> db_moves;
    public int max_tiles = 7;
    public int num_tile;
    public List<Tile> path;
    protected BoardManager boardManager;

    public Direction direction = Direction.Up;

    public bool hasAction = false;

    public void Awake()
    {
        ResetDirection();
        
    }

    public void Start()
    {
        var boardManagerGo = GameObject.Find("BoardManager");
        boardManager = boardManagerGo.GetComponent<BoardManager>();

        var gridManagerGo = GameObject.Find("GridManager");

        if(gameObject.name != "Player")
        {
            var gridManagerCopy = Instantiate(gridManagerGo);
            gridManagerCopy.name = "GridManager_" + gameObject.name;
            gridManager = gridManagerCopy.GetComponent<GridManager>();
        }
        else
        {
            gridManager = gridManagerGo.GetComponent<GridManager>();
        }

        var x = int.Parse(transform.position.x.ToString());
        var z = int.Parse(transform.position.z.ToString());
        var tile = gridManager.GetTileByName(string.Format("{0}_{1}", x, z));
        tile_s = tile;

        Reached();
    }

    public void Update()
    {
        if ( selected_tile_s != null && !moving && tile_s != selected_tile_s && selected_tile_s != null)
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
        }

        if (moving)
        {
            float step = move_speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, db_moves[0].position, step);
            var tdist = Vector3.Distance(tr_body.position, db_moves[0].position);
            if (tdist < 0.001f)
            {
                tile_s.db_chars.Remove(this);
                tile_s = tar_tile_s.db_path_lowest[num_tile];
                tile_s.db_chars.Add(this);
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
                    if (gridManager.find_path == efind_path.once_per_turn || gridManager.find_path == efind_path.max_tiles)
                        gridManager.find_paths_static(this);
                    //gm_s.hover_tile(selected_tile_s);
                }
                Reached();
            }
        }
    }

    public void move_tile(Tile ttile)
    {
        num_tile = 0;
        tar_tile_s = ttile;

        //0 - body_move, 1 - body_look, 2 - head_look, 3 - eyes_look, target tile marker
        db_moves[0].parent = null;
        db_moves[1].parent = null;
        db_moves[4].parent = null;

        //move_speed = 1;

        var tpos = new Vector3(0, 0, 0);
        if (!big)
        {
            tpos = tar_tile_s.transform.position;
            
        }
        else
        if (big)
        {
            tpos += tar_tile_s.transform.position + tar_tile_s.db_neighbors[1].tile_s.transform.position + tar_tile_s.db_neighbors[2].tile_s.transform.position + tar_tile_s.db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
            tpos /= 4;
        }

        db_moves[4].position = tpos; //Tar Tile Marker//
        db_moves[4].gameObject.SetActive(true); //Tar Tile Marker//

        tpos = new Vector3(0, 0, 0);
        if (!big)
        {
            tpos += tar_tile_s.db_path_lowest[num_tile].transform.position;
            nextTile = tar_tile_s.db_path_lowest[num_tile];
        }
        else
        if (big)
        {
            tpos += tar_tile_s.db_path_lowest[num_tile].transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[2].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
            tpos /= 4;
            nextTile = tar_tile_s.db_path_lowest[num_tile];
        }

        tpos.y = transform.position.y;
        db_moves[0].position = tpos;
        
        db_moves[1].position = tpos;

        moving = true;
        moving_tiles = true;
        body_looking = true;
        
        StartMove();
    }

    public void ClearPath()
    {
        gridManager.ClearPath(this);
    }

    public void FindPathRealTime(Tile t)
    {
        gridManager.find_paths_realtime(this, t);
    }

    protected virtual void ResetDirection()
    {
        var rotateY = transform.localRotation.eulerAngles.y;
        direction = (Direction)System.Enum.Parse(typeof(Direction), ((rotateY -= (rotateY %= 90)) / 90).ToString(), true);
        //Debug.Log(gameObject.name + "方向:" + direction);
    }
    public void Reached()
    {
        ResetDirection();
        OnReached();
    }

    virtual protected void OnReached()
    {
        
    }

    public void StartMove()
    {
        OnStartMove();
    }

    virtual protected void OnStartMove()
    {
        
    }


    #region 动画事件回调
    public virtual void FootL()
    {

    }

    public virtual void FootR()
    {

    }

    #endregion
}
