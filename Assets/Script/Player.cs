

using UnityEngine;

public class Player : character
{
    public Animator animator;

    public static Player instance;

    void Awake()
    {
        instance = this;
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }

    override protected void OnReached()
    {
        base.OnReached();
        boardManager.PickItem(tile_s.name,this);
        animator.CrossFade("Player_Idle",0.1f);
        Debug.Log(string.Format("{0}到达{1}", gameObject.name, tile_s.gameObject.name));
    }

    override protected void OnStartMove()
    {
        base.OnStartMove();
        animator.CrossFade("Player_Run",0.1f);
        Debug.Log(string.Format("{0}开始行走{1}", gameObject.name, tar_tile_s.name));
    }

    public override void FootL()
    {
        base.FootL();
    }

    public override void FootR()
    {
        base.FootR();

    }
}
