﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

    public static int MAX_LEVEL = 35;

    public static bool teaching = false;

    public Transform guideArrow;
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
        resLoaded = false;
    }
    bool resLoaded = false;
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
        player.m_animator.SetInteger("result", 0);
        player.bottleCount = 0;
        delayShowEndTimer = 0;
        bottleSelectingTarget = false;
        gameCanvas.level = playingLevel+1;
        gameCanvas.Show();
        gameCanvas.InitWithBoardManager(boardMgr);
        cameraSettingCanvas.InitWithGameCamera(camera, player);
        cameraSettingCanvas.SetExpand(false);
        result = GameResult.NONE;


        guideArrow = (GameObject.Instantiate(Resources.Load("Prefab/GuideArrow")) as GameObject).transform;
        guideArrow.gameObject.SetActive(false);

        Debug.Log("当前场景名称:" + sceneName);
        Addressables.LoadAssetAsync<GameObject>(string.Format("Assets/__Resources/Prefab/Scene/{0}/{1}.prefab", chapter, chapter+"-"+index)).Completed += onScenePrefabLoaded;
    }


    void onScenePrefabLoaded(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj)
    {
        Debug.Log("加载好的远程场景预设");
        var sceneNode = GameObject.Find("Scene");
        sceneNode.transform.localPosition = Vector3.zero;
        var instance = Instantiate(obj.Result);
        instance.transform.parent = sceneNode.transform;
        instance.transform.localPosition = Vector3.zero;
        resLoaded = true;
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
        player.m_animator.SetInteger("result",-1);
    }

    public void WinGame()
    {
        result = GameResult.WIN;
        delayShowEndTimer = 2;
        playing = false;
        player.m_animator.SetInteger("result",1);
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

        // 更新主角站立狀態
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
        
        if (player.currentAction == null && !enemyActionRunning)
        {
            ShowGuide();
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
                player.CheckWhitsle();
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
                GridTile tile = player.gridManager.GetTileByName(node.name);
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
                    player.currentAction = Utils.CreatePlayerAction(ActionType.ThrowBottle, tile); // 
                    boardManager.HideAllSuqreContour();
                    bottleSelectingTarget = false;
                    player.CheckBottle();
                    Debug.Log("扔瓶子:"+ node.name);
                }
            }
        }
    }

    public void HideGuide()
    {
        if (guideArrow == null) return;
        guideArrow.gameObject.SetActive(false);
    }
    


    WalkThroughStep showingStep = null;
    void ShowGuide()
    {
        if (resLoaded == false) return;
        if (guideArrow == null) return;
        if (teaching == false)
        {
            guideArrow.gameObject.SetActive(false);
            gameCanvas.HideGuides();
            gameCanvas.HideItemGuides();
            return;
        }
        if (boardManager.steps?.Count>0)
        {
            var currentStep = boardManager.steps[0];
            if( showingStep?.actionType == currentStep.actionType && showingStep?.tileName == currentStep.tileName)
            {
                return ;
            }
            showingStep = currentStep;

            if (currentStep.actionType == ActionType.PlayerMove)
            {
                gameCanvas.HideGuides();
                gameCanvas.HideItemGuides();

                guideArrow.gameObject.SetActive(true);

                var node = boardManager.FindNode(currentStep.tileName);
                if (node)
                {
                    guideArrow.gameObject.SetActive(true);
                    //guideArrow.transform.GetChild(0).GetComponent<Animator>().Play("Guide");
                    guideArrow.transform.position = node.transform.position;
                }
            }
            else if (currentStep.actionType == ActionType.BlowWhistle)
            {
                guideArrow.gameObject.SetActive(false);
                gameCanvas.HideItemGuides();

                gameCanvas.ShowWhitsleGuide();
            }
            else if (currentStep.actionType == ActionType.ThrowBottle)
            {
                guideArrow.gameObject.SetActive(false);
                gameCanvas.HideItemGuides();
                if (currentStep.tileName == "")
                {
                    gameCanvas.ShowBottleGuide();
                }
                else
                {
                    var node = boardManager.FindNode(currentStep.tileName);
                    if (node)
                    {
                        guideArrow.gameObject.SetActive(true);
                        guideArrow.transform.position = node.transform.position;
                    }
                }
            }
            else if(currentStep.actionType == ActionType.Steal)
            {
                guideArrow.gameObject.SetActive(false);
                gameCanvas.ShowStealGuide();
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

                var linkLine = player.boardManager.FindLine(player.currentTile.name, tile.name);
                if(linkLine == null || linkLine.through== false)
                {
                    if(linkLine==null)
                    {
                        Debug.Log("路径点连接GameObject名字出错");
                    }
                    
                    return;
                }

                if (player.moving || player.currentTile != tile)
                {
                    player.currentAction = Utils.CreatePlayerAction( ActionType.PlayerMove, tile);
                    //Debug.Log("主角行为====移动");
                }
            }
        }
    }

    public void BlowWhistle()
    {
        player.currentAction = Utils.CreatePlayerAction(ActionType.BlowWhistle,player.currentTile);
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
            player.currentAction = Utils.CreatePlayerAction(ActionType.Steal, player.currentTile);
        }
    }

    List<string> bottleSelectable = new List<string>();
    public void BottleSelectTarget()
    {
        player.bottle.gameObject.SetActive(true);
        bottleSelectable.Clear();
        bottleSelectingTarget = true;
        player.m_animator.SetInteger("bottle", 0);
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

        if(showingStep!=null)
        {
            if(showingStep.actionType == ActionType.ThrowBottle && showingStep.tileName == "")
            {
                boardManager.steps.RemoveAt(0);
                ShowGuide();
            }

        }
    }

    public void CancelBottleSelectTarget()
    {
        boardManager.HideAllSuqreContour();
        player.m_animator.SetInteger("bottle", -1);
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

    public void CutBarbedWire(PincersItem item)
    {
        player.currentAction = new ActionPincersCut(player, item);
    }

    public void JumpIntoManholeCover(ManholeCoverItem item)
    {
        player.currentAction = new ActionJumpManholeCover(player, item);

    }

    public void SkipPlayerTurn()
    {
        player.currentAction = new ActionTurnDirection(player, player.direction);
    }
}
