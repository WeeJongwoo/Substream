using Item;
using Localization;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace BlackMarket
{
    public class PurchaseController : MonoBehaviour
    {
        // -- UI 캐싱 --
        private VisualElement _purchaseRoot;

        private Label _popupName;
        private Label _popupInfo;
        private Label _popupPrice;

        private VisualElement _popupIcon;
        private VisualElement _popupBackground;

        private Label _requiredAmountTitle;
        private Button _btnBuy;
        private Button _btnCancel;

        // -- 상태 변수 --
        private ItemData _selectedItemData;
        private int _selectedSlotIdx = -1;

        // -- 이벤트 --
        public event Action<int, ItemData> OnPurchaseConfirmed;
        
        /// <summary>
        /// 매니저가 생성한 UI Root를 넘겨받아 초기화
        /// </summary>
        public void ConnectUI(VisualElement root)
        {
            _purchaseRoot = root;

            _popupName = _purchaseRoot.Q<Label>("Lbl_PopupName");
            _popupInfo = _purchaseRoot.Q<Label>("Lbl_PopupInfo");
            _popupPrice = _purchaseRoot.Q<Label>("Lbl_PopupPrice");
            _popupIcon = _purchaseRoot.Q<VisualElement>("Img_PopupIcon");
            _popupBackground = _purchaseRoot.Q<VisualElement>("Img_PopupIcon");
            _requiredAmountTitle = _purchaseRoot.Q<VisualElement>("RequiredAmount").Q<Label>("Title");

            // 자신이 담당하는 버튼 이벤트는 스스로 연결
            _btnBuy = _purchaseRoot.Q<Button>("Btn_Buy");
            _btnCancel = _purchaseRoot.Q<Button>("Btn_Cancel");

            if (_btnBuy != null) {
                _btnBuy.clicked -= OnBuyClicked;
                _btnBuy.clicked += OnBuyClicked;
            }
            if (_btnCancel != null) {
                _btnCancel.clicked -= ClosePopup;
                _btnCancel.clicked += ClosePopup;
            }
        }

        /// <summary>
        /// 매니저가 특정 슬롯을 클릭했을 때 호출하여 팝업
        /// </summary>
        public void OpenPopup(int slotIdx, ItemData item)
        {
            _selectedSlotIdx = slotIdx;
            _selectedItemData = item;

            if (_popupName != null) _popupName.text = LocalizationManager.GetText(item.Name);
            if (_popupInfo != null) _popupInfo.text = LocalizationManager.GetText(item.Info);
            if (_popupPrice != null) _popupPrice.text = item.Price.ToString("N0");

            if (_popupIcon != null) {
                _popupIcon.SetImage(item.Image, item.OffsetX, item.OffsetY, item.Scale);
                ItemSlotController.ApplyRarityBorderColor(_popupBackground, item.Rarity);
            }

            _purchaseRoot.ShowPopupFade();
        }

        public void ClosePopup()
        {
            _purchaseRoot?.HidePopupFade();
            _selectedSlotIdx = -1;
            _selectedItemData = null;
        }

        /// <summary>
        /// 블랙마켓 매니저에게 구매 버튼이 눌렸음을 알림
        /// </summary>
        private void OnBuyClicked()
        {
            if (_selectedItemData == null) return;

            OnPurchaseConfirmed?.Invoke(_selectedSlotIdx, _selectedItemData);
        }

        /// <summary>
        /// 다국어 변경 시 이 컨트롤러가 팝업 번역을 스스로 갱신
        /// </summary>
        public void RefreshTranslation()
        {
            if (_purchaseRoot == null) return;

            if (_requiredAmountTitle != null) 
                _requiredAmountTitle.text = LocalizationManager.GetText(UIKeys.BlackMarket.PURCHASE_REQUIRED_AMOUNT_TITLE);

            if (_btnBuy != null)
                _btnBuy.text = LocalizationManager.GetText(UIKeys.BlackMarket.BTN_PURCHASE);

            if (_btnCancel != null)
                _btnCancel.text = LocalizationManager.GetText(UIKeys.Common.BTN_CLOSE);


            if (_selectedItemData != null) {
                if (_popupName != null) _popupName.text = LocalizationManager.GetText(_selectedItemData.Name);
                if (_popupInfo != null) _popupInfo.text = LocalizationManager.GetText(_selectedItemData.Info);
            }
        }
    }
}