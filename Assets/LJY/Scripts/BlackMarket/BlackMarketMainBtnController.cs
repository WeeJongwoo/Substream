using Localization;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace BlackMarket
{
    /// <summary>
    /// 블랙마켓 메인기능과 연결된 버튼 컨트롤러
    /// </summary>
    [RequireComponent(typeof(BlackMarketManager))]
    [RequireComponent(typeof(SavingsController))]
    public class BlackMarketMainBtnController : MonoBehaviour
    {
        private BlackMarketManager _bmManager;
        private SavingsController _savingsController;

        [Header("버튼 이름")]
        [SerializeField][Tooltip("블랙마켓 퇴장 버튼")] private string _exitBtnName;
        [SerializeField][Tooltip("상품 초기화 버튼")] private string _refreshBtnName;
        [SerializeField][Tooltip("세팅 창 열기 버튼")] private string _settingBtnName;
        [SerializeField][Tooltip("저축 창 열기 버튼")] private string _savingsBtnName;
        [SerializeField][Tooltip("멤버십 창 열기 버튼")] private string _membershipBtnName = "Btn_Membership";

        // ------------------------------------
        private Button _exitBtn;
        private Button _refreshBtn;
        private Button _settingBtn;
        private Button _savingsBtn;
        private Button _membershipBtn;
        // ------------------------------------

        public void OnEnable()
        {
            _bmManager = GetComponent<BlackMarketManager>();
            _savingsController = GetComponent<SavingsController>();

            // 다국어 변경 이벤트
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        public void OnDisable()
        {
            if (_exitBtn != null) { _exitBtn.clicked -= OnClickExit; }
            if (_refreshBtn != null) { _refreshBtn.clicked -= OnRefreshSlots; }
            if (_settingBtn != null) { _settingBtn.clicked -= OnPopUpSettingPanel; }
            if (_savingsBtn != null) { _savingsBtn.clicked -= OnClickOpenSavings; }
            if (_membershipBtn != null) { _membershipBtn.clicked -= OnClickOpenMembership; }

            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        /// <summary>
        /// 언어가 변경되었을 때 즉시 호출되어 동적 텍스트들을 다시 그림
        /// </summary>
        private void OnLanguageChanged(LanguageType newLanguage)
        {
            // 현재 Manager가 들고 있는 레벨 값을 가져와서 텍스트 UI 새로고침
            if (_bmManager == null) {
                Debug.LogError("블랙마켓 매니저 미할당");
                return;
            }

            _settingBtn.text = LocalizationManager.GetText(UIKeys.Common.BTN_SETTIG);
            _exitBtn.text = LocalizationManager.GetText(UIKeys.Common.BTN_EXIT);
            UpdateRefreshBtnText(_bmManager.CurRemainingRefreshCount);
            UpdateMembershipBtnText(_bmManager.CurMembershipLevel);
            UpdateSavingsBtnText(_bmManager.CurSavingsLevel);
            _bmManager.RefreshLocalization();
        }

        /// <summary>
        /// UI가 생성된 후 Manager에 의해 호출되어 버튼을 연결
        /// </summary>
        /// <param name="root">블랙마켓 UI의 루트 요소</param>
        public void ConnectButtonEvt(VisualElement root)
        {
            _exitBtn = root.Q<Button>(_exitBtnName);
            _refreshBtn = root.Q<Button>(_refreshBtnName);
            _settingBtn = root.Q<Button>(_settingBtnName);
            _savingsBtn = root.Q<Button>(_savingsBtnName);
            _membershipBtn = root.Q<Button>(_membershipBtnName);

            if (_exitBtn != null) _exitBtn.clicked += OnClickExit;
            else Debug.LogWarning($"Button '{_exitBtnName}' 를 찾을 수 없습니다");

            if (_refreshBtn != null) _refreshBtn.clicked += OnRefreshSlots;
            else Debug.LogWarning($"Button '{_refreshBtnName}' 를 찾을 수 없습니다");

            if (_settingBtn != null) _settingBtn.clicked += OnPopUpSettingPanel;
            else Debug.LogWarning($"Button '{_settingBtnName}' 를 찾을 수 없습니다");

            if (_savingsBtn != null) _savingsBtn.clicked += OnClickOpenSavings;
            else Debug.LogWarning($"Button '{_savingsBtn}' 를 찾을 수 없습니다");

            if (_membershipBtn != null) _membershipBtn.clicked += OnClickOpenMembership;
            else Debug.LogWarning($"Button '{_membershipBtn}' 를 찾을 수 없습니다");
        }

        /// <summary>
        /// 새로고침 버튼 텍스트 갱신
        /// </summary>
        /// <param name="count"></param>
        public void UpdateRefreshBtnText(int count)
        {
            if (_refreshBtn == null) {
                Debug.LogWarning("새로고침 버튼을 찾을 수 없습니다");
                return;
            }
            string localizedText = LocalizationManager.GetText(UIKeys.BlackMarket.BTN_REFRESH);
            _refreshBtn.text = $"{localizedText} X {count}";
        }

        /// <summary>
        /// 저축 버튼 텍스트 갱신
        /// </summary>
        public void UpdateSavingsBtnText(int level)
        {
            if (_savingsBtn == null) {
                Debug.LogWarning("저축 버튼을 찾을 수 없습니다");
                return;
            }
            string localizedText = LocalizationManager.GetText(UIKeys.BlackMarket.BTN_SAVINGS);
            _savingsBtn.text = $"{localizedText} {level}";
        }

        /// <summary>
        /// 멤버십 버튼 텍스트 갱신
        /// </summary>
        public void UpdateMembershipBtnText(int level)
        {
            if (_membershipBtn == null) {
                Debug.LogWarning("멤버십 버튼을 찾을 수 없습니다");
                return;
            }
            string localizedText = LocalizationManager.GetText(UIKeys.BlackMarket.BTN_MEMBERSHIP);
            _membershipBtn.text = $"{localizedText} {level}";
        }

        /// <summary>
        /// 새로고침 버튼 상태 갱신
        /// </summary>
        /// <param name="state">새로고침 상태</param>
        public void UpdateRefreshBtnState(RefreshState state)
        {
            _refreshBtn.style.opacity = state == RefreshState.Active ? 1f : 0.5f;
        }

        // ---- 이벤트 콜백 ----
        private void OnClickExit() => _bmManager?.CloseBlackMarket();
        private void OnRefreshSlots() => _bmManager?.HandleRefreshRequest();
        private void OnPopUpSettingPanel() => SettingPanelController.Instance?.OpenPanel();
        private void OnClickOpenSavings() => _savingsController?.OpenPopup(_bmManager.CurSavingsLevel, _bmManager.Savings);
        private void OnClickOpenMembership() => _bmManager?.OpenMembershipPopup();
    }
}