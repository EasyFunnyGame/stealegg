
using UnityEngine;
public class EnemyDistracted : Enemy
{
    public int breath = 1;

    public override void Start()
    {
        this.checkRange = 1;
        this.sleeping = true;
        this.patroling = false;
        base.Start();
    }

    private float sleepSoundTime = 2.5f;
    private int sleepSoundType = 1;

    protected override void Update()
    {
        base.Update();

        if (!sleeping) return;
        
        if(sleepSoundTime <=0)
        {
            sleepSoundTime = 2.5f;

            if(sleepSoundType == 1)
            {
                AudioPlay.Instance.EnemySleepIn();
            }
            else
            {
                AudioPlay.Instance.EnemySleepIn();
            }
        }
        else
        {
            sleepSoundTime -= Time.deltaTime;
        }
    }

    public override void ReachedOriginal()
    {
        base.ReachedOriginal();
        icons.shuijiao.gameObject.SetActive(true);
        icons.tanhao.gameObject.SetActive(false);
        icons.fanhui.gameObject.SetActive(false);
        icons.wenhao.gameObject.SetActive(false);
        targetIdleType = 0;
        idleType = 0;
        sleeping = true;
        sleepSoundTime = 0;
    }

    //public override bool LureBottle(string tileName)
    //{
    //    UpdateRouteMark();
    //    return base.LureBottle(tileName);
    //}

    //public override bool LureWhistle(string tileName)
    //{
    //    var result = base.LureWhistle(tileName);
    //    UpdateRouteMark();
    //    return result;
    //}

    //public override bool LureSteal(string tileName)
    //{
    //    UpdateRouteMark();
    //    return base.LureSteal(tileName);
    //}
}
