using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

public class MasterManager : MonoBehaviour
{

#if true // System
    [SerializeField]
    private CharacterManager m_characterManager;
    [SerializeField]
    private TurnManager m_turnManager;
    [SerializeField]
    private CardManager m_cardManager;
    [SerializeField]
    private MonsterManager m_monsterManager;
    [SerializeField]
    private FlowScheduleManager m_skillScheduleManager;
#endif

#if true // UI
    [SerializeField]
    private CharacterUIManager m_characterUIManager;
    [SerializeField]
    private TurnUIManager m_turnUIManager;
    [SerializeField]
    private CardUIManager m_cardUIManager;
    [SerializeField]
    private MonsterUIManager m_monsterUIManager;
#endif

#if true // DataBase
    [SerializeField]
    private DontDestroyOnLoadManager DataBaseManager;
#endif

#if true // Getter
    public CharacterManager CharacterManager { get => m_characterManager; }
    public TurnManager TurnManager { get => m_turnManager;}
    public CardManager CardManager { get => m_cardManager; }
    public MonsterManager MonsterManager { get => m_monsterManager; }
    public FlowScheduleManager SkillScheduleManager { get => m_skillScheduleManager; }

    public CharacterUIManager CharacterUIManager { get => m_characterUIManager; }
    public TurnUIManager TurnUIManager { get => m_turnUIManager; }
    public CardUIManager CardUIManager { get => m_cardUIManager; }
    public MonsterUIManager MonsterUIManager { get => m_monsterUIManager; }
#endif

#if true // Container
    private LinkedList<IUpdatableManager> m_IUpdatableManagers = new LinkedList<IUpdatableManager>();
    private LinkedList<BaseSystem> m_managers = new LinkedList<BaseSystem>();
    private LinkedList<BaseUI> m_UIManagers = new LinkedList<BaseUI>();
#endif

    public void Awake()
    {
        DataBaseManager.Initialize();

        m_managers.AddLast(m_characterManager);
        m_managers.AddLast(m_monsterManager);
        m_managers.AddLast(m_turnManager);
        m_managers.AddLast(m_cardManager);
        m_managers.AddLast(m_skillScheduleManager);

        m_UIManagers.AddLast(m_characterUIManager);
        m_UIManagers.AddLast(m_monsterUIManager);
        m_UIManagers.AddLast(m_turnUIManager);
        m_UIManagers.AddLast(m_cardUIManager);

        m_characterUIManager.Bind(m_characterManager);
        m_monsterUIManager.Bind(m_monsterManager);
        m_turnUIManager.Bind(m_turnManager);
        m_cardUIManager.Bind(m_cardManager);

        /*********************************SearchUpdatableManager*********************************/

        for (LinkedListNode<BaseSystem> node = m_managers.First; node != null; node = node.Next)
        {
            IUpdatableManager temtManager;
            if (node.Value.TryGetComponent<IUpdatableManager>(out temtManager)) m_IUpdatableManagers.AddFirst(temtManager);
        }

        for (LinkedListNode<BaseUI> node = m_UIManagers.First; node != null; node = node.Next)
        {
            IUpdatableManager temtManager;
            if (node.Value.TryGetComponent<IUpdatableManager>(out temtManager)) m_IUpdatableManagers.AddFirst(temtManager);
        }
        /***************************************Initialize***************************************/

        for (LinkedListNode<BaseSystem> node = m_managers.First; node != null; node = node.Next)
        {
            node.Value.Initialize();
        }
        for (LinkedListNode<BaseUI> node = m_UIManagers.First; node != null; node = node.Next)
        {
            node.Value.Initialize();
        }

        /***********************************InitializeReference**********************************/

        for (LinkedListNode<BaseSystem> node = m_managers.First; node != null; node = node.Next)
        {
            node.Value.InitializeReference(this);
        }
        for (LinkedListNode<BaseUI> node = m_UIManagers.First; node != null; node = node.Next)
        {
            node.Value.InitializeReference(this);
        }

        /***********************************DataBaseInitialize***********************************/

        for (LinkedListNode<BaseSystem> node = m_managers.First; node != null; node = node.Next)
        {
            BaseManager temtManager;
            if (!node.Value.TryGetComponent<BaseManager>(out temtManager)) continue;
            if (!temtManager.ConnectsDataBase()) Debug.Log(node.Value.gameObject.name);
            temtManager.DataInitialize();
        }
        for (LinkedListNode<BaseUI> node = m_UIManagers.First; node != null; node = node.Next)
        {
            BaseManager temtManager;
            if (!node.Value.TryGetComponent<BaseManager>(out temtManager)) continue;
            if (!temtManager.ConnectsDataBase()) Debug.Log(node.Value.gameObject.name);
            temtManager.DataInitialize();
        }

        /************************************Synchronization************************************/
        
        Synchronization();
    }

    public void Update()
    {
        foreach(var u in m_IUpdatableManagers)
        {
            u.Execute();
        }
    }

    public void Synchronization()
    {
        foreach (var manager in m_managers)
        {
            manager.Synchronization();
        }
    }

    public void SetTurn()
    {
        m_skillScheduleManager.RegistSetTurnEventFlow();
    }

    public void ApplySetTurn()
    {
        Debug.Log("SETTURN");
        foreach (var manager in m_managers)
        {
            manager.SetTurn();
        }
    }

    public void ApplyUISetTurn()
    {
        foreach (var uiManager in m_UIManagers)
        {
            uiManager.SetTurn();
        }
    }



    public bool UseCard(Card card)
    {
        if(!m_turnManager.SetEther(card.CardData.Cost))
        {
            return false;
        }
        m_turnUIManager.UseCard(card);
        card.Execute();

        return true;
    }

    public void UnitDying(TargetPair unit)
    {
        Debug.Log(unit.isCharacter + "" + unit.position);
        m_skillScheduleManager.UnitDying(unit);
    }

    public void ApplyUnitDying(Unit unit)
    {
        foreach (var manager in m_managers)
        {
            manager.UnitDying(unit);
        }
    }

    public void ApplyUIUnitDying(Unit unit)
    {
        foreach (var uiManager in m_UIManagers)
        {
            uiManager.UnitDying(unit);
        }
    }
}
