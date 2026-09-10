using Localization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils
{
    public class VisualNovelController : MonoBehaviour
    {
        [Header("Typing Settings")]
        [Tooltip("글자가 하나씩 출력되는 간격(초)을 설정. 숫자가 작을수록 글자가 빨리 나타남")]
        [SerializeField] private float _typingSpeed = 0.05f;

        [Header("Camera Settings")]
        [Tooltip("캐릭터가 포커싱될 때 화면 전체가 확대되는 배율")]
        [SerializeField] private float _zoomScale = 1.3f;
        [Tooltip("카메라가 다른 캐릭터로 이동하거나 줌인/줌아웃될 때 걸리는 애니메이션 시간(초)")]
        [SerializeField] private float _cameraMoveSpeed = 0.5f;

        [Header("Portrait Settings")]
        [Tooltip("화면에 나타나는 캐릭터 스탠딩 일러스트의 기본 크기 배율")]
        [SerializeField] private float _portraitDefaultScale = 0.6f;
        [Tooltip("말하는 캐릭터가 바뀔 때, 스탠딩 일러스트의 명암(밝기)이 변하는 데 걸리는 시간(초)")]
        [SerializeField] private float _portraitColorTransitionTime = 0.3f;
        [Tooltip("현재 말하고 있는(포커싱된) 캐릭터에게 씌워질 색상 필터 (기본: 흰색)")]
        [SerializeField] private Color _focusColor = Color.white;
        [Tooltip("현재 말하지 않는(포커싱 해제된) 캐릭터에게 씌워질 색상 필터 (기본: 어두운 회색)")]
        [SerializeField] private Color _unfocusColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        [Header("Auto & UI Settings")]
        [Tooltip("오토 모드에서 한 줄의 대사가 끝까지 타이핑된 후, 자동으로 다음 대사로 넘어가기 전까지 대기하는 시간(초)")]
        [SerializeField] private float _autoReadWaitTime = 1.0f;
        [Tooltip("오토 모드가 켜져 있을 때 화면의 Auto 버튼에 적용될 배경 색상")]
        [SerializeField] private Color _btnAutoOnColor = new Color(0.5f, 0.8f, 0.5f, 0.8f);
        [Tooltip("오토 모드가 꺼져 있을 때 화면의 Auto 버튼에 적용될 기본 배경 색상")]
        [SerializeField] private Color _btnAutoOffColor = new Color(0.23f, 0.23f, 0.23f, 0.8f);

        [Header("Visual Tree Asset")]
        [SerializeField] private VisualTreeAsset _settingPanelUXML;
        [SerializeField] private VisualTreeAsset _logPanelUXML;
        [SerializeField] private VisualTreeAsset _logTextPanelUXML;

        private UIDocument _uiDocument;
        private VisualElement _rootContainer;
        private Label _nameLabel;
        private Label _textLabel;

        private VisualElement _characterContainer;
        private List<VisualElement> _portraitSlots = new List<VisualElement>();
        private VisualElement _touchArea;
        private Button _settingBtn;

        private Button _autoBtn;
        private Button _skipBtn;
        private Button _logBtn;

        private VisualElement _logPanelRoot;
        private ScrollView _logScrollView;
        private Button _closeLogBtn;

        // 다국어 갱신을 위한 Key 캐싱
        private string _currentNameKey;
        private string _currentTextKey;

        // 타이핑 연출 관련 상태
        private Coroutine _typingCoroutine;
        private bool _isTyping = false;
        private string _targetLocalizedText = "";

        public bool IsReadAuto = false;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            var root = _uiDocument.rootVisualElement;

            _rootContainer = root.Q<VisualElement>("RootContainer");
            _nameLabel = root.Q<Label>("Lbl_Name");
            _textLabel = root.Q<Label>("Lbl_Text");
            _touchArea = root.Q<VisualElement>("TouchArea");
            _characterContainer = root.Q<VisualElement>("CharacterContainer");
            _settingBtn = root.Q<Button>("Btn_Setting");

            _autoBtn = root.Q<Button>("Btn_Auto");
            _skipBtn = root.Q<Button>("Btn_Skip");
            _logBtn = root.Q<Button>("Btn_Log");
        }

        private void Start()
        {
            InitializeSettingPanel();
            InitializeLogPanel();

            SetupCameraTransition();

            _touchArea.RegisterCallback<ClickEvent>(OnScreenClicked);
            LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
            _settingBtn.clicked += OnSettingBtnClicked;
            _autoBtn.clicked += OnAutoRead;
            _skipBtn.clicked += OnSkipRead;
            _logBtn.clicked += OnOpenLog;
            _closeLogBtn.clicked += OnCloseLog;

            HideUI();
        }

        private void OnDisable()
        {
            _touchArea?.UnregisterCallback<ClickEvent>(OnScreenClicked);

            LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
            _settingBtn.clicked -=OnSettingBtnClicked;
            _autoBtn.clicked -= OnAutoRead;
            _skipBtn.clicked -= OnSkipRead;
            _logBtn.clicked -= OnOpenLog;
            if (_closeLogBtn != null) _closeLogBtn.clicked -= OnCloseLog;
        }

        private void InitializeSettingPanel()
        {
            if (_settingPanelUXML != null) {
                VisualElement settingRoot = _settingPanelUXML.Instantiate();
                settingRoot.SetAbsolutePosition();
                settingRoot.style.display = DisplayStyle.None;
                settingRoot.AddToClassList("popup-fade-base");
                _rootContainer.Add(settingRoot);

                settingRoot.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());

                if (SettingPanelController.Instance != null) {
                    SettingPanelController.Instance.ConnectSettingUI(settingRoot);
                }
            }
        }

        private void InitializeLogPanel()
        {
            if (_logPanelUXML == null) return;

            // Log UI 생성 및 화면 덮기
            _logPanelRoot = _logPanelUXML.Instantiate();
            _logPanelRoot.SetAbsolutePosition();
            _logPanelRoot.style.display = DisplayStyle.None;
            _rootContainer.Add(_logPanelRoot);

            _logScrollView = _logPanelRoot.Q<ScrollView>("LogScrollView");
            _closeLogBtn = _logPanelRoot.Q<Button>("Btn_CloseLog");
            _closeLogBtn.clicked += OnCloseLog;

            // 로그창 밖을 클릭해도 안 꺼지게 / 클릭 이벤트가 게임 화면으로 넘어가지 않게 방지
            _logPanelRoot.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
        }

        private void SetupCameraTransition()
        {
            _characterContainer.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(_cameraMoveSpeed, TimeUnit.Second) });
            _characterContainer.style.transitionProperty = new StyleList<StylePropertyName>(new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("scale") });
            _characterContainer.style.transitionTimingFunction = new StyleList<EasingFunction>(new List<EasingFunction> { new EasingFunction(EasingMode.EaseOut) });
        }

        public void ShowUI()
        {
            _rootContainer.style.display = DisplayStyle.Flex;
        }

        public void HideUI()
        {
            _rootContainer.style.display = DisplayStyle.None;
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _isTyping = false;
        }

        /// <summary>
        /// VisualNovelManager에서 호출하여 대사 갱신 및 타이핑 연출 시작
        /// </summary>
        public void UpdateLine(string nameKey, string textKey, List<Sprite> sprites, List<int> focusIndices)
        {
            _currentNameKey = nameKey;
            _currentTextKey = textKey;
            _nameLabel.text = LocalizationManager.GetText(_currentNameKey);

            int characterCount = sprites.Count;
            while (_portraitSlots.Count < characterCount) {
                VisualElement newSlot = new VisualElement();
                newSlot.name = $"Portrait_{_portraitSlots.Count}";
                newSlot.style.height = Length.Percent(100);
                newSlot.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Contain));
                newSlot.style.backgroundPositionX = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Center));
                newSlot.style.backgroundPositionY = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Bottom));

                newSlot.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(_portraitColorTransitionTime, TimeUnit.Second) });
                newSlot.style.transitionProperty = new StyleList<StylePropertyName>(new List<StylePropertyName> { new StylePropertyName("unity-background-image-tint-color") });
                
                _characterContainer.Add(newSlot);
                _portraitSlots.Add(newSlot);
            }

            for (int i = 0; i < _portraitSlots.Count; i++) {
                if (i < characterCount) {
                    _portraitSlots[i].style.display = DisplayStyle.Flex;
                    _portraitSlots[i].style.width = Length.Percent(100f / characterCount);

                    _portraitSlots[i].SetImage(sprites[i], 0, 0, _portraitDefaultScale);

                    if (focusIndices.Contains(i) || (focusIndices.Count > 0 && focusIndices[0] == -1)) {
                        _portraitSlots[i].style.unityBackgroundImageTintColor = _focusColor;
                        continue;
                    }

                    _portraitSlots[i].style.unityBackgroundImageTintColor = _unfocusColor;
                    continue;
                }

                _portraitSlots[i].style.display = DisplayStyle.None;
            }

            MoveCameraTo(focusIndices, characterCount);

            _targetLocalizedText = LocalizationManager.GetText(_currentTextKey);

            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypewriterRoutine(_targetLocalizedText));
        }

        private void MoveCameraTo(List<int> focusIndices, int characterCount)
        {
            if (focusIndices == null || focusIndices.Count <= 0) {
                Debug.LogError($"[VisualNovelController] 비정상적인 리스트가 검출되었습니다.");
                return;
            }

            // 포커스가 없거나, 인덱스를 초과했거나, 등장인물이 없는 경우 원위치
            if (focusIndices[0] == -1 || focusIndices.Count >= characterCount || characterCount == 0) {
                _characterContainer.style.translate = new StyleTranslate(new Translate(Length.Percent(0), Length.Percent(0), 0));
                _characterContainer.style.scale = new StyleScale(new Scale(new Vector3(1f, 1f, 1f)));
                return;
            }
            // 화면 등분 비율
            float slotWidthPercent = 100f / characterCount;

            float targetCenter = 0;
            foreach (int i in focusIndices) {
                targetCenter += (i * slotWidthPercent) + (slotWidthPercent / 2f); // 타겟의 중심점 위치 = (내 인덱스 * 너비) + (너비 / 2)
            }

            targetCenter /= focusIndices.Count;

            // 화면의 정중앙에서 타겟 중심점을 뺀 만큼 컨테이너를 이동
            float translateX = 50f - targetCenter;
            _characterContainer.style.translate = new StyleTranslate(new Translate(Length.Percent(translateX), Length.Percent(0), 0));
            _characterContainer.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(Length.Percent(targetCenter), Length.Percent(100)));
            _characterContainer.style.scale = new StyleScale(new Scale(new Vector3(_zoomScale, _zoomScale, 1f)));
        }

        /// <summary>
        /// 텍스트를 한 글자씩 출력
        /// </summary>
        private IEnumerator TypewriterRoutine(string text)
        {
            _isTyping = true;
            _textLabel.text = ""; // 텍스트 초기화

            foreach (char c in text) {
                _textLabel.text += c;
                yield return new WaitForSeconds(_typingSpeed);
            }

            _isTyping = false;
        }

        /// <summary>
        /// 화면을 클릭했을 때의 처리 로직
        /// </summary>
        private void OnScreenClicked(ClickEvent evt)
        {
            if (IsReadAuto) {
                OffAutoRead();
            }

            // 스킵 기능
            if (_isTyping) {
                if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

                _textLabel.text = _targetLocalizedText;
                _isTyping = false;
            }
            else { 
                VisualNovelManager.Instance.NextLine(); // 타이핑이 끝났다면 매니저에게 다음 대사를 요청
            }
        }

        private void OnSkipRead()
        {
            VisualNovelManager.Instance.EndEpisode();
        }

        private void OnAutoRead()
        {
            IsReadAuto = !IsReadAuto;

            if (IsReadAuto) {
                _autoBtn.style.backgroundColor = new StyleColor(_btnAutoOnColor);
                StartCoroutine(AutoReading());
            }
            else {
                OffAutoRead();
            }
        }

        public void OffAutoRead()
        {
            IsReadAuto = false;
            _autoBtn.style.backgroundColor = new StyleColor(_btnAutoOffColor);
        }

        private IEnumerator AutoReading()
        {
            while (IsReadAuto) {
                if (_isTyping) {
                    yield return null;
                    continue;
                }

                yield return new WaitForSeconds(_autoReadWaitTime);

                if (IsReadAuto) {
                    VisualNovelManager.Instance.NextLine();
                }
            }
        }

        private void OnOpenLog()
        {
            OffAutoRead();
            _logScrollView.Clear();

            // 0번부터 현재 인덱스 이전까지의 대사 불러오기
            int curIdx = VisualNovelManager.Instance.CurIdx;

            for (int i = 0; i <= curIdx; i++) {
                VNLineData data = VisualNovelManager.Instance.GetLineData(i);
                if (data == null) continue;

                VisualElement logEntry = _logTextPanelUXML.Instantiate();

                Label nameLbl = logEntry.Q<Label>("Log_Name");
                Label textLbl = logEntry.Q<Label>("Log_Text");

                nameLbl.text = LocalizationManager.GetText(data.SpeakerNameKey);
                textLbl.text = LocalizationManager.GetText(data.TextKey);

                _logScrollView.Add(logEntry);
            }

            _logPanelRoot.style.display = DisplayStyle.Flex;

            _logScrollView.schedule.Execute(() => _logScrollView.scrollOffset = new Vector2(0, _logScrollView.contentContainer.layout.height)).StartingIn(10);
        }

        private void OnCloseLog()
        {
            _logPanelRoot.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 인게임에서 언어가 변경되었을 때 즉시 호출됨
        /// </summary>
        private void HandleLanguageChanged(LanguageType newLang)
        {
            if (_rootContainer.style.display == DisplayStyle.Flex) {
                _nameLabel.text = LocalizationManager.GetText(_currentNameKey);
                _targetLocalizedText = LocalizationManager.GetText(_currentTextKey);

                if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
                _textLabel.text = _targetLocalizedText;
                _isTyping = false;
            }
        }

        private void OnSettingBtnClicked()
        {
            OffAutoRead();
            SettingPanelController.Instance?.OpenPanel();
        }
    }
}