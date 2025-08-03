using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private GameManager m_GameManager;

    private Dictionary<string, DataScriptableObjects> m_DataBaseDic;

    [SerializeField]
    private LocalUserDataBase m_localUser;
    [SerializeField]
    private PrototypeCharacterDataBase m_prototypeCharacter;
    [SerializeField]
    private CharacterDataBase m_character;
    [SerializeField]
    private CardDataBase m_card;
    [SerializeField]
    private UseCardDataBase m_useCard;



    public LocalUserDataBase LocalUser { get => m_localUser; }
    public PrototypeCharacterDataBase PrototypeCharacter { get => m_prototypeCharacter; }
    public CharacterDataBase Character { get => m_character; }
    public CardDataBase Card { get => m_card; }
    public UseCardDataBase UseCard { get => m_useCard; }

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
            Destroy(gameObject); // 吝汗 积己 规瘤
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
            Destroy(gameObject); // 吝汗 积己 规瘤
        }

        m_DataBaseDic = new Dictionary<string, DataScriptableObjects>();

        m_localUser = new LocalUserDataBase();
        m_prototypeCharacter = new PrototypeCharacterDataBase();
        m_character = new CharacterDataBase();
        m_card = new CardDataBase();
        m_useCard = new UseCardDataBase();

        m_localUser.LocalUserList.Clear();
        m_prototypeCharacter.PrototypeCharacterList.Clear();
        m_character.CharacterList.Clear();
        m_card.CardList.Clear();
        m_useCard.UseCardList.Clear();

        m_localUser.LocalUser.Clear();
        m_prototypeCharacter.PrototypeCharacter.Clear();
        m_character.Character.Clear();
        m_card.Card.Clear();
        m_useCard.UseCard.Clear();

        m_DataBaseDic.Add("LocalUser", m_localUser);
        m_DataBaseDic.Add("PrototypeCharacter", m_prototypeCharacter);
        m_DataBaseDic.Add("Character", m_character);
        m_DataBaseDic.Add("Card", m_card);
        m_DataBaseDic.Add("UseCard", m_useCard);

        GameObject[] DDO = GameObject.FindObjectsOfType<GameObject>(false);
        foreach (var ddo in DDO)
        {
            if (ddo.CompareTag("DDO") && ddo.name == "CSVManager")// && SceneManager.GetActiveScene() != ddo.scene)
            {
                m_CSVManager = ddo.GetComponent<CSVManager>();
            }
            
            if (ddo.CompareTag("DDO") && ddo.name == "GameManager")// && SceneManager.GetActiveScene() != ddo.scene)
            {
                m_GameManager = ddo.transform.gameObject.GetComponent<GameManager>();
            }
        }
        DDO = null;

        m_CSVManager.Initialize(m_DataBaseDic);
        m_GameManager.Initialize();

        m_localUser.TranslateListToDic(m_GameManager.SelectUserID);
        m_prototypeCharacter.TranslateListToDic(m_GameManager.SelectUserID);
        m_character.TranslateListToDic(m_GameManager.SelectUserID);
        m_card.TranslateListToDic(m_GameManager.SelectUserID);
        m_useCard.TranslateListToDic(m_GameManager.SelectUserID);

        foreach(var LU in m_localUser.LocalUserList)
        {
            Debug.Log("LocalUser :" + LU.ID);
        }
        foreach (var PU in m_prototypeCharacter.PrototypeCharacterList)
        {
            Debug.Log("PrototypeUnit : " + PU.ID + " " + PU.Name + " " + PU.InstanceCounter);
        }
        foreach (var U in m_character.CharacterList)
        {
            Debug.Log("Unit : " + U.UserID + " " + U.PrototypeUnitID + " " + U.InstanceID);

        }
        foreach (var C in m_card.CardList)
        {
            Debug.Log("Card : " + C.ID + " " + C.Name + " " + C.Type + " " + C.Rank + " " + C.Cost + " " + C.TargetCount + " " + C.Explanation);
        }
        foreach (var UC in m_useCard.UseCardList) 
        {
            Debug.Log("UseCard :" + UC.PrototypeUnitID + " " + UC.CardID);
        }
    }

    public bool saveData()
    {
        m_localUser.TranslateDicToListAtSaveDatas(m_GameManager.SelectUserID);
        m_prototypeCharacter.TranslateDicToListAtSaveDatas(m_GameManager.SelectUserID);
        m_character.TranslateDicToListAtSaveDatas(m_GameManager.SelectUserID);
        m_card.TranslateDicToListAtSaveDatas(m_GameManager.SelectUserID);
        m_useCard.TranslateDicToListAtSaveDatas(m_GameManager.SelectUserID);
        return m_CSVManager.SaveToCSVAllFile(m_DataBaseDic);
    }
}
