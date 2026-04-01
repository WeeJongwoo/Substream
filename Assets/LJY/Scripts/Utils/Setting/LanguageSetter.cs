using System;
using UnityEngine;
using UnityEngine.UIElements;
using Localization;

namespace Utils
{
    /// <summary>
    /// 언어 설정을 제어함
    /// </summary>
    public class LanguageSetter : MonoBehaviour
    {
        public static LanguageSetter Instance { get; private set; }

        [Header("UI 이름 설정")]
        [SerializeField] private string _languageDropdownName = "Dropdown_Language";

        private Label _lblLanguageTitle;
        private DropdownField _languageDropdown;
        private Label _lblDescription;

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
        /// <param name="root">언어 세팅 페이지 UI Root</param>
        public void ConnectSettingUI(VisualElement root)
        {
            _lblLanguageTitle = root.Q<Label>("Title");
            _languageDropdown = root.Q<DropdownField>(_languageDropdownName);
            _lblDescription = root.Q<Label>("Description");

            if (_languageDropdown == null) {
                Debug.LogWarning($"[{nameof(LanguageSetter)}] '{_languageDropdownName}'를 찾을 수 없습니다");
                return;
            }

            // Enum에 정의된 언어 목록을 가져와 드롭다운 선택지에 할당
            _languageDropdown.choices.Clear();
            foreach (LanguageType lang in Enum.GetValues(typeof(LanguageType))) {
                _languageDropdown.choices.Add(lang.ToString());
            }

            // 현재 시스템에 적용된 언어로 드롭다운 초기값 세팅
            _languageDropdown.value = LocalizationManager.CurrentLanguage.ToString();

            // UI 상에서 값 변경 시 이벤트 콜백 등록
            _languageDropdown.RegisterValueChangedCallback(evt => {
                if (!_languageDropdown.choices.Contains(evt.newValue)) return;

                if (Enum.TryParse(evt.newValue, out LanguageType newLanguage)) {
                    LocalizationManager.ChangeLanguage(newLanguage);
                }
                else {
                    Debug.LogError($"[LanguageSetter] 지원하지 않는 언어 타입입니다 : {evt.newValue}");
                }
            });
        }

        public void RefreshTranslation()
        {
            _lblLanguageTitle.text = LocalizationManager.GetText(UIKeys.Setting.LANGUAGE_TITLE);
            _languageDropdown.Q<Label>().text = LocalizationManager.GetText(UIKeys.Setting.LANGUAGE_DROPDOWN_TITLE);
            _lblDescription.text = LocalizationManager.GetText(UIKeys.Setting.LANGUAGE_DESCRIPTION);
        }
    }
}