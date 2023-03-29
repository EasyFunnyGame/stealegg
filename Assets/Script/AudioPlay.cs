using UnityEngine;
using UnityEngine.SceneManagement;
using WeChatWASM;
public class AudioPlay : MonoBehaviour
{
    public static AudioPlay Instance;

    public AudioManager manager;

    public static float bgmVolume = 1;

    public static float sfxVolume = 1;

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

    public static void MusicVolume()
    {
        AudioManager.audioBGM.volume = bgmVolume;
    }

    public static void SoundVolume()
    {
        AudioManager.audioPlayArray.ForEach((context) => { if(context != AudioManager.audioBGM) context.volume = sfxVolume; });
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
            //Debug.Log("播放主界面背景音乐:" + audioSrc);
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
    string backgroundSrc = "";

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
            PlaySilencMusic();
            //AudioManager.audioBGM.Play();
        });
        // 手动停止
        AudioManager.audioBGM.OnStop(() =>
        {
            AudioManager.audioBGM = null;
        });
    }
    public void PlaySilencMusic()
    {
        if (!init) return;
        var index = new System.Random().Next(1, 4);
        backgroundSrc = AudioManager.audioList[index];
        if (AudioManager.audioBGM != null)
        {
            manager.RemoveAudio(AudioManager.audioBGM, true);
        }
        inGameAudioSrc = backgroundSrc;
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

    public WXInnerAudioContext PlaySFX(int index, float volumeScale = 1)
    {
        var src = AudioManager.sfxList[index];
        //Debug.Log("sound url:       " + src.Replace("https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/",""));
        var audioPlayRightNow = manager.CreateAudio();
        
        if (audioPlayRightNow == null)
        {
            return null;
        }
        audioPlayRightNow.volume *= volumeScale;
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
        return audioPlayRightNow;
    }

    public void PlayerFootLeft()
    {
        var lineName = Game.Instance.player.walkingLineType;
        var height = Game.Instance.player.up;
        //Debug.Log("玩家行进路线左"+ lineName + " 高度:" + height);
        var index = -1;
        if (lineName == "Hor_Normal_Visual" || lineName == "Hor_Doted_Visual")
        {
            index = new System.Random().Next(0, 3);
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

    public void PlayerFootRight()
    {
        var lineName = Game.Instance.player.walkingLineType;
        var height = Game.Instance.player.up;
        //Debug.Log("玩家行进路线右" + lineName + " 高度:" + height);
        var index = -1;
        if (lineName == "Hor_Normal_Visual" || lineName == "Hor_Doted_Visual")
        {
            index = new System.Random().Next(3, 6);
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

    public void PlayEnemyAlert(Enemy enemy)
    {
        var index = -1;
        if (enemy is EnemyStatic || enemy is EnemyDistracted)
        {
            index = new System.Random().Next(72, 77);
        }
        
        else if (enemy is EnemyPatrol || enemy is EnemySentinel)
        {
            index = new System.Random().Next(55, 58);
        }
        if (index != -1)
        {
            Instance.PlaySFX(index);
        }
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

    public void PlayReachExit()
    {
        Instance.PlaySFX(91);
    }

    public void PlayBeCaught()
    {
        Instance.PlaySFX(93);
    }

    public void PlayWin()
    {
        Instance.PlaySFX(92);
    }

    public void PlayFail()
    {
        Instance.PlaySFX(94);
    }

    public void PlayClick()
    {
        Instance.PlaySFX(89);
    }

    private WXInnerAudioContext sleepSound ;
    public void Speep(EnemyDistracted enemy)
    {
        if (!enemy) return;
        if(enemy.breath == 1)
        {
            EnemySleepIn();
        }
        else
        {
            EnemySleepOut();
        }
        enemy.breath *= -1;
    }

    public void EnemySleepIn()
    {
        var index = new System.Random().Next(77, 80);
        sleepSound =  Instance.PlaySFX(index,0.6f);
    }

    public void EnemySleepOut()
    {
        var index = new System.Random().Next(80, 83);
        sleepSound = Instance.PlaySFX(index, 0.6f);
    }

    public void StopSleepSound()
    {
        sleepSound?.Stop();
    }

    public void HideInTree()
    {
        Instance.PlaySFX(38);
    }

    public void WalkOutTree()
    {
        Instance.PlaySFX(39);
    }

    public void ThroughWireNet()
    {
        Instance.PlaySFX(32);
    }

    public void ThroughCuttedWireNet()
    {
        Instance.PlaySFX(33);
    }

    public void PlayPrincersCut()
    {
        int[] sounds1 = { 34, 36 };
        int[] sound2 = { 35, 37 };
        int[][] sounds = { sounds1, sound2};

        var index = new System.Random().Next(0, 2);
        var s = sounds[index];

        Instance.PlaySFX(s[0]);
        Instance.PlaySFX(s[1]);
    }

}
