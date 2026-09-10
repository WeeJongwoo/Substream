using UnityEngine;
using Audio.Data;

namespace Audio.Controller
{
    /// <summary>
    /// 받은 오디오 소스를 재생함
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        public static AudioController Instance { get; private set; }

        [Header("오디오 데이터")]
        [SerializeField] private AudioDatabaseSO _audioDB;
        [SerializeField] private AudioSettingsSO _audioSettings;

        [Header("오디오 소스")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _voSource;

        private float _curBgmBaseVolume = 1f;

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                Destroy(this);
            }

            if (_audioDB == null || _audioSettings == null || _bgmSource == null || _sfxSource == null || _voSource == null) {
                Debug.Log($"[ Audio Null List ]\n" +
                    $"DB : {_audioDB.name}\n" +
                    $"Setting : {_audioSettings.name}\n" +
                    $"BGM : {_bgmSource.name}\n" +
                    $"SFX : {_sfxSource.name}\n" +
                    $"VO : {_voSource.name}\n");
            }

            _audioDB.Initialize();
        }

        private void OnEnable()
        {
            if (_audioSettings != null)
                _audioSettings.OnSettingsChanged += UpdateAudioSettings;
        }

        private void OnDisable()
        {
            if (_audioSettings != null)
                _audioSettings.OnSettingsChanged -= UpdateAudioSettings;
        }

        private void Start()
        {
            // 초기 세팅 적용
            UpdateAudioSettings();
        }

        /// <summary>
        /// 설정값 실제 반영
        /// </summary>
        private void UpdateAudioSettings()
        {
            if (_audioSettings == null) return;

            bool isMasterMuted = !_audioSettings.isMasterOn;
            float masterVol = _audioSettings.masterVolume;

            if (_bgmSource != null) {
                _bgmSource.mute = isMasterMuted || !_audioSettings.isBgmOn;
                _bgmSource.volume = masterVol * _audioSettings.bgmVolume * _curBgmBaseVolume;
            }
            if (_sfxSource != null) {
                _sfxSource.mute = isMasterMuted || !_audioSettings.isSfxOn;
                _sfxSource.volume = masterVol * _audioSettings.sfxVolume;
            }
            if (_voSource != null) {
                _voSource.mute = isMasterMuted || !_audioSettings.isVoOn;
                _voSource.volume = masterVol * _audioSettings.voVolume;
            }
        }

        /// <summary>
        /// Playing BGM audio (루프 재생)
        /// </summary>
        /// <param name="id"></param>
        public void PlayBGM(string id)
        {
            if (_bgmSource == null || _audioDB == null) return;

            if (_audioDB.TryGetAudioData(id, out var data)) {
                _curBgmBaseVolume = data.volume;
                _bgmSource.clip = data.clip;
                _bgmSource.loop = true;
                _bgmSource.Play();

                UpdateAudioSettings();
            }
        }

        /// <summary>
        /// Playing SFX audio (중첩 재생 가능)
        /// </summary>
        /// <param name="id"></param>
        public void PlaySFX(string id)
        {
            if (_sfxSource == null || _audioDB == null) return;

            if (_audioDB.TryGetAudioData(id, out var data)) {
                _sfxSource.PlayOneShot(data.clip, data.volume);
            }
        }

        /// <summary>
        /// Playing Voice Over audio (중첩 재생 가능)
        /// </summary>
        /// <param name="id">호출하는 스크립트에서 제공함</param>
        public void PlayVO(string id)
        {
            if (_voSource == null || _audioDB == null) return;

            if (_audioDB.TryGetAudioData(id, out var data)) {
                _voSource.PlayOneShot(data.clip, data.volume);
            }
        }
    }
}