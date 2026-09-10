using Audio.Controller;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [System.Serializable]
    public class VNLineData
    {
        public string SpeakerNameKey;
        public string TextKey;
        public string AudioKey;

        // 다중 스탠딩
        public List<string> PortraitKeys = new List<string>();
        public List<int> FocusSlotIndices = new List<int>();
    }

    /// <summary>
    /// 순차적인 비주얼 노벨 스크립트 진행을 관리하는 매니저
    /// </summary>
    public class VisualNovelManager : MonoBehaviour
    {
        public static VisualNovelManager Instance { get; private set; }

        [SerializeField] private VisualNovelController _uiController;

        private List<VNLineData> _currentEpisodeLines = new List<VNLineData>(); // 현재 재생 중인 에피소드의 전체 대사 리스트
        private int _currentIndex = 0; // 현재 읽고 있는 대사의 인덱스
        private Dictionary<string, Sprite> _episodeSprites = new Dictionary<string, Sprite>();

        public bool IsPlaying { get; private set; } // 비주얼 노벨이 진행 중인지 여부
        public int CurIdx => _currentIndex;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

        }

        /// <summary>
        /// 특정 에피소드를 시작할 때 호출
        /// </summary>
        public void StartEpisode(string episodeID)
        {
            IsPlaying = true;
            _currentIndex = 0;
            _episodeSprites.Clear();

            _currentEpisodeLines = LoadEpisodeFromDB(episodeID);

            if (_currentEpisodeLines.Count > 0) {
                PreloadSprites();
                _uiController.ShowUI();
                PlayCurrentLine();
            }
            else {
                Debug.LogWarning($"[{episodeID}] 에피소드 데이터가 없습니다.");
                EndEpisode();
            }
        }

        /// <summary>
        /// 현재 에피소드에 필요한 모든 일러스트를 한 번만 로드하여 딕셔너리에 저장
        /// </summary>
        private void PreloadSprites()
        {
            foreach (var line in _currentEpisodeLines) {
                foreach (string portraitKey in line.PortraitKeys) {
                    // 키가 비어있지 않고, 아직 딕셔너리에 없는 이미지라면 로드
                    if (!string.IsNullOrEmpty(portraitKey) && !_episodeSprites.ContainsKey(portraitKey)) {
                        Sprite loadedSprite = Resources.Load<Sprite>($"Portraits/{portraitKey}");

                        if (loadedSprite != null) {
                            _episodeSprites.Add(portraitKey, loadedSprite);
                        }
                        else {
                            Debug.LogWarning($"[VisualNovelManager] 일러스트 로드 실패 : Portraits/{portraitKey}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 다음 대사로 넘어가기
        /// </summary>
        public void NextLine()
        {
            if (!IsPlaying) return;

            _currentIndex++;

            if (_currentIndex < _currentEpisodeLines.Count) {
                PlayCurrentLine();
            }
            else {
                EndEpisode(); // 대사가 끝남
            }
        }

        /// <summary>
        /// 현재 인덱스의 대사와 연출을 실행
        /// </summary>
        private void PlayCurrentLine()
        {
            VNLineData currentData = _currentEpisodeLines[_currentIndex];

            // 현재 라인에 필요한 스프라이트 리스트 생성
            List<Sprite> currentSprites = new List<Sprite>();
            foreach (string key in currentData.PortraitKeys) {
                Sprite sprite = null;
                if (!string.IsNullOrEmpty(key) && _episodeSprites.TryGetValue(key, out sprite)) {
                    currentSprites.Add(sprite);
                }
                else {
                    // 키가 비어있거나 이미지를 찾지 못한 경우 null을 추가하여 빈 슬롯 처리
                    currentSprites.Add(null);
                }
            }

            // UI 컨트롤러에 다중 캐릭터 리스트와 포커스 인덱스 전달
            _uiController.UpdateLine(currentData.SpeakerNameKey, currentData.TextKey, currentSprites, currentData.FocusSlotIndices);

            // 오디오 재생
            if (!string.IsNullOrEmpty(currentData.AudioKey)) {
                AudioController.Instance.PlayVO(currentData.AudioKey);
            }

            // 기타 연출 처리 로직
        }

        /// <summary>
        /// 에피소드 종료
        /// </summary>
        public void EndEpisode()
        {
            IsPlaying = false;
            _currentIndex = 0;
            _currentEpisodeLines.Clear();
            _episodeSprites.Clear();

            _uiController.OffAutoRead();
            _uiController.HideUI();

            Debug.Log("[VisualNovelManager] 에피소드 재생 종료");
        }

        /// <summary>
        /// 에피소드 로드
        /// </summary>
        private List<VNLineData> LoadEpisodeFromDB(string episodeID)
        {
            List<VNLineData> loadedLines = new List<VNLineData>();

            List<Dictionary<string, object>> dataList = CSVReader.Read("VisualNovelTable");
            if (dataList == null || dataList.Count == 0) {
                Debug.LogError("[ VisualNovelManager ] VisualNovelTable.csv 데이터를 불러올 수 없습니다");
                return loadedLines;
            }

            foreach (var row in dataList) {
                string rowEpisodeID = row.ContainsKey("EpisodeID") ? row["EpisodeID"].ToString() : "";
                if (rowEpisodeID != episodeID) continue; // 요청한 에피소드 ID와 다르면 건너뜀

                VNLineData lineData = new VNLineData();

                // 기본 텍스트 및 오디오 데이터 할당
                lineData.SpeakerNameKey = row.ContainsKey("SpeakerNameKey") && row["SpeakerNameKey"] != null ? row["SpeakerNameKey"].ToString() : "";
                lineData.TextKey = row.ContainsKey("TextKey") && row["TextKey"] != null ? row["TextKey"].ToString() : "";
                lineData.AudioKey = row.ContainsKey("AudioKey") && row["AudioKey"] != null ? row["AudioKey"].ToString() : "";

                // FocusSlotIndices 파싱
                if (row.ContainsKey("FocusSlotIndex") && row["FocusSlotIndex"] != null) {
                    string rawData = row["FocusSlotIndex"].ToString().Trim();

                    if (string.IsNullOrEmpty(rawData)) { continue; }

                    string[] slots = rawData.Split(',');
                    foreach (string slot in slots) {
                        if (int.TryParse(slot.Trim(), out int parsedIndex)) {
                            lineData.FocusSlotIndices.Add(parsedIndex);
                        }
                    }

                    if (lineData.FocusSlotIndices.Count <= 0) {
                        lineData.FocusSlotIndices.Add(-1); // 공백일 경우 -1로 예외 처리
                    }
                }

                if (row.ContainsKey("PortraitKeys") && row["PortraitKeys"] != null) {
                    string rawPortraitKeys = row["PortraitKeys"].ToString();
                    if (!string.IsNullOrEmpty(rawPortraitKeys)) {
                        // 쉼표 단위로 분리하고 공백 제거 후 리스트에 추가
                        string[] keys = rawPortraitKeys.Split(',');
                        foreach (string key in keys) {
                            lineData.PortraitKeys.Add(key.Trim());
                        }
                    }
                }

                // 비정상 데이터 거르기
                if (string.IsNullOrEmpty(lineData.TextKey)) {
                    Debug.LogWarning($"[VisualNovelManager] {episodeID} 에피소드 중 TextKey가 누락된 행이 있습니다");
                    continue;
                }

                loadedLines.Add(lineData);
            }

            Debug.Log($"[VisualNovelManager] 에피소드 '{episodeID}' 로드 완료. (총 {loadedLines.Count}줄)");
            return loadedLines;
        }

        /// <summary>
        /// Log 기능용
        /// </summary>
        /// <param name="index">현재까지 읽은 대사 index 번호</param>
        /// <returns></returns>
        public VNLineData GetLineData(int index)
        {
            if (index >= 0 && index < _currentEpisodeLines.Count)
                return _currentEpisodeLines[index];
            return null;
        }
    }
}