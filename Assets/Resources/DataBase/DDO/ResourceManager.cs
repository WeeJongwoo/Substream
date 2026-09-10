using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Text;


public class ResourceManager : MonoBehaviour
{
    private static ResourceManager instance;

    public static ResourceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ResourceManager>();
                if (instance == null)
                {
                    GameObject NewResourceManager = new GameObject();
                    instance = NewResourceManager.AddComponent<ResourceManager>();
                    DontDestroyOnLoad(instance);
                    instance.Initialize();
                }
            }
            return instance;
        }
    }

    [SerializeField]
    private int m_selectUserID = 0;   //선택한 유저의 ID
    [SerializeField]
    private int m_selectStageID = 0;  //선택한 전투 스테이지의 ID
    StringBuilder m_keyBuilder = new StringBuilder();

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
        "Card_Frame",
        "Character_Portrait"
    };

    private Dictionary<string, Sprite> Battle_BGI_Dic = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> Card_Cost_Dic = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> Card_Frame_Dic = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> Character_Portrait_Dic = new Dictionary<string, Sprite>();

    public Dictionary<string, Sprite> Battle_BGI_DIC { get { return  Battle_BGI_Dic; } }
    public Dictionary<string, Sprite> Card_Cost_DIC { get { return Card_Cost_Dic; } }
    public Dictionary<string, Sprite> Card_Frame_DIC { get { return Card_Frame_Dic; } }
    public Dictionary<string, Sprite> Character_Portrait_DIC { get { return Character_Portrait_Dic; } }

    public Sprite Battle_BGI(int id)
    {
        return Battle_BGI_Dic[KeyConvert(id)];
    }
    public Sprite Card_Cost(int id)
    {
        return Card_Cost_Dic[KeyConvert(id)];
    }
    public Sprite Card_Frame(int id)
    {
        return Card_Frame_Dic[KeyConvert(id)];
    }
    public Sprite Character_Portrait(int id)
    {
        return Character_Portrait_Dic[KeyConvert(id)];
    }

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {

            Destroy(gameObject); // 중복 생성 방지
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
                // 제네릭 타입 파라미터(키,값 타입) 반환
                Type[] genericArgs = type.GetGenericArguments();
                Type keyType = genericArgs[0];
                Type valueType = genericArgs[1];
                if (keyType != typeof(string))
                {
                    continue;
                }

                // Resources에서 요소 로드
                objects = Resources.LoadAll(path, valueType);
                var dictInstance = fieldInfo.GetValue(this);
                if (objects == null || objects.Length == 0 || dictInstance == null)
                {
                    continue;
                }

                // 리스트에 요소 추가
                MethodInfo addMethod = dictInstance.GetType().GetMethod("Add", new[] { typeof(string), valueType });
                if (addMethod == null)
                {
                    continue;
                }

                foreach (var obj in objects)
                {
                    string fullName = (obj as UnityEngine.Object)?.name ?? null;
                    string prefix = path + "_";         // p는 현재 폴더 이름, 예: "Battle_BGI"

                    string key = fullName.StartsWith(prefix)
                        ? fullName.Substring(prefix.Length)
                        : null; // 혹시 prefix가 안 붙어 있으면 전체 이름 사용

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

    public string KeyConvert(int id)
    {
        m_keyBuilder.Clear();
        for (int k = 3; k > -1; k--)
        {
            int cost = id;
            int p = (int)Math.Pow(10, k);
            int c = cost / p;
            if (c == 0) m_keyBuilder.Append(c);
            else
            {
                m_keyBuilder.Append(id);
                break;
            }
        }
        return m_keyBuilder.ToString();
    }

    private object GetFieldByString(string fieldName)
    {
        // Reflection을 사용하여 필드에 접근
        FieldInfo field = GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field != null)
        {
            return field.GetValue(this); // 필드의 값을 반환
        }
        else
        {
            Console.WriteLine($"{fieldName} not found.");
#if UNITY_EDITOR
            Debug.Log($"{fieldName} not found.");
            return null;
#endif
        }
    }
}
