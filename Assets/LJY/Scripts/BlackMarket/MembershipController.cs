using Localization;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace BlackMarket
{
    public class MembershipController : MonoBehaviour
    {
        public event Action OnUpgradeRequested;

        // -- UI 캐싱 --
        private VisualElement _membershipRoot;

        private Label _lblMembershipTitle;
        private Label _lblMembershipDescription;

        private Label _lblCurMembershipLevelTitle;
        private Label _lblCurMembershipLevel;

        private Label _lblMembershipUpgradeCostTitle;
        private Label _lblMembershipUpgradeCost;

        private Button _btnUpgrade;
        private Button _btnClose;

        /// <summary>
        /// 매니저가 생성한 UI Root를 넘겨받아 초기화
        /// </summary>
        public void ConnectUI(VisualElement root)
        {
            _membershipRoot = root;

            _lblMembershipTitle = _membershipRoot.Q<VisualElement>("Header").Q<Label>("Title");
            _lblMembershipDescription = _membershipRoot.Q<VisualElement>("Header").Q<Label>("Description");

            _lblCurMembershipLevelTitle = _membershipRoot.Q<VisualElement>("CurMembershipLevel").Q<Label>("Title");
            _lblCurMembershipLevel = _membershipRoot.Q<VisualElement>("CurMembershipLevel").Q<Label>("Value");

            _lblMembershipUpgradeCostTitle = _membershipRoot.Q<VisualElement>("PromotionCost").Q<Label>("Title");
            _lblMembershipUpgradeCost = _membershipRoot.Q<VisualElement>("PromotionCost").Q<Label>("Value");

            _btnUpgrade = _membershipRoot.Q<Button>("Btn_UpgradeMembership");
            _btnClose = _membershipRoot.Q<Button>("Btn_CloseMembership");

            if (_btnUpgrade != null) {
                _btnUpgrade.clicked -= OnUpgradeClicked;
                _btnUpgrade.clicked += OnUpgradeClicked;
            }
            if (_btnClose != null) {
                _btnClose.clicked -= ClosePopup;
                _btnClose.clicked += ClosePopup;
            }
        }

        /// <summary>
        /// 매니저가 팝업을 열 때 현재 정보들을 넘겨줌
        /// </summary>
        public void OpenPopup(int curLevel, int nextCost)
        {
            UpdateUI(curLevel, nextCost);
            _membershipRoot?.ShowPopupFade();
        }

        public void ClosePopup()
        {
            _membershipRoot?.HidePopupFade();
        }

        /// <summary>
        /// 레벨업을 하거나 창을 열 때 텍스트를 갱신
        /// </summary>
        public void UpdateUI(int curLevel, int nextCost)
        {
            if (_lblCurMembershipLevel != null) {
                _lblCurMembershipLevel.text = $"Lv. {curLevel}";
            }

            if (_lblMembershipUpgradeCost != null) {
                // nextCost가 -1로 들어오면 최대 레벨로 간주
                if (nextCost < 0) {
                    _lblMembershipUpgradeCost.text = "Max Level";
                }
                else {
                    _lblMembershipUpgradeCost.text = $"{nextCost:N0} G";
                }
            }
        }

        /// <summary>
        /// 실제 업그레이드 가능 여부 검사 및 재화 차감은 매니저에게 위임
        /// </summary>
        private void OnUpgradeClicked()
        {
            OnUpgradeRequested?.Invoke();
        }

        /// <summary>
        /// 다국어 변경 시 이 컨트롤러가 팝업 번역을 스스로 갱신
        /// </summary>
        public void RefreshTranslation()
        {
            if (_membershipRoot != null) {
                _lblMembershipTitle.text = LocalizationManager.GetText(UIKeys.BlackMarket.MEMBERSHIP_TITLE);
                _lblMembershipDescription.text = LocalizationManager.GetText(UIKeys.BlackMarket.MEMBERSHIP_DESCRIPTION);
                _lblCurMembershipLevelTitle.text = LocalizationManager.GetText(UIKeys.BlackMarket.MEMBERSHIP_CURLEVEL);
                _lblMembershipUpgradeCostTitle.text = LocalizationManager.GetText(UIKeys.BlackMarket.MEMBERSHIP_PROMOTION_REQUIRMENT);
                _btnUpgrade.text = LocalizationManager.GetText(UIKeys.BlackMarket.BTN_MEMBERSHIP_UPGRADE);
                _btnClose.text = LocalizationManager.GetText(UIKeys.Common.BTN_CLOSE);
            }
        }
    }
}