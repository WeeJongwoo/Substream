using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

public class CSVManager : MonoBehaviour
{
    private static CSVManager instance;

    private List<string> FILE_NAME = new List<string> { 
        "Card",
        "LocalUser",
        "PrototypeCharacter",
        "Character",
        "UseCard",
    };
    //CSV���� �Ľ� ���� ���� ����
    private List<Dictionary<string, object>> LocalUser = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> PrototypeCharacter = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> Character = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> Card = new List<Dictionary<string, object>>();
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
            Destroy(gameObject); // �ߺ� ���� ����
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
        // Reflection�� ����Ͽ� �ʵ忡 ����
        FieldInfo field = this.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

        if (field != null)
        {
            return field.GetValue(this); // �ʵ��� ���� ��ȯ
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

    private void ConvertCSVToScriptableObject(string dataName, List<Dictionary<string, object>> parsedData, Dictionary<string,DataScriptableObjects> datacontainers)
    {
        //�ҷ��� data���� + "Data" ���ڿ��� �߰��Ͽ� Ÿ���� ã��
        Type type = Type.GetType(dataName + "Data");
        if (type == null)
        {
            Debug.LogError($"Type not found: {dataName + "Data"}");
            return;
        }
        //�������� ������ ��ü�� �Ӽ����� �ҷ���
        FieldInfo[] fieldes = type.GetFields();

        //�ҷ��� �Ӽ����� �̸��� ������ ������ ����
        string[] fieldesName = new string[fieldes.Length];
        int counter = 0;

        //�ҷ��� �Ӽ����� �̸����� ����
        foreach (FieldInfo field in fieldes)
        {
            fieldesName[counter] = field.Name;
            counter++;
        }

        foreach (Dictionary<string, object> data in parsedData)
        {
            // ã�� Ÿ�Կ� �°� �ش� Ÿ���� ���� ����
            var newData = Activator.CreateInstance(type);// ~~Data Ŭ����
            if (newData == null)
            {
                Debug.LogError("Failed to create instance.");
                return;
            }

            for (int i = 0; i < fieldesName.Length; i++)
            {
                // �Ӽ��� �߿� �ش� �Ӽ� ���� �̸��� ���ٸ�
                if (data.ContainsKey(fieldesName[i]))
                {
                    if (fieldes[i].FieldType.IsEnum) 
                    {
                        // enum Ÿ���̶�� enum���� �Ľ��ؼ� �ش� �Ӽ��� ����
                        fieldes[i].SetValue(newData, Enum.Parse(fieldes[i].FieldType, data[fieldesName[i]].ToString()));
                    }
                    else
                    {
                        // data���� ������ ���� �Ӽ��� Ÿ�Կ� �°� ��ȯ�Ͽ� newData�� �ش� �Ӽ��� ����
                        fieldes[i].SetValue(newData, Convert.ChangeType(data[fieldesName[i]], fieldes[i].FieldType));
                    }
                }
            }
            SaveDataListInDataScriptableObject(dataName, newData, datacontainers[dataName]);
        }
    }


    private bool SaveDataListInDataScriptableObject(string dataName, object newData, DataScriptableObjects container)
    {
        // �������� Ÿ���� ��������
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

        // �������� ��ȯ Ÿ�� ��������
        var currentList = field.GetValue(container);

        
        // IList���� Ȯ��
        if (currentList is IList list)
        {
            /*
            // �������� Number �Ӽ��� ���� FieldInfo ���
            FieldInfo dataNumberFieldInfo = newData.GetType().GetField("Number");
            if (dataNumberFieldInfo == null)
            {
                dataNumberFieldInfo = newData.GetType().GetField("id");
                if(dataNumberFieldInfo == null)
                {
                    return false;
                }
            }

            // �ߺ� üũ
            bool isDuplicate = false;
            foreach (var existingData in list)
            {
                // ���� �������� Number �� ��������
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
                // ���ο� ������ �߰�
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
            //scriptableObject�� Ÿ�� �ε�
            Type type = Type.GetType(fn + "DataList");
            if (type == null) {
                result = false;
                return result;
            }

            //scriptableObject �ε�
            DataScriptableObjects ddl = datacontainers[fn];
            if (ddl == null)
            {
                Debug.LogError(fn);
                result = false;
                return result;
            }

            //scriptableObject�� ���� ��ġ ����
            string filePath = Path.Combine(Application.dataPath + "/Resources", fn + ".csv");

            //dialog ���Ͽ� ����
            result = SaveToCSV(fn, ddl, filePath);
        }
        return result;
    }

    private static bool SaveToCSV(string fileName, DataScriptableObjects data, string filePath)
    {
        // data ���� listFieldName�� �ش��ϴ� �ʵ带 ã��
        FieldInfo listField = data.GetType().GetField(fileName + "Datas");
        if (listField == null) return false;

        // �ʵ带 ���� ������ ����Ʈ�� ������
        var dataList = listField.GetValue(data) as IList;
        if (dataList == null || dataList.Count == 0) return false;

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // ù �׸��� Ÿ�� ������ ������� ��� ����
            var firstItem = dataList[0];
            var itemType = firstItem.GetType();
            var fields = itemType.GetFields();

            // CSV ��� �ۼ�
            List<string> headers = new List<string>();
            foreach (var field in fields)
            {
                headers.Add(field.Name);
            }
            writer.WriteLine(string.Join(",", headers));

            // �� �׸��� CSV�� �ۼ�
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
