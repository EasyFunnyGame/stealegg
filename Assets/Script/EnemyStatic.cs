using UnityEngine;

public class EnemyStatic : Enemy
{

    public override void Start()
    {
        this.checkRange = 3;
        this.sleeping = false;
        this.patroling = false;
        base.Start();
        
    }

    public override void Reached()
    {
        base.Reached();
        sleeping = false;
    }

    public override void ReachedOriginal()
    {
        base.ReachedOriginal();
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        sleeping = false;
        targetIdleType = 0;
    }
}
