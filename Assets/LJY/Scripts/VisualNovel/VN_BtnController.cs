using UnityEngine;
using UnityEngine.UIElements;
using Utils;

public class VN_BtnController : MonoBehaviour
{
    public VisualNovelManager _vnManager;

    UIDocument _root;
    Button button;

    void OnEnable()
    {
        _root = GetComponent<UIDocument>();
        if (_root == null || _root.rootVisualElement == null) return;

        button = _root.rootVisualElement.Q<Button>("Button");

        if (button != null) {
            button.clicked += A;
        }
        else {
            Debug.LogWarning("[VN_BtnController] 'Button'이라는 이름의 요소를 찾을 수 없습니다!");
        }
    }

    private void OnDisable()
    {
        if (button != null) {
            button.clicked -= A;
        }
    }

    private void A()
    {
        if (_vnManager != null) {
            _vnManager.StartEpisode("EP_01");
        }
        else {
            Debug.LogError("VisualNovelManager가 연결되지 않았습니다!");
        }
    }
}