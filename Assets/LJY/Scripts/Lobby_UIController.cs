using UnityEngine;
using UnityEngine.UIElements;

public class Lobby_UIController : MonoBehaviour
{
    private Button _foldButton;
    private VisualElement _hiddenButtonContainer;


    void Start()
    {
        var root = this.gameObject.GetComponent<UIDocument>().rootVisualElement;

        _hiddenButtonContainer = root.Q<VisualElement>("Hidden_Button_Container");
        _foldButton = root.Q<Button>("Fold_Button");

        _hiddenButtonContainer.style.display = DisplayStyle.None;
        _hiddenButtonContainer.RegisterCallback<TransitionEndEvent>(OnFoldContainer);

        _foldButton.RegisterCallback<ClickEvent>(OnFoldingButton);
    }

    private void OnFoldingButton(ClickEvent evt)
    {
        if (_hiddenButtonContainer.style.display == DisplayStyle.Flex)
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
            _hiddenButtonContainer.style.display = DisplayStyle.None;
    }
}
