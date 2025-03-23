using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


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

    private List<string> path = new List<string> {

    };

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
        Type elementType;
        foreach (var p in path)
        {
            fieldInfo = GetType().GetField(p, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (fieldInfo == null)
            {
                continue;
            }

            type = fieldInfo.FieldType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // ���׸� Ÿ�� �Ķ����(���� Ÿ��) ��ȯ
                elementType = type.GetGenericArguments()[0];

                // Resources���� ��� �ε�
                objects = Resources.LoadAll(p, elementType);
                if (objects == null || objects.Length == 0)
                {
                    continue;
                }

                var listInstance = fieldInfo.GetValue(this);
                if (listInstance == null)
                {
                    // �ʵ尡 null�̶�� �� ����Ʈ ����
                    listInstance = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                    fieldInfo.SetValue(this, listInstance);
                }

                // ����Ʈ�� ��� �߰�
                var addMethod = listInstance.GetType().GetMethod("Add");
                foreach (var obj in objects)
                {
                    addMethod.Invoke(listInstance, new[] { obj });
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
