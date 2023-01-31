using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public grid_manager gridManager;

    public BoardManager boardMgr;

    public GameCamera gameCamera;

    public new string name;

    public Player player;

    public Item star;

    public Item graff;

    public Item end;

    public List<Item> lureBottles = new List<Item>();

    public List<Item> pincerses = new List<Item>();

    public List<Item> grouthes = new List<Item>();

    public List<Item> manholeCovers = new List<Item>();

    public List<Enemy> enemies = new List<Enemy>();

    private void Awake()
    {
        boardMgr = GetComponent<BoardManager>();
        if(boardMgr == null)
            throw new System.Exception("No BoardManager Attached To This GameObject");
        boardMgr.init(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        gridManager.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetMouseButtonDown(0))
        {
            if( Input.touchCount == 1  && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Debug.Log("鼠标按下");
            }
            Ray ray = gameCamera.camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if(Physics.Raycast(ray,out hitInfo,100,LayerMask.GetMask("Square")))
            {
                var node = hitInfo.transform.parent.parent;
                if(node==null)
                {
                    Debug.Log("鼠标按下Error 1");
                    return;
                }
                var nodeScript = node.GetComponent<BoardNode>();
                if(nodeScript==null)
                {
                    Debug.Log("鼠标按下Error 2");
                    return;
                }
                var coord = nodeScript.coord;
                

                var tileIndex = coord.x * Mathf.RoundToInt(gridManager.v2_grid.y) + coord.z;
                var tile = gridManager.db_tiles[tileIndex];
                //Debug.Log("节点:" + coord.name + "块的名称" + tile.name);
                if ( player.moving ||  player.tile_s != tile)
                {
                    player.selected_tile_s = tile;
                    player.gm_s.find_paths_realtime(player, tile);
                }
            }
        }
    }
}
