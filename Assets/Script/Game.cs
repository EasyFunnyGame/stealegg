using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameResult
{
    NONE,

    WIN,

    FAIL,
}

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }
    public MainCanvas mainCanvas;
    public MsgCanvas msgCanvas;
    public GameCanvas gameCanvas;
    public GameEndCanvas endCanvas;
    public EnergyGainCanvas energyGainCanvas;
    public HintGainCanvas hintGainCanvas;
    public SettingCanvas settingCanvas;
    public ChapterCanvas chapterCanvas;
    public GraffCanvas graffCanvas;
    public CameraSettingCanvas cameraSettingCanvas;

    public Dictionary<string, int> scores = new Dictionary<string, int>();

    public int energy;

    public int playingLevel;

    public string currentLevelName;

    public BoardManager boardManager;

    public new GameCamera camera;

    public Player player;

    public bool pausing = false;

    public bool playing = false;

    public GameResult result;

    public bool graffed = false;

    public bool bottleSelectingTarget = false;

    public static int MAX_LEVEL = 4;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        energy = PlayerPrefs.GetInt("Energy", -1);
        if (energy == -1)
            energy = 10;

        mainCanvas.Show();
        msgCanvas.Show();
        gameCanvas.Hide();
        endCanvas.Hide();
        energyGainCanvas.Hide();
        hintGainCanvas.Hide();
        settingCanvas.Hide();
        chapterCanvas.Hide();
        graffCanvas.Hide();
        cameraSettingCanvas.Show();
    }

    public void StartGame(string sceneName)
    {
        PlayLevel(sceneName);

        energy--;

        chapterCanvas.Hide();

        result = GameResult.NONE;

        Save();
    }

    public void PlayLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SceneLoaded(BoardManager boardMgr , string sceneName)
    {
        graffed = false;
        boardManager = boardMgr;
        var nameArr = sceneName.Split('-');
        var chapter = int.Parse(nameArr[0]);
        var index = int.Parse(nameArr[1]);
        playingLevel = (chapter - 1) * 12 + (index - 1);
        currentLevelName = sceneName;
        camera = GameObject.Find("GameCamera").GetComponent<GameCamera>();
        player = GameObject.Find("Player").GetComponent<Player>();
        player.bottleCount = 0;
        delayShowEndTimer = 0;
        bottleSelectingTarget = false;
        gameCanvas.level = playingLevel+1;
        gameCanvas.Show();
        gameCanvas.InitWithBoardManager(boardMgr);
        cameraSettingCanvas.InitWithGameCamera(camera, player);
        cameraSettingCanvas.SetExpand(false);
        result = GameResult.NONE;
        Debug.Log("当前场景名称:" + sceneName);
    }

    public void EndGame()
    {
        gameCanvas.Hide();
        endCanvas.Show();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public float delayShowEndTimer = 0;

    // Update is called once per frame
    void Update()
    {
        if(bottleSelectingTarget)
        {
            ListenBottleTargetSelect();
            return;
        }
        if (graffCanvas.gameObject.activeSelf)
        {
            return;
        }

        if (playing)
        {
            GamePlayingUpdate();
        }

        if(delayShowEndTimer > 0)
        {
            delayShowEndTimer -= Time.deltaTime;
            if(delayShowEndTimer<0)
            {
                EndGame();
            }
        }
    }

    public void FailGame()
    {
        result = GameResult.FAIL;
        delayShowEndTimer = 2;
        playing = false;
        player.m_animator.SetTrigger("fail");
    }

    public void WinGame()
    {
        result = GameResult.WIN;
        delayShowEndTimer = 2;
        playing = false;
    }

    void GamePlayingUpdate()
    {
        enemyActionRunning = false;
        if (player == null) return;

        var beFound = false;

        if (player.currentAction != null)
        {
            if (player.currentAction.CheckComplete())
            {
                // 主角动作完成回调
                player.currentAction = null;
                // 更新敌人行为
                for (var i = 0; i < boardManager.enemies.Count; i++)
                {
                    var enemy = boardManager.enemies[i];
                    enemy.CheckAction();
                }
            }
            else
            {
                player.currentAction.Run();
            }
        }
        else
        {
            for(var i = 0; i < boardManager.enemies.Count; i++)
            {
                var enemy = boardManager.enemies[i];
                if(!beFound && (enemy.foundPlayerTile != null || enemy.hearSoundTile != null))
                {
                    beFound = true;
                }
                if(enemy.currentAction!=null)
                {
                    var complete = enemy.currentAction.CheckComplete();
                    if(complete)
                    {
                        enemy.currentAction = null;
                    }
                    else
                    {
                        enemy.currentAction.Run();
                        enemyActionRunning = true;
                    }
                }
            }
        }
        var player_idle_type = player.m_animator.GetFloat("idle_type");
        if(beFound)
        {
            player_idle_type += .1f;
            if(player_idle_type>=1)
            {
                player_idle_type = 1;
            }
            player.m_animator.SetFloat("idle_type", player_idle_type);
        }
        else
        {
            player_idle_type -= .1f;
            if (player_idle_type <= 0)
            {
                player_idle_type = 0;
            }
            player.m_animator.SetFloat("idle_type", player_idle_type);
        }

        if(player.currentAction == null && !enemyActionRunning)
        {
            ListenClick();
        }
        if(enemyActionRunning)
        {
            gameCanvas.DisableBottle();
            gameCanvas.DisableWhistle();
        }
        else
        {
            if (!player.moving)
            {
                player.CheckBottle();
                player.CheckWhistle();
            }
        }
    }

    void ListenBottleTargetSelect()
    {
        if (pausing) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.m_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100, LayerMask.GetMask("Square")))
            {
                var node = hitInfo.transform.parent.parent;
                if (bottleSelectable.IndexOf(node.name)==-1)
                {
                    return;
                }
                var nodePosition = node.transform.position;

                var tileIndex = Mathf.RoundToInt(nodePosition.x * player.gridManager.v2_grid.y + nodePosition.z);

                // Debug.Log("节点:" + coord.name + "块的名称" + tile.name);
                GridTile tile = player.gridManager.db_tiles[tileIndex];

                for (var i = 0; i < boardManager.enemies.Count; i++)
                {
                    var enemy = boardManager.enemies[i];
                    if (enemy.coord.name == tile.name)
                    {
                        return;
                    }
                }

                if (player.moving || player.currentTile != tile)
                {
                    player.currentAction = new ActionThrowBottle(player, node.gameObject.name, tile.transform.position);
                    boardManager.HideAllSuqreContour();
                    bottleSelectingTarget = false;
                    player.CheckBottle();
                    Debug.Log("扔瓶子:"+ node.name);
                }
            }
        }
    }

    void ListenClick()
    {
        if (pausing) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.m_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100, LayerMask.GetMask("Square")))
            {
                var node = hitInfo.transform.parent.parent;
                var nodePosition = node.transform.position;

                var tile = player.gridManager.GetTileByName(node.name);

                for ( var i = 0; i < boardManager.enemies.Count;  i++ )
                {
                    var enemy = boardManager.enemies[i];
                    if(enemy.coord.name == node.name)
                    {
                        return;
                    }
                }

                if (player.moving || player.currentTile != tile)
                {
                    player.currentAction = new ActionPlayerMove(player, tile);
                    Debug.Log("主角行为====移动");
                }
            }
        }
    }

    public void BlowWhistle()
    {
        player.currentAction = new ActionBlowWhistle(player);
    }

    public void UseBottle()
    {
        player.currentAction = null;
    }

    public void ThrowBottle()
    {
        player.currentAction = null;
    }

    public void Save()
    {
        PlayerPrefs.SetInt("Energy", energy);
    }

    bool enemyActionRunning = false;

    public void Steal()
    {
        if(enemyActionRunning)
        {
            return;
        }
        if(!graffed)
        {
            graffed = true;
            player.currentAction = new ActionSteal(player, boardManager.allItems[player.currentTile.name]);
        }
    }

    List<string> bottleSelectable = new List<string>();
    public void BottleSelectTarget()
    {
        player.bottle.gameObject.SetActive(true);
        bottleSelectable.Clear();
        bottleSelectingTarget = true;
        player.m_animator.SetBool("bottle_select", true);
        var nodes = boardManager.FindNodesAround(player.currentTile.name,3);
        foreach(var kvp in nodes)
        {
            var nodeName = kvp.Key;
            var nodeGo = kvp.Value;
            var node = nodeGo.GetComponent<BoardNode>();
            
            var selectable = true;
            foreach(var enemy in boardManager.enemies)
            {
                if(enemy.coord.name == nodeName)
                {
                    selectable = false;
                    continue;
                }
            }
            if(player.startTileName == nodeName)
            {
                selectable = false;
            }
            if(player.currentTile.name == nodeName)
            {
                selectable = false;
            }
            node.contour.gameObject.SetActive(selectable);
            if(selectable)
            {
                bottleSelectable.Add(nodeName);
            }
        }
    }

    public void CancelBottleSelectTarget()
    {
        boardManager.HideAllSuqreContour();
        player.m_animator.SetBool("bottle_cancel", true);
        player.bottle.gameObject.SetActive(false);
        bottleSelectingTarget = false;
    }

    public void BottleThorwed(string targetTile)
    {
        var targetArray = targetTile.Split('_');
        var x = int.Parse(targetArray[0]);
        var z = int.Parse(targetArray[1]);

        foreach (var enemy in boardManager.enemies)
        {
            var coord = enemy.coord;
            var distanceFromX = Mathf.Abs(x - coord.x);
            var distanceFromZ = Mathf.Abs(z - coord.z);
            if (distanceFromX <= 2 && distanceFromZ <= 2)
            {
                enemy.LureBottle(targetTile);
            }
            
        }
        bottleSelectingTarget = false;
    }

    public void ReachEnd()
    {
        if(graffed)
        {
            var level = PlayerPrefs.GetInt("Level");
            if (playingLevel >= level)
            {
                level = playingLevel;
                PlayerPrefs.SetInt("Level", playingLevel + 1);
            }
            WinGame();
            Save();
        }
    }
}
