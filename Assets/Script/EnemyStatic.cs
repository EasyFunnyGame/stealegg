using UnityEngine;

public class EnemyStatic : Enemy
{
    public override void Reached()
    {
        sleeping = false;
        base.Reached();
    }

    public override void OnReachedOriginal()
    {
        icons.shuijiao.gameObject.SetActive(false);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        sleeping = false;
        m_animator.Play("Player_Idle");
    }
}
