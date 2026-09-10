using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Lobby
{
    public class Lobby_UIController : MonoBehaviour
    {
        private bool _isAnimating = false;
        private Button _foldButton;
        private VisualElement _hiddenButtonContainer;

        [SerializeField] private List<string> _popupPannelButtons;
        private Button _exitPannelButton;
        private VisualElement _pannel;

        [SerializeField] private List<string> _mainContentButtons;
        private Dictionary<string, string> _mainContentScenes = new Dictionary<string, string>();

        private SceneLoader _sceneLoader;

        private SoundManager _soundManager;
        [SerializeField] private List<AudioClip> audioClips;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _sceneLoader = GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>();
            _soundManager = GetComponent<SoundManager>();

            PannelInit(root);
            HiddenContainerInit(root);
            ExitPannelButtonInit(root);
            MainContentInit(root);
        }

        private void PannelInit(VisualElement root)
        {
            foreach (string name in _popupPannelButtons) {
                Button button = root.Q<Button>(name);
                button.RegisterCallback<ClickEvent>(OnPopupWindow);
            }
            _pannel = root.Q<VisualElement>("Window_Container");
            _pannel.style.display = DisplayStyle.None;
        }

        private void HiddenContainerInit(VisualElement root)
        {
            _hiddenButtonContainer = root.Q<VisualElement>("Hidden_Button_Container");
            _hiddenButtonContainer.RemoveFromClassList("hidden_button-container_unfold");
            _hiddenButtonContainer.style.display = DisplayStyle.None;
            _hiddenButtonContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEndEvents);

            _foldButton = root.Q<Button>("Fold_Button");
            _foldButton.RegisterCallback<ClickEvent>(OnFoldingButton);
        }

        private void ExitPannelButtonInit(VisualElement root)
        {
            _exitPannelButton = root.Q<Button>("Exit_Pannel_Button");
            _exitPannelButton.RegisterCallback<ClickEvent>(OnExitPannel);
        }

        private void MainContentInit(VisualElement root)
        {
            // 메인 컨텐츠 씬으로 이동하는 버튼 연결 작업
            foreach (string name in _mainContentButtons) {
                Button button = root.Q<Button>(name);
                button.RegisterCallback<ClickEvent>(OnLoadingScreen);
                _soundManager.SetButtonSoundEvent<PointerEnterEvent>(button, audioClips[(int)SoundTrack.b_hover]);
                _soundManager.SetButtonSoundEvent<ClickEvent>(button, audioClips[(int)SoundTrack.b_clicked]);
            }

            // 연결된 메인 컨텐츠용 버튼과 각 씬의 연결 작업
            List<string> SceneNameList =
                _mainContentButtons.Select(
                button => button.Contains("-")
                ? button.Substring(0, button.IndexOf("-")) + "Scene" : button + "Scene").ToList();

            foreach (string name in _mainContentButtons) {
                _mainContentScenes.Add(name, SceneNameList[_mainContentButtons.IndexOf(name)]);
            }
        }

        private void OnFoldingButton(ClickEvent evt)
        {
            if (_isAnimating) return;
            _isAnimating = true;

            if (_hiddenButtonContainer.ClassListContains("hidden_button-container_unfold")) {
                _hiddenButtonContainer.RemoveFromClassList("hidden_button-container_unfold");
            }
            else {
                _hiddenButtonContainer.style.display = DisplayStyle.Flex;
                _hiddenButtonContainer.AddToClassList("hidden_button-container_unfold");
            }
        }

        private void OnTransitionEndEvents(TransitionEndEvent evt)
        {
            if (!_hiddenButtonContainer.ClassListContains("hidden_button-container_unfold")) {
                _hiddenButtonContainer.style.display = DisplayStyle.None;
            }

            _isAnimating = false;
        }

        private void OnPopupWindow(ClickEvent evt)
        {
            _pannel.style.display = DisplayStyle.Flex;
        }

        private void OnExitPannel(ClickEvent evt)
        {
            _pannel.style.display = DisplayStyle.None;
        }

        // 클릭한 버튼의 이름(key)을 이용해 해당 씬 이름을 딕셔너리에서 가져온 후 로딩 처리 시작
        private void OnLoadingScreen(ClickEvent evt)
        {
            Button clickedButton = evt.currentTarget as Button;
            if (clickedButton != null) {
                if (_mainContentScenes.TryGetValue(clickedButton.name, out string sceneName)) {
                    // 로딩 화면 페이드 아웃과 프로그래스바 업데이트를 포함한 코루틴 실행
                    StartCoroutine(_sceneLoader.LoadSceneWithFade(sceneName));
                }
            }
        }
    }
}