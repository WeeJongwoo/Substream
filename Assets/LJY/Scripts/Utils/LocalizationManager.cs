using System;
using System.Collections.Generic;
using UnityEngine;

namespace Localization
{
    // 지원할 언어 목록
    public enum LanguageType
    {
        KR,
        EN,
        JP, 
        CN, 
    }

    /// <summary>
    /// DialogueManager에서 설정한 캐릭터의 대사를 전부 가져옴
    /// </summary>
    public static class LocalizationManager
    {
        // 현재 게임에 설정된 언어
        public static LanguageType CurrentLanguage = LanguageType.KR;

        // Key : String Key (예 : BM_Enter_01)
        // Value : Dictionary<언어, 실제 텍스트>
        private static Dictionary<string, Dictionary<LanguageType, string>> _localDB = new Dictionary<string, Dictionary<LanguageType, string>>();

        // 언어가 실시간으로 변경되었을 때 켜져있는 UI들이 텍스트를 갱신할 수 있도록 날리는 이벤트
        public static Action<LanguageType> OnLanguageChanged;

        private static readonly LanguageType[] _cachedLanguageKeys;
        private static readonly string[] _cachedLanguageStrings;

        static LocalizationManager()
        {
            _cachedLanguageKeys = (LanguageType[])Enum.GetValues(typeof(LanguageType));
            _cachedLanguageStrings = new string[_cachedLanguageKeys.Length];

            // Enum 값들을 미리 문자열로 변환하여 배열에 저장
            for (int i = 0; i < _cachedLanguageKeys.Length; i++) {
                _cachedLanguageStrings[i] = _cachedLanguageKeys[i].ToString();
            }
        }

        /// <summary>
        /// 게임 시작 시 한 번만 호출하여 다국어 DB를 로드함
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            LoadFromCSV();
        }

        private static void LoadFromCSV()
        {
            _localDB.Clear();

            List<Dictionary<string, object>> dataList = CSVReader.Read("LocalizationTable");
            if (dataList == null || dataList.Count == 0) {
                Debug.LogError("[LocalizationManager] LocalizationTable.csv 데이터를 불러올 수 없습니다");
                return;
            }

            foreach (var row in dataList) {
                string key = row.ContainsKey("TextKey") ? row["TextKey"].ToString() : "";
                if (string.IsNullOrEmpty(key)) continue;

                var textDict = new Dictionary<LanguageType, string>(_cachedLanguageKeys.Length);

                // Enum에 정의된 모든 언어를 순회하며 동적으로 딕셔너리에 매핑
                for (int i = 0; i < _cachedLanguageKeys.Length; i++) {
                    var lang = _cachedLanguageKeys[i];
                    string langStr = _cachedLanguageStrings[i];
                    string translatedText = row.ContainsKey(langStr) ? row[langStr].ToString() : "";

                    // 엑셀에서 줄바꿈을 \n으로 입력했을 경우, 실제 이스케이프 문자로 치환
                    translatedText = translatedText.Replace("<br>", "\n");
                    translatedText = translatedText.Replace("\\n", "\n");
                    textDict[lang] = translatedText;
                }

                _localDB[key] = textDict;
            }

            Debug.Log($"[LocalizationManager] 다국어 데이터 로드 완료 (총 {_localDB.Count}개 키)");
        }

        /// <summary>
        /// 텍스트 키를 넣으면 현재 설정된 언어의 번역본을 반환
        /// </summary>
        public static string GetText(string key)
        {
            if (string.IsNullOrEmpty(key) == false) {
                // 내용 누락
                if (_localDB.TryGetValue(key, out var textDict)) {
                    if (textDict.TryGetValue(CurrentLanguage, out string localizedText)) {
                        return string.IsNullOrEmpty(localizedText) ? $"[ No_Translation : {key} ]" : localizedText;
                    }
                }
            }
            // Key 값 누락
            Debug.LogWarning($"[ LocalizationManager ] 번역 키를 찾을 수 없습니다 : {key}");
            return $"[Missing:{key}]";
        }

        /// <summary>
        /// 인게임 설정창 등에서 언어를 변경할 때 호출
        /// </summary>
        public static void ChangeLanguage(LanguageType newLanguage)
        {
            if (CurrentLanguage == newLanguage) return;

            CurrentLanguage = newLanguage;
            Debug.Log($"[LocalizationManager] 시스템 언어 변경 : {CurrentLanguage}");

            // UI들에게 언어가 바뀌었다고 방송(Broadcast)
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }
    }
}