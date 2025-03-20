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

    void Start()
    {
        var root = this.gameObject.GetComponent<UIDocument>().rootVisualElement;

        foreach (string name in _popupPannelButtons)
        {
            Button button = root.Q<Button>(name);
            button.RegisterCallback<ClickEvent>(OnPopupWindow);
        }
        _pannel = root.Q<VisualElement>("Window_Container");
        _pannel.style.display = DisplayStyle.None;

        _popupLineWindowButton = root.Q<Button>("LD_Button");
        _popupLineWindowButton.RegisterCallback<ClickEvent>(OnPopupLine);

        _lineWindow = root.Q<VisualElement>("Line_Window");
        _lineWindow.style.display = DisplayStyle.None;

        _hiddenButtonContainer = root.Q<VisualElement>("Hidden_Button_Container");
        _hiddenButtonContainer.RemoveFromClassList("hidden_button-container_unfold");
        _hiddenButtonContainer.style.display = DisplayStyle.None;
        _hiddenButtonContainer.RegisterCallback<TransitionEndEvent>(OnFoldContainer);

        _foldButton = root.Q<Button>("Fold_Button");
        _foldButton.RegisterCallback<ClickEvent>(OnFoldingButton);

        _exitPannelButton = root.Q<Button>("Exit_Pannel_Button");
        _exitPannelButton.RegisterCallback<ClickEvent>(OnExitPannel);

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

    private void OnFoldContainer(TransitionEndEvent evt)
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
}
