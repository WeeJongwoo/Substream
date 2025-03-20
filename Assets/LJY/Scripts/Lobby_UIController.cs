using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Lobby_UIController : MonoBehaviour
{
    private bool _isAnimating = false;
    private Button _foldButton;
    private VisualElement _hiddenButtonContainer;

    [SerializeField] private List<string> _popupPannelButtons;
    private Button _exitPannelButton;
    private VisualElement _pannel;

    private Button _popupLineWindowButton;
    private VisualElement _lineWindow;

    [SerializeField] private List<string> _mainContentButtons;
    private VisualElement _loadingScreen;
    private ProgressBar _loadingProgressBar;
    private SceneLoader sceneLoader;

    void Start()
    {
        VisualElement root = this.gameObject.GetComponent<UIDocument>().rootVisualElement;

        PannelInit(root);
        PopupLineWindowInit(root);
        HiddenContainerInit(root);
        ExitPannelButtonInit(root);
        MainContentButtonInit(root);
    }

    private void PannelInit(VisualElement root)
    {
        foreach (string name in _popupPannelButtons)
        {
            Button button = root.Q<Button>(name);
            button.RegisterCallback<ClickEvent>(OnPopupWindow);
        }
        _pannel = root.Q<VisualElement>("Window_Container");
        _pannel.style.display = DisplayStyle.None;
    }

    private void PopupLineWindowInit(VisualElement root)
    {
        _popupLineWindowButton = root.Q<Button>("LD_Button");
        _popupLineWindowButton.RegisterCallback<ClickEvent>(OnPopupLine);

        _lineWindow = root.Q<VisualElement>("Line_Window");
        _lineWindow.style.display = DisplayStyle.None;
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

    private void MainContentButtonInit(VisualElement root)
    {
        foreach (string name in _mainContentButtons)
        {
            Button button = root.Q<Button>(name);
            button.RegisterCallback<ClickEvent>(OnLoadingScreen);
        }
    }

    private void OnFoldingButton(ClickEvent evt)
    {
        if (_isAnimating) return; 
        _isAnimating = true;

        if (_hiddenButtonContainer.ClassListContains("hidden_button-container_unfold"))
        {
            _hiddenButtonContainer.RemoveFromClassList("hidden_button-container_unfold");
        }
        else
        {
            _hiddenButtonContainer.style.display = DisplayStyle.Flex;
            _hiddenButtonContainer.AddToClassList("hidden_button-container_unfold");
        }
    }

    private void OnTransitionEndEvents(TransitionEndEvent evt)
    {
        if (!_hiddenButtonContainer.ClassListContains("hidden_button-container_unfold"))
        {
            _hiddenButtonContainer.style.display = DisplayStyle.None;
        }

        if (!_lineWindow.ClassListContains("line_window-popup"))
            _lineWindow.style.display = DisplayStyle.None;

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

    private void OnPopupLine(ClickEvent evt)
    {
        _lineWindow.style.display = DisplayStyle.Flex;
        _lineWindow.AddToClassList("line_window-popup");

        Invoke("OnPopdownLine", 5f);
    }

    private void OnPopdownLine()
    {
        _lineWindow.RemoveFromClassList("line_window-popup");
    }

    private void OnLoadingScreen(ClickEvent ect)
    {

    }
}
