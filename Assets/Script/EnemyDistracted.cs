﻿
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

    private float sleepSoundTime = 3f;
    private int sleepSoundType = 1;

    protected override void Update()
    {
        base.Update();

        if (!sleeping) return;

        if (Game.Instance.chapterCanvas.gameObject.activeInHierarchy || Game.Instance.mainCanvas.gameObject.activeInHierarchy) return;
        
        if(sleepSoundTime <=0)
        {
            sleepSoundTime = 4f;

            if(sleepSoundType == 1)
            {
                AudioPlay.Instance?.EnemySleepIn();
                sleepSoundType = 0;
            }
            else
            {
                sleepSoundType = 1;
                AudioPlay.Instance?.EnemySleepOut();
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
        checkRange = 1;
    }
}
