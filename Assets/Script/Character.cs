using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Character : MonoBehaviour
{
    public GridManager gridManager;
    public Animator m_animator;
    public bool body_looking;
    public bool moving;
    public bool moving_tiles;
    public float move_speed = 1f;
    public float rotate_speed = 1f;
    public Color col;
    public Transform tr_body;
    public string lastTileName;
    private GridTile _currentTile;
    public GridTile currentTile
    {
        set
        {
            if(_currentTile)
            {
                lastTileName = _currentTile.name;
            }
            _currentTile = value;
        }
        get
        {
            return _currentTile;
        }
    }
    public GridTile tar_tile_s;
    public GridTile selected_tile_s;
    public GridTile nextTile;
    public List<Transform> db_moves;
    public int max_tiles = 7;
    public int num_tile;
    public BoardManager boardManager;
    public Direction direction = Direction.Up;
    public bool rotation = false;
    public bool reachedTile = false;

    public Direction originalDirection = Direction.Up;

    public Direction targetDirection = Direction.Up;

    public Coord originalCoord;

    public Coord coord;

    public List<GridTile> path = new List<GridTile>();

    public ActionBase currentAction = null;

    //public string lastTileName;

    public virtual void Start()
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

        currentTile = tile;

        originalDirection = direction;
        originalCoord = coord.Clone();
    }

    public void move_tile(GridTile ttile)
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

    public void UpdateMoves(GridTile ttile)
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

    public void UpdateTargetDirection(GridTile targetTile)
    {
        if (targetTile == null)
        {
            return;
        }

        var tileNameArr = targetTile.name.Split('_');
        var nxtTileX = int.Parse(tileNameArr[0]);
        var nxtTileZ = int.Parse(tileNameArr[1]);

        tileNameArr = currentTile.name.Split('_');
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

    public bool Goto(string tileName)
    {
        var tile = gridManager.GetTileByName(tileName);
        if (!tile) return false;
        FindPathRealTime(tile);
        return true;
    }

    public void FindPathRealTime(GridTile t)
    {
        selected_tile_s = t;
        gridManager.find_paths_realtime(this, t);
        UpdateMoves(t);
        path = t.db_path_lowest;
        UpdateTargetDirection(nextTile);
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
        return boardManager.FindNodesAround(currentTile.name, range);
    }


    public void Clear()
    {
        for (int x = 0; x < gridManager.db_tiles.Count; x++)
            gridManager.db_tiles[x].db_path_lowest.Clear();
        selected_tile_s = null;
        nextTile = null;
        tar_tile_s = null;
        db_moves.ForEach((Transform cube) => {
            cube.parent = transform;
            cube.transform.localPosition = new Vector3(0, 0, 0);
        });
    }

    public void ResetMoves()
    {
        db_moves.ForEach((Transform cube) => {
            cube.parent = transform;
            cube.transform.localPosition = new Vector3(0, 0, 0);
        });
    }

    public bool CanReachInSteps(string tileName,int step = 1)
    {
        if (gridManager == null) return false;
        var tile = gridManager.GetTileByName(tileName);
        if (tile != null)
        {
            var pathLength = 0;
            selected_tile_s = tile;
            gridManager.find_paths_realtime(this, tile);
            pathLength = tile.db_path_lowest.Count;
            Clear();
            return pathLength <= step;
        }
        return false;
    }


    #region 动画事件回调
    public virtual void FootL()
    {

    }

    public virtual void FootR()
    {

    }

    public virtual void PlayerReached()
    {

    }

    public virtual void AnimationEnd(string clipName)
    {

    }

    public virtual void ReadyThrowBottle()
    {

    }

    public virtual void AfterStealVegetable()
    {

    }

    #endregion
}
