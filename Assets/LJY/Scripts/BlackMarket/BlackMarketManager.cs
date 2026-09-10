using Audio.Controller;
using Audio.Data;
using Item;
using Localization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace BlackMarket
{
    [System.Serializable]
    [Tooltip("멤버십 단계별 각 희귀도의 아이템 등장확률을 설정한다.")]
    struct RarityProbability
    {
        [Tooltip("멤버십 단계 이름")]
        public string Name;

        [Range(0, 100)] public int CommonWeight;
        [Range(0, 100)] public int RareWeight;
        [Range(0, 100)] public int UniqueWeight;
        [Range(0, 100)] public int LegendaryWeight;

        // 가중치 총합 계산
        public int GetTotalWeight() => CommonWeight + RareWeight + UniqueWeight + LegendaryWeight;
    }

    [System.Serializable]
    [Tooltip("저축 단계별 상한선 및 효과 설정")]
    struct Savings
    {
        public int value;
        [Range(0, 10)] public int additiveSlots;
        [Range(0, 10)] public int additiveRefreshing;
    }

    public enum RefreshState
        {
            /// <summary>
            /// 저축 단계 미달
            /// </summary>
            Disabled,
            /// <summary>
            /// 활성화
            /// </summary>
            Active,
            /// <summary>
            /// 이미 사용함
            /// </summary>
            Locked, 
        }

    [RequireComponent(typeof(BlackMarketMainBtnController))]
    [RequireComponent(typeof(PurchaseController))]
    [RequireComponent(typeof(SavingsController))]
    [RequireComponent(typeof(MembershipController))]
    public class BlackMarketManager : MonoBehaviour
    {
        public static BlackMarketManager Instance;

        private PurchaseController _purchaseController;
        private SavingsController _savingsController;
        private MembershipController _membershipController;

        [Header("Debug 변수")]
        public long CurrentGold = 999999999999998;
        public int Savings = 500;

        [Header("UI References")]
        [SerializeField] private UIDocument _blackMarketUID;
        [SerializeField] private VisualTreeAsset _blackMarketUXML;
        [SerializeField] private VisualTreeAsset _itemSlotUXML;
        [SerializeField] private VisualTreeAsset _settingPanelUXML;
        [SerializeField] private VisualTreeAsset _purchaseUXML;
        [SerializeField] private VisualTreeAsset _savingsUXML;
        [SerializeField] private VisualTreeAsset _membershipUXML;

        [Header("Settings")]
        [SerializeField] [Tooltip("슬롯이 추가되는 저축액 기준")]
        private List<Savings> savingsThresholds = new List<Savings>();

        [SerializeField] [Tooltip("멤버십 등급별 필요 비용 구간")]
        private List<int> membershipThresholds = new List<int>();

        [SerializeField] [Tooltip("멤버십 등급별 등장 확률 (인덱스 0 = 0단계 확률)")]
        private List<RarityProbability> rarityProbabilities = new List<RarityProbability>();

        [Tooltip("저축 0단계 기준, 기본 지급되는 아이템 슬롯 개수")]
        [SerializeField] private int _baseSlotCount = 6;

        [Header("Local User Data")]
        [SerializeField] [Tooltip("로컬 상의 플레이어 데이터")]
        private LocalUserDataBase _localUserData;

        [Header("Databases")]
        [SerializeField] [Tooltip("아이템 이미지 매핑 SO")]
        private ItemIconDatabase _itemIconDatabase;

        /// <summary>
        /// 현재 남은 새로고침 횟수
        /// </summary>
        public int CurRemainingRefreshCount => _additiveRefreshingCount;
        /// <summary>
        /// 현재 저축 단계
        /// </summary>
        public int CurSavingsLevel { get; private set; } = 0;
        /// <summary>
        /// 현재 멤버십 Level
        /// </summary>
        public int CurMembershipLevel { get; private set; } = 0;
        /// <summary>
        /// 최종 슬롯 개수
        /// </summary>
        public int TotalSlotCount => _baseSlotCount + _additiveSlotCount;

        // -- 런타임 변수 --
        private int _additiveSlotCount = 0;
        private int _additiveRefreshingCount = 0;
        private int _usedRefreshingCount = 0;
        private int _curMembershipLevel = 0;
        private RarityProbability _curProbability;
        private RefreshState _curRefreshState;
        private List<int> _prevItemIDs;

        // -- UID 변수 --
        private VisualElement _blackMarketRoot;
        private Label _coinTitleLabel;
        private Label _coinValue;
        private VisualElement _slotContainer;

        // -- 아이템 --
        private ItemDatabase _itemDatabase;
        private List<ItemData> _itemsData;
        private List<ItemSlotController> _slotControllers = new List<ItemSlotController>();

        // -- 대사 매니저 --
        private DialogueManager _dmBlackMarket;

        // -- UI Controller --
        private CharacterWidgetController _cwController;
        private BlackMarketMainBtnController _btnController;

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(this);
            }

            if (_itemIconDatabase != null) {
                _itemIconDatabase.Initialize();
            }
            else {
                Debug.LogError("ItemIconDatabaseSO가 할당되지 않았습니다");
            }

            _purchaseController = GetComponent<PurchaseController>();
            _savingsController = GetComponent<SavingsController>();
            _membershipController = GetComponent<MembershipController>();
            _btnController = GetComponent<BlackMarketMainBtnController>();
            _cwController = GetComponent<CharacterWidgetController>();

            _prevItemIDs = new List<int>();
            _itemDatabase = new ItemDatabase();
            _itemsData = new List<ItemData>();
            _dmBlackMarket = new DialogueManager();

            _purchaseController.OnPurchaseConfirmed += ProcessPurchase;
            _savingsController.OnDepositRequested += RequestDeposit;
            _savingsController.OnWithdrawRequested += RequestWithdraw;
            _membershipController.OnUpgradeRequested += RequestUpgradeMembership;

        }

        private void OnDestroy()
        {
            if (_purchaseController != null) {
                _purchaseController.OnPurchaseConfirmed -= ProcessPurchase;
            }
            if (_savingsController != null) {
                _savingsController.OnDepositRequested -= RequestDeposit;
                _savingsController.OnWithdrawRequested -= RequestWithdraw;
            }
            if (_membershipController != null) {
                _membershipController.OnUpgradeRequested -= RequestUpgradeMembership;
            }
            if (_slotControllers != null) {
                foreach (var slot in _slotControllers) {
                    if (slot != null) {
                        slot.OnSlotClicked -= HandleSlotClicked;
                    }
                }
            }
        }

        // ----- 진입 및 퇴장 기능 -----
        /// <summary>
        /// 블랙마켓 UI 열기
        /// </summary>
        public void OpenBlackMarket()
        {
            // 데이터 초기화
            Initialize(Savings, CurMembershipLevel);

            // 아이템 슬롯 생성
            StartMarket();
        }

        /// <summary>
        /// 블랙마켓 UI 닫기
        /// </summary>
        public void CloseBlackMarket()
        {
            AudioController.Instance.PlaySFX(AudioKeys.SFX_DOOR);
            _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.EXIT, null);

            _blackMarketRoot?.RemoveFromHierarchy(); // UI 제거
            _blackMarketRoot = null;

            _cwController.OnCharacterClicked -= HandleCharacterClick;
        }

        // ----- 캐릭터 위젯 기능 -----
        /// <summary>
        /// 캐릭터 위젯을 클릭했을 경우 발생하는 대사 호출 모음
        /// </summary>
        private void HandleCharacterClick()
        {
            if (CurrentGold < 100) {
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.GREETING_POOR, _cwController);
                return;
            }
            else if (CurMembershipLevel >= 2) {
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.GREETING_VIP, _cwController);
                return;
            }
            _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.GREETING_NORMAL, _cwController);
        }

        // ----- 블랙마켓 초기화 기능 -----
        /// <summary>
        /// 블랙마켓 생성 절차
        /// </summary>
        /// <param name="currentSavings">현재 플레이어의 누적 저축액</param>
        /// <param name="currentMembershipFee">현재 판에서 지불한 멤버십 비용</param>
        private void Initialize(long curSavings, int startMembershipLevel)
        {
            if (_blackMarketUID == null) {
                Debug.LogError("UIDocument 미할당");
                return;
            }
            if (!_blackMarketUXML || !_itemSlotUXML || !_purchaseUXML || !_savingsUXML || !_membershipUXML || !_settingPanelUXML) {
                Debug.LogError($"UXML 미할당\n" +
                    $"  {_blackMarketUXML}\n" +
                    $"  {_itemSlotUXML}\n" +
                    $"   {_purchaseUXML}\n" +
                    $"   {_savingsUXML}\n" +
                    $"   {_membershipUXML}\n" +
                    $"    {_settingPanelUXML}\n");
                return;
            }

            // UI 생성 및 화면에 추가
            _blackMarketRoot = _blackMarketUXML.Instantiate();
            _blackMarketRoot.style.flexGrow = 1;
            _blackMarketUID.rootVisualElement.Add(_blackMarketRoot);
            _blackMarketRoot.style.visibility = Visibility.Hidden;

            // 캐릭터 위젯 초기화 및 이벤트 연결
            _cwController?.Initialize(_blackMarketRoot);
            _cwController.OnCharacterClicked += HandleCharacterClick;

            // 블랙마켓 UI 연결
            _coinTitleLabel = _blackMarketRoot.Q<VisualElement>("Coin_Panel").Q<Label>("Title");
            _coinValue = _blackMarketRoot.Q<VisualElement>("Coin_Panel").Q<Label>("Value");
            _slotContainer = _blackMarketRoot.Q<VisualElement>("Item_Grid_Container");

            // 팝업 패널 UI 생성
            CreateAndHidePanels();

            // 메인 버튼 이벤트 연결
            _btnController?.ConnectButtonEvt(_blackMarketRoot);
            _btnController?.UpdateMembershipBtnText(CurMembershipLevel);

            // 저축/멤버십 효과 계산
            CalculateSavingsEffect(curSavings);
            CalculateMembershipLevel(startMembershipLevel);
            Debug.Log(
                $"[ BlackMarket ]\n" +
                $"  저축액 : {curSavings}\n" +
                $"      추가 슬롯 : {_additiveSlotCount}\n" +
                $"      추가 새로고침 : {_additiveRefreshingCount}\n" +
                $"  멤버십 Lv : {_curMembershipLevel}\n" +
                $"      일반 : {rarityProbabilities[_curMembershipLevel].CommonWeight}\n" +
                $"      희귀 : {rarityProbabilities[_curMembershipLevel].RareWeight}\n" +
                $"      영웅 : {rarityProbabilities[_curMembershipLevel].UniqueWeight}\n" +
                $"      전설 : {rarityProbabilities[_curMembershipLevel].LegendaryWeight}\n");

            // 새로고침 초기화
            _usedRefreshingCount = 0;
            UpdateRefreshButtonUI();

            _slotControllers.Clear();
            _slotContainer.Clear();

            // 초기 아이템 슬롯 생성
            for (int i = 0; i < TotalSlotCount; i++) {
                VisualElement slot = _itemSlotUXML.Instantiate();
                slot.name = $"ItemSlot_{i}";
                _slotContainer.Add(slot);

                ItemSlotController slotController = new ItemSlotController(slot, i);
                slotController.OnSlotClicked += HandleSlotClicked; // 슬롯이 클릭되었을 때 실행될 함수를 연결
                _slotControllers.Add(slotController);
            }

            // 아이템 데이터베이스 초기화
            _itemDatabase.Initialize(_itemIconDatabase);
            _itemDatabase.SetExcludeItemIDs(null);

            // 블랙마켓 대사 매니저 초기화
            _dmBlackMarket.Initialize("NPC_BlackMarket", DialogueKeys.BlackMarket.NPC_ID);

            var ddoManager = DontDestroyOnLoadManager.Instance;
            if (ddoManager != null && ddoManager.ResourceManager != null) {
                // int curUserID = ddoManager.ResourceManager.SelectUserID;
                // var curUserData = ddoManager.LocalUser(curUserID);

                // List<int> currentPartyIDs = curUserData.PartyCharacterIDs;
                // List<int> ownedOpartsIDs = curUserData.OwnedOpartsIDs;
                // _coinValue.text = curUserData.Gold.ToString("N0");

                // <---- 임시 더미 데이터 ---->
                List<int> curPartyIDs = new List<int> { 0, 1, 2, 3 };
                List<int> ownedOpartsIDs = new List<int> { 101 };
                _coinTitleLabel.text = LocalizationManager.GetText(UIKeys.BlackMarket.TOTAL_COIN_TITLE);
                _coinValue.text = CurrentGold.ToString("N0");

                // 조건에 맞는 아이템 풀 사전 생성
                _itemDatabase.CreateItemPool(curPartyIDs, ownedOpartsIDs);
            }

            // 아이템을 UI에 등기함
            RegistMarketItems();
        }

        // ----- 팝업 생성/초기화 기능 -----
        /// <summary>
        /// 블랙마켓에 존재하는 팝업 패널 UI의 생성 및 초기화
        /// </summary>
        private void CreateAndHidePanels()
        {
            // --- 세팅 패널 ---
            if (_settingPanelUXML != null) {
                VisualElement settingRoot = _settingPanelUXML.Instantiate();
                settingRoot.SetAbsolutePosition();
                settingRoot.style.display = DisplayStyle.None;
                settingRoot.AddToClassList("popup-fade-base");
                _blackMarketRoot.Add(settingRoot);

                if (SettingPanelController.Instance != null) {
                    SettingPanelController.Instance.ConnectSettingUI(settingRoot);
                }
            }

            // --- 구매 팝업 패널 ---
            if (_purchaseUXML != null) {
                VisualElement purchaseRoot = _purchaseUXML.Instantiate();
                purchaseRoot.SetAbsolutePosition();
                purchaseRoot.style.display = DisplayStyle.None;
                purchaseRoot.AddToClassList("popup-fade-base");
                _blackMarketRoot.Add(purchaseRoot);

                _purchaseController.ConnectUI(purchaseRoot);
            }

            // --- 저축 팝업 패널 ---
            if (_savingsUXML != null) {
                VisualElement savingsRoot = _savingsUXML.Instantiate();
                savingsRoot.SetAbsolutePosition();
                savingsRoot.style.display = DisplayStyle.None;
                savingsRoot.AddToClassList("popup-fade-base");
                _blackMarketRoot.Add(savingsRoot);

                _savingsController.ConnectUI(savingsRoot);
            }

            // --- 멤버십 팝업 패널 ---
            if (_membershipUXML != null) {
                VisualElement membershipRoot = _membershipUXML.Instantiate();
                membershipRoot.SetAbsolutePosition();
                membershipRoot.style.display = DisplayStyle.None;
                membershipRoot.AddToClassList("popup-fade-base");
                _blackMarketRoot.Add(membershipRoot);

                _membershipController.ConnectUI(membershipRoot);
            }
        }

        // ----- 시작 기능 -----
        /// <summary>
        /// 블랙마켓 노드를 클릭하면 해당 함수가 작동될 수 있도록 해야 함
        /// </summary>
        public void StartMarket()
        {
            // 설정된 언어로 번역
            SettingPanelController.Instance.RefreshTranslation();
            _purchaseController.RefreshTranslation();
            _savingsController.RefreshTranslation();
            _membershipController.RefreshTranslation();

            // 오디오 이벤트
            AudioController.Instance.PlayBGM(AudioKeys.BGM_BLACKMARKET);
            AudioController.Instance.PlaySFX(AudioKeys.SFX_DOOR);
            AudioController.Instance.PlaySFX(AudioKeys.SFX_DOORBELL);

            // 입장 시 대사, VO 출력
            _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.ENTER, _cwController);

            _blackMarketRoot.style.visibility = Visibility.Visible;
        }
         
        // ----- 아이템 슬롯 기능 -----
        /// <summary>
        /// 저축액을 바탕으로 저축 효과를 계산 및 적용함
        /// </summary>
        /// <param name="savings">현 저축액</param>
        private void CalculateSavingsEffect(long savings)
        {
            CurSavingsLevel = 0;
            _additiveSlotCount = 0;
            _additiveRefreshingCount = 0;

            // 저축액이 기준치를 넘을 때마다 별도의 효과 추가
            for (int i = 0; i < savingsThresholds.Count; i++) {
                if (savings >= savingsThresholds[i].value) {
                    _additiveSlotCount = savingsThresholds[i].additiveSlots;
                    _additiveRefreshingCount = savingsThresholds[i].additiveRefreshing - _usedRefreshingCount;
                    CurSavingsLevel = i;
                }
                else { break; }
            }

            _curRefreshState = (_additiveRefreshingCount > 0) ? RefreshState.Active : RefreshState.Disabled;
            _btnController?.UpdateSavingsBtnText(CurSavingsLevel);
        }

        /// <summary>
        /// 블랙마켓 입장 시, 멤버십 Level을 바탕으로 희귀도별 아이템 등장 확률을 세팅함
        /// </summary>
        /// <param name="startMembershipLevel">초기 멤버십 확률</param>
        private void CalculateMembershipLevel(int startMembershipLevel)
        {
            CurMembershipLevel = Mathf.Clamp(startMembershipLevel, 0, rarityProbabilities.Count - 1);
            _curProbability = rarityProbabilities[CurMembershipLevel];
        }

        /// <summary>
        /// 블랙마켓 아이템을 새로 등기함
        /// </summary>
        private void RegistMarketItems()
        {
            AddItemSlot(); // 저축하면 새로고침 시 슬롯 추가
            List<System.Type> curSlotTypes = GenerateSlotTypes(TotalSlotCount); // 아이템 종류 생성

            _itemsData.Clear();

            GenerateItems(curSlotTypes); // 아이템 결정/데이터 호출
            BindSlotUI();
        }

        /// <summary>
        /// 아이템 슬롯 추가
        /// </summary>
        private void AddItemSlot()
        {
            while (_slotControllers.Count < TotalSlotCount) {
                VisualElement slot = _itemSlotUXML.Instantiate();
                int newIdx = _slotControllers.Count;
                slot.name = $"ItemSlot_{newIdx}";
                _slotContainer.Add(slot);

                ItemSlotController newController = new ItemSlotController(slot, newIdx);
                newController.OnSlotClicked += HandleSlotClicked; // 이벤트 연결
                _slotControllers.Add(newController);
            }
        }

        /// <summary>
        /// 아이템 슬롯 삭제
        /// </summary>
        private void RemoveItemSlot()
        {
            while (_slotControllers.Count > TotalSlotCount) {
                int lastIdx = _slotControllers.Count - 1;

                if (_itemsData.Count > lastIdx) {
                    _itemsData.RemoveAt(lastIdx);
                }

                ItemSlotController controllerToRemove = _slotControllers[lastIdx];
                controllerToRemove.OnSlotClicked -= HandleSlotClicked;
                controllerToRemove.Dispose();
                _slotContainer.Remove(controllerToRemove.RootElement);
                _slotControllers.RemoveAt(lastIdx);
            }
        }

        /// <summary>
        /// System.Type을 활용하여 요구한 슬롯 개수에 맞춰 랜덤하게 아이템 타입 리스트 생성
        /// </summary>
        // 카드 3장, 오파츠 2개를 보장함
        private List<System.Type> GenerateSlotTypes(int totalSlotCount)
        {
            List<System.Type> slotTypes = new List<System.Type>();

            // 최소 수량 보장
            slotTypes.Add(typeof(CardData));
            slotTypes.Add(typeof(CardData));
            slotTypes.Add(typeof(CardData));
            slotTypes.Add(typeof(OpartsData));
            slotTypes.Add(typeof(OpartsData));

            // 나머지 랜덤 할당
            for (int i = 5; i < totalSlotCount; i++) {
                slotTypes.Add(Random.Range(0, 2) == 0 ? typeof(CardData) : typeof(OpartsData));
            }

            // 무작위 배치
            for (int i = 0; i < slotTypes.Count; i++) {
                int tempIdx = Random.Range(i, slotTypes.Count);
                System.Type temp = slotTypes[i];
                slotTypes[i] = slotTypes[tempIdx];
                slotTypes[tempIdx] = temp;
            }

            return slotTypes;
        }

        /// <summary>
        /// 아이템 생성/저장
        /// </summary>
        /// <param name="slotTypes">요구되는 아이템 타입 (오파츠 or 카드)</param>
        private void GenerateItems(List<System.Type> slotTypes)
        {
            for (int i = 0; i < TotalSlotCount; i++) {
                ItemRarity rarity = GetRandomRarity(); // 현재 등급에 맞는 희귀도 결정
                System.Type requiredType = slotTypes[i];
                var itemData = _itemDatabase.GetRandomItem(requiredType, rarity); // 요구 타입 및 희귀도를 만족하는 아이템 중 랜덤으로 가져옴

                if (itemData == null) {
                    Debug.LogWarning($"아이템을 불러올 수 없습니다\n\n" +
                        $"[ 조건 ]\n" +
                        $"  아이템 타입 : {requiredType},\n" +
                        $"  희귀도 : {rarity}");
                    continue;
                }
                _itemsData.Add(itemData); // 해당 희귀도의 아이템 로드
            }
        }

        /// <summary>
        /// 현재 멤버십 등급에 맞춰 랜덤한 희귀도를 반환하는 함수
        /// </summary>
        private ItemRarity GetRandomRarity()
        {
            int totalWeight = _curProbability.GetTotalWeight();
            int randomValue = Random.Range(0, totalWeight);

            // 가중치 랜덤
            if (randomValue < _curProbability.CommonWeight)
                return ItemRarity.Common;

            randomValue -= _curProbability.CommonWeight;
            if (randomValue < _curProbability.RareWeight)
                return ItemRarity.Rare;

            randomValue -= _curProbability.RareWeight;
            if (randomValue < _curProbability.UniqueWeight)
                return ItemRarity.Unique;

            return ItemRarity.Legendary;
        }

        /// <summary>
        /// UI에 정보를 기재함
        /// </summary>
        private void BindSlotUI()
        {
            for (int i = 0; i < TotalSlotCount; i++) {
                ItemData itemData = _itemsData[i];
                _slotControllers[i].SetItemData(itemData);
            }
        }

        /// <summary>
        /// 불특정 슬롯 클릭 시 호출
        /// </summary>
        private void HandleSlotClicked(int slotIdx, ItemData itemData)
        {
            _purchaseController.OpenPopup(slotIdx, itemData);
            AudioController.Instance.PlaySFX(AudioKeys.SFX_OPENSLOT);
        }

        // ----- 새로고침 기능 -----
        /// <summary>
        /// 새로고침 로직
        /// </summary>
        public void HandleRefreshRequest()
        {
            switch (_curRefreshState) {
                case RefreshState.Disabled:
                    _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.REFRESH_DISABLED, _cwController);
                    break;

                case RefreshState.Locked:
                    _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.REFRESH_LOCKED, _cwController);
                    break;

                case RefreshState.Active:
                    _additiveRefreshingCount--;
                    _usedRefreshingCount++;
                    if (_additiveRefreshingCount <= 0) {
                        _curRefreshState = RefreshState.Locked;
                    }

                    UpdateRefreshButtonUI();

                    _prevItemIDs.Clear();
                    foreach (var item in _itemsData) {
                        if (item== null) continue;
                        _prevItemIDs.Add(item.ID);
                    }
                    _itemDatabase.SetExcludeItemIDs(_prevItemIDs);

                    RegistMarketItems();

                    _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.REFRESH_SUCCESS, _cwController);
                    break;
            }
        }

        /// <summary>
        /// 새로고침 시, 새로고침 버튼의 UI 업데이트
        /// </summary>
        private void UpdateRefreshButtonUI()
        {
            if (_btnController == null) return;

            _btnController.UpdateRefreshBtnText(CurRemainingRefreshCount);
            _btnController.UpdateRefreshBtnState(_curRefreshState);
        }

        // ----- 구매 기능 -----
        // PurchaseController.cs 참고
        /// <summary>
        /// PurchaseController에서 구매 버튼을 눌렀을 때 실행되는 실제 거래 로직
        /// </summary>
        public void ProcessPurchase(int slotIdx, ItemData itemData)
        {
            var ddoManager = DontDestroyOnLoadManager.Instance;
            if (ddoManager == null || ddoManager.ResourceManager == null) return;
            // if (_localUserData == null) return;

            if (CurrentGold >= itemData.Price) {
                CurrentGold -= itemData.Price;
                // _localUserData.Gold = currentGold;
                _coinValue.text = CurrentGold.ToString("N0");

                if (itemData is CardData card) {
                    Debug.Log($"카드 획득 처리: {card.Name}");
                    // _localUserData.AddCard(card.ID);
                }
                else if (itemData is OpartsData oparts) {
                    Debug.Log($"오파츠 획득 처리: {oparts.Name}");
                    // _localUserData.AddOparts(oparts.ID);
                }

                // 상품 슬롯 잠금 상태로 변경
                _itemsData[slotIdx] = null;
                _slotControllers[slotIdx].LockSlotUI();

                AudioController.Instance.PlaySFX(AudioKeys.SFX_COIN);
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.BUY_SUCCESS, _cwController);

                _purchaseController.ClosePopup();
            }
            else {
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.NOT_ENOUGH_MONEY, _cwController);
            }
        }

        // ----- 저축 기능 -----
        // SavingsController.cs 참고
        /// <summary>
        ///슬라이더 값을 참조하여 입금 처리
        /// </summary>
        public void RequestDeposit(int amount)
        {
            if (CurrentGold >= amount) {
                CurrentGold -= amount;
                Savings += amount;

                RefreshSavingsState();
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.DEPOSIT_SUCCESS, _cwController);
            }
            else {
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.NOT_ENOUGH_MONEY, _cwController);
            }
        }

        /// <summary>
        /// 슬라이더 값을 참조하여 인출 처리
        /// </summary>
        public void RequestWithdraw(int amount)
        {
            if (Savings >= amount) {
                long expectedSavings = Savings - amount;
                int expectedAdditiveSlots = 0;
                int expectedTotalRefreshing = 0;

                for (int i = 0; i < savingsThresholds.Count; i++) {
                    if (expectedSavings >= savingsThresholds[i].value) {
                        expectedAdditiveSlots = savingsThresholds[i].additiveSlots;
                        expectedTotalRefreshing = savingsThresholds[i].additiveRefreshing;
                    }
                    else break;
                }

                int expectedTotalSlots = _baseSlotCount + expectedAdditiveSlots;
                int purchasedCount = 0;
                foreach (var item in _itemsData) {
                    if (item == null) purchasedCount++;
                }

                bool isRefreshingValid = expectedTotalRefreshing >= _usedRefreshingCount;
                bool isSlotValid = !(expectedTotalSlots < TotalSlotCount && purchasedCount > 0);

                // 인출로 인해 새로고침 개수가 줄어들 예정인데, 사용해버려서 교환 불가하다면 허용하지 않음
                // 인출로 인해 슬롯 개수가 줄어들 예정인데, 이미 하나라도 샀다면 허용하지 않음
                if (isRefreshingValid && isSlotValid) {
                    Savings -= amount;
                    CurrentGold += amount;

                    RefreshSavingsState();

                    // 인출하면 슬롯 감소 즉시 반영
                    RemoveItemSlot();

                    _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.WITHDRAW_SUCCESS, _cwController);
                }
                else {
                    _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.WITHDRAW_DENIED, _cwController);
                }
            }
            else {
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.NOT_ENOUGH_MONEY, _cwController);
            }
        }

        /// <summary>
        /// 입출금 발생 시 데이터 및 UI 갱신
        /// </summary>
        private void RefreshSavingsState()
        {
            // 골드 UI 갱신
            if (_coinValue != null) {
                _coinValue.text = CurrentGold.ToString("N0");
            }

            // 내부 데이터 재계산
            CalculateSavingsEffect(Savings);
            UpdateRefreshButtonUI();

            _savingsController.UpdateUI(CurSavingsLevel, Savings);
        }

        // ----- 멤버십 기능 -----
        // MembershipController.cs 참고
        /// <summary>
        /// 멤버십 팝업 열기 요청
        /// </summary>
        public void OpenMembershipPopup()
        {
            // 현재 레벨과 비용을 계산해서 컨트롤러에 전달
            bool isMaxLevel = CurMembershipLevel >= membershipThresholds.Count - 1;
            int nextCost = isMaxLevel ? -1 : membershipThresholds[CurMembershipLevel + 1];

            _membershipController.OpenPopup(CurMembershipLevel, nextCost);
        }

        /// <summary>
        /// 멤버십 다음 단계 업그레이드 요청
        /// </summary>
        public void RequestUpgradeMembership()
        {
            if (CurMembershipLevel >= membershipThresholds.Count - 1) {
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.MEMBERSHIP_MAX, _cwController);
                return;
            }

            int nextLevel = CurMembershipLevel + 1;
            int requiredCost = membershipThresholds[nextLevel];

            if (CurrentGold >= requiredCost) {
                // 골드 차감 및 레벨업
                CurrentGold -= requiredCost;
                // if (_localUserData != null) _localUserData.Gold = CurrentGold;
                CurMembershipLevel = nextLevel;
                _curProbability = rarityProbabilities[CurMembershipLevel];

                // UI 갱신
                _btnController?.UpdateMembershipBtnText(CurMembershipLevel);

                bool isMaxLevel = CurMembershipLevel >= membershipThresholds.Count - 1;
                int nextCost = isMaxLevel ? -1 : membershipThresholds[CurMembershipLevel + 1];
                _membershipController.UpdateUI(CurMembershipLevel, nextCost);

                AudioController.Instance.PlaySFX(AudioKeys.SFX_COIN);
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.MEMBERSHIP_UP, _cwController);
            }
            else {
                _dmBlackMarket.PlayDialogue(DialogueKeys.BlackMarket.NOT_ENOUGH_MONEY, _cwController);
            }
        }

        // --- 번역 기능 ---
        // LocalizationManager.cs 참고
        /// <summary>
        /// 언어가 변경되었을 때 현재 활성화된 화면의 번역 텍스트를 즉시 갱신
        /// </summary>
        public void RefreshLocalization()
        {
            _coinTitleLabel.text = LocalizationManager.GetText(UIKeys.BlackMarket.TOTAL_COIN_TITLE);

            // 아이템 슬롯들 이름 갱신
            if (_slotControllers != null) {
                foreach (var slot in _slotControllers) {
                    slot.RefreshTranslation();
                }
            }

            // 만약 상세 팝업이 켜져 있다면 팝업 이름 갱신
            SettingPanelController.Instance.RefreshTranslation();
            _purchaseController.RefreshTranslation();
            _savingsController.RefreshTranslation();
            _membershipController.RefreshTranslation();
        }
    }
}