using UnityEngine;
using System;

namespace Audio.Data
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Audio/AudioSettings")]
    public class AudioSettingsSO : ScriptableObject
    {
        // -- Audio Mute --
        public bool isMasterOn = true;
        public bool isBgmOn = true;
        public bool isSfxOn = true;
        public bool isVoOn = true;

        // -- Audio Volumes --
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float bgmVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        [Range(0f, 1f)] public float voVolume = 1f;

        /// <summary>
        /// 오디오 설정이 변경되었을 시 발생될 이벤트 변수
        /// </summary>
        public event Action OnSettingsChanged;

        public void ApplyChanges()
        {
            OnSettingsChanged?.Invoke();
        }
    }
}