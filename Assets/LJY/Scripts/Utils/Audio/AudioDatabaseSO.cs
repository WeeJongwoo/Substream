using System.Collections.Generic;
using UnityEngine;

namespace Audio.Data
{
    [CreateAssetMenu(fileName = "NewAudioDatabase", menuName = "Audio/AudioDB")]
    public class AudioDatabaseSO : ScriptableObject
    {
        [System.Serializable]
        public struct AudioData
        {
            public string ID;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
        }

        [Tooltip("사용하는 모든 음성을 등록하세요")]
        public List<AudioData> audioList = new List<AudioData>();

        // -- 런타임 변수 --
        private Dictionary<string, AudioData> _audioDict;

        public void Initialize()
        {
            if (_audioDict != null) return;

            _audioDict = new Dictionary<string, AudioData>(audioList.Count);

            foreach (var audio in audioList) {
                if (!_audioDict.ContainsKey(audio.ID)) {
                    _audioDict.Add(audio.ID, audio);
                }
            }
        }

        /// <summary>
        /// ID를 통해 오디오 데이터를 반환
        /// </summary>
        public bool TryGetAudioData(string id, out AudioData data)
        {
            if (_audioDict == null) {
                Debug.LogError("[AudioDatabaseSO] 초기화되지 않았습니다. Awake에서 Initialize()를 호출하세요.");
                data = default;
                return false;
            }

            if (!_audioDict.TryGetValue(id, out data)) {
                Debug.LogWarning($"[AudioDatabaseSO] 등록되지 않은 음성 ID입니다 : {id}");
                return false;
            }

            return true;
        }
    }
}
