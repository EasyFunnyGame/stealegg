using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStatus
{
    PREPARED,

    PLAYING,

    WIN,

    FAIL
}

public enum Turn
{
    NONE,
    PLAYER,
    ENEMY,
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

    public Dictionary<string, int> scores = new Dictionary<string, int>();

    public GameStatus status;

    public int energy;

    public int level;

    public string currentLevelName;

    public BoardManager boardManager;

    public ActionBase playerAction;

    public bool graffable = false;

    public bool graffing = false;

    public bool graffed = false;

    public GameCamera gCamera;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        status = GameStatus.PREPARED;

        level = PlayerPrefs.GetInt("Level");
        if (level > 36) level = 36;

        energy = PlayerPrefs.GetInt("Energy", -1);
        if (energy == -1)
            energy = 5;

        mainCanvas.Show();
        msgCanvas.Show();
        gameCanvas.Hide();
        endCanvas.Hide();
        energyGainCanvas.Hide();
        hintGainCanvas.Hide();
        settingCanvas.Hide();
        chapterCanvas.Hide();
        graffCanvas.Hide();
    }

    public void StartGame(string sceneName, int index)
    {
        PlayLevel(sceneName);

        energy--;

        chapterCanvas.Hide();

        gameCanvas.index = index;

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

        gCamera = GameObject.Find("Main Camera").GetComponent<GameCamera>();

        graffed = false;

        graffable = false;

        graffing = false;

        gameCanvas.Show();
        Debug.Log("当前场景名称" + sceneName);
    }

    public void EndGame()
    {
        gameCanvas.Hide();
        endCanvas.Show();
        status = GameStatus.PREPARED;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(gameCanvas.canvasGroup.alpha>0)
        {
            return;
        }
        if (status == GameStatus.PLAYING)
        {
            GamePlayingUpdate();
        }
        if (status == GameStatus.FAIL || status == GameStatus.WIN)
        {
            EndGame();
        }
    }

    void GamePlayingUpdate()
    {
        var enemyActionRunning = false;
        if(playerAction != null)
        {
            if (playerAction.CheckComplete())
            {
                // 主角动作完成回调
                playerAction = null;

                // 更新敌人行为
                for (var i = 0; i < boardManager.enemies.Count; i++)
                {
                    var enemy = boardManager.enemies[i];
                    enemy.CheckAction();
                }
            }
            else
            {
                playerAction.Run();
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

        if(playerAction == null && !enemyActionRunning && !graffing)
        {
            ListenClick();
        }
    }

    void ListenClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = GameCamera.Instance.camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if(graffable && Physics.Raycast(ray, out hitInfo, 100, LayerMask.GetMask("Item")))
            {
                Debug.Log("开始画画");
                //graffing = true;
                //var graffCameraTransform = GameObject.Find("GraffCamera");
                //if(graffCameraTransform != null)
                //{
                //    gCamera.SetGraffTarget(graffCameraTransform.transform);
                //}
            }
            else if (Physics.Raycast(ray, out hitInfo, 100, LayerMask.GetMask("Square")))
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

                var tileIndex = coord.x * BoardManager.Instance.height + coord.z;

                // Debug.Log("节点:" + coord.name + "块的名称" + tile.name);
                GridTile tile = Player.Instance.gridManager.db_tiles[tileIndex];

                for ( var i = 0; i < boardManager.enemies.Count;  i++ )
                {
                    var enemy = boardManager.enemies[i];
                    if(enemy.coord.name == tile.name)
                    {
                        return;
                    }
                }
                if (Player.Instance.moving || Player.Instance.tile_s != tile)
                {
                    playerAction = new ActionPlayerMove(Player.Instance,ActionType.PlayerMove, tile);
                }
            }
        }
    }

    public void UseWhistle()
    {
        var nodes = boardManager.FindNodesAround(Player.Instance.tile_s.name ,2);
        foreach(var kvp in nodes)
        {
            var name = kvp.Key;
            for(var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                if (enemy.coord.name == name)
                {
                    enemy.Alert(Player.Instance.tile_s.name);
                }
            }
        }
    }

    

    public void UseBottle()
    {

    }

    public void ThrowBottle()
    {

    }

    public void Save()
    {
        PlayerPrefs.SetInt("Energy", energy);
    }

    
}
