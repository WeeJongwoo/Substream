// README
// 패키징 형식의 게임 개발로 방향성을 수정
// 이에 블랙마켓에 사용될 아이템 이미지는 Resources를 거치지 않고 SO를 직접 거치도록 설계함
// 
// 위 방식으로 제작할 경우 얻는 이점은 아래와 같음
// 1. 안정성과 직관성 : 에디터에서 드래그 앤 드롭으로 이미지를 넣기 때문에, 눈으로 직접 확인이 가능하고 누락된 이미지를 바로 찾을 수 있음
// 2. 성능 보장 : 게임이 실행될 때 Resources.Load 같은 무거운 문자열 탐색 과정을 거치지 않고, 이미 메모리상에 직렬화 된 주소를 바로 가져오기 때문에 초기 로딩과 매핑 속도가 매우 빠름
// 3. 코드의 간결함 : 단 몇 줄만으로 전체 데이터베이스에 이미지를 씌울 수 있음
// 
// 만약 아래 두 가지 상황일 경우, 해당 스크립트를 삭제하고 Resources.Load를 통해 외부 DB에 파일 경로를 연동시킬 것을 권고함
// 1. 게임의 개발 방향성이 신규 캐릭터/장비가 추가/수정/삭제되는 라이브 서비스 형식으로 바뀔 경우
// 2. 아이템 개수가 수천~수만 개 단위로 넘어갈 경우

using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [System.Serializable]
    public struct ItemIconData
    {
        [Tooltip("아이템 이름(직관적으로 구분하기 위해 필요)")]
        public string Name;
        [Tooltip("아이템 ID")]
        public int ID;
        [Tooltip("아이템 이미지")]
        public Sprite Icon;
    }

    /// <summary>
    /// 아이템의 이미지를 제공함
    /// </summary>
    [CreateAssetMenu(fileName = "ItemIconDatabase", menuName = "BlackMarket/Item Icon Database")]
    public class ItemIconDatabase : ScriptableObject
    {
        [SerializeField] [Tooltip("아이템 ID와 이미지를 매핑")]
        private List<ItemIconData> _iconList = new List<ItemIconData>();

        private Dictionary<int, Sprite> _iconDict;

        /// <summary>
        /// 게임 시작 시 리스트를 딕셔너리로 변환하여 세팅함
        /// </summary>
        public void Initialize()
        {
            _iconDict = new Dictionary<int, Sprite>();
            foreach (var data in _iconList) {
                if (!_iconDict.ContainsKey(data.ID)) {
                    _iconDict.Add(data.ID, data.Icon);
                }
                else {
                    Debug.LogWarning($"[ItemIconDatabase] 중복된 아이템 ID가 존재합니다 : {data.ID}");
                }
            }
        }

        /// <summary>
        /// ID를 기반으로 Sprite를 반환함
        /// </summary>
        public Sprite GetIcon(int id)
        {
            if (_iconDict == null) Initialize();
            if (_iconDict.TryGetValue(id, out Sprite icon)) {
                return icon;
            }

            Debug.LogWarning($"[ItemIconDatabase] ID '{id}'에 해당하는 이미지를 찾을 수 없습니다");
            return null;
        }
    }
}