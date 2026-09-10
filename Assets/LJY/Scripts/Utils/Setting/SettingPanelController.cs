using Localization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils
{
    public class SettingPanelController : MonoBehaviour
    {
        public static SettingPanelController Instance { get; private set; }

        [Header("UXML References")]
        [SerializeField] private VisualTreeAsset _audioPageUXML;
        [SerializeField] private VisualTreeAsset _languagePageUXML;

        private VisualElement _settingRoot;

        private Label _lblSettingTitle;

        private Button _btnAudioTab;
        private Button _btnLanguageTab;

        private VisualElement _pagesContainer;
        private Button _btnClose;

        // -- 런타임 변수 --
        private Dictionary<Button, VisualElement> _tabPages = new Dictionary<Button, VisualElement>();
        private Button _currentActiveTab;

        // -- 스타일 상수 (선택된 탭과 아닌 탭의 배경색) --
        private readonly StyleColor COLOR_TAB_ACTIVE = new StyleColor(new Color32(80, 80, 80, 255));
        private readonly StyleColor COLOR_TAB_INACTIVE = new StyleColor(Color.clear);
        private readonly StyleColor COLOR_TEXT_ACTIVE = new StyleColor(Color.white);
        private readonly StyleColor COLOR_TEXT_INACTIVE = new StyleColor(new Color32(200, 200, 200, 255));

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
                return;
            }
            else {
                Destroy(gameObject);
            }

        }

        /// <summary>
        /// 생성한 세팅 루트 UI를 연결받아 내부 요소들을 초기화
        /// </summary>
        public void ConnectSettingUI(VisualElement root)
        {
            _settingRoot = root;

            _lblSettingTitle = _settingRoot.Q<Label>("Title");
            _btnAudioTab = _settingRoot.Q<Button>("Tab_Audio");
            _btnLanguageTab = _settingRoot.Q<Button>("Tab_Language");

            // 공통 닫기 버튼
            _btnClose = _settingRoot.Q<Button>("Btn_CloseSetting");
            if (_btnClose != null) {
                _btnClose.clicked -= ClosePanel;
                _btnClose.clicked += ClosePanel;
            }

            // 페이지 컨테이너
            _pagesContainer = _settingRoot.Q<VisualElement>("Pages_Container");
            if (_pagesContainer == null) {
                Debug.LogError("Pages_Container를 찾을 수 없습니다.");
                return;
            }

            // 페이지 주입 및 탭 초기화
            InitializeTabsAndPages();
        }

        /// <summary>
        /// 각 설정창 생성/초기화 및 Setter 연결
        /// </summary>
        private void InitializeTabsAndPages()
        {
            _tabPages.Clear();

            // 오디오 페이지 설정 및 AudioSetter 연결
            if (_audioPageUXML != null) {
                VisualElement audioPage = _audioPageUXML.Instantiate().Q("Page_Audio");
                _pagesContainer.Add(audioPage);

                RegisterTab(_btnAudioTab, audioPage);

                if (AudioSetter.Instance != null) {
                    AudioSetter.Instance.ConnectSettingUI(audioPage);
                }
            }

            // 언어 페이지 설정 및 LanguageSetter 연결
            if (_languagePageUXML != null) {
                VisualElement languagePage = _languagePageUXML.Instantiate().Q("Page_Language");
                _pagesContainer.Add(languagePage);

                RegisterTab(_btnLanguageTab, languagePage);

                if (LanguageSetter.Instance != null) {
                    LanguageSetter.Instance.ConnectSettingUI(languagePage);
                }
            }

            // 오디오 탭을 기본으로 설정
            if (_settingRoot.Q<Button>("Tab_Audio") != null) {
                SelectTab(_settingRoot.Q<Button>("Tab_Audio"));
            }
        }

        /// <summary>
        /// 책갈피 버튼에 세팅 관련 버튼 이벤트 등록 및 UI 저장
        /// </summary>
        /// <param name="tabBtn">각 UI 활성화 버튼</param>
        /// <param name="page">활성화 될 탭 UI</param>
        private void RegisterTab(Button tabBtn, VisualElement page)
        {
            if (tabBtn == null || page == null) return;

            // 클릭 이벤트 등록
            tabBtn.clicked += () => SelectTab(tabBtn);
            _tabPages.Add(tabBtn, page);
        }

        /// <summary>
        /// 특정 설정창을 화면에 표시함
        /// </summary>
        /// <param name="selectedTab">선택한 설정창 열람 버튼</param>
        private void SelectTab(Button selectedTab)
        {
            if (selectedTab == _currentActiveTab) return;

            foreach (var kvp in _tabPages) {
                Button btn = kvp.Key;
                VisualElement page = kvp.Value;

                if (btn == selectedTab) {
                    // 활성화 처리
                    page.style.display = DisplayStyle.Flex;
                    btn.style.backgroundColor = COLOR_TAB_ACTIVE;
                    btn.style.color = COLOR_TEXT_ACTIVE;
                    _currentActiveTab = selectedTab;
                }
                else {
                    // 비활성화 처리
                    page.style.display = DisplayStyle.None;
                    btn.style.backgroundColor = COLOR_TAB_INACTIVE;
                    btn.style.color = COLOR_TEXT_INACTIVE;
                }
            }
        }

        public void RefreshTranslation()
        {
            if (_settingRoot != null) {
                _lblSettingTitle.text = LocalizationManager.GetText(UIKeys.Setting.TITLE);
                _btnAudioTab.text = LocalizationManager.GetText(UIKeys.Setting.BTN_AUDIO);
                _btnLanguageTab.text = LocalizationManager.GetText(UIKeys.Setting.BTN_LANGUAGE);
                _btnClose.text = LocalizationManager.GetText(UIKeys.Common.BTN_CLOSE);
            }
            AudioSetter.Instance.RefreshTranslation();
            LanguageSetter.Instance.RefreshTranslation();
        }


        /// <summary>
        /// 세팅 패널 열기
        /// </summary>
        public void OpenPanel() => _settingRoot?.ShowPopupFade();
        /// <summary>
        /// 세팅 패널 닫기
        /// </summary>
        public void ClosePanel() => _settingRoot?.HidePopupFade();
    }
}