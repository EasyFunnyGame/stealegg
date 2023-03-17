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

    public int playingLevel;

    public string currentLevelName;

    public BoardManager boardManager;

    public new GameCamera camera;

    public FreeDraw.DrawingSettings draw_setting;

    public FreeDraw.Drawable draw_able;

    public Camera draw_camera;

    public Player player;

    public bool pausing = false;

    public bool playing = false;

    public GameResult result;

    public bool stealed = false;

    public bool bottleSelectingTarget = false;

    public static int MAX_LEVEL = 35;

    public static bool teaching = false;

    public static int clearTeaching = 0;

    public Transform guideArrow;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        var energy = PlayerPrefs.GetInt(UserDataKey.Energy, -999);
        if (energy == -999)
            energy = 10;
        PlayerPrefs.GetInt(UserDataKey.Energy, energy);

        mainCanvas.Show();
        msgCanvas.Show();
        gameCanvas.Hide();
        endCanvas.Hide();
        energyGainCanvas.Hide();
        hintGainCanvas.Hide();
        settingCanvas.Hide();
        chapterCanvas.Hide();
        graffCanvas.Hide();
        cameraSettingCanvas.Hide();
    }

    public void StartGame(string sceneName)
    {
        PlayLevel(sceneName);

        var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
        PlayerPrefs.SetInt(UserDataKey.Energy, energy);
        PlayerPrefs.Save();
        chapterCanvas.Hide();

        result = GameResult.NONE;

        Save();
    }

    public void PlayLevel(string sceneName)
    {
        AudioPlay.Instance.PlayBackGroundMusic();
        //if(!sceneName.StartsWith("3"))
        {
            SceneManager.LoadScene(sceneName);
        }
        //else
        //{
        //    loadingSceneName = sceneName;
        //    StartCoroutine("LoadsScene");
        //}
        resLoaded = false;
    }


    bool resLoaded = false;
    public void SceneLoaded(BoardManager boardMgr , string sceneName)
    {
        stealed = false;
        boardManager = boardMgr;
        var nameArr = sceneName.Split('-');
        var chapter = int.Parse(nameArr[0]);
        var index = int.Parse(nameArr[1]);
        playingLevel = (chapter - 1) * 12 + (index - 1);
        currentLevelName = sceneName;

        var drawObject = GameObject.Find("Draw");
        if (drawObject != null)
        {
            draw_camera = GameObject.Find("draw_camera").GetComponent<Camera>();
            draw_camera?.gameObject.SetActive(false);
            draw_setting = GameObject.Find("DrawingSettings").GetComponent<FreeDraw.DrawingSettings>();
            draw_setting?.gameObject.SetActive(false);
            draw_able = GameObject.Find("Drawable").GetComponent<FreeDraw.Drawable>();
            draw_able?.gameObject.SetActive(false);
            if (draw_camera && draw_able)
            {
                draw_able.cam = draw_camera;
            }
        }
       

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

        resLoaded = true;

        if (teaching)
        {
            clearTeaching++;
            if (clearTeaching > 1)
            {
                teaching = false;
                clearTeaching = 0;
            }
            gameCanvas.onClickStartPlayingGameHandler();
        }
        camera.upper = false;
    }


    public void EndGame()
    {
        gameCanvas.Hide();
        endCanvas.Show();
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
            if (result == GameResult.WIN && !endCanvas.gameObject.activeSelf)
            {
                player.transform.Translate(new Vector3(0, 0, 0.02f));
            }
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
        player.m_animator.SetInteger("result",-1);
        AudioPlay.Instance.PlayBeCaught();
    }

    public void WinGame()
    {
        result = GameResult.WIN;
        delayShowEndTimer = 2;
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
        player.founded = beFound;
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
        
        if (result == GameResult.NONE && player.currentAction == null && !enemyActionRunning)
        {
            ShowGuide();
            ListenClick();
            toolCheckDelay -= Time.deltaTime;
        }
        else
        {
            toolCheckDelay = 0.25f;
            gameCanvas.DisableBottle();
            //gameCanvas.DisableWhistle();
        }
        player.CheckWhitsle();
    }

    float toolCheckDelay = 0.5f;
    
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
                    gameCanvas.btn_bottle_cancel.gameObject.SetActive(false);
                }
                camera.upper = false;
            }
        }
    }

    public void HideGuide()
    {
        if (guideArrow == null) return;
        guideArrow.gameObject.SetActive(false);
    }
    


    public WalkThroughStep showingStep = null;
    public void ShowGuide()
    {
        if (resLoaded == false) return;
        if (guideArrow == null) return;
        if (teaching == false)
        {
            guideArrow.gameObject.SetActive(false);
            gameCanvas.HideWhitsleAndBottleGuides();
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
                gameCanvas.HideWhitsleAndBottleGuides();
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
                gameCanvas.HideItemGuides();
                gameCanvas.ShowStealGuide();
            }
            else if(currentStep.actionType == ActionType.PincersCut)
            {
                guideArrow.gameObject.SetActive(false);
                gameCanvas.HideWhitsleAndBottleGuides();
                gameCanvas.ShowPincersGuide(currentStep.tileName);
            }
            else if(currentStep.actionType == ActionType.ManHoleCover)
            {
                guideArrow.gameObject.SetActive(false);
                gameCanvas.HideWhitsleAndBottleGuides();
                gameCanvas.HideItemGuides();
                gameCanvas.ShowManHoleCoverGuide(currentStep.tileName);
            }
            else if (currentStep.actionType == ActionType.TurnDirection)
            {
                guideArrow.gameObject.SetActive(false);
                gameCanvas.HideWhitsleAndBottleGuides();
                gameCanvas.HideItemGuides();
                gameCanvas.ShowSkipTurnGuide(currentStep.tileName);
            }
        }
    }

    void ListenClick()
    {
        if(toolCheckDelay<=0)
        {
            player.CheckBottle();
            //player.CheckWhitsle();
        }
        else
        {
            //gameCanvas.DisableWhistle();
            gameCanvas.DisableBottle();
        }

        if (pausing) return;
        if (Input.GetMouseButtonDown(0))
        {
            //FindPathTest("3_2","3_4");
            //return;
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
                        AudioPlay.Instance.ClickUnWalkable();
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
                    AudioPlay.Instance.ClickUnWalkable();
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
        
    }

    bool enemyActionRunning = false;

    public void Steal()
    {
        if(enemyActionRunning)
        {
            return;
        }
        if(!stealed)
        {
            stealed = true;
            player.currentAction = Utils.CreatePlayerAction(ActionType.Steal, player.currentTile);
        }
    }

    List<string> bottleSelectable = new List<string>();
    public void BottleSelectTarget()
    {
        camera.upper = true;
        List<GameObject> allNodes = new List<GameObject>();
        foreach(var kvp in boardManager.nodes)
        {
            allNodes.Add(kvp.Value.gameObject);
        }
        camera.SetTargets(allNodes.ToArray());


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
            //if(player.startTileName == nodeName)
            //{
            //    selectable = false;
            //}
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

        if(teaching && showingStep!=null)
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
        camera.upper = false;
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
        if(stealed)
        {
            var level = 13;// PlayerPrefs.GetInt(UserDataKey.Level);
            if (playingLevel >= level)
            {
                level = playingLevel;
                PlayerPrefs.SetInt(UserDataKey.Level, playingLevel + 1);
                PlayerPrefs.Save();
            }
            WinGame();
            Save();
        }
    }

    public void CutBarbedWire(PincersItem item)
    {
        player.currentAction = new ActionPincersCut(player, item);
        if (teaching && showingStep != null)
        {
            if (showingStep.actionType == ActionType.PincersCut && showingStep.tileName == item.coord.name)
            {
                boardManager.steps.RemoveAt(0);
                ShowGuide();
            }
        }
    }

    public void JumpIntoManholeCover(ManholeCoverItem item)
    {
        player.currentAction = new ActionJumpManholeCover(player, item);

    }

    public void SkipPlayerTurn()
    {

        player.currentAction = new ActionTurnDirection(player, player.direction);
        if (teaching && showingStep != null)
        {
            if (showingStep.actionType == ActionType.TurnDirection)
            {
                boardManager.steps.RemoveAt(0);
                ShowGuide();
            }
        }
    }

    private void FindPathTest(string from , string to )
    {
        var fromTile = player.gridManager.GetTileByName(from);
        var toTile = player.gridManager.GetTileByName (to);
        player.gridManager.find_paths_realtime(player, toTile, fromTile);
        //player.UpdateMoves(player.nextTile);
        player.path = toTile.db_path_lowest;
        player.UpdateTargetDirection(player.nextTile);
    }
}
