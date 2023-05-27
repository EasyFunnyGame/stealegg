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
    public float rotate_speed = 0.5f;
    public Color col;
    public Transform tr_body;
    public string lastTileName;
    public GridTile _currentTile;
    public int Uid = 0;

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

    public Direction _direction = Direction.Up;

    public bool rotation = false;
    public bool reachedTile = false;

    public Direction originalDirection = Direction.Up;

    public Direction targetDirection = Direction.Up;

    public Coord originalCoord;

    public Coord coord;

    public Coord lastCoord;

    public List<string> path = new List<string>();

    public ActionBase currentAction = null;

    public Direction direction
    {
        set { _direction = value; }
        get
        {
            // 
            var xOffset = 0;

            //
            var zOffset = 0;

            // 直接用角度计算
            var bodyRotation = tr_body.GetChild(0).transform.rotation.eulerAngles.y;

            while (bodyRotation < 0)
            {
                bodyRotation += 360;
            }

            while (bodyRotation > 360)
            {
                bodyRotation -= 360;
            }

            if (Mathf.Abs(90 - bodyRotation) < 1)
            {
                //Debug.Log("右" + Enemy.count);
                _direction = Direction.Right;
            }
            else if (Mathf.Abs(180 - bodyRotation) < 1)
            {
                //Debug.Log("下" + Enemy.count);
                _direction = Direction.Down;
            }
            else if (Mathf.Abs(270 - bodyRotation) < 1 )
            {
                //Debug.Log("左" + Enemy.count);
                _direction = Direction.Left;
            }
            else
            {
                //Debug.Log("上 " + Enemy.count);
                _direction = Direction.Up;
            }
            return _direction;
        }
    }


    public Coord GetDirectionCoord()
    {
        // 
        var xOffset = 0;

        //
        var zOffset = 0;

        // 直接用角度计算
        var bodyRotation = tr_body.GetChild(0).transform.rotation.eulerAngles.y;

        while (bodyRotation < 0)
        {
            bodyRotation += 360;
        }

        while (bodyRotation > 360)
        {
            bodyRotation -= 360;
        }

        if (Mathf.Abs(90 - bodyRotation) < 5)
        {
            // Debug.Log("右" + Enemy.count);
            xOffset = 1;
        }
        else if (Mathf.Abs(180 - bodyRotation) < 5)
        {
            // Debug.Log("下" + Enemy.count);
            zOffset = -1;
        }
        else if (Mathf.Abs(270 - bodyRotation) < 5)
        {
            // Debug.Log("左" + Enemy.count);
            xOffset = -1;
        }
        else
        {
            // Debug.Log("上 " + Enemy.count);
            zOffset = 1;
        }
        return new Coord(xOffset, zOffset, 0);
    }


    // 行为链条
    //public List<ActionBase> actionChains = new List<ActionBase>();

    public virtual void Start()
    {
        var boardManagerGo = GameObject.Find("BoardManager");
        boardManager = boardManagerGo.GetComponent<BoardManager>();

        var gridManagerGo = GameObject.Find("GridManager");

        if (gameObject.name != "Player")
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
            Uid = Enemy.count;
            gameObject.name += ("[" + Enemy.count.ToString() + "]");
        }
        else
        {
            gridManager = gridManagerGo.GetComponent<GridManager>();
        }

        var x = int.Parse(transform.position.x.ToString());
        var z = int.Parse(transform.position.z.ToString());

        var tile = gridManager.GetTileByName(string.Format("{0}_{1}", x, z));

        coord = new Coord(tile.name, transform.position.y);

        currentTile = tile;

        targetDirection = direction;
        originalDirection = direction;
        originalCoord = coord.Clone();
        lastCoord = coord.Clone();
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

    public Direction LookAt(string tileName)
    {
        var tileNameArr = tileName.Split('_');
        var nxtTileX = int.Parse(tileNameArr[0]);
        var nxtTileZ = int.Parse(tileNameArr[1]);

        tileNameArr = currentTile.name.Split('_');
        var tileX = int.Parse(tileNameArr[0]);
        var tileZ = int.Parse(tileNameArr[1]);
        targetDirection = _direction;
        if (nxtTileX - tileX > 0 && nxtTileZ == tileZ)
        {
            targetDirection = Direction.Right;
            db_moves[1].position = transform.position + new Vector3(1, 0, 0);
        }
        else if (nxtTileX - tileX < 0 && nxtTileZ == tileZ)
        {
            targetDirection = Direction.Left;
            db_moves[1].position = transform.position + new Vector3(-1, 0, 0);
        }
        else if (nxtTileX == tileX && nxtTileZ - tileZ > 0)
        {
            targetDirection = Direction.Up;
            db_moves[1].position = transform.position + new Vector3(0, 0, 1);
        }
        else if (nxtTileX == tileX && nxtTileZ - tileZ < 0)
        {
            targetDirection = Direction.Down;
            db_moves[1].position = transform.position + new Vector3(0,0,-1);
        }
        if(targetDirection == _direction)
        {
            body_looking = false;
        }
        else
        {
            body_looking = true;
        }
        return targetDirection;
    }

    public List<string> GetPathFromTo(GridTile to, GridTile from)
    {
        var tempTile = _currentTile;

        _currentTile = from;
        var xOffSet = 0;
        var zOffset = 0;
        if (direction == Direction.Up)
        {
            zOffset = 1;
        }
        else if (direction == Direction.Down)
        {
            zOffset = -1;
        }
        else if (direction == Direction.Left)
        {
            xOffSet = -1;
        }
        else if (direction == Direction.Right)
        {
            xOffSet = 1;
        }
        gridManager.find_paths_realtime(this, to);
        if (to.db_path_lowest.Count <= 0)
        {
            Debug.LogWarning(this.gameObject.name + " ---查找路径点失败，该点不可达----" + to.gameObject.name );
            return null;
        }
        GridTile fromTile = null;
        var frontalTileName = string.Format("{0}_{1}", coord.x + xOffSet, coord.z + zOffset);
        if (frontalTileName != to.gameObject.name)
        {
            var boardNode = boardManager.FindNode(frontalTileName);
            if (boardNode?.gameObject.activeSelf == true)
            {
                var frontalTile = gridManager.GetTileByName(frontalTileName);
                if (frontalTile)
                {
                    for (var index = 0; index < currentTile.db_neighbors.Count; index++)
                    {
                        var neighbor = currentTile.db_neighbors[index];
                        if (neighbor.tile_s?.name == frontalTileName && neighbor.blocked == false)
                        {
                            fromTile = neighbor.tile_s;
                            break;
                        }
                    }
                }
            }
        }

        selected_tile_s = to;

        if (fromTile)
        {
            gridManager.find_paths_realtime(this, to);
            var path1 = new List<string>();
            for (var idx = 0; idx < selected_tile_s.db_path_lowest.Count; idx++)
            {
                path1.Add(selected_tile_s.db_path_lowest[idx].name);
            }

            gridManager.find_paths_realtime(this, to, fromTile);
            var path2 = new List<string>();
            for (var idx = 0; idx < selected_tile_s.db_path_lowest.Count; idx++)
            {
                path2.Add(selected_tile_s.db_path_lowest[idx].name);
            }

            if ((path2.Count + 1) > path1.Count)
            {
                gridManager.find_paths_realtime(this, to);
            }
            else
            {
                gridManager.find_paths_realtime(this, to, fromTile);
                var pathLowest = new List<GridTile>();
                pathLowest.Add(fromTile);
                pathLowest.AddRange(selected_tile_s.db_path_lowest);
                selected_tile_s.db_path_lowest = pathLowest;
            }
        }
        else
        {
            gridManager.find_paths_realtime(this, to);
        }

        var p = new List<string>();

        for (var index = 0; index < selected_tile_s.db_path_lowest.Count; index++)
        {
            p.Add(selected_tile_s.db_path_lowest[index].gameObject.name);
        }
        _currentTile = tempTile;
        return p;
    }


    public bool FindPathRealTime(GridTile to, GridTile from, bool useFastestWay )
    {

        if(useFastestWay)
        {
            var xOffSet = 0;
            var zOffset = 0;
            if (direction == Direction.Up)
            {
                zOffset = 1;
            }
            else if (direction == Direction.Down)
            {
                zOffset = -1;
            }
            else if (direction == Direction.Left)
            {
                xOffSet = -1;
            }
            else if (direction == Direction.Right)
            {
                xOffSet = 1;
            }
            gridManager.find_paths_realtime(this, to);
            if (to.db_path_lowest.Count <= 0)
            {
                Debug.LogWarning(this.gameObject.name + "查找路径点失败，该点不可达" + to.gameObject.name);
                return false;
            }
            GridTile fromTile = null;
            var frontalTileName = string.Format("{0}_{1}", coord.x + xOffSet, coord.z + zOffset);
            if (frontalTileName != to.gameObject.name)
            {
                var boardNode = boardManager.FindNode(frontalTileName);
                if (boardNode?.gameObject.activeSelf == true)
                {
                    var frontalTile = gridManager.GetTileByName(frontalTileName);
                    if (frontalTile)
                    {
                        for (var index = 0; index < currentTile.db_neighbors.Count; index++)
                        {
                            var neighbor = currentTile.db_neighbors[index];
                            if (neighbor.tile_s?.name == frontalTileName && neighbor.blocked == false)
                            {
                                fromTile = neighbor.tile_s;
                                break;
                            }
                        }
                    }
                }
            }

            selected_tile_s = to;

            if (fromTile)
            {
                gridManager.find_paths_realtime(this, to);
                var path1 = new List<string>();
                for (var idx = 0; idx < selected_tile_s.db_path_lowest.Count; idx++)
                {
                    path1.Add(selected_tile_s.db_path_lowest[idx].name);
                }

                gridManager.find_paths_realtime(this, to, fromTile);
                var path2 = new List<string>();
                for (var idx = 0; idx < selected_tile_s.db_path_lowest.Count; idx++)
                {
                    path2.Add(selected_tile_s.db_path_lowest[idx].name);
                }

                if ((path2.Count + 1) > path1.Count)
                {
                    gridManager.find_paths_realtime(this, to);
                }
                else
                {
                    gridManager.find_paths_realtime(this, to, fromTile);
                    var pathLowest = new List<GridTile>();
                    pathLowest.Add(fromTile);
                    pathLowest.AddRange(selected_tile_s.db_path_lowest);
                    selected_tile_s.db_path_lowest = pathLowest;
                }
            }
            else
            {
                gridManager.find_paths_realtime(this, to);
            }
        }
        else
        {
            selected_tile_s = to;
            gridManager.find_paths_realtime(this, to);
        }
        

        UpdateMoves(selected_tile_s);

        path.Clear();

        for (var index = 0; index < selected_tile_s.db_path_lowest.Count; index++)
        {
            path.Add(selected_tile_s.db_path_lowest[index].gameObject.name);
        }

        LookAt(nextTile.name);

        return true;
    }

    public void UpdateMoves(GridTile ttile)
    {
        //num_tile = 0;

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

        tpos = tar_tile_s.db_path_lowest[0].transform.position;

        tpos.y = transform.position.y;

        db_moves[0].position = tpos;

        db_moves[1].position = tpos;

        nextTile = tar_tile_s.db_path_lowest[0];

        //UpdateTargetDirection(nextTile);
    }

    public virtual void Turned()
    {
        body_looking = false;
        m_animator.SetBool("moving", false);
        _direction = targetDirection;
    }

    public virtual void Reached()
    {
        lastCoord = coord.Clone();
        coord = new Coord(transform.position);
        transform.position = new Vector3(coord.x,transform.position.y,coord.z);
        //ResetDirection();
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

    public int StepsReach(string tileName)
    {
        if (gridManager == null) return -1;
        var tile = gridManager.GetTileByName(tileName);
        if (tile != null)
        {
            var pathLength = 0;
            selected_tile_s = tile;
            gridManager.find_paths_realtime(this, tile);
            pathLength = tile.db_path_lowest.Count;
            Clear();
            return pathLength;
        }
        return -1;
    }

    public int ReachInStepsFrom(string to, string from)
    {
        if (gridManager == null) return int.MaxValue;
        var fromTile = gridManager.GetTileByName(from);
        var toTile = gridManager.GetTileByName(to);
        if (fromTile == null || toTile == null) return int.MaxValue;

        var tempCurrentTile = _currentTile;

        _currentTile = fromTile;

        var pathLength = 0;
        selected_tile_s = toTile;
        gridManager.find_paths_realtime(this, toTile);
        pathLength = toTile.db_path_lowest.Count;
        Clear();
        _currentTile = tempCurrentTile;
        return pathLength;
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

    public Coord front
    {
        get
        {
            var x = coord.x;
            var z = coord.z;
            var coordName = "";
            if(direction == Direction.Up)
            {
                z += 1;
            }
            else if(direction == Direction.Down)
            {
                z-=1;
            }
            else if (direction == Direction.Left)
            {
                x -= 1;
            }
            else if (direction == Direction.Right)
            {
                x += 1;
            }
            coordName = string.Format("{0}_{1}", x, z);
            if (!boardManager) return Coord.Illegal;
            var node = boardManager.FindNode(coordName);
            if (node == null) return Coord.Illegal;
            return new Coord(x, z, node.transform.position.y);
        }
    }

    public Coord frontTwo
    {
        get
        {
            var x = coord.x;
            var z = coord.z;
            var coordName = "";
            if (direction == Direction.Up)
            {
                z += 2;
            }
            else if (direction == Direction.Down)
            {
                z -= 2;
            }
            else if (direction == Direction.Left)
            {
                x -= 2;
            }
            else if (direction == Direction.Right)
            {
                x += 2;
            }
            coordName = string.Format("{0}_{1}", x, z);
            if (!boardManager) return Coord.Illegal;
            var node = boardManager.FindNode(coordName);
            if (node == null) return Coord.Illegal;
            return new Coord(x, z, node.transform.position.y);
        }
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

    public virtual void Pick()
    {


    }

    public virtual void Lure()
    {

    }


    public virtual void StopLookAround()
    {

    }

    #endregion
}
