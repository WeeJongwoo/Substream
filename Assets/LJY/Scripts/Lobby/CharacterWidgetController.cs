using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils
{
    public class CharacterWidgetController : MonoBehaviour
    {
        [Header("이미지 컨트롤러")]
        public Sprite CharacterSprite;
        public int SpriteOffsetX;
        public int SpriteOffsetY;
        public int SpriteScaleRatio;

        [Header("UI Element 이름")]
        [SerializeField] private string CHAR_BUTTON = "LD_Button";
        [SerializeField] private string CHAR_IMAGE = "LD_Character_image";
        [SerializeField] private string SPEECH_BUBBLE = "Line_Window";
        [SerializeField] private string SPEAKER_NAME = "Lbl_SpeakerName";
        [SerializeField] private string SPEECH_TXT = "Lbl_LineText";

        public Action OnCharacterClicked;

        // -- 런타임 변수 --
        private VisualElement _characterArea;
        private Button _characterButton;
        private VisualElement _lineWindow;
        private Label _lblSpeakerName;
        private Label _lblLine;

        private const string POPUP_CLASS = "line_window-popup";

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null) Initialize(uiDocument.rootVisualElement);
        }

        void OnDisable()
        {
            if (_characterButton != null) _characterButton.UnregisterCallback<ClickEvent>(OnPopupLine);
            if (_lineWindow != null) _lineWindow.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
        }

        public void Initialize(VisualElement root)
        {
            if (root == null) {
                Debug.LogWarning("Root 미할당");
                return;
            }
            _characterButton = root.Q<Button>(CHAR_BUTTON);
            _characterArea = root.Q<VisualElement>(CHAR_IMAGE);

            _lineWindow = root.Q<VisualElement>(SPEECH_BUBBLE);
            _lblSpeakerName = root.Q<Label>(SPEAKER_NAME);
            _lblLine = root.Q<Label>(SPEECH_TXT);

            if (_characterButton != null)
                _characterButton.RegisterCallback<ClickEvent>(OnPopupLine);

            if (_lineWindow != null)
            {
                _lineWindow.style.display = DisplayStyle.None;
                _lineWindow.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            }

            SetupCharacterImage();
        }

        private void SetupCharacterImage()
        {
            if (_characterArea != null && CharacterSprite != null)
            {
                _characterArea.SetImage(CharacterSprite, SpriteOffsetX, SpriteOffsetY, SpriteScaleRatio);
            }
        }

        public void ShowLine(string speaker, string lineTxt)
        {
            if (_lblSpeakerName != null && _lblLine != null)
            {
                _lblSpeakerName.text = speaker;
                _lblLine.text = lineTxt;
            }
            OnPopup();
        }

        private void OnPopupLine(ClickEvent evt)
        {
            OnCharacterClicked?.Invoke();
        }

        private void OnPopup()
        {
            if (_lineWindow == null) return;

            CancelInvoke(nameof(OnPopdownLine));

            _lineWindow.style.display = DisplayStyle.Flex;

            _lineWindow.schedule.Execute(() =>
            {
                _lineWindow.AddToClassList(POPUP_CLASS);
            }).StartingIn(1);

            Invoke(nameof(OnPopdownLine), 5f);
        }

        private void OnPopdownLine()
        {
            if (_lineWindow != null)
                _lineWindow.RemoveFromClassList(POPUP_CLASS);
        }

        /// <summary>
        /// 애니메이션 종료 시점에 이동이 끝난 UI를 비활성화 하도록 제어함
        /// </summary>
        /// <param name="evt">UI 이동 종료 이벤트</param>
        private void OnTransitionEnd(TransitionEndEvent evt)
        {
            if (_lineWindow != null && !_lineWindow.ClassListContains(POPUP_CLASS))
            {
                _lineWindow.style.display = DisplayStyle.None;
            }
        }
    }
}