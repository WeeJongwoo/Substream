using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 공통으로 사용되는 사운드 트랙의 열거집합
/// <para>>> int 형으로 캐스팅 필요</para>
/// <para>>> 사운드 트랙 추가가 필요할 경우 SoundManager에서 수정 가능</para>
/// </summary>
public enum SoundTrack
{
    b_hover = 0,
    b_clicked = 1,
    backGroundSound = 2,
}

public class SoundManager : MonoBehaviour
{
    [SerializeField] private bool _buttonEffect = false;

    private Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
    private List<AudioSource> _audioSource = new List<AudioSource>();

    private void Start()
    {
        GameObject container = GameObject.Find("AudioSourceContainer").gameObject;
        if (_buttonEffect) { 
            _audioSource.Add(container.transform.Find("MainContentButtons").GetComponent<AudioSource>()); 
        }
    }

    /// <summary>
    /// UI Element에서 제공하는 버튼의 사운드 이벤트 연결 함수
    /// </summary>
    /// <typeparam name="T">>> UI Element에서 제공하는 이벤트 클래스</typeparam>
    /// <param name="button">>> UI Element에서 제공하는 버튼 오브젝트</param>
    /// <param name="clip">>> 오디오 클립</param>
    public void SetButtonSoundEvent<T>(Button button, AudioClip clip) where T : EventBase
    {
        if (typeof(T) == typeof(PointerEnterEvent)) {
            button.RegisterCallback<PointerEnterEvent>(PlayHoverSound);
            if (!sounds.ContainsKey(clip.name))
                sounds.Add(clip.name, clip);
        }
        else if (typeof(T) == typeof(ClickEvent)) {
            button.RegisterCallback<ClickEvent>(PlaySelectSound);
            if (!sounds.ContainsKey(clip.name))
                sounds.Add(clip.name, clip);
        }
    }

    private void PlayHoverSound(PointerEnterEvent evt)
    {
        AudioSource audioSource = GetAudioSource("MainContentButtons");
        if (_audioSource != null) {
            audioSource.PlayOneShot(sounds["hover"]);
        }
    }

    private void PlaySelectSound(ClickEvent evt)
    {
        AudioSource audioSource = GetAudioSource("MainContentButtons");
        if (_audioSource != null) {
            audioSource.PlayOneShot(sounds["select"]);
        }
    }

    private AudioSource GetAudioSource(string sourceName)
    {
        return _audioSource.Find(src => src.gameObject.name == sourceName);
    }
}
