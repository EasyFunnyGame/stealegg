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

    public Turn turn = Turn.NONE;

    public BoardManager boardManager;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        status = GameStatus.PREPARED;

        turn = Turn.NONE;

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

    public void SceneLoaded(BoardManager boardMgr)
    {
        boardManager = boardMgr;
        status = GameStatus.PLAYING;
        turn = Turn.PLAYER;
        gameCanvas.Show();
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
        if (turn == Turn.PLAYER)
        {
            PlayerTurnUpdate();
        }
        else if (turn == Turn.ENEMY)
        {
            EnemyTurnUpdate();
        }
    }

    void PlayerTurnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = GameCamera.Instance.camera.ScreenPointToRay(Input.mousePosition);
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

                var tileIndex = coord.x * BoardManager.Instance.height + coord.z;

                // Debug.Log("节点:" + coord.name + "块的名称" + tile.name);
                Tile tile = Player.Instance.gridManager.db_tiles[tileIndex];
                if (Player.Instance.moving || Player.Instance.tile_s != tile)
                {
                    Player.Instance.selected_tile_s = tile;
                    Player.Instance.FindPathRealTime(tile);
                }
            }
        }
    }

    void EnemyTurnUpdate()
    {
        var allEnd = true;
        var enemiesCount = boardManager.enemies.Count;
        for (var index = 0; index < enemiesCount; index++)
        {
            var enemy = boardManager.enemies[index];
            if (enemy.hasAction)
            {
                allEnd = false;
            }
        }
        if (allEnd)
        {
            turn = Turn.PLAYER;
        }
    }

    public void UseWhistle()
    {
        var nodes = boardManager.FindNodesAround(Player.Instance.tile_s.name ,2);
        foreach(var kvp in nodes)
        {
            var name = kvp.Key;
            var node = kvp.Value;
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

    public void CheckPlayerTurnEnd()
    {
        if(status == GameStatus.PLAYING)
        {
            turn = Turn.ENEMY;
            var enemiesCount = boardManager.enemies.Count;
            for (var index = 0; index < enemiesCount; index++)
            {
                var enemy = boardManager.enemies[index];
                enemy.CheckAction();
            }
        }
    }

    public void CheckEnemiesTurnEnd()
    {
        
    }

    public void Save()
    {
        PlayerPrefs.SetInt("Energy", energy);
    }

    
}
