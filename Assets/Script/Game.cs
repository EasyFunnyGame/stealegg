﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameResult
{
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

    public Dictionary<string, int> scores = new Dictionary<string, int>();

    public int energy;

    public int level;

    public string currentLevelName;

    public BoardManager boardManager;

    public bool graffable = false;

    public bool graffing = false;

    public bool graffed = false;

    public GameCamera gCamera;

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

        gCamera = GameObject.Find("GameCamera").GetComponent<GameCamera>();

        graffed = false;

        graffable = false;

        graffing = false;

        delayShowEndTimer = 0;

        gameCanvas.Show();

        gameCanvas.InitWithBoardManager(boardMgr);

        Debug.Log("当前场景名称" + sceneName);
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
        Player.Instance.animator.CrossFade("Player_GiveUp",0.1f);
        delayShowEndTimer = 2;
    }

    void GamePlayingUpdate()
    {
        var enemyActionRunning = false;
        if (Player.Instance == null) return;
        if(Player.Instance.currentAction != null)
        {
            if (Player.Instance.currentAction.CheckComplete())
            {
                // 主角动作完成回调
                Player.Instance.currentAction = null;
                // 更新敌人行为
                for (var i = 0; i < boardManager.enemies.Count; i++)
                {
                    var enemy = boardManager.enemies[i];
                    enemy.CheckAction();
                }
            }
            else
            {
                Player.Instance.currentAction.Run();
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

        if(Player.Instance.currentAction == null && !enemyActionRunning && !graffing)
        {
            ListenClick();
        }
        if(enemyActionRunning)
        {
            gameCanvas.DisableBottle();
            gameCanvas.DisableWhistle();
        }
    }

    void ListenClick()
    {
        if (pausing) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = gCamera.m_camera.ScreenPointToRay(Input.mousePosition);
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

                if (Player.Instance.moving || Player.Instance.currentTile != tile)
                {
                    Player.Instance.currentAction = new ActionPlayerMove(Player.Instance,ActionType.PlayerMove, tile);
                    Debug.Log("主角行为====移动");
                }
            }
        }
    }

    public void UseWhistle()
    {
        var nodes = boardManager.FindNodesAround(Player.Instance.currentTile.name ,2);
        foreach(var kvp in nodes)
        {
            var name = kvp.Key;
            for(var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                if (enemy.coord.name == name)
                {
                    enemy.Alert(Player.Instance.currentTile.name);
                    Debug.Log("主角行为====吹哨");
                }
            }
        }
        Player.Instance.currentAction = null;
    }

    public void UseBottle()
    {
        Player.Instance.currentAction = null;
    }

    public void ThrowBottle()
    {
        Player.Instance.currentAction = null;
    }

    public void Save()
    {
        PlayerPrefs.SetInt("Energy", energy);
    }

    
}
