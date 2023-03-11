using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioPlay : MonoBehaviour
{
    public static AudioPlay Instance;

    public AudioManager manager;

    private void Awake()
    {
        Instance = this;
    }

    static bool init = false;

    private void Update()
    {
        if(init == false)
        {
            init = AudioManager.loaded;
            PlayMusic();
        }
    }

    public void PlayMusic()
    {
        if(SceneManager.GetActiveScene().name== "Main")
        {
            PlayMain();
        }
        else
        {
            PlayBackGroundMusic();
        }
    }


    public void PlayMain()
    {
        if (!init) return;
        if (AudioManager.audioBGM != null)
        {
            manager.RemoveAudio(AudioManager.audioBGM, true);
        }
        var audioSrc = AudioManager.audioList[0];
        // 长音频在使用后需要销毁
        AudioManager.audioBGM = manager.CreateAudio();
        AudioManager.audioBGM.loop = true;
        AudioManager.audioBGM.src = audioSrc;
        AudioManager.audioBGM.OnCanplay(() =>
        {
            AudioManager.audioBGM.Play();
            Debug.Log("播放主界面背景音乐:" + audioSrc);
        });
        // 自动播放停止
        AudioManager.audioBGM.OnEnded(() =>
        {
            PlayMain();
        });
        // 手动停止
        AudioManager.audioBGM.OnStop(() =>
        {
            AudioManager.audioBGM = null;
        });
    }


    string inGameAudioSrc = "";
    public void PlayBackGroundMusic()
    {
        if (!init) return;
        var index = new System.Random().Next(4, 9);
        
        if (AudioManager.audioBGM != null)
        {
            manager.RemoveAudio(AudioManager.audioBGM, true);
        }
        inGameAudioSrc = AudioManager.audioList[index];
        // 长音频在使用后需要销毁
        AudioManager.audioBGM = manager.CreateAudio();
        // audioBGM.loop = true;
        AudioManager.audioBGM.src = inGameAudioSrc;
        AudioManager.audioBGM.OnCanplay(() =>
        {
            AudioManager.audioBGM.Play();
            //Debug.Log("播放场景内背景音乐:" + inGameAudioSrc);
        });
        // 自动播放停止
        AudioManager.audioBGM.OnEnded(() =>
        {
            AudioManager.audioBGM.Play();
        });
        // 手动停止
        AudioManager.audioBGM.OnStop(() =>
        {
            AudioManager.audioBGM = null;
        });
    }


    public void PlaySFX(int index)
    {
        var src = AudioManager.sfxList[index];

        var audioPlayRightNow = manager.CreateAudio();
        if (audioPlayRightNow == null)
        {
            return;
        }
        // 自动播放停止
        audioPlayRightNow.OnEnded(() =>
        {
            manager.RemoveAudio(audioPlayRightNow);
        });
        // 手动停止
        audioPlayRightNow.OnStop(() =>
        {
            manager.RemoveAudio(audioPlayRightNow);
        });
        // 如果要设置的src和原音频对象一致，可以直接播放
        if (audioPlayRightNow.src == src)
        {
            audioPlayRightNow.Play();
        }
        else
        {
            // 如果当前音频已经下载过，并且配置了缓存本地，就算设置needDownload为false也不会重复下载
            audioPlayRightNow.src = src;
            audioPlayRightNow.Play();
        }
    }

    public void PlayerFootLeft()
    {
        var lineName = Game.Instance.player.walkingLineType;
        var height = Game.Instance.player.up;
        Debug.Log("玩家行进路线左"+ lineName + " 高度:" + height);
        var index = -1;
        if (lineName == "Hor_Normal_Visual" || lineName == "Hor_Doted_Visual")
        {
            index = new System.Random().Next(0, 4);
        }
        else if (lineName == "StairsUp_Normal_Visual" && height == 1)
        {
            index = new System.Random().Next(6, 9);
        }
        else if (lineName == "StairsUp_Normal_Visual" && height == -1)
        {
            index = new System.Random().Next(9, 12);
        }
        else if (lineName == "ClimbUp_Doted_Visual" && height == 1)
        {
            index = 21;
        }
        else if (lineName == "ClimbUp_Doted_Visual" && height == -1)
        {
            index = 22;
        }
        if (index != -1)
        {
            Instance.PlaySFX(index);
        }
    }

    public void EnemyFootLeft(Enemy enemy)
    {
        if (!enemy) return;
        var lineName = enemy.walkingLineType;
        var height = enemy.up;
        var index = -1;
        if (lineName == "StairsUp_Normal_Visual" && height == 1)
        {
            index = 54;
        }
        else if (lineName == "StairsUp_Normal_Visual" && height == -1)
        {
            index = 53;
        }
        else
        {
            index = new System.Random().Next(44, 53);
        }
        if (index != -1)
        {
            Instance.PlaySFX(index);
        }
    }

    public void EnemyFootRight(Enemy enemy)
    {
        if (!enemy) return;
        var lineName = enemy.walkingLineType;
        var height = enemy.up;
        var index = -1;
        if (lineName == "StairsUp_Normal_Visual" && height == 1)
        {
            index = 54;
        }
        else if (lineName == "StairsUp_Normal_Visual" && height == -1)
        {
            index = 53;
        }
        else
        {
            index = new System.Random().Next(44, 53);
        }
        if (index != -1)
        {
            Instance.PlaySFX(index);
        }
    }

    public void PlayerFootRight()
    {
        var lineName = Game.Instance.player.walkingLineType;
        var height = Game.Instance.player.up;
        Debug.Log("玩家行进路线右" + lineName + " 高度:" + height);
        var index = -1;
        if(lineName == "Hor_Normal_Visual" || lineName == "Hor_Doted_Visual")
        {
            index = new System.Random().Next(3, 6);
        }
        else if(lineName == "StairsUp_Normal_Visual" && height == 1)
        {
            index = new System.Random().Next(6, 9);
        }
        else if (lineName == "StairsUp_Normal_Visual" && height == -1)
        {
            index = new System.Random().Next(9, 12);
        }
        else if (lineName == "ClimbUp_Doted_Visual" && height == 1)
        {
            index = 21;
        }
        else if (lineName == "ClimbUp_Doted_Visual" && height == -1)
        {
            index = 22;
        }
        if (index != -1)
        {
            Instance.PlaySFX(index);
        }
    }

    public void ClickUnWalkable()
    {
        var index = new System.Random().Next(13, 17);
        Instance.PlaySFX(index);
    }

    public void PlayerBlowWhitsle()
    {
        var index = new System.Random().Next(17, 21);
        Instance.PlaySFX(index);
    }

    public void PlayPickSfx()
    {
        var player = Game.Instance.player;
        if(player.pickedBottle)
        {
            PlayerPickBottle();
        }
    }

    public void PlayerPickBottle()
    {
        var index = new System.Random().Next(23, 25);
        Instance.PlaySFX(index);
    }


    public void PlayerThrowBottle()
    {
        var index = new System.Random().Next(25, 27);
        Instance.PlaySFX(index);
    }

    public void PlayerBottleGrounded()
    {
        Instance.PlaySFX(27);
    }

    public void PlayJumpOut()
    {
        Instance.PlaySFX(29);
    }
    public void PlayJumpIn()
    {
        Instance.PlaySFX(28);
    }

    public void PlayFound()
    {
        Instance.PlaySFX(30);
    }

    public void PlayCatch(Enemy enemy)
    {
        var index = -1;
        if (enemy is EnemyStatic)
        {
            index = new System.Random().Next(83, 86);
        }
        else if (enemy is EnemyDistracted)
        {
            index = new System.Random().Next(83, 86);
        }
        else if (enemy is EnemyPatrol)
        {
            index = new System.Random().Next(66, 69);
        }
        else if (enemy is EnemySentinel)
        {
            index = new System.Random().Next(66, 69);
        }
        if (index != -1)
        {
            Instance.PlaySFX(index);
        }
    }

    public void PlayNotFound(Enemy enemy)
    {
        var index = -1;
        if (enemy is EnemyStatic)
        {
            index = new System.Random().Next(69, 72);
        }
        else if(enemy is EnemyDistracted)
        {
            index = new System.Random().Next(69, 72);
        }
        else if (enemy is EnemyPatrol)
        {
            index = new System.Random().Next(58, 63);
        }
        else if (enemy is EnemySentinel)
        {
            index = new System.Random().Next(58, 63);
        }
        if(index != -1)
        {
            Instance.PlaySFX(index);
        }
        
    }

    public void PlayHeard()
    {
        Instance.PlaySFX(31);
    }

    public void PlayStarGain()
    {
        Instance.PlaySFX(87);
    }

    public void PlayWatchTurn()
    {
        var index = new System.Random().Next(63, 66);
        Instance.PlaySFX(index);
    }
}
