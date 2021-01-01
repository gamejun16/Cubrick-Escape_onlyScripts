using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    Button1, Button2, Button3, 
}

public class UIAudioManager : MonoBehaviour
{
    static public UIAudioManager instance;

    public AudioSource audioSource;
    [SerializeField] AudioMixer audioMixer;

    public AudioClip[] sources;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        float sound;
        DataContainer.instance.GetSettingInfo(out sound);

        audioMixer.SetFloat("EFF", sound);
    }

    /// <summary>
    /// 믹서 볼륨 조절
    /// </summary>
    /// <param name="mixerName">타겟 믹서 명</param>
    /// <param name="value">데시벨 값</param>
    public void SetMixerVolume(string mixerName, float value)
    {
        // mute
        if (value == -20)
            value = -80;


        audioMixer.SetFloat(mixerName, value);
    }

    public void AudioPlay(int idx)
    {
        audioSource.clip = sources[idx];
        audioSource.Play();
    }

}
