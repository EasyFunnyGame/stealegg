using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeChatWASM;

public class AudioManager : MonoBehaviour
{
    // cdn路径音频最多支持10个同时在线播放，先下载后的音频（needDownload）最多支持32个同时播放，先初始化10个
    //private static int DEFAULT_AUDIO_COUNT = 10;

    // 创建音频对象池，复用对象
    private static Queue<WXInnerAudioContext> audioPool = new Queue<WXInnerAudioContext>();

    public static string[] sfxList =
    {
         // 盗窃者-脚步声-左
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_normal_left_1.mp3",// 0
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_normal_left_2.mp3",// 1
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_normal_left_3.mp3",// 2
        // 盗窃者-脚步声-右
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_normal_right_1.mp3",// 3
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_normal_right_2.mp3",// 4
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_normal_right_3.mp3",// 5
        // 盗窃者-脚步声-台阶-上
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_stair_up_1.mp3",// 6
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_stair_up_2.mp3",// 7
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_stair_up_3.mp3",// 8
        // 盗窃者-脚步声-台阶-下
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_stair_down_1.mp3",// 9
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_stair_down_2.mp3",// 10
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_stair_down_3.mp3",// 11
        // 盗窃者-出口逃离的脚步声
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_step_exit.mp3",// 12
        // 盗窃者-无效移动
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_deny_1.mp3",// 13
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_deny_2.mp3",// 14
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_deny_3.mp3",// 15
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_deny_4.mp3",// 16
        // 盗窃者-吹口哨
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_whistle_1.mp3",// 17
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_whistle_2.mp3",// 18
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_whistle_3.mp3",// 19
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_whistle_4.mp3",// 20
        // 盗窃者-跳上  
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_climb_up.mp3",// 21
        // 盗窃者-跳下
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_climb_down.mp3",// 22
        // 盗窃者-拿到酒瓶
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/lure_bottle_get_1.mp3",// 23
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/lure_bottle_get_2.mp3",// 24
        // 盗窃者-投掷酒瓶的声音
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/lure_bottle_throw_1.mp3",// 25
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/lure_bottle_throw_2.mp3",// 26
        // 酒瓶破碎的声音
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_lure_break.mp3",// 27
        // 盗窃者-跳入下水道声音 
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/manhole_jump_in.mp3",// 28
        // 盗窃者-跳出下水道声音
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/manhole_jump_out.mp3",// 29
        // 盗窃者-位置锁定的声音
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/mystery_pluck_1.mp3",// 30
        // 声波引起的-位置锁定
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/mystery_pluck_2.mp3",// 31
        // 盗窃者-穿越破的铁丝网的声音
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/player_crossfence.mp3",// 32
        // 盗窃者-穿越剪开的铁丝网
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_player_grilling_cut_pick.mp3",// 33
        // 盗窃者-剪铁丝网
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_player_grilling_cut_01.mp3",// 34
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_player_grilling_cut_02.mp3",// 35
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_player_grilling_cut_shake_01.mp3",// 36
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_player_grilling_cut_shake_02.mp3",// 37
        // 盗窃者-藏进树里
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_player_hiding_in.mp3",// 38
        // 盗窃者-树里出来
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_player_hiding_out.mp3",// 39
        // 下水道 跳入点  打开+关闭
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_manhole_openstart.mp3",// 40 
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_manhole_opendest.mp3",// 41
        // 下水道 跳出点  打开+关闭
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_manhole_closestart.mp3",// 42
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_manhole_closedest.mp3",// 43
        // 所有农主-脚步声
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_footsteps_concrete_01.mp3",// 44
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_footsteps_concrete_02.mp3",// 45
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_footsteps_concrete_03.mp3",// 46
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_footsteps_concrete_04.mp3",// 47
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_footsteps_concrete_05.mp3",// 48
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_footsteps_concrete_06.mp3",// 49
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_footsteps_concrete_07.mp3",// 50
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_footsteps_concrete_08.mp3",// 51
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_footsteps_concrete_09.mp3",// 52
        // 所有农主-脚步音-下楼梯
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_move_stairs_down.mp3",// 53
        // 所有农主-脚步音-上楼梯
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_move_stairs_up.mp3",// 54
        // 女农主-警觉-！ 进入追击状态
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_alerted_01.mp3",// 55
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_alerted_02.mp3",// 56
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_alerted_03.mp3",// 57
        // 女农主-放弃-？追击结束  疑问
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_abandon_01.mp3",// 58
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_abandon_02.mp3",// 59
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_abandon_03.mp3",// 60
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_abandon_04.mp3",// 61
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_abandon_05.mp3",// 62
        // 女农主（拿望远镜）-转向的声音
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_change_direction_01.mp3",// 63
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_change_direction_02.mp3",// 64
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_change_direction_03.mp3",// 65
        // 女农主-逮捕到  呼喊声
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_arrest_01.mp3",// 66
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_arrest_02.mp3",// 67
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_female_arrest_03.mp3",// 68
        // 男农主--放弃-？追击结束  疑问
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_abandon_01.mp3",// 69
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_abandon_02.mp3",// 70
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_abandon_03.mp3",// 71
        // 男农主-警觉-！ 进入追击状态
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_alerted_01.mp3",// 72
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_alerted_02.mp3",// 73
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_alerted_03.mp3",// 74
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_alerted_04.mp3",// 75
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_alerted_05.mp3",// 76
        // 男农主-睡觉声音-Z  呼入
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_snore_inhale_01.mp3",// 77
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_snore_inhale_02.mp3",// 78
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_snore_inhale_03.mp3",// 79
        // 男农主-睡觉声音-Z  呼出
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_snore_exhale_01.mp3",// 80
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_snore_exhale_02.mp3",// 81
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_guard_snore_exhale_03.mp3",// 82
        // 男农主-逮捕到 呼喊声
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_arrest_01.mp3",// 83
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_arrest_02.mp3",// 84
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/guard_male_arrest_03.mp3",// 85 
        // ui-游戏logo出现的声音
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/ui_city_reveal.mp3",// 86
        // ui-点击喷漆图标
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_player_graph_start.mp3",// 87
        // ui-开始手动喷漆声音
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_player_graff_pov_loop.mp3",// 88
        // ui-点击声音
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/ui_click.mp3",// 89
        // ui-得到奖励的声音-体力
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/Music_Jingle_Bonus_v2.mp3",// 90
        // ui-成功逃脱音效
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/ui_success_1.mp3",// 91
        // ui-关卡成功完成
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/ui_success_2.mp3",// 92
        // ui-被追捕到音效
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/ui_fail_1.mp3",// 93
        // ui-关卡失败
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/ui_fail_2.mp3",// 94
        // ui-关卡结束-照相机
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/sfx_endlevel_photograph.mp3",// 95
        // ui-无效/错误点击
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/ui_click_wrong.mp3",// 96
    };

    // 当前场景需要预下载的音频列表
    public static string[] audioList = {
        // 背景音乐-主界面
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/background_main.mp3",// 0
        // 关卡音乐-纯音乐
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/background_1.mp3",// 1
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/background_2.mp3",// 2
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/background_3.mp3",// 3
        // 关卡音乐-节奏音乐
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/start_game_1.mp3",// 4
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/start_game_2.mp3",// 5 
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/start_game_3.mp3",// 6
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/start_game_4.mp3",// 7
        "https://cx-game.oss-cn-shanghai.aliyuncs.com/Assets/Audio/start_game_5.mp3",// 8
    };

    // 正在播放的音频对象列表
    public static List<WXInnerAudioContext> audioPlayArray = new List<WXInnerAudioContext>();

    // 背景音乐
    public static WXInnerAudioContext audioBGM = null;

    private bool isDestroyed = false;

    private int createdAudioCount = 0;

    public static bool loaded = false;

    // 初始化
    public void Start()
    {
        // // 创建音频对象池，创建时设置属性需要下载
        // for (var i = 0; i < DEFAULT_AUDIO_COUNT; i++)
        // {
        //     addAudio(false);
        // };

        // 批量下载音频文件
        if(!AudioManager.loaded)
        {
            downloadAudio();
        }
        
    }

    // 从缓存池中获取音频实例
    // 注意：发现一些偶现的bug，目前不推荐使用！！
    private WXInnerAudioContext getAudio()
    {
        if (this.isDestroyed)
        {
            return null;
        }

        if (audioPool.Count == 0)
        {
            addAudio(false);
        }

        var audio = audioPool.Dequeue();
        audio.needDownload = true;

        if (!audioPlayArray.Contains(audio))
        {
            audioPlayArray.Add(audio);
        }

        return audio;
    }

    // 从缓存池中获取音频实例
    public WXInnerAudioContext CreateAudio()
    {
        if (isDestroyed)
        {
            return null;
        }

        var audio = addAudio(true);
        audio.volume = AudioPlay.sfxVolume;
        if (!audioPlayArray.Contains(audio))
        {
            audioPlayArray.Add(audio);
        }

        return audio;
    }

    // 销毁或回收实例
    public void RemoveAudio(WXInnerAudioContext audio, bool needDestroy = true, bool forceDestroy = false)
    {
        audio.OffCanplay();
        if (audioPlayArray.Contains(audio))
        {
            audioPlayArray.Remove(audio);
        }
        if (needDestroy)
        {
            if (WXInnerAudioContext.Dict.ContainsValue(audio) || forceDestroy)
            {
                audio.Destroy();
                createdAudioCount -= 1;
            }
        }
        else
        {
            if (!audioPool.Contains(audio))
            {
                audioPool.Enqueue(audio);
            }
        }

        //Debug.Log("___________________");
        //Debug.Log("已创建InnerAudio:" + createdAudioCount + " 对象池:" + audioPool.Count + " 正在播放:" + audioPlayArray.Count);
        //Debug.Log("___________________");
    }

    // 创建InnerAudioContext实例
    // 参数needDestroy表示是否需要在播放完之后销毁，目前建议都先销毁再创建使用
    private WXInnerAudioContext addAudio(bool needDestroy = true)
    {
        if (createdAudioCount > 32)
        {
            //Debug.LogError("最多只支持同时使用32个InnerAudio");
            for(var index = 0; index < audioPlayArray.Count; index++)
            {
                WXInnerAudioContext playingAudio = audioPlayArray[index];
                if(!playingAudio.isPlaying)
                {
                    RemoveAudio(playingAudio, true, true);
                }
            }
        }

        var audio = WX.CreateInnerAudioContext(new InnerAudioContextParam() { needDownload = true });

        createdAudioCount += 1;

        // 自动播放停止
        audio.OnEnded(() =>
        {
            //Debug.Log(audio.instanceId + " OnEnded");
            RemoveAudio(audio, needDestroy);
        });

        // 加载出错
        audio.OnError(() =>
        {
            //Debug.Log(audio.instanceId + "audio OnError");
            audio.Stop();
            RemoveAudio(audio, needDestroy);
        });

        // 手动停止
        audio.OnStop(() =>
        {
            //Debug.Log(audio.instanceId + "audio OnStop");
            RemoveAudio(audio, needDestroy);
        });

        // 暂停
        audio.OnPause(() =>
        {
            //Debug.Log(audio.instanceId + "audio OnPause");
        });

        // 播放成功
        audio.OnPlay(() =>
        {
            //Debug.Log(audio.instanceId + "audio OnPlay");
        });

        if (!needDestroy)
        {
            audioPool.Enqueue(audio);
        }

        return audio;
    }

    // 预下载音频
    private void downloadAudio()
    {
        // 预下载音频
        WX.PreDownloadAudios(audioList, (int res) =>
        {
            if (res == 0)
            {
                AudioManager.loaded = true;
                // 下载成功
                //playAfterDownload(0,false);

                // 下载后播放第2个音频
                // playAfterDownload(1);
            }
            else
            {
                // 下载失败
            }
        });
    }

    public void Play(int index, bool isShort) {
        var audioIndex = CreateAudio();

        if (audioIndex == null)
        {
            return;
        }

        // 如果要设置的src和原音频对象一致，可以直接播放
        if (audioIndex.src == audioList[index])
        {
            audioIndex.Play();
        }
        else
        {
            // 对于已经设置了needDownload为true的audio，设置src后就会开始下载对应的音频文件
            // 如果该文件已经下载过，并且配置了缓存本地，就不会重复下载
            // 如果该文件没有下载过，等同于先调用WX.PreDownloadAudios下载后再播放
            audioIndex.src = audioList[index];

            // 短音频可以直接调用Play
            if (isShort)
            {
                audioIndex.Play();
            }
            else
            {
                // 长音频在可以播放时播放
                audioIndex.OnCanplay(() =>
                {
                    audioIndex.Play();
                });
            }
        }
    }


    // 播放音频
    public void playAfterDownload(int index, bool isShort)
    {
        var audioIndex = CreateAudio();

        if (audioIndex == null)
        {
            return;
        }

        // 如果要设置的src和原音频对象一致，可以直接播放
        if (audioIndex.src == audioList[index])
        {
            audioIndex.Play();
        }
        else
        {
            // 对于已经设置了needDownload为true的audio，设置src后就会开始下载对应的音频文件
            // 如果该文件已经下载过，并且配置了缓存本地，就不会重复下载
            // 如果该文件没有下载过，等同于先调用WX.PreDownloadAudios下载后再播放
            audioIndex.src = audioList[index];

            // 短音频可以直接调用Play
            if (isShort)
            {
                audioIndex.Play();
            }
            else
            {
                // 长音频在可以播放时播放
                audioIndex.OnCanplay(() =>
                {
                    audioIndex.Play();
                });
            }
        }
    }

    // 不缓存立即播放
    public void playRightNow(int index)
    {
        // 如果是需要在当前场景立刻播放的音频，则不设置needDownload，音频会边下边播
        // 但是再次使用该音频时会因为没有下载而需要再次下载，并不推荐这样使用
        var audioPlayRightNow = CreateAudio();

        audioPlayRightNow.needDownload = false;

        if (audioPlayRightNow == null)
        {
            return;
        }

        // 如果要设置的src和原音频对象一致，可以直接播放
        if (audioPlayRightNow.src == audioList[index])
        {
            audioPlayRightNow.Play();
        }
        else
        {
            // 如果当前音频已经下载过，并且配置了缓存本地，就算设置needDownload为false也不会重复下载
            audioPlayRightNow.src = audioList[index];

            // 在可以播放时播放
            audioPlayRightNow.OnCanplay(() =>
            {
                audioPlayRightNow.Play();
            });
        }
    }

    // 暂停所有在播放的音乐
    public void pauseAllAudio()
    {
        audioPlayArray.ForEach(audio =>
        {
            audio.Pause();
        });
    }

    // 重新播放所有在播放的音乐
    public void resumeAllAudio()
    {
        audioPlayArray.ForEach(audio =>
        {
            // innerAudio没有resume，直接用play重新播放
            audio.Play();
        });
    }

    // 停止所有在播放的音乐
    public void stopAllAudio()
    {
        if(audioPlayArray!=null && audioPlayArray.Count > 0)
        {
            audioPlayArray.ForEach(audio =>
            {
                audio.OffCanplay();
                audio.Stop();
            });
        }
    }

    // 播放短音频
    public void playShort()
    {
        var index = new System.Random().Next(0, 5);
        Debug.Log("playShort:" + index);

        playAfterDownload(index, true);
    }

    // 同时播放5个短音频（测试用）
    public void playShort5()
    {
        for (var i = 0; i < 5; i++)
        {
            this.playShort();
        };
    }

    // 播放背景音乐
    public void playBGM()
    {
        var index = new System.Random().Next(5, 10);
        Debug.Log("Play:" + index);

        if (audioBGM != null)
        {
            RemoveAudio(audioBGM, true);
        }
        // 长音频在使用后需要销毁
        audioBGM = CreateAudio();
        // audioBGM.loop = true;
        audioBGM.src = audioList[index];
        audioBGM.OnCanplay(() =>
        {
            audioBGM.Play();
        });
        // 自动播放停止
        audioBGM.OnEnded(() =>
        {
            audioBGM = null;
        });
        // 手动停止
        audioBGM.OnStop(() =>
        {
            audioBGM = null;
        });
    }

    // 销毁场景
    private void OnDestroy()
    {
        this.isDestroyed = true;
        this.stopAllAudio();
    }
}