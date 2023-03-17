using UnityEngine.UI;

public class SettingCanvas : BaseCanvas
{
    public Button btn_clz;

    public Slider sld_bgm_volume;

    public Slider sld_sfx_volume;

    private void Awake()
    {
        btn_clz.onClick.AddListener(() => { this.Hide(); AudioPlay.Instance.PlayClick(); });
        sld_bgm_volume.onValueChanged.AddListener(onChangeBgmVolumeHandler);
        sld_sfx_volume.onValueChanged.AddListener(onChangeSfxVolumeHandler);
    }

    void onChangeBgmVolumeHandler(float volume)
    {
        PlayerPrefs.SetFloat(UserDataKey.MusicVolume, volume);
        PlayerPrefs.Save();
        AudioPlay.bgmVolume = PlayerPrefs.GetFloat(UserDataKey.MusicVolume, 1f);
        AudioPlay.MusicVolume();
    }

    void onChangeSfxVolumeHandler(float volume)
    {
        PlayerPrefs.SetFloat(UserDataKey.SfxVolume, volume);
        PlayerPrefs.Save();
        AudioPlay.sfxVolume = PlayerPrefs.GetFloat(UserDataKey.SfxVolume, 1f);
        AudioPlay.SoundVolume();
    }

    protected override void OnShow()
    {
        AudioPlay.bgmVolume = PlayerPrefs.GetFloat(UserDataKey.MusicVolume, 1f);
        AudioPlay.sfxVolume = PlayerPrefs.GetFloat(UserDataKey.SfxVolume, 1f);
        sld_bgm_volume.value = AudioPlay.bgmVolume;
        sld_sfx_volume.value = AudioPlay.sfxVolume;
    }

    protected override void OnHide()
    {

    }
}
