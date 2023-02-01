

public class Player : character
{
    public BoardManager boardManager;

    public static Player instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }

    override protected void Reached()
    {
        base.Reached();
        boardManager.PickItem(tile_s.name,this);
    }
}
