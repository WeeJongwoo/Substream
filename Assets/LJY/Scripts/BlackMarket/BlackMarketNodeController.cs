using UnityEngine;
using UnityEngine.UIElements;
using BlackMarket;

namespace UI.BlackMarket
{
    /// <summary>
    /// 월드맵 등의 노드에서 블랙마켓 진입 버튼을 관리하는 스크립트
    /// </summary>
    public class BlackMarketNodeController : MonoBehaviour
    {
        [Header("Node UI Settings")]
        [SerializeField] private string _enterButtonName = "Button";

        private UIDocument _nodeUIDocument;
        private Button _enterBtn;

        private void OnEnable()
        {
            _nodeUIDocument = GetComponent<UIDocument>();

            if (_nodeUIDocument != null) {
                var root = _nodeUIDocument.rootVisualElement;
                _enterBtn = root.Q<Button>(_enterButtonName);

                if (_enterBtn != null) {
                    _enterBtn.clicked += OnClickEnterShop;
                }
                else {
                    Debug.LogWarning($"'{_enterButtonName}' 버튼을 찾을 수 없습니다. UXML 이름을 확인하세요.");
                }
            }
        }

        private void OnClickEnterShop()
        {
            if (BlackMarketManager.Instance != null) {
                BlackMarketManager.Instance.OpenBlackMarket();
            }
            else {
                Debug.LogError("BlackMarketManager 인스턴스가 씬에 없습니다.");
            }
        }
    }
}