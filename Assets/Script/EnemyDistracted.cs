public class EnemyDistracted : Enemy
{
    public int breath = 1;

    public override void Start()
    {
        this.sleeping = true;
        base.Start();
    }

    public override void OnReachedOriginal()
    {
        base.OnReachedOriginal();
        icons.shuijiao.gameObject.SetActive(true);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        targetIdleType = 0;
        idleType = 0;
        sleeping = true;
        UpdateRouteMark();
    }

    public override bool LureBottle(string tileName)
    {
        sleeping = false;
        UpdateRouteMark();
        return base.LureBottle(tileName);
    }

    public override bool LureWhistle(string tileName)
    {
        sleeping = false;
        UpdateRouteMark();
        return base.LureWhistle(tileName);
    }

    public override bool LureSteal(string tileName)
    {
        sleeping = false;
        UpdateRouteMark();
        return base.LureSteal(tileName);
    }
}
