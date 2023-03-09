using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    public static AudioPlay Instance;

    public AudioManager manager;

    private void Awake()
    {
        Instance = this;
    }

    bool _init = false;

    private void Update()
    {
        if(_init==false)
        {
            _init = manager.loaded;
            PlayBackGround();
        }
    }

    public void PlayBackGround()
    {
        if(Game.Instance.mainCanvas.gameObject.activeSelf)
        {
            PlayMain();
        }
        else
        {
            PlayBackGround();
        }
    }


    public void PlayMain()
    {
        if (!_init) return;
        if (manager.audioBGM != null)
        {
            manager.RemoveAudio(manager.audioBGM, true);
        }
        // 长音频在使用后需要销毁
        manager.audioBGM = manager.CreateAudio();
        manager.audioBGM.loop = true;
        manager.audioBGM.src = AudioManager.audioList[0];
        manager.audioBGM.OnCanplay(() =>
        {
            manager.audioBGM.Play();
        });
        // 自动播放停止
        manager.audioBGM.OnEnded(() =>
        {
            //manager.audioBGM = null;
            Debug.Log("循环播放");
        });
        // 手动停止
        manager.audioBGM.OnStop(() =>
        {
            manager.audioBGM = null;
        });
        Debug.Log("播放主界面音乐");
    }


    public void PlayStartGame()
    {
        if (!_init) return;
        var index = new System.Random().Next(1, 3);
        Debug.Log("Play:" + index);
        if (manager.audioBGM != null)
        {
            manager.RemoveAudio(manager.audioBGM, true);
        }
        // 长音频在使用后需要销毁
        manager.audioBGM = manager.CreateAudio();
        // audioBGM.loop = true;
        manager.audioBGM.src = AudioManager.audioList[index];
        manager.audioBGM.OnCanplay(() =>
        {
            manager.audioBGM.Play();
        });
        // 自动播放停止
        manager.audioBGM.OnEnded(() =>
        {
            PlayBackGroundMusic();
        });
        // 手动停止
        manager.audioBGM.OnStop(() =>
        {
            manager.audioBGM = null;
        });
    }

    public void PlayBackGroundMusic()
    {
        if (!_init) return;
        var index = new System.Random().Next(4, 8);
        Debug.Log("Play:" + index);
        if (manager.audioBGM != null)
        {
            manager.RemoveAudio(manager.audioBGM, true);
        }
        // 长音频在使用后需要销毁
        manager.audioBGM = manager.CreateAudio();
        // audioBGM.loop = true;
        manager.audioBGM.src = AudioManager.audioList[index];
        manager.audioBGM.OnCanplay(() =>
        {
            manager.audioBGM.Play();
        });
        // 自动播放停止
        manager.audioBGM.OnEnded(() =>
        {
            index = new System.Random().Next(4, 8);
            manager.audioBGM.src = AudioManager.audioList[index];
        });
        // 手动停止
        manager.audioBGM.OnStop(() =>
        {
            manager.audioBGM = null;
        });
    }

}
