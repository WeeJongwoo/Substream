using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject NewDontDestroyOnLoadManager = new GameObject();
                    instance = NewDontDestroyOnLoadManager.AddComponent<GameManager>();
                    DontDestroyOnLoad(instance);
                    instance.Initialize();
                }
            }
            return instance;
        }
    }

    [SerializeField]
    private int m_selectUserID = 0;   //������ ������ ID
    [SerializeField]
    private int m_selectStageID = 0;  //������ ���� ���������� ID

    public int SelectUserID
    {
        get { return m_selectUserID; }
        set { m_selectUserID = value; }
    }
    public int SelectStageID
    {
        get { return m_selectStageID; }
        set { m_selectStageID = value; }
    }

    private List<string> m_paths = new List<string> {
        "Battle_BGI",
        "Card_Cost",
        "Card_Frame"
    };

    private Dictionary<string,Sprite> Battle_BGI_Dic = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> Card_Cost_Dic = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> Card_Frame_Dic = new Dictionary<string, Sprite>();
   
    public Dictionary<string, Sprite> Battle_BGI { get { return  Battle_BGI_Dic; } }
    public Dictionary<string, Sprite> Card_Cost { get { return Card_Cost_Dic; } }
    public Dictionary<string, Sprite> Card_Frame { get { return Card_Frame_Dic; } }
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {

            Destroy(gameObject); // �ߺ� ���� ����
            return;
        }
        //Initialize();
    }

    public void Initialize()
    {
        FieldInfo fieldInfo;
        object[] objects;
        Type type;

        foreach (var path in m_paths)
        {
            fieldInfo = GetType().GetField(path + "_Dic", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                continue;
            }

            type = fieldInfo.FieldType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // ���׸� Ÿ�� �Ķ����(Ű,�� Ÿ��) ��ȯ
                Type[] genericArgs = type.GetGenericArguments();
                Type keyType = genericArgs[0];
                Type valueType = genericArgs[1];
                if (keyType != typeof(string))
                {
                    continue;
                }

                // Resources���� ��� �ε�
                objects = Resources.LoadAll(path, valueType);
                var dictInstance = fieldInfo.GetValue(this);
                if (objects == null || objects.Length == 0 || dictInstance == null)
                {
                    continue;
                }

                // ����Ʈ�� ��� �߰�
                MethodInfo addMethod = dictInstance.GetType().GetMethod("Add", new[] { typeof(string), valueType });
                if (addMethod == null)
                {
                    continue;
                }

                foreach (var obj in objects)
                {
                    string fullName = (obj as UnityEngine.Object)?.name ?? null;
                    string prefix = path + "_";         // p�� ���� ���� �̸�, ��: "Battle_BGI"

                    string key = fullName.StartsWith(prefix)
                        ? fullName.Substring(prefix.Length)
                        : null; // Ȥ�� prefix�� �� �پ� ������ ��ü �̸� ���

                    if (key != null)
                    {
                        addMethod.Invoke(dictInstance, new[] { key, obj });
                    }
                }
            }

            
        }
        objects = null;

        /*
        sorting(monster);
        sorting(prototypeUnit);
        sorting(prototypeWeapon);
        sorting(prisonerBodyImg);
        sorting(prisonerBodyAnim);
        sorting(prisonerHeadImg);
        sorting(prisonerHeadAnim);
        sorting(baseTileImg);
        sorting(weaponImg);
        sorting(terrain);*/
    }


    private object GetFieldByString(string fieldName)
    {
        // Reflection�� ����Ͽ� �ʵ忡 ����
        FieldInfo field = GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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

    private void sorting<T>(List<T> list) where T : UnityEngine.Object
    {
        list.Sort((a, b) =>
        {
            int numA = int.Parse(a.name);
            int numB = int.Parse(b.name);
            return numA.CompareTo(numB);
        });

    }

    private void TranslateListToDic<T>(string pathName, List<T> list) where T : UnityEngine.Object
    {
        FieldInfo fieldInfo;
        object[] objects;
        Type type;
        Type elementType;

        fieldInfo = GetType().GetField(pathName + "Dic", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (fieldInfo == null)
        {
            return;
        }

        type = fieldInfo.FieldType;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<string,GameObject>))
        {   
            foreach(var l in list)
            {

            }
        }
    }
}
