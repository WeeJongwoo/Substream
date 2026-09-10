using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CSVManager : MonoBehaviour
{
    private static CSVManager instance;

    private List<string> FILE_NAME = new List<string> { 
        "UseCard",
        "CardSkillTable",
        "CardTable",
        "CharacterTable",
        "LocalUser",
        "StatusEffect",
    };
    //CSV파일 파싱 직후 저장 공간
    private List<Dictionary<string, object>> CardSkillTable = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> CardTable = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> CharacterTable = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> LocalUser = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> StatusEffect = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> UseCard = new List<Dictionary<string, object>>();

    public void Initialize(Dictionary<string, DataScriptableObjects> database)
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }

        foreach (var fn in FILE_NAME)
        {
            object fnValue = GetFieldByString(fn);
            if(fnValue is List<Dictionary<string,object>>fnList)
            {
                fnList = CSVReader.Read(fn);
                SetFieldByString(fn, fnList);
                ConvertCSVToScriptableObject(fn, fnList, database);
            }
        }
    }

    private object GetFieldByString(string fieldName)
    {
        // Reflection을 사용하여 필드에 접근
        FieldInfo field = this.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

        if (field != null)
        {
            return field.GetValue(this); // 필드의 값을 반환
        }
        else
        {
            Console.WriteLine($"{fieldName} not found.");
            Debug.Log($"{fieldName} not found.");
            return null;
        }
    }

    private void SetFieldByString(string fieldName, object value)
    {
        FieldInfo field = this.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        field?.SetValue(this, value);
    }

    private bool ConvertCSVToScriptableObject(string dataName, List<Dictionary<string, object>> parsedData, Dictionary<string,DataScriptableObjects> datacontainers)
    {
        //불러올 data파일 + "Data" 문자열을 추가하여 타입을 찾음
        Type type = Type.GetType(dataName + "Data");
        if (type == null)
        {
            Debug.LogError($"Type not found: {dataName + "Data"}");
            return false;
        }
        //동적으로 생성한 객체의 속성들을 불러옴
        FieldInfo[] fieldes = type.GetFields();

        //불러온 속성들의 이름을 저장할 공간을 생성
        string[] fieldesName = new string[fieldes.Length];
        int counter = 0;

        //불러온 속성들의 이름들을 저장
        foreach (FieldInfo field in fieldes)
        {
            fieldesName[counter] = field.Name;
            counter++;
        }

        foreach (Dictionary<string, object> data in parsedData)
        {
            // 찾은 타입에 맞게 해당 타입을 동적 생성
            object newData = Activator.CreateInstance(type);// ~~Data 클래스
            if (newData == null)
            {
                Debug.LogError("Failed to create instance.");
                return false;
            }

            for (int i = 0; i < fieldesName.Length; i++)
            {
                if (fieldesName[i].StartsWith('#'))
                {
                    fieldesName[i] = fieldesName[i].Substring(1);
                }
                // 속성들 중에 해당 속성 값이 이름과 같다면
                if (data.ContainsKey(fieldesName[i]))
                {
                    if (fieldes[i].FieldType.IsEnum) 
                    {
                        // enum 타입이라면 enum으로 파싱해서 해당 속성에 저장
                        fieldes[i].SetValue(newData, Enum.Parse(fieldes[i].FieldType, data[fieldesName[i]].ToString()));
                    }
                    else
                    {
                        // data에서 가져온 값을 속성의 타입에 맞게 변환하여 newData의 해당 속성에 저장
                        fieldes[i].SetValue(newData, Convert.ChangeType(data[fieldesName[i]], fieldes[i].FieldType));
                    }
                }
            }
            if (!datacontainers.ContainsKey(dataName))
            {
                return false;
            }
            else
            {
                if (!SaveDataListInDataScriptableObject(dataName, newData, datacontainers[dataName]))
                { 
                    return false; 
                }

            }
        }
        return true;
    }


    private bool SaveDataListInDataScriptableObject(string dataName, object newData, DataScriptableObjects container)
    {
        // 동적으로 타입을 가져오기
        Type type = Type.GetType(dataName + "DataBase");
        if (type == null)
        {
            return false;
        }

        FieldInfo field = type.GetField(dataName+"List");
        if(field == null)
        {
            return false;
        }

        // 동적으로 반환 타입 가져오기
        var currentList = field.GetValue(container);

        
        // IList인지 확인
        if (currentList is IList list)
        {
            /*
            // 데이터의 Number 속성에 대한 FieldInfo 얻기
            FieldInfo dataNumberFieldInfo = newData.GetType().GetField("Number");
            if (dataNumberFieldInfo == null)
            {
                dataNumberFieldInfo = newData.GetType().GetField("id");
                if(dataNumberFieldInfo == null)
                {
                    return false;
                }
            }

            // 중복 체크
            bool isDuplicate = false;
            foreach (var existingData in list)
            {
                // 기존 데이터의 Number 값 가져오기
                FieldInfo existingNumberFieldInfo = existingData.GetType().GetField("Number");
                if(existingNumberFieldInfo == null)
                {
                    existingNumberFieldInfo = existingData.GetType().GetField("id");
                    if (existingNumberFieldInfo == null)
                    {
                        return false;
                    }
                }
                if (existingNumberFieldInfo != null)
                {
                    var existingNumberValue = existingNumberFieldInfo.GetValue(existingData);
                    if (existingNumberValue.Equals(dataNumberFieldInfo.GetValue(newData)))
                    {
                        isDuplicate = true;
                        break;
                    }
                }
            }

            if (!isDuplicate)
            {
                // 새로운 데이터 추가
                list.Add(newData);
            }*/

            list.Add(newData);
        }
        return true;
    }

    public bool SaveToCSVAllFile(Dictionary<string, DataScriptableObjects> datacontainers)
    {
        bool result = true;
        foreach (var fn in FILE_NAME)
        {
            //scriptableObject의 타입 로드
            Type type = Type.GetType(fn + "DataList");
            if (type == null) {
                result = false;
                return result;
            }

            //scriptableObject 로드
            DataScriptableObjects ddl = datacontainers[fn];
            if (ddl == null)
            {
                Debug.LogError(fn);
                result = false;
                return result;
            }

            //scriptableObject의 파일 위치 저장
            string filePath = Path.Combine(Application.dataPath + "/Resources", fn + ".csv");

            //dialog 파일에 저장
            result = SaveToCSV(fn, ddl, filePath);
        }
        return result;
    }

    private static bool SaveToCSV(string fileName, DataScriptableObjects data, string filePath)
    {
        // data 내에 listFieldName에 해당하는 필드를 찾음
        FieldInfo listField = data.GetType().GetField(fileName + "Datas");
        if (listField == null) return false;

        // 필드를 통해 데이터 리스트를 가져옴
        var dataList = listField.GetValue(data) as IList;
        if (dataList == null || dataList.Count == 0) return false;

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // 첫 항목의 타입 정보를 기반으로 헤더 생성
            var firstItem = dataList[0];
            var itemType = firstItem.GetType();
            var fields = itemType.GetFields();

            // CSV 헤더 작성
            List<string> headers = new List<string>();
            foreach (var field in fields)
            {
                headers.Add(field.Name);
            }
            writer.WriteLine(string.Join(",", headers));

            // 각 항목을 CSV로 작성
            foreach (var item in dataList)
            {
                List<string> values = new List<string>();
                foreach(var field in fields)
                {
                    var value = field.GetValue(item)?.ToString() ?? "";
                    values.Add(value);
                }
                writer.WriteLine(string.Join(",", values));
            }
        }
        return true;

    }
}
