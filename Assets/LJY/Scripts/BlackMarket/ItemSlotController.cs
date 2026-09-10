using System;
using UnityEngine;
using UnityEngine.UIElements;
using Localization;
using Item;
using Utils;

namespace BlackMarket
{
    public class ItemSlotController
    {
        public ItemData CurrentItem { get; private set; }
        public int SlotIndex { get; private set; }

        // 매니저에게 클릭 이벤트를 전달
        public event Action<int, ItemData> OnSlotClicked;

        // UI 요소 캐싱
        private VisualElement _rootElement;
        private Label _lblName;
        private Label _lblPrice;
        private VisualElement _imgIcon;
        private VisualElement _rarityBackground;
        /// <summary>
        /// 구매 후 null이 되어 추적 불가한 슬롯을 추적하는 용도(구매 후 번역 기능 사용 시 발생하는 오류 방어용)
        /// </summary>
        private string _cachedNameKey;

        public VisualElement RootElement => _rootElement;

        /// <summary>
        /// 슬롯의 최상위 VisualElement와 자신의 인덱스를 넘겨받아 초기화
        /// </summary>
        public ItemSlotController(VisualElement rootElement, int slotIndex)
        {
            _rootElement = rootElement;
            SlotIndex = slotIndex;

            // UI 요소 한 번만 찾아서 캐싱
            _lblName = _rootElement.Q<Label>("Lbl_ItemName");
            _lblPrice = _rootElement.Q<Label>("Lbl_ItemPrice");
            _imgIcon = _rootElement.Q<VisualElement>("Img_ItemIcon");
            _rarityBackground = _rootElement.Q<VisualElement>("RarityBackground");

            // 클릭 이벤트 등록
            _rootElement.RegisterCallback<ClickEvent>(OnClick);
        }

        /// <summary>
        /// 다국어 설정이 변경되었을 때 아이템 이름을 갱신
        /// </summary>
        public void RefreshTranslation()
        {
            if (string.IsNullOrEmpty(_cachedNameKey)) return;

            if (_lblName != null) {
                _lblName.text = LocalizationManager.GetText(_cachedNameKey);
            }
        }

        /// <summary>
        /// 매니저로부터 아이템 데이터를 넘겨받아 UI를 갱신
        /// </summary>
        public void SetItemData(ItemData itemData)
        {
            CurrentItem = itemData;

            // 이미 팔렸거나 빈 슬롯 잠금 처리
            if (CurrentItem == null) {
                LockSlotUI();
                return;
            }

            // 번역 텍스트 키 저장
            _cachedNameKey = CurrentItem.Name;

            // 잠금 해제
            _rootElement.style.opacity = 1f;

            // 다국어 텍스트 및 가격 바인딩
            if (_lblName != null) {
                _lblName.text = LocalizationManager.GetText(_cachedNameKey);
            }
            if (_lblPrice != null) {
                _lblPrice.text = CurrentItem.Price.ToString("N0");
            }

            // 이미지 바인딩
            if (_imgIcon != null) {
                _imgIcon.SetImage(CurrentItem.Image, CurrentItem.OffsetX, CurrentItem.OffsetY, CurrentItem.Scale);
            }

            // 테두리 색상 적용
            ApplyRarityBorderColor(_rarityBackground, CurrentItem.Rarity);
        }

        /// <summary>
        /// 아이템이 구매되었거나 없을 때 슬롯을 비활성화 시각 처리
        /// </summary>
        public void LockSlotUI()
        {
            CurrentItem = null;
            _rootElement.style.opacity = 0.3f;
        }

        /// <summary>
        /// 슬롯이 클릭되었을 때 실행되는 내부 콜백
        /// </summary>
        private void OnClick(ClickEvent evt)
        {
            if (CurrentItem == null) return;

            // 구독하고 있는 매니저에게 클릭 사실을 알림
            OnSlotClicked?.Invoke(SlotIndex, CurrentItem);
        }

        /// <summary>
        /// UI가 파괴될 때 이벤트를 해제
        /// </summary>
        public void Dispose()
        {
            _rootElement?.UnregisterCallback<ClickEvent>(OnClick);
        }


        // --- 관리자 클래스에서도 재사용할 수 있도록 책임을 옮긴 정적 메서드 ---
        /// <summary>
        /// 희귀도에 따른 공통 색상 반환 로직
        /// </summary>
        public static Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return Color.black;
                case ItemRarity.Rare: return Color.red;
                case ItemRarity.Unique: return Color.blue;
                case ItemRarity.Legendary: return Color.yellow;
                default:
                    Debug.LogError("아이템 희귀도 색깔 미지정");
                    return Color.black;
            }
        }

        /// <summary>
        /// 특정 VisualElement에 희귀도에 맞는 테두리 색상을 일괄 적용하는 로직
        /// </summary>
        public static void ApplyRarityBorderColor(VisualElement element, ItemRarity rarity)
        {
            if (element == null) return;

            StyleColor borderColor = new StyleColor(GetRarityColor(rarity));
            element.style.borderBottomColor = borderColor;
            element.style.borderLeftColor = borderColor;
            element.style.borderRightColor = borderColor;
            element.style.borderTopColor = borderColor;
        }
    }
}