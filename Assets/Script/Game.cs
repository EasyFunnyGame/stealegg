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
    public TranslateCanvas translateCanvas;

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

    public int gainEergy = 0;

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
        translateCanvas.Hide();
    }

    public void StartGame(string sceneName)
    {
        PlayLevel(sceneName);
        chapterCanvas.Hide();
        result = GameResult.NONE;
        gainEergy = 0;
    }

    public void PlayLevel(string sceneName)
    {
        AudioPlay.Instance.PlayBackGroundMusic();
        SceneManager.LoadScene(sceneName);
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

        boardManager.Ready();

        CleearMoves();
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
            if (result == GameResult.WIN && !endCanvas.gameObject.activeSelf && player.gameObject && player.gameObject.activeSelf)
            {
                player.transform.Translate(new Vector3(0, 0, 0.02f));
            }
        }

        if(delayShowEndTimer > 0)
        {
            delayShowEndTimer -= Time.deltaTime;
            if (delayShowEndTimer < 0)
            {
                EndGame();
            }
        }
    }

    public void FailGame(Enemy enemy)
    {
        result = GameResult.FAIL;
        delayShowEndTimer = 2;
        player.m_animator.SetInteger("result",-1);
        AudioPlay.Instance.PlayBeCaught();
        player.failCamera.gameObject.SetActive(true);

        var playerHead = player.transform.position + new Vector3(0, 1f, 0);

        var playerPos = new Vector3(player.transform.position.x,0, player.transform.position.z);
        var enemyPos  = new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z);

        var direction = playerPos - enemyPos;
        var normalized = direction.normalized;

        player.failCamera.transform.position = playerHead + normalized;


        var playerForwardNormalized = player.tr_body.transform.forward.normalized;


        var angle = Mathf.Abs(Vector3.Angle(enemy.tr_body.forward, player.tr_body.forward));
        angle %= 360;
        if (angle > 1 && angle < 180)
        {
            player.failCamera.transform.position += (playerForwardNormalized/2);
        }
        else
        {
            player.failCamera.transform.position += (player.tr_body.transform.right.normalized/2);
        }
        
        player.failCamera.transform.LookAt(enemy.transform.position) ;
    }

    public void WinGame()
    {
        result = GameResult.WIN;
        delayShowEndTimer = 2;
        player.m_animator.SetInteger("result",1);

        if(gainEergy >0 )
        {
            var energy = PlayerPrefs.GetInt(UserDataKey.Energy);
            energy += gainEergy;
            PlayerPrefs.SetInt(UserDataKey.Energy,energy);
            PlayerPrefs.Save();
        }
    }

    public bool enemyTurn = false;
    public bool playerTurn = false;

    void GamePlayingUpdate()
    {
        enemyActionRunning = false;
        if (player == null) return;
        if (player.justSteal) return;
        if (player.currentAction != null)
        {
            playerTurn = true;
            enemyTurn = false;
            if (player.currentAction.CheckComplete())
            {
                // 主角动作完成回调
                player.currentAction = null;

                playerTurn = false;
                enemyTurn = true;

                
                // 更新敌人行为
                for (var i = 0; i < boardManager.enemies.Count; i++)
                {
                    var enemy = boardManager.enemies[i];
                    enemy.CheckAction();
                }
                boardManager.coordLure.SetNoTurn();
                boardManager.growthLure.SetNoTurn();
            }
            else
            {
                player.currentAction.Run();
            }
        }
        else
        {
            
            for (var i = 0; i < boardManager.enemies.Count; i++)
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

        
        if (result == GameResult.NONE && player.currentAction == null && !enemyActionRunning)
        {
            ShowGuide();
            UpdateMoves();
            // ListenClick();
        }

        player.CheckWhitsle();
        player.CheckBottle();
    }


    void ListenBottleTargetSelect()
    {
        if (pausing) return;

        if (Input.GetMouseButtonDown(0) )
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

                    var allItems = boardManager.allItems;
                    if(allItems.ContainsKey(node.gameObject.name))
                    {
                        var nodeItem = allItems[node.gameObject.name];
                        if(nodeItem != null)
                        {
                            if(nodeItem.itemType == ItemType.Growth)
                            {
                                guideArrow.transform.Translate(new Vector3(0, 1.0f, 0));
                            }

                        }
                    }
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

            if (player.currentTile.name == nodeName)
            {
                selectable = false;
            }
            if(!selectable)
            {
                continue;
            }
            foreach (var enemy in boardManager.enemies)
            {
                if (enemy.coord.name == nodeName)
                {
                    selectable = false;
                    break;
                }
            }
            if (!selectable)
            {
                continue;
            }
            var influenceAnyEnemy = false;
            foreach (var enemy in boardManager.enemies)
            {
                if (Mathf.Abs(enemy.coord.x - nodeGo.coord.x) <= 2 && Mathf.Abs(enemy.coord.z - nodeGo.coord.z) <= 2)
                {
                    influenceAnyEnemy = true;
                    break;
                }
            }
            if(!influenceAnyEnemy)
            {
                continue;
            }

            node.contour.gameObject.SetActive(selectable);
            node.contour.transform.localPosition = new Vector3(0,0.025f,0);
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
        //
        camera.SetTargets(new List<GameObject>().ToArray());
    }

    public void BottleThorwed(string tileName)
    {
        var targetArray = tileName.Split('_');
        var x = int.Parse(targetArray[0]);
        var z = int.Parse(targetArray[1]);
        var height = boardManager.FindNode(tileName)?.transform.position.y ?? 0;

        boardManager.coordLure = new Coord(x, z, height);
        boardManager.rangeLure = 2;

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
        player.currentAction = new ActionTurnDirection(player, player.direction, false);
        if (teaching && showingStep != null)
        {
            if (showingStep.actionType == ActionType.TurnDirection)
            {
                boardManager.steps.RemoveAt(0);
                ShowGuide();
            }
        }
    }

    //private void FindPathTest(string from , string to )
    //{
    //    var fromTile = player.gridManager.GetTileByName(from);
    //    var toTile = player.gridManager.GetTileByName (to);
    //    player.gridManager.find_paths_realtime(player, toTile, fromTile);
    //    player.path = toTile.db_path_lowest;
    //    player.UpdateTargetDirection(player.nextTile);
    //}

    public void ClickGameBoard()
    {
        if (player == null) return;
        if (!playing) return;
        if (bottleSelectingTarget)
        {
            Ray ray1 = camera.m_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo1;

            if (Physics.Raycast(ray1, out hitInfo1, 100, LayerMask.GetMask("Square")))
            {
                var node1 = hitInfo1.transform.parent.parent;
                if (bottleSelectable.IndexOf(node1.name) == -1)
                {
                    return;
                }
                GridTile tile1 = player.gridManager.GetTileByName(node1.name);
                for (var i = 0; i < boardManager.enemies.Count; i++)
                {
                    var enemy = boardManager.enemies[i];
                    if (enemy.coord.name == tile1.name)
                    {
                        return;
                    }
                }

                if (player.moving || player.currentTile != tile1)
                {
                    player.currentAction = Utils.CreatePlayerAction(ActionType.ThrowBottle, tile1); // 
                    boardManager.HideAllSuqreContour();
                    bottleSelectingTarget = false;
                    gameCanvas.btn_bottle_cancel.gameObject.SetActive(false);
                }
                camera.upper = false;
            }
            return;
        }
        if (graffCanvas.gameObject.activeSelf) return;
        if (pausing) return;
        if (player.justSteal) return;
        if (result != GameResult.NONE) return;
        if (player.currentAction != null) return;
        enemyActionRunning = false;
        for (var i = 0; i < boardManager.enemies.Count; i++)
        {
            var enemy = boardManager.enemies[i];
            if (enemy.currentAction != null)
            {
                enemyActionRunning = true;
                break;
            }
        }

        if (enemyActionRunning)
        {
            return;
        }

        var position = Input.mousePosition;
        //FindPathTest("3_2","3_4");
        //return;
        Ray ray = camera.m_camera.ScreenPointToRay(position);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 100, LayerMask.GetMask("Square")))
        {

            var nodeName = "";
            var item = hitInfo.transform.GetComponent<Item>();
            if(item!=null)
            {
                nodeName = item.coord.name;
            }
            else
            {
                nodeName = hitInfo.transform.parent.parent.name;
            }
            var tile = player.gridManager.GetTileByName(nodeName);

            for (var i = 0; i < boardManager.enemies.Count; i++)
            {
                var enemy = boardManager.enemies[i];
                if (enemy.coord.name == nodeName)
                {
                    AudioPlay.Instance.ClickUnWalkable();
                    return;
                }
            }

            var linkLine = player.boardManager.FindLine(player.currentTile.name, tile.name);
            if (linkLine == null || linkLine.through == false)
            {
                if (linkLine == null)
                {
                    //Debug.Log("路径点连接GameObject名字出错");
                }
                AudioPlay.Instance.ClickUnWalkable();
                return;
            }

            if (player.moving || player.currentTile != tile)
            {
                player.currentAction = Utils.CreatePlayerAction(ActionType.PlayerMove, tile);
                //Debug.Log("主角行为====移动");
            }
        }
    }

    private List<Animator> enemyMoves = new List<Animator>();
    private Dictionary<string, Animator> showingMoves = new Dictionary<string, Animator>();

    private void CleearMoves()
    {
        foreach(var kvp in showingMoves)
        {
            //if (kvp.Value.gameObject.) continue;
            //Destroy(kvp.Value.gameObject);
        }
        showingMoves.Clear();
        for(var index = 0; index < enemyMoves.Count; index++)
        {
            //Destroy(enemyMoves[index]);
        }
        enemyMoves.Clear();
    }

    public void AddMoves(Animator enemyMove)
    {
        enemyMoves.Add(enemyMove);
        enemyMove.gameObject.SetActive(false);
        //Debug.Log("添加  enemy move " + enemyMoves.Count);
    }

    public void UpdateMoves(string playName ="")
    {
        // 所有需要显示的追踪点
        var tracingCoords = new List<string>();
        if(!string.IsNullOrEmpty(playName))
        {
            tracingCoords.Add(playName);
        }
        for(var index = 0; index < boardManager.enemies.Count; index++)
        {
            var enemy = boardManager.enemies[index];
            var coord = enemy.coordTracing;
            if (!coord.isLegal) continue;
            tracingCoords.Add(coord.name);
        }

        // 删掉已经不需要继续显示的特效
        var removeKeys = new List<string>();
        foreach (var kvp in showingMoves)
        {
            if (tracingCoords.IndexOf(kvp.Key) == -1)
            {
                kvp.Value.gameObject.SetActive(false);
                enemyMoves.Add(kvp.Value);
                removeKeys.Add(kvp.Key);
            }
        }
        for(var index = 0; index  < removeKeys.Count; index++)
        {
            showingMoves.Remove(removeKeys[index]);
        }

        // 如果没有显示，则添加
        for (var index = 0; index < tracingCoords.Count; index ++)
        {
            var coordName = tracingCoords[index];
            var showingMove = showingMoves.ContainsKey(coordName);
            if (showingMove) continue;
            if (enemyMoves.Count <= 0) continue;
            var move = enemyMoves[0];
            enemyMoves.RemoveAt(0);
            var node = boardManager.FindNode(coordName);
            if (node == null) continue;
            move.gameObject.SetActive(true);
            move.transform.position = node.transform.position;
            move.transform.parent = null;
            showingMoves.Add(coordName, move);
        }
        
    }
}
