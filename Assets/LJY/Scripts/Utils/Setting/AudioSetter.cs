using Audio.Data;
using Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils
{
    /// <summary>
    /// 오디오 설정을 제어함
    /// </summary>
    public class AudioSetter : MonoBehaviour
    {
        public static AudioSetter Instance { get; private set; }

        [Header("세팅 데이터")]
        [SerializeField] private AudioSettingsSO _audioSettings;

        [Header("뮤트 Toggles 이름")]
        [SerializeField] private string _marsterTgl = "Tgl_Master";
        [SerializeField] private string _bgmTgl = "Tgl_BGM";
        [SerializeField] private string _sfxTgl = "Tgl_SFX";
        [SerializeField] private string _voTgl = "Tgl_VO";

        [Header("볼륨 Sliders 이름")]
        [SerializeField] private string _masterSld = "Sld_Master";
        [SerializeField] private string _bgmSld = "Sld_BGM";
        [SerializeField] private string _sfxSld = "Sld_SFX";
        [SerializeField] private string _voSld = "Sld_VO";

        // -- Labels --
        private Label _lblAudioTitle;

        // -- Toggles --
        private Toggle _tglMaster;
        private Toggle _tglBGM;
        private Toggle _tglSFX;
        private Toggle _tglVO;

        // -- Sliders --
        private Slider _sldMaster;
        private Slider _sldBGM;
        private Slider _sldSFX;
        private Slider _sldVO;

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 세팅 패널과 연결
        /// </summary>
        /// <param name="root">세팅 패널 UI Root</param>
        public void ConnectSettingUI(VisualElement root)
        {
            if (_audioSettings == null) {
                Debug.LogWarning("AudioSettingsSO가 할당되지 않았습니다");
                return;
            }

            _lblAudioTitle = root.Q<Label>("Title");

            _tglMaster = root.Q<Toggle>(_marsterTgl);
            _tglBGM = root.Q<Toggle>(_bgmTgl);
            _tglSFX = root.Q<Toggle>(_sfxTgl);
            _tglVO = root.Q<Toggle>(_voTgl);

            _sldMaster = root.Q<Slider>(_masterSld);
            _sldBGM = root.Q<Slider>(_bgmSld);
            _sldSFX = root.Q<Slider>(_sfxSld);
            _sldVO = root.Q<Slider>(_voSld);

            // 초기값 덮어씌우기 (SO 데이터 >> UI 반영)
            if (_tglMaster != null) _tglMaster.value = _audioSettings.isMasterOn;
            if (_tglBGM != null) _tglBGM.value = _audioSettings.isBgmOn;
            if (_tglSFX != null) _tglSFX.value = _audioSettings.isSfxOn;
            if (_tglVO != null) _tglVO.value = _audioSettings.isVoOn;

            if (_sldMaster != null) _sldMaster.value = _audioSettings.masterVolume;
            if (_sldBGM != null) _sldBGM.value = _audioSettings.bgmVolume;
            if (_sldSFX != null) _sldSFX.value = _audioSettings.sfxVolume;
            if (_sldVO != null) _sldVO.value = _audioSettings.voVolume;

            // 토글 이벤트 등록 (UI 변경 >> SO 반영)
            _tglMaster?.RegisterValueChangedCallback(evt => {
                _audioSettings.isMasterOn = evt.newValue;
                _audioSettings.ApplyChanges();
            });
            _tglBGM?.RegisterValueChangedCallback(evt => {
                _audioSettings.isBgmOn = evt.newValue;
                _audioSettings.ApplyChanges();
            });
            _tglSFX?.RegisterValueChangedCallback(evt => {
                _audioSettings.isSfxOn = evt.newValue;
                _audioSettings.ApplyChanges();
            });
            _tglVO?.RegisterValueChangedCallback(evt => {
                _audioSettings.isVoOn = evt.newValue;
                _audioSettings.ApplyChanges();
            });

            // 슬라이더 이벤트 등록 (UI 변경 >> SO 반영)
            _sldMaster?.RegisterValueChangedCallback(evt => {
                _audioSettings.masterVolume = evt.newValue;
                _audioSettings.ApplyChanges();
            });
            _sldBGM?.RegisterValueChangedCallback(evt => {
                _audioSettings.bgmVolume = evt.newValue;
                _audioSettings.ApplyChanges();
            });
            _sldSFX?.RegisterValueChangedCallback(evt => {
                _audioSettings.sfxVolume = evt.newValue;
                _audioSettings.ApplyChanges();
            });
            _sldVO?.RegisterValueChangedCallback(evt => {
                _audioSettings.voVolume = evt.newValue;
                _audioSettings.ApplyChanges();
            });
        }

        public void RefreshTranslation()
        {
            _lblAudioTitle.text = LocalizationManager.GetText(UIKeys.Setting.AUDIO_TITLE);
            _tglMaster.label = LocalizationManager.GetText(UIKeys.Setting.AUDIO_MASTER);
            _tglBGM.label = LocalizationManager.GetText(UIKeys.Setting.AUDIO_BGM);
            _tglSFX.label = LocalizationManager.GetText(UIKeys.Setting.AUDIO_SFX);
            _tglVO.label = LocalizationManager.GetText(UIKeys.Setting.AUDIO_VO);
        }
    }
}