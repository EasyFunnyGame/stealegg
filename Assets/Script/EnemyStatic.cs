using UnityEngine;

public class EnemyStatic : Enemy
{
    private void Awake()
    {
        
    }

    public override void Reached()
    {
        base.Reached();
        sleeping = false;
    }

    public override void OnReachedOriginal()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        sleeping = false;
        targetIdleType = 0;
    }
}
