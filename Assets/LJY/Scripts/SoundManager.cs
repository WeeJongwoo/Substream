using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private bool _buttonEffect = false;

    private Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
    private List<AudioSource> _audioSource = new List<AudioSource>();

    private void Start()
    {
        GameObject container = GameObject.Find("AudioSourceContainer").gameObject;
        if (_buttonEffect)
        { _audioSource.Add(container.transform.Find("MainContentButtons").GetComponent<AudioSource>()); }
    }

    public void SetHoverSoundEvent(Button button, AudioClip clip)
    {
        button.RegisterCallback<PointerEnterEvent>(PlayHoverSound);
        if (!sounds.ContainsKey(clip.name))
            sounds.Add(clip.name, clip);
    }

    public void SetSelectSoundEvent(Button button, AudioClip clip)
    {
        button.RegisterCallback<ClickEvent>(PlaySelectSound);
        if (!sounds.ContainsKey(clip.name))
            sounds.Add(clip.name, clip);
    }

    private void PlayHoverSound(PointerEnterEvent evt)
    {
        AudioSource audioSource = GetAudioSource("MainContentButtons");
        if (_audioSource != null)
        {
            audioSource.PlayOneShot(sounds["hover"]);
        }
    }

    private void PlaySelectSound(ClickEvent evt)
    {
        AudioSource audioSource = GetAudioSource("MainContentButtons");
        if (_audioSource != null)
        {
            audioSource.PlayOneShot(sounds["select"]);
        }
    }

    private AudioSource GetAudioSource(string sourceName)
    {
        return _audioSource.Find(src => src.gameObject.name == sourceName);
    }
}
