using System.Collections.Generic;
using UnityEngine;

public enum GameStatus
{
    NOT_START,

    STARTED,

    MY_TURN,

    ENEMY_TURN,

    WIN,

    FAIL
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

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        status = GameStatus.NOT_START;
    }

    public void StartGame()
    {

    }


    public void WinGame()
    {

    }


    public void FailGame()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
