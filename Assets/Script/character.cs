using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Character : MonoBehaviour
{
    public GridManager gridManager;
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
    protected BoardManager boardManager;
    public Direction direction = Direction.Up;
    public bool rotation = false;
    public bool reachedTile = false;

    public Direction originalDirection = Direction.Up;

    public Direction targetDirection = Direction.Up;

    public Coord originalCoord;

    public Coord coord;


    public List<Tile> path = new List<Tile>();

    public void Awake()
    {
       
    }

    public void Start()
    {
        var boardManagerGo = GameObject.Find("BoardManager");
        boardManager = boardManagerGo.GetComponent<BoardManager>();

        var gridManagerGo = GameObject.Find("GridManager");

        if(gameObject.name != "Player")
        {
            gridManagerGo = GameObject.Find("GridManager_Enemy");
            if (Enemy.count == 0)
            {
                gridManager = gridManagerGo.GetComponent<GridManager>();
            }
            else
            {
                var gridManagerCopy = Instantiate(gridManagerGo);
                gridManagerCopy.name += gameObject.name;
                gridManagerCopy.name += Enemy.count;
                gridManager = gridManagerCopy.GetComponent<GridManager>();
            }
            Enemy.count++;
           
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
        originalDirection = direction;
        originalCoord = coord.Clone();
    }

    public virtual void Update()
    {
        if (Game.Instance.status != GameStatus.PLAYING) return;


        //if (selected_tile_s != null && !moving && tile_s != selected_tile_s && selected_tile_s != null)
        //{
        //    if (selected_tile_s.db_path_lowest.Count > 0)
        //        move_tile(selected_tile_s);
        //    else
        //        print("no valid tile selected");
        //}

        //if (body_looking)
        //{
        //    Vector3 tar_dir = db_moves[1].position - tr_body.position;
        //    Vector3 new_dir = Vector3.RotateTowards(tr_body.forward, tar_dir, rotate_speed * Time.deltaTime / 2, 0f);
        //    new_dir.y = 0;
        //    tr_body.transform.rotation = Quaternion.LookRotation(new_dir);

        //    var angle = Vector3.Angle(tar_dir, tr_body.forward);
        //    if (angle <= 1)
        //    {
        //        ResetDirection();
        //        body_looking = false;
        //        StartMove();
        //    }

        //    return;
        //}


        //if (moving)
        //{
        //    float step = move_speed * Time.deltaTime;
        //    transform.position = Vector3.MoveTowards(transform.position, db_moves[0].position, step);
        //    var tdist = Vector3.Distance(tr_body.position, db_moves[0].position);
        //    if (tdist < 0.001f)
        //    {
        //        //tile_s.db_chars.Remove(this);
        //        tile_s = tar_tile_s.db_path_lowest[num_tile];
        //        //tile_s.db_chars.Add(this);
        //        if (moving_tiles && num_tile < tar_tile_s.db_path_lowest.Count - 1)
        //        {
        //            num_tile++;
        //            var tpos = tar_tile_s.db_path_lowest[num_tile].transform.position;
        //            if (big) //Large chars//
        //            {
        //                tpos = new Vector3(0, 0, 0);
        //                tpos += tar_tile_s.db_path_lowest[num_tile].transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[2].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
        //                tpos /= 4; //Takes up 4 tiles//
        //            }
        //            tpos.y = transform.position.y;
        //            db_moves[0].position = tpos;
        //            nextTile = tar_tile_s.db_path_lowest[num_tile];
        //            db_moves[1].position = tpos;
        //        }
        //        else
        //        {
        //            db_moves[4].gameObject.SetActive(false);
        //            moving = false;
        //            moving_tiles = false;
        //        }
        //        Reached();
        //    }
        //}
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
        
        tpos = tar_tile_s.transform.position;

        db_moves[4].position = tpos; //Tar Tile Marker//
        db_moves[4].gameObject.SetActive(true); //Tar Tile Marker//

        tpos = new Vector3(0, 0, 0);
        
        tpos += tar_tile_s.db_path_lowest[num_tile].transform.position;

        tpos.y = transform.position.y;
        db_moves[0].position = tpos;
        
        db_moves[1].position = tpos;

        moving = true;
        moving_tiles = true;

        //UpdateTargetDirection(nextTile);

        if(!body_looking)
        {
            StartMove();
        }
    }

    public void UpdateMoves(Tile ttile)
    {
        num_tile = 0;

        tar_tile_s = ttile;

        //0 - body_move, 1 - body_look, 2 - head_look, 3 - eyes_look, target tile marker
        db_moves[0].parent = null;
        db_moves[1].parent = null;
        db_moves[4].parent = null;

        //move_speed = 1;

        var tpos = new Vector3(0, 0, 0);
        tpos = tar_tile_s.transform.position;
        db_moves[4].position = tpos; //Tar Tile Marker//
        db_moves[4].gameObject.SetActive(true); //Tar Tile Marker//

        tpos = new Vector3(0, 0, 0);
       
        tpos = tar_tile_s.db_path_lowest[num_tile].transform.position;

        tpos.y = transform.position.y;

        db_moves[0].position = tpos;

        db_moves[1].position = tpos;

        nextTile = tar_tile_s.db_path_lowest[num_tile];

        //UpdateTargetDirection(nextTile);
    }

    public void UpdateTargetDirection(Tile targetTile)
    {
        if (targetTile == null)
        {
            return;
        }

        var tileNameArr = targetTile.name.Split('_');
        var nxtTileX = int.Parse(tileNameArr[0]);
        var nxtTileZ = int.Parse(tileNameArr[1]);

        tileNameArr = tile_s.name.Split('_');
        var tileX = int.Parse(tileNameArr[0]);
        var tileZ = int.Parse(tileNameArr[1]);
        targetDirection = direction;
        if (nxtTileX - tileX == 1 && nxtTileZ == tileZ)
        {
            targetDirection = Direction.Right;
        }
        else if (nxtTileX - tileX == -1 && nxtTileZ == tileZ)
        {
            targetDirection = Direction.Left;
        }
        else if (nxtTileX == tileX && nxtTileZ - tileZ == 1)
        {
            targetDirection = Direction.Up;
        }
        else if (nxtTileX == tileX && nxtTileZ - tileZ == -1)
        {
            targetDirection = Direction.Down;
        }
        if(targetDirection == direction)
        {
            body_looking = false;
        }
        else
        {
            body_looking = true;
        }
    }

    //public void ClearPath()
    //{
    //    gridManager.ClearPath(this);
    //}

    public void FindPathRealTime(Tile t)
    {
        selected_tile_s = t;
        gridManager.find_paths_realtime(this, t);
        UpdateMoves(t);
        path = t.db_path_lowest;
        UpdateTargetDirection(nextTile);
        //for (int x = 0; x < gridManager.db_tiles.Count; x++)
        //    gridManager.db_tiles[x].db_path_lowest.Clear(); //Clear all previous lowest paths for this char//
    }

    public virtual void ResetDirection()
    {
        var rotateY = transform.localRotation.eulerAngles.y;

        while(rotateY >= 360)
        {
            rotateY -= 360;
        }
        while (rotateY <= -360)
        {
            rotateY += 360;
        }
        
        rotateY = rotateY / 90;
        rotateY = Mathf.RoundToInt(rotateY);
        if (rotateY > 3)
        {
            rotateY -= 4;
            Debug.Log("计算方向出错");
        }
        direction = (Direction)System.Enum.Parse(typeof(Direction), rotateY.ToString(), true);

        targetDirection = direction;
        //Debug.Log(gameObject.name + "方向:" + direction);
    }

    public virtual void Reached()
    {
        coord = new Coord(transform.position);
        ResetDirection();
        
    }

    public virtual void StartMove()
    {
       
    }


    public Dictionary<string,BoardNode> FindNodesAround(int range)
    {
        return boardManager.FindNodesAround(tile_s.name, range);
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
