using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MasterManager : MonoBehaviour
{
    private LinkedList<IUpdatableManager> m_IUpdatableManagers;
    [SerializeField]
    private CharacterManager m_characterManager;
    [SerializeField]
    private CharacterUIManager m_characterUIManager;
    [SerializeField]
    private TurnManager m_turnManager;
    [SerializeField]
    private TurnUIManager m_turnUIManager;
    [SerializeField]
    private CardManager m_cardManager;
    [SerializeField]
    private CardUIManager m_cardUIManager;
    [SerializeField]
    private MonsterManager m_monsterManager;
    [SerializeField]
    private MonsterUIManager m_monsterUIManager;

    [SerializeField]
    private SkillScheduleManager m_skillScheduleManager;

    private LinkedList<BaseManager> m_managers;

    [SerializeField]
    private DontDestroyOnLoadManager DDOManager;

    public SkillScheduleManager SkillScheduleManager
    {
        get { return m_skillScheduleManager; }
    }
    public void Awake()
    {
        DDOManager.Initialize();

        m_managers = new LinkedList<BaseManager>();
        m_IUpdatableManagers = new LinkedList<IUpdatableManager>();
        m_managers.AddLast(m_characterManager);
        m_managers.AddLast(m_characterUIManager);
        m_managers.AddLast(m_monsterManager);
        m_managers.AddLast(m_monsterUIManager);
        m_managers.AddLast(m_turnManager);
        m_managers.AddLast(m_turnUIManager);
        m_managers.AddLast(m_cardManager);
        m_managers.AddLast(m_cardUIManager);
        m_managers.AddLast(m_skillScheduleManager);
        
        for (LinkedListNode<BaseManager> node = m_managers.First; node != null; node = node.Next)
        {
            node.Value.Initialize(this, m_turnManager);
            BaseManager temtManager;
            if (!node.Value.TryGetComponent<BaseManager>(out temtManager)) continue;
            temtManager.ConnectsDataBase();
            temtManager.DataInitialize(m_turnManager, m_characterManager, m_monsterManager);
        }

        for (LinkedListNode<BaseManager> node = m_managers.First; node != null; node = node.Next)
        {
            IUpdatableManager temtManager;
            if(node.Value.TryGetComponent<IUpdatableManager>(out temtManager)) m_IUpdatableManagers.AddFirst(temtManager); ;
        }

        m_characterUIManager.Synchronization(m_characterManager);
        m_monsterUIManager.Synchronization(m_monsterManager);
        m_turnUIManager.Synchronization(m_turnManager);
        m_cardUIManager.Synchronization(m_cardManager);
    }

    public void Update()
    {
        foreach(var u in m_IUpdatableManagers)
        {
            u.Execute();
        }
    }

    public void SetTurn()
    {
        for (LinkedListNode<BaseManager> node = m_managers.First; node != null; node = node.Next)
        {
            node.Value.SetTurn(m_turnManager,m_characterManager, m_monsterManager, m_cardManager);
        }

        m_turnUIManager.Synchronization(m_turnManager);
        m_cardUIManager.Synchronization(m_cardManager);

    }

    public void UseCard(Button card, int Cost)
    {
        if(m_turnManager.SetEther(Cost))
        {
            card.gameObject.SetActive(false);
        }
        if (m_cardUIManager.ActiveCardNum == 0) SetTurn();

        m_characterUIManager.Synchronization(m_characterManager);
        m_monsterUIManager.Synchronization(m_monsterManager);
        m_turnUIManager.Synchronization(m_turnManager);
    }
}
