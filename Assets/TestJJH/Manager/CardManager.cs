using System;
using System.Collections.Generic;
using System.Net;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class CardManager : BaseSystem
{
    private TurnManager m_turnManager;
    private CharacterManager m_characterManager;
    private MonsterManager m_monsterManager;
    struct UnitCardPair
    {
        public Unit s_unit;
        public List<Card> s_card;
    };

    private UnitCardPair m_hand;
    private Dictionary<Unit,List<Card>> m_deck;
    private Dictionary<Unit,List<Card>> m_graveyard;
    private Dictionary<Unit, List<Card>> m_tempQueueForCardsToBeAdded;

    public List<Card> TemtQueueForCardsToBeAdded
    {
        get { return m_tempQueueForCardsToBeAdded[m_hand.s_unit]; }
    }

    public List<Card> Hand
    {
        get { return m_hand.s_card; }
    }
    public List<Card> Deck
    {
        get { return m_deck[m_hand.s_unit]; }
    }
    public List<Card> Graveyard
    {
        get { return m_graveyard[m_hand.s_unit]; }
    }

    [SerializeField]
    private int m_activeCardNum;
    public int ActiveCardNum
    {
        get { return m_activeCardNum; }
    }
    public override void Initialize()
    {
        m_hand = new UnitCardPair();
        m_hand.s_card = new List<Card>();

        m_deck = new Dictionary<Unit, List<Card>>();
        m_graveyard = new Dictionary<Unit, List<Card>>();
        m_tempQueueForCardsToBeAdded = new Dictionary<Unit, List<Card>>();
    }

    public override void InitializeReference(MasterManager masterManager)
    {
        m_masterManager = masterManager;
        m_turnManager = masterManager.TurnManager;
        m_characterManager = masterManager.CharacterManager;
        m_monsterManager = masterManager.MonsterManager;
    }

    public override void DataInitialize()
    {
        Unit NowTurnUnit = m_turnManager.CurrentTurnUnit;
        UnitCardPair NowTurnUnitpair = new UnitCardPair();
        NowTurnUnitpair.s_unit = NowTurnUnit;
        NowTurnUnitpair.s_card = new List<Card>();
        foreach (var uc in DataBase.UseCardDataBase.UseCardTable)
        {
            if (NowTurnUnit.IngameUnitID() == uc.Value.UnitID)
            {
                Card newCard = new Card();
                newCard.Initialize(
                    NowTurnUnitpair.s_unit,
                    DataBase.CardTable(uc.Value.CardID),
                    m_masterManager.SkillScheduleManager);
                NowTurnUnitpair.s_card.Add(newCard);
            }
        }
        if (!m_deck.ContainsKey(NowTurnUnitpair.s_unit))
        {
            m_deck.Add(NowTurnUnitpair.s_unit, NowTurnUnitpair.s_card);
            m_graveyard.Add(NowTurnUnitpair.s_unit, new List<Card>());
            m_tempQueueForCardsToBeAdded.Add(NowTurnUnitpair.s_unit, new List<Card>());
        }

        foreach (var unit in m_turnManager.Units)
        {
            UnitCardPair pair = new UnitCardPair();
            pair.s_unit = unit;
            pair.s_card = new List<Card>();
            foreach (var uc in DataBase.UseCardDataBase.UseCardTable)
            {
                if (unit.IngameUnitID() == uc.Value.UnitID)
                {
                    Card newCard = new Card();
                    newCard.Initialize(
                        pair.s_unit,
                        DataBase.CardTable(uc.Value.CardID),
                        m_masterManager.SkillScheduleManager);
                    pair.s_card.Add(newCard);
                }
            }
            if (!m_deck.ContainsKey(pair.s_unit))
            {
                m_deck.Add(pair.s_unit, pair.s_card);
                m_graveyard.Add(pair.s_unit, new List<Card>());
                m_tempQueueForCardsToBeAdded.Add(pair.s_unit, new List<Card>());
            }
        }
        // 현 턴이 몬스터면 동작 없음
        if (!m_turnManager.CurrentTurnUnit.IsCharacter)
        {
            return;
        }

        // 현 턴의 유닛을 지정
        m_hand.s_unit = m_turnManager.CurrentTurnUnit;

        if (!m_deck.ContainsKey(m_hand.s_unit)) return;

        Unit key = m_hand.s_unit;
        // 무조건 5장 뽑음
        for (int i = 0; i < 5; i++)
        {
            // 덱에 카드 없으면 묘지의 카드 모두 이동
            if (m_deck[key].Count == 0)
            {
                foreach (var g in m_graveyard[key])
                {
                    m_deck[key].Add(g);
                }
                m_graveyard[key].Clear();
            }
            // 랜덤으로 1장 뽑아 핸드에 넣고 덱에서 제외
            int address = UnityEngine.Random.Range(0, m_deck[key].Count);
            var a = m_deck[key][address];
            m_hand.s_card.Add(m_deck[key][address]);
            m_deck[key].RemoveAt(address);
        }

        m_activeCardNum = m_hand.s_card.Count;
    }

    public void HandShaker()
    {
        // 전 턴의 핸드를 묘지로 이동 
        foreach(var g in m_hand.s_card)
        {
            m_graveyard[m_hand.s_unit].Add(g);
        }
        m_hand.s_card.Clear();

        // 전 턴의 임시 추가 카드도 묘지로 이동
        foreach (var gt in m_tempQueueForCardsToBeAdded[m_hand.s_unit])
        {
            m_graveyard[m_hand.s_unit].Add(gt);
        }
        m_tempQueueForCardsToBeAdded[m_hand.s_unit].Clear();

        // 현 턴이 몬스터면 동작 없음
        if (!m_turnManager.CurrentTurnUnit.IsCharacter)
        {
            return;
        }

        // 현 턴의 유닛을 지정
        m_hand.s_unit = m_turnManager.CurrentTurnUnit;
        
        if (!m_deck.ContainsKey(m_hand.s_unit)) return;

        Unit key = m_hand.s_unit;
        // 무조건 5장 뽑음
        for (int i = 0; i < 5; i++)
        {
            // 덱에 카드 없으면 묘지의 카드 모두 이동
            if (m_deck[key].Count == 0)
            {
                foreach (var g in m_graveyard[key])
                {
                    m_deck[key].Add(g);
                }
                m_graveyard[key].Clear();
            }
            // 랜덤으로 1장 뽑아 핸드에 넣고 덱에서 제외
            int address = UnityEngine.Random.Range(0, m_deck[key].Count);
            var a = m_deck[key][address];
            m_hand.s_card.Add(m_deck[key][address]);
            m_deck[key].RemoveAt(address);
        }

        m_activeCardNum = m_hand.s_card.Count;
    }

    public void DrawCard(ActionContext context, int amount)
    {
        amount = amount < m_deck.Count ? amount : m_deck.Count;
        Unit key = m_hand.s_unit;

        for (int i = 0; i < amount; i++) 
        {
            // 랜덤으로 1장 뽑아 핸드에 넣고 덱에서 제외
            int address = UnityEngine.Random.Range(0, m_deck[key].Count);

            m_tempQueueForCardsToBeAdded[key].Add(m_deck[key][address]);
            
            m_deck[key].RemoveAt(address);
        }

        context.UIApplyHelper.DrawCard(amount);
    }

    public void DiscardCard(int position)
    {

    }

    public override void SetTurn()
    {
        HandShaker();
    }
    public override void UnitDying(Unit unit)
    {
        if (m_deck.ContainsKey(unit))
        {
            m_deck[unit].Clear();
            m_deck.Remove(unit);
        }
        else if (m_graveyard.ContainsKey(unit))
        {
            m_graveyard[unit].Clear();
            m_graveyard.Remove(unit);
        }
    }

    public override void UseCard(Card card)
    {
        m_activeCardNum--;
        if(m_hand.s_card.Contains(card))
        {
            m_hand.s_card.Remove(card);
        }
        if (m_graveyard.ContainsKey(card.Unit))
        {
            m_graveyard [card.Unit].Add(card);
        } 
    }

    public void AddTemtQueueCardsToHand()
    {
        foreach(var card in m_tempQueueForCardsToBeAdded[m_turnManager.CurrentTurnUnit])
        {
            m_hand.s_card.Add(card);
        }
        m_tempQueueForCardsToBeAdded[m_turnManager.CurrentTurnUnit].Clear();
    }
}
