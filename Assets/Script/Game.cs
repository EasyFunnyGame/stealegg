using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    public Transform ui;

    public MainCanvas mainCanvas;
    public MsgCanvas msgCanvas;



    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
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
