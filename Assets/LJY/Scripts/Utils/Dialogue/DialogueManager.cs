using Audio.Controller;
using Localization;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [System.Serializable]
    public class DialogueData
    {
        public string TextKey;
        public string AudioKey;
    }

    /// <summary>
    /// 원하는 캐릭터의 ID/상황 ID/대사 ID/오디오 ID를 관리/이용함
    /// </summary>
    public class DialogueManager
    {
        private string _npcID;
        private string _npcNameKey;

        private Dictionary<string, List<DialogueData>> _dialoguePool = new Dictionary<string, List<DialogueData>>();

        public void Initialize(string npcID, string npcNameKey)
        {
            _npcID = npcID;
            _npcNameKey = npcNameKey;

            LoadDialogueFromDB();
        }

        private void LoadDialogueFromDB()
        {
            _dialoguePool.Clear();

            List<Dictionary<string, object>> dataList = CSVReader.Read("DialogueTable");

            if (dataList == null || dataList.Count == 0) {
                Debug.LogError("[DialogueManager] DialogueTable.csv 데이터를 불러올 수 없습니다");
                return;
            }

            foreach (var row in dataList) {
                // 현재 초기화된 NPC_ID와 일치하는 행만 가져옴
                string rowNpcID = row.ContainsKey("NPC_ID") ? row["NPC_ID"].ToString() : "";
                if (rowNpcID != _npcID) continue;

                string situationKey = row.ContainsKey("Situation") ? row["Situation"].ToString() : "";
                string textKey = row.ContainsKey("TextKey") ? row["TextKey"].ToString() : "";
                string audioKey = row.ContainsKey("AudioKey") && row["AudioKey"] != null
                                  ? row["AudioKey"].ToString() : "";

                // 유효하지 않은 데이터 건너뛰기
                if (string.IsNullOrEmpty(situationKey) || string.IsNullOrEmpty(textKey)) {
                    Debug.LogWarning($"[DialogueManager] {_npcID}의 데이터 중 Situation 또는 TextKey가 누락된 행이 있습니다");
                    continue;
                }

                // 해당 Situation 키가 딕셔너리에 없으면 새로운 리스트 생성
                if (!_dialoguePool.ContainsKey(situationKey)) {
                    _dialoguePool[situationKey] = new List<DialogueData>();
                }

                // 데이터 객체 생성 후 상황별 리스트에 추가
                _dialoguePool[situationKey].Add(new DialogueData {
                    TextKey = textKey,
                    AudioKey = audioKey
                });
            }

            Debug.Log($"[DialogueManager] {_npcID} 대사 로드 완료 (총 {_dialoguePool.Count}개 상황 분류됨)");
        }

        public void PlayDialogue(string situationKey, CharacterWidgetController widgetController)
        {
            // 해당 캐릭터에게 제시된 상황에 맞는 대사가 있는지 검사
            if (!_dialoguePool.ContainsKey(situationKey) || _dialoguePool[situationKey].Count == 0) {
                Debug.LogWarning($"[{_npcID}] '{situationKey}' 상황에 대한 대사 데이터가 없습니다");
                return;
            }

            // 상황에 맞는 대사 리스트를 가져와서 그 중 랜덤으로 하나를 선택
            List<DialogueData> situationLines = _dialoguePool[situationKey];
            int randIdx = UnityEngine.Random.Range(0, situationLines.Count);
            DialogueData selectedData = situationLines[randIdx];

            // 사용자가 설정한 언어로 이름 및 대사를 가져옴
            string localizedName = LocalizationManager.GetText(_npcNameKey);
            string localizedLine = LocalizationManager.GetText(selectedData.TextKey);

            // UI 출력
            if (widgetController != null) {
                widgetController.ShowLine(localizedName, localizedLine);
            }

            // Voice Over 음성 재생
            if (!string.IsNullOrEmpty(selectedData.AudioKey)) {
                AudioController.Instance.PlayVO(selectedData.AudioKey);
            }
        }
    }
}