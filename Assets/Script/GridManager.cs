using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR
[CustomEditor(typeof(GridManager)), CanEditMultipleObjects]
class grid_editor : Editor
{
    public override void OnInspectorGUI()
    {
        GridManager gm_s = (GridManager)target;
        if (GUILayout.Button("Make Grid"))
            gm_s.make_grid();
        if (GUILayout.Button("Make Circle"))
            gm_s.make_circle();

        DrawDefaultInspector();
    }
}
#endif


public class GridManager : MonoBehaviour
{
    public efind_path find_path;
    public Vector2 v2_grid;
    public GridLayout gridLayout;
    public GameObject go_pref_tile;
    public List<GridTile> db_tiles;
    public List<int> db_direction_order;

    //**On_hover/On_Click Pathfinding**//
    public void find_paths_realtime(Character tchar, GridTile tar_tile_s, GridTile fromTile = null)
    {
        tchar.num_tile = 0;
        var ttile = fromTile ?? tchar.currentTile;
        for (int x = 0; x < db_tiles.Count; x++)
            db_tiles[x].db_path_lowest.Clear(); //Clear all previous lowest paths for this char//

        int up = (int)ttile.v2xy.x - (int)tar_tile_s.v2xy.x;
        int right = (int)tar_tile_s.v2xy.y - (int)ttile.v2xy.y;
        int down = (int)tar_tile_s.v2xy.x - (int)ttile.v2xy.x;
        int left = (int)ttile.v2xy.y - (int)tar_tile_s.v2xy.y;

        db_direction_order.Clear();

        
        if (up >= right && up >= down && up >= left)
        {
            //db_direction_order.Add(1);
            //db_direction_order.Add(2);
            //db_direction_order.Add(3);
            //db_direction_order.Add(0);

            // original
            db_direction_order.Add(0);
            db_direction_order.Add(1);
            db_direction_order.Add(2);
            db_direction_order.Add(3);
        }
        else if (right >= up && right >= down && right >= left )
        {
            //db_direction_order.Add(0);
            //db_direction_order.Add(1);
            //db_direction_order.Add(2);
            //db_direction_order.Add(3);

            // original
            db_direction_order.Add(1);
            db_direction_order.Add(2);
            db_direction_order.Add(3);
            db_direction_order.Add(0);
        }

        else if (down >= up && down >= right && down >= left)
        {
            //db_direction_order.Add(3);
            //db_direction_order.Add(0);
            //db_direction_order.Add(1);
            //db_direction_order.Add(2);

            // original
            db_direction_order.Add(2);
            db_direction_order.Add(3);
            db_direction_order.Add(0);
            db_direction_order.Add(1);
        }
        else
        //if (left >= up && left >= right && left >= down)
        {
            //db_direction_order.Add(2);
            //db_direction_order.Add(3);
            //db_direction_order.Add(0);
            //db_direction_order.Add(1);

            // original
            db_direction_order.Add(3);
            db_direction_order.Add(0);
            db_direction_order.Add(1);
            db_direction_order.Add(2);
        }
        List<GridTile> db_tpath = new List<GridTile>();
        find_next_path_realtime(tchar, ttile, db_tpath, tar_tile_s);
    }

    void find_next_path_realtime(Character tchar, GridTile ttile, List<GridTile> db_tpath, GridTile tar_tile_s)
    {
        for (int x = 0; x < ttile.db_neighbors.Count; x++)
        {
            var donum = db_direction_order[x];
            var ntile = ttile.db_neighbors[donum].tile_s;
            if (ttile.db_neighbors[donum].tile_s != null && !db_tpath.Contains(ntile) && !ttile.db_neighbors[donum].blocked) //Check if tile, if not already used, if not blocked//
            {
                if (tar_tile_s.db_path_lowest.Count == 0 || db_tpath.Count < tar_tile_s.db_path_lowest.Count)
                {
                    if (ntile.db_path_lowest.Count == 0 || db_tpath.Count + 1 < ntile.db_path_lowest.Count)
                    {
                        ntile.db_path_lowest.Clear();
                        for (int i = 0; i < db_tpath.Count; i++)
                        {
                            ntile.db_path_lowest.Add(db_tpath[i]);
                        }

                        ntile.db_path_lowest.Add(ntile);

                        if (ttile != tar_tile_s)
                            find_next_path_realtime(tchar, ntile, ntile.db_path_lowest, tar_tile_s);
                    }
                }
            }
        }
    }

    public GridTile GetTileByName(string name)
    {
        for(var index = 0; index < db_tiles.Count; index++)
        {
            if (db_tiles[index].name == name)
                return db_tiles[index];
        }
        return null;
    }


    public void make_grid()
    {
        //Clear Old Tiles//
        for (int i = 0; i < db_tiles.Count; i++)
            DestroyImmediate(db_tiles[i].gameObject);
        db_tiles.Clear();

        for (int x = 0; x < v2_grid.x; x++)
        {
            for (int y = 0; y < v2_grid.y; y++)
            {
                var tgo = (GameObject) Instantiate(go_pref_tile, go_pref_tile.transform.position, go_pref_tile.transform.rotation, gridLayout.transform);
                tgo.SetActive(true);
                tgo.name = x + "_" + y;
                var ttile = tgo.GetComponent<GridTile>();
                ttile.v2xy = new Vector2(x, y);
                db_tiles.Add(ttile);
            }
        }

        for (int x = 0; x < db_tiles.Count; x++)
        {
            for (int y = 0; y < db_tiles.Count; y++)
            {
                if (db_tiles[x].v2xy.x - db_tiles[y].v2xy.x == 1 && db_tiles[x].v2xy.y == db_tiles[y].v2xy.y)
                    db_tiles[x].db_neighbors[0].tile_s = db_tiles[y]; //Up//
                else
                    if (db_tiles[x].v2xy.x == db_tiles[y].v2xy.x && db_tiles[y].v2xy.y - db_tiles[x].v2xy.y == 1)
                    db_tiles[x].db_neighbors[1].tile_s = db_tiles[y]; //Right//
                else
                    if (db_tiles[y].v2xy.x - db_tiles[x].v2xy.x == 1 && db_tiles[x].v2xy.y == db_tiles[y].v2xy.y)
                    db_tiles[x].db_neighbors[2].tile_s = db_tiles[y]; //Down//
                else
                    if (db_tiles[x].v2xy.x == db_tiles[y].v2xy.x && db_tiles[x].v2xy.y > db_tiles[y].v2xy.y)
                    db_tiles[x].db_neighbors[3].tile_s = db_tiles[y]; //Left//
            }
        }

        gridLayout.Layout(Mathf.RoundToInt(this.v2_grid.x), Mathf.RoundToInt(this.v2_grid.y), 1);
    }


    public void make_circle()
    {
        //glg.enabled = false;

        var pos_mid = db_tiles[0].transform.position + db_tiles[db_tiles.Count - 1].transform.position;
        pos_mid /= 2;
        var max_dist = Vector3.Distance(pos_mid, db_tiles[0].transform.position);
        var circle_dist = max_dist * 0.68f;

        var tcount = db_tiles.Count;
        for (int i = tcount - 1; i > -1; i--)
        {
            var ttile = db_tiles[i];
            var tdist = Vector3.Distance(pos_mid, ttile.transform.position);
            
            if (tdist > circle_dist)
            {
                db_tiles.Remove(ttile);
                DestroyImmediate(ttile.gameObject);
            }
        }
    }

    private void Awake()
    {
        gridLayout.gameObject.SetActive(false);
    }
}
