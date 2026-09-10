using System.Collections.Generic;
using UnityEngine;
using System;

namespace Item
{
    #region ItemData
    public enum ItemRarity
    {
        Common,
        Rare,
        Unique,
        Legendary, 
        None, 
    }

    public enum ItemType
    {
        None, 
        Oparts, 
        Card, 
    }

    [System.Serializable]
    public class ItemData
    {
        public int ID;
        public string Name;
        public ItemRarity Rarity;
        public int Price;
        public string Info;

        public Sprite Image;

        public int OffsetX;
        public int OffsetY;
        public float Scale;
    }

    public class CardData : ItemData
    {
        /// <summary>
        /// 카드 소유자의 ID
        /// </summary>
        public int OwnerCharacterID;
    }

    public class OpartsData : ItemData
    {
        /// <summary>
        /// 소지 중복 여부
        /// </summary>
        public bool CanAppearShop;
    }
    #endregion

    class ItemDatabase
    {
        // --- 멤버 변수 ---
        private ItemIconDatabase _iconDB;

        // --- 리스트 ---
        private List<ItemData> _masterItemDB = new List<ItemData>();
        private List<int> _excludeItemIDs = new List<int>();

        // FindAll은 호출마다 List 객체를 찍어내므로 GC를 부름
        // 따라서 미리 분류하도록 최적화
        private Dictionary<ItemRarity, List<CardData>> _cardPoolByRarity;
        private Dictionary<ItemRarity, List<OpartsData>> _opartsPoolByRarity;

        // 희귀도가 고갈되면 전체 탐색을 진행할 재사용 리스트
        private List<CardData> _fallbackCardPool;
        private List<OpartsData> _fallbackOpartsPool;

        // --- 상수 선언 ---
        const int DUMMY_ITEM_ID = 9999;

        /// <summary>
        /// 아이템 정보를 파싱하며 이미지 데이터를 연결함
        /// </summary>
        /// <param name="iconDB"></param>
        public void Initialize(ItemIconDatabase iconDB)
        {
            _iconDB = iconDB;
            _cardPoolByRarity = new Dictionary<ItemRarity, List<CardData>>();
            _opartsPoolByRarity = new Dictionary<ItemRarity, List<OpartsData>>();
            _fallbackCardPool = new List<CardData>();
            _fallbackOpartsPool = new List<OpartsData>();

            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity))) {
                _cardPoolByRarity[rarity] = new List<CardData>();
                _opartsPoolByRarity[rarity] = new List<OpartsData>();
            }

            LoadDataFromCSVReader();
            MapAllImagesToMasterDB();
        }

        /// <summary>
        /// CSVReader >> 블랙마켓에서 필요한 아이템 데이터를 파싱하고 마스터 Item DB를 채움
        /// </summary>
        private void LoadDataFromCSVReader()
        {
            _masterItemDB.Clear();

            List<Dictionary<string, object>> itemDataList = CSVReader.Read("ItemSlotTable");
            if (itemDataList == null || itemDataList.Count == 0) {
                Debug.LogError("[ItemDatabase] ItemTable.csv 데이터를 불러올 수 없습니다");
                return;
            }

            foreach (var row in itemDataList) {
                if (!row.ContainsKey("ID") || !row.ContainsKey("Type")) continue;

                // 공통 속성 파싱
                int id = Convert.ToInt32(row["ID"]);
                string typeStr = row["Type"].ToString();

                string name = row.ContainsKey("Name") ? row["Name"].ToString() : "";
                ItemRarity rarity = (System.Enum.TryParse<ItemRarity>(row["Rarity"].ToString(), true, out ItemRarity parsedRarity)) ? parsedRarity : ItemRarity.None;
                int price = row.ContainsKey("Price") ? Convert.ToInt32(row["Price"]) : 0;
                string info = row.ContainsKey("Info") ? row["Info"].ToString() : "";
                int offsetX = row.ContainsKey("OffsetX") ? Convert.ToInt32(row["OffsetX"]) : 0;
                int offsetY = row.ContainsKey("OffsetY") ? Convert.ToInt32(row["OffsetY"]) : 0;
                float scale = row.ContainsKey("Scale") ? Convert.ToSingle(row["Scale"]) : 1f;

                ItemData newItem = null;

                if (typeStr == "Card") {
                    newItem = new CardData {
                        ID = id,
                        Name = name,
                        Rarity = rarity,
                        Price = price,
                        Info = info,
                        OffsetX = offsetX,
                        OffsetY = offsetY,
                        Scale = scale,

                        OwnerCharacterID = row.ContainsKey("OwnerCharacterID") ? Convert.ToInt32(row["OwnerCharacterID"]) : 0
                    };
                }
                else if (typeStr == "Oparts") {
                    newItem = new OpartsData {
                        ID = id,
                        Name = name,
                        Rarity = rarity,
                        Price = price,
                        Info = info,
                        OffsetX = offsetX,
                        OffsetY = offsetY,
                        Scale = scale,

                        CanAppearShop = true
                    };
                }
                else {
                    Debug.LogWarning($"[ItemDatabase] 알 수 없는 아이템 타입입니다: {typeStr} (ID: {id})");
                    continue;
                }
                _masterItemDB.Add(newItem);
            }
            Debug.Log($"[ItemDatabase] CSVReader 파싱 완료 (총 {_masterItemDB.Count}개 아이템 로드됨)");
        }

        /// <summary>
        ///  SO에 저장된 이미지를 ID를 통해 매핑
        /// </summary>
        private void MapAllImagesToMasterDB()
        {
            if (_iconDB == null) {
                Debug.LogError("[ItemDatabase] ItemIconDatabase가 연결되지 않았습니다");
                return;
            }

            foreach (var item in _masterItemDB) {
                item.Image = _iconDB.GetIcon(item.ID);
            }

            Debug.Log("[ItemDatabase] 모든 아이템에 이미지 매핑 완료");
        }

        /// <summary>
        /// 이번 블랙마켓 씬에 등장할 수 있는 전체 아이템 풀 세팅
        /// </summary>
        public void CreateItemPool(List<int> partyCharacterIDs, List<int> ownedOpartsIDs)
        {
            foreach (var list in _cardPoolByRarity.Values) list.Clear();
            foreach (var list in _opartsPoolByRarity.Values) list.Clear();

            foreach (var item in _masterItemDB) {
                // 새로고침 시, 직전 아이템들은 제외됨
                if (_excludeItemIDs != null && _excludeItemIDs.Contains(item.ID)) { continue; }

                // 파티 내 캐릭터의 ID와 일치하는 카드만 추가
                if (item is CardData card 
                && partyCharacterIDs.Contains(card.OwnerCharacterID)) {
                    _cardPoolByRarity[card.Rarity].Add(card);
                }
                // 상점 등장 가능한 오파츠이면서, 현재 보유 중이 아닌 경우만 추가
                else if (item is OpartsData oparts 
                && oparts.CanAppearShop 
                && !ownedOpartsIDs.Contains(oparts.ID)) {
                    _opartsPoolByRarity[oparts.Rarity].Add(oparts);
                }
            }
        }

        /// <summary>
        /// 플레이어가 블랙마켓을 통해 얻지 말아야 할 아이템 ID 값을 전달함
        /// </summary>
        /// <param name="ids"></param>
        public void SetExcludeItemIDs(List<int> ids)
        {
            _excludeItemIDs.Clear();
            if (ids == null) { return; }

            _excludeItemIDs.AddRange(ids);
        }

        /// <summary>
        /// 요청한 타입에 맞춰 해당 풀에서 아이템 추출
        /// </summary>
        /// <param name="type">요청한 아이템 타입</param>
        /// <param name="rarity">희귀도</param>
        /// <returns>아이템 데이터</returns>
        public ItemData GetRandomItem(System.Type type, ItemRarity rarity)
        {
            if (type == typeof(CardData)) {
                return ExtractItem(_cardPoolByRarity, _fallbackCardPool, rarity, type);
            }
            else if (type == typeof(OpartsData)) {
                return ExtractItem(_opartsPoolByRarity, _fallbackOpartsPool, rarity, type);
            }

            Debug.LogError($"알 수 없는 타입 요청 : {type}");
            return null;
        }

        /// <summary>
        /// 생성 가능한 아이템만 필터링하여 랜덤 반환
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool"></param>
        /// <param name="rarity"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private ItemData ExtractItem<T>(Dictionary<ItemRarity, List<T>> pool, List<T> fallbackPool,  ItemRarity rarity, System.Type type) where T : ItemData
        {
            // 해당 희귀도의 아이템 필터링
            List<T> filteredItems = pool[rarity];

            // 해당 희귀도에 아이템이 남아있는 경우 정상 추출
            if (filteredItems.Count > 0) {
                int ranIdx = UnityEngine.Random.Range(0, filteredItems.Count);
                T selectedItem = filteredItems[ranIdx];
                filteredItems.RemoveAt(ranIdx); // 중복 등장 방지

                return selectedItem;
            }

            // 해당 희귀도에 아이템이 고갈된 경우, 희귀도 상관없이 남은 아이템 중 하나를 반환
            fallbackPool.Clear();
            foreach (var list in pool.Values) {
                fallbackPool.AddRange(list);
            }

            // 풀이 완전 고갈 시 더미 반환
            if (fallbackPool.Count == 0) {
                Debug.LogWarning($"[{type.Name}] 남은 아이템이 없어 더미 아이템으로 대체됩니다\n");
                return GetDummyItem(type, rarity); 
            }

            // 희귀도를 무시하고 남은 아이템 중 무작위 추출
            int fallbackIdx = UnityEngine.Random.Range(0, fallbackPool.Count);
            T fallbackItem = fallbackPool[fallbackIdx];
            pool[fallbackItem.Rarity].Remove(fallbackItem); // 중복 등장 방지

            return fallbackItem;
        }

        private ItemData GetDummyItem(System.Type type, ItemRarity rarity)
        {
            int ranPrice = UnityEngine.Random.Range(100, 80000);
            int dummyID = DUMMY_ITEM_ID;

            if (type == typeof(CardData)) {
                return new CardData {
                    ID = dummyID,
                    Name = "품절된 카드",
                    Rarity = rarity,
                    Price = ranPrice,
                    Image = _iconDB != null ? _iconDB.GetIcon(dummyID) : null,
                    OffsetX = 0,
                    OffsetY = 0,
                    Scale = 1f,
                };
            }
            return new OpartsData {
                ID = dummyID,
                Name = "품절된 오파츠",
                Rarity = rarity,
                Price = ranPrice,
                Image = _iconDB != null ? _iconDB.GetIcon(dummyID) : null,
                OffsetX = 0,
                OffsetY = 0,
                Scale = 1f,
            };
        }
    }
}