using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[System.Serializable]
public class DontDestroyOnLoadManager : MonoBehaviour
{
#if true
    private static DontDestroyOnLoadManager instance;

    public static DontDestroyOnLoadManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DontDestroyOnLoadManager>();
                if (instance == null)
                {
                    GameObject NewDontDestroyOnLoadManager = new GameObject();
                    instance = NewDontDestroyOnLoadManager.AddComponent<DontDestroyOnLoadManager>();
                    DontDestroyOnLoad(instance);
                    instance.Initialize();
                }
            }
            return instance;
        }
    }

    [SerializeField]
    private CSVManager m_CSVManager;
    [SerializeField]
    private ResourceManager m_ResourceManager;

    public ResourceManager ResourceManager
    {
        get { return m_ResourceManager; }
    }

    private Dictionary<string, DataScriptableObjects> m_DataBase;
#endif
    /// <summary>
    /// 데이터 베이스 클래스 선언
    /// </summary>
    [SerializeField]
    private CardTableDataBase m_cardTable;
    [SerializeField]
    private CardSkillTableDataBase m_cardSkillTable;
    [SerializeField]
    private CharacterTableDataBase m_CharacterTable;
    [SerializeField]
    private LocalUserDataBase m_localUser;
    [SerializeField]
    private StatusEffectDataBase m_statusEffect;
    [SerializeField]
    private UseCardDataBase m_useCard;

    /// <summary>
    /// 데이터 베이스 클래스 GETTER 선언 및 정의
    /// </summary>
    public CardTableDataBase CardTableDataBase { get => m_cardTable; }
    public CardSkillTableDataBase CardSkillTableDataBase { get => m_cardSkillTable; }
    public CharacterTableDataBase CharacterTableDataBase { get => m_CharacterTable; }
    public LocalUserDataBase LocalUserDataBase { get => m_localUser; }
    public StatusEffectDataBase StatusEffectDataBase { get => m_statusEffect; }
    public UseCardDataBase UseCardDataBase { get => m_useCard; }

    /// <summary>
    /// 데이터 베이스에서 기본키를 기준으로 쿼리한 값을 반환
    /// </summary>
    public CardTableData CardTable(int ID)
    {
        return m_cardTable.CardTable[ID];
    }
    public CardSkillTableData CardSkillTable(int ID)
    {
        return m_cardSkillTable.CardSkillTable[ID];
    }
    public LocalUserData LocalUser(int ID)
    {
        return m_localUser.LocalUser[ID];
    }
    public CharacterTableData PrototypeCharacter(int ID)
    {
        return m_CharacterTable.CharacterTable[ID];
    }
    public CharacterTableData Character(int ID)
    {
        return m_CharacterTable.CharacterTable[ID];
    }
    public UseCardData UseCard(int PrototypeUnitID, int CardID)
    {
        var key = (PrototypeUnitID, CardID);
        return m_useCard.UseCard[key];
    }


    private void Awake()
    {
        /*
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }*/
    }

    public void Initialize()
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

        //데이터 베이스 집합 딕셔너리 정의
        m_DataBase = new Dictionary<string, DataScriptableObjects>();

        //데이터 베이스 테이블 정의
        m_cardSkillTable = new CardSkillTableDataBase();
        m_cardTable = new CardTableDataBase();
        m_CharacterTable = new CharacterTableDataBase();
        m_localUser = new LocalUserDataBase();
        m_statusEffect = new StatusEffectDataBase();
        m_useCard = new UseCardDataBase();

        //데이터 베이스 테이블을 데이터 베이스에 추가
        m_DataBase.Add("CardSkillTable", m_cardSkillTable);
        m_DataBase.Add("CardTable", m_cardTable);
        m_DataBase.Add("CharacterTable", m_CharacterTable);
        m_DataBase.Add("LocalUser", m_localUser);
        m_DataBase.Add("StatusEffect", m_statusEffect);
        m_DataBase.Add("UseCard", m_useCard);

        //데이터 베이스의 테이블 초기화
        foreach (var DB in m_DataBase)
        {
            DB.Value.ClearContainer();
        }

        GameObject[] DDO = GameObject.FindObjectsOfType<GameObject>(false);
        foreach (var ddo in DDO)
        {
            if (ddo.CompareTag("DDO") && ddo.name == "CSVManager")// && SceneManager.GetActiveScene() != ddo.scene)
            {
                m_CSVManager = ddo.GetComponent<CSVManager>();
            }
            
            if (ddo.CompareTag("DDO") && ddo.name == "GameManager")// && SceneManager.GetActiveScene() != ddo.scene)
            {
                m_ResourceManager = ddo.transform.gameObject.GetComponent<ResourceManager>();
            }
        }
        DDO = null;

        m_CSVManager.Initialize(m_DataBase);
        m_ResourceManager.Initialize();

        //각 테이블의 리스트 테이블을 딕셔너리 테이블로 변형
        foreach(var DB in  m_DataBase)
        {
            DB.Value.TranslateListToDic(m_ResourceManager.SelectUserID);
        }
    }

    public bool saveData()
    {
        //각 테이블의 리스트 테이블을 딕셔너리 테이블로 변형
        foreach (var DB in m_DataBase)
        {
            DB.Value.TranslateDicToListAtSaveDatas(m_ResourceManager.SelectUserID);
        }
        return m_CSVManager.SaveToCSVAllFile(m_DataBase);
    }
}
