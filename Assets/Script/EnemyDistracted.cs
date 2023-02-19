public class EnemyDistracted : Enemy
{
    
    public override void OnReachedOriginal()
    {
        icons.shuijiao.gameObject.SetActive(true);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
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
