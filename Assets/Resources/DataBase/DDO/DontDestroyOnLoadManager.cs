using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[System.Serializable]
public class DontDestroyOnLoadManager : MonoBehaviour
{
    private static DontDestroyOnLoadManager instance;

    public static DontDestroyOnLoadManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<DontDestroyOnLoadManager>();
                if(instance == null)
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

    [SerializeField]
    private CardTableDataBase m_cardTable;
    [SerializeField]
    private LocalUserDataBase m_localUser;
    [SerializeField]
    private SkillTableDataBase m_skillTable;
    [SerializeField]
    private StatusEffectDataBase m_statusEffect;
    [SerializeField]
    private UseCardTableDataBase m_useCardTable;
    [SerializeField]
    private UnitTableDataBase m_unitTable;


    public CardTableDataBase CardTableDataBase { get => m_cardTable; }
    public UnitTableDataBase UnitTableDataBase { get => m_unitTable; }
    public LocalUserDataBase LocalUserDataBase { get => m_localUser; }
    public SkillTableDataBase SkillTableDataBase { get => m_skillTable; }
    public StatusEffectDataBase StatusEffectDataBase { get => m_statusEffect; }
    public UseCardTableDataBase UseCardDataBase { get => m_useCardTable; }

    public SkillTableData SkillTable(int ID)
    {
        return m_skillTable.SkillTable[ID];
    }
    public UnitTableData UnitTable(int ID)
    {
        return m_unitTable.UnitTable[ID];
    }
    public CardTableData CardTable(int ID)
    {
        return m_cardTable.CardTable[ID];
    }
    public LocalUserData LocalUser(int ID)
    {
        return m_localUser.LocalUser[ID];
    }
    public UseCardTableData UseCard(int PrototypeUnitID, int CardID)
    {
        var key = (PrototypeUnitID, CardID);
        return m_useCardTable.UseCardTable[key];
    }

    private void Awake()
    {

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

        
        m_DataBase = new Dictionary<string, DataScriptableObjects>();

        m_cardTable = new CardTableDataBase();
        m_localUser = new LocalUserDataBase();
        m_skillTable = new SkillTableDataBase();
        m_statusEffect = new StatusEffectDataBase();
        m_unitTable = new UnitTableDataBase();
        m_useCardTable = new UseCardTableDataBase();

        m_DataBase.Add("CardTable", m_cardTable);
        m_DataBase.Add("LocalUser", m_localUser);
        m_DataBase.Add("SkillTable", m_skillTable);
        m_DataBase.Add("StatusEffect", m_statusEffect);
        m_DataBase.Add("UnitTable", m_unitTable);
        m_DataBase.Add("UseCardTable", m_useCardTable);

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

        foreach(var DB in  m_DataBase)
        {
            DB.Value.TranslateListToDic(m_ResourceManager.SelectUserID);
        }
    }

    public bool saveData()
    {
        foreach (var DB in m_DataBase)
        {
            DB.Value.TranslateDicToListAtSaveDatas(m_ResourceManager.SelectUserID);
        }
        return m_CSVManager.SaveToCSVAllFile(m_DataBase);
    }
}
