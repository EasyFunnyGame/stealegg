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
            Debug.Log("音乐资源加载完毕");
        }
    }

    public void PlayBackGroundMusic()
    {
        var index = new System.Random().Next(5, 10);
        Debug.Log("Play:" + index);

        if (manager.audioBGM != null)
        {
            manager.RemoveAudio(manager.audioBGM, true);
        }
        // 长音频在使用后需要销毁
        manager.audioBGM = manager.CreateAudio();
        // audioBGM.loop = true;
        manager.audioBGM.src = audioList[index];
        manager.audioBGM.OnCanplay(() =>
        {
            manager.audioBGM.Play();
        });
        // 自动播放停止
        manager.audioBGM.OnEnded(() =>
        {
            manager.audioBGM = null;
        });
        // 手动停止
        manager.audioBGM.OnStop(() =>
        {
            manager.audioBGM = null;
        });
    }

    public void StopBackGroundMusic()
    {

    }

    public void PlayStartGameMusic()
    {

    }

    public void StopStartGameMusic()
    {

    }
}
