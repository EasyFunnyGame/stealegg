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

    public int level;

    public string currentLevelName;

    public BoardManager boardManager;

    public bool graffable = false;

    public bool graffing = false;

    public bool graffed = false;

    public new GameCamera camera;

    public Player player;

    public bool pausing = false;

    public bool playing = false;

    public GameResult result;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        level = PlayerPrefs.GetInt("Level");
        if (level > 36) level = 36;

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

    public void StartGame(string sceneName, int index)
    {
        PlayLevel(sceneName);

        energy--;

        chapterCanvas.Hide();

        gameCanvas.index = index;

        result = GameResult.NONE;

        Save();
    }

    public void PlayLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SceneLoaded(BoardManager boardMgr , string sceneName)
    {
        boardManager = boardMgr;
        
        currentLevelName = sceneName;

        camera = GameObject.Find("GameCamera").GetComponent<GameCamera>();

        player = GameObject.Find("Player").GetComponent<Player>();

        graffed = false;

        graffable = false;

        graffing = false;

        delayShowEndTimer = 0;

        gameCanvas.Show();

        gameCanvas.InitWithBoardManager(boardMgr);

        cameraSettingCanvas.InitWithGameCamera(camera, player);

        Debug.Log("当前场景名称:" + sceneName);

        cameraSettingCanvas.SetExpand(false);

        result = GameResult.NONE;
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
        if (playing )
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
        player.m_animator.CrossFade("Player_GiveUp",0.1f);
        delayShowEndTimer = 2;
        playing = false;
    }

    public void WinGame()
    {
        result = GameResult.WIN;
        delayShowEndTimer = 2;
        playing = false;
    }

    void GamePlayingUpdate()
    {
        var enemyActionRunning = false;
        if (player == null) return;
        if(player.currentAction != null)
        {
            if (player.currentAction.CheckComplete())
            {
                // 主角动作完成回调
                player.currentAction = null;
                // 更新敌人行为
                for (var i = 0; i < boardManager.enemies.Count; i++)
                {
                    var enemy = boardManager.enemies[i];
                    // enemy.PlayerWalkIntoSight();
                    // enemy.TryFoundPlayer();
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

        if(player.currentAction == null && !enemyActionRunning && !graffing)
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
                if (node == null)
                {
                    Debug.Log("鼠标按下Error 1");
                    return;
                }
                var nodeScript = node.GetComponent<BoardNode>();
                if (nodeScript == null)
                {
                    Debug.Log("鼠标按下Error 2");
                    return;
                }
                var coord = nodeScript.coord;

                var tileIndex = coord.x * boardManager.height + coord.z;

                // Debug.Log("节点:" + coord.name + "块的名称" + tile.name);
                GridTile tile = player.gridManager.db_tiles[tileIndex];

                for ( var i = 0; i < boardManager.enemies.Count;  i++ )
                {
                    var enemy = boardManager.enemies[i];
                    if(enemy.coord.name == tile.name)
                    {
                        return;
                    }
                }

                if (player.moving || player.currentTile != tile)
                {
                    player.currentAction = new ActionPlayerMove(player, ActionType.PlayerMove, tile);
                    Debug.Log("主角行为====移动");
                }
            }
        }
    }

    public void UseWhistle()
    {
        var nodes = boardManager.FindNodesAround(player.currentTile.name ,2);
        foreach(var kvp in nodes)
        {
            var name = kvp.Key;
            for(var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                if (enemy.coord.name == name)
                {
                    enemy.Alert(player.currentTile.name);
                    Debug.Log("主角行为====吹哨");
                }
            }
        }
        player.currentAction = null;
        player.PlayWhitsle();
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

    
}
