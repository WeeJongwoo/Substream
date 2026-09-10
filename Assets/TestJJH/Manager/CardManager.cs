using System;
using System.Collections.Generic;
using System.Net;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static Spine.Unity.Editor.SkeletonBaker.BoneWeightContainer;

public class CardManager : BaseSystem
{
    private TurnManager m_turnManager;
    private CharacterManager m_characterManager;
    private MonsterManager m_monsterManager;
    private CardUIManager m_cardUIManager;
    struct UnitCardPair
    {
        public Unit s_unit;
        public List<Card> s_card;
    };

    private Dictionary<Unit, List<Card>> m_hand;
    private Dictionary<Unit, List<Card>> m_deck;
    private Dictionary<Unit, List<Card>> m_graveyard;
    private Dictionary<Unit, List<Card>> m_tempQueueForCardsToBeAdded;

    public List<Card> TemtQueueForCardsToBeAdded
    {
        get { return m_tempQueueForCardsToBeAdded[m_turnManager.CurrentTurnUnit]; }
    }

    public Dictionary<Unit, List<Card>> Hand
    {
        get { return m_hand; }
    }
    public Dictionary<Unit, List<Card>> Deck
    {
        get { return m_deck; }
    }
    public Dictionary<Unit, List<Card>> Graveyard
    {
        get { return m_graveyard; }
    }

    public List<Card> NowHand
    {
        get { return m_hand[m_turnManager.CurrentTurnUnit]; }
    }

    [SerializeField]
    private int m_activeCardNum;

    public int ActiveCardNum
    {
        get { return m_activeCardNum; }
    }

    public override void Initialize()
    {
        m_hand = new Dictionary<Unit, List<Card>>();
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

        m_cardUIManager = masterManager.CardUIManager;
    }

    public override void DataInitialize()
    {
        Unit NowTurnUnit = m_turnManager.CurrentTurnUnit;
        m_deck.Add(NowTurnUnit, new List<Card>());
        m_hand.Add(NowTurnUnit, new List<Card>());
        m_graveyard.Add(NowTurnUnit, new List<Card>());
        m_tempQueueForCardsToBeAdded.Add(NowTurnUnit, new List<Card>());

        foreach (var uc in DataBase.UseCardDataBase.UseCardTable)
        {
            if (NowTurnUnit.IngameUnitID() == uc.Value.UnitID)
            {
                Card newCard = new Card();
                newCard.Initialize(NowTurnUnit, DataBase.CardTable(uc.Value.CardID));
                m_deck[NowTurnUnit].Add(newCard);
            }
        }

        foreach (var unit in m_turnManager.Units)
        {
            m_deck.Add(unit, new List<Card>());
            m_hand.Add(unit, new List<Card>());
            m_graveyard.Add(unit, new List<Card>());
            m_tempQueueForCardsToBeAdded.Add(unit, new List<Card>());

            foreach (var uc in DataBase.UseCardDataBase.UseCardTable)
            {
                if(unit.IngameUnitID() == uc.Value.UnitID)
                {
                    Card newCard = new Card();
                    newCard.Initialize(unit,DataBase.CardTable(uc.Value.CardID));
                    m_deck[unit].Add(newCard);
                }
            }
        }

        HandShaker();
        AddTemtQueueCardsToHand();
    }

    public void HandShaker()
    {
        Unit NowTurnUnit = m_turnManager.CurrentTurnUnit;

        // 현 턴이 몬스터면 동작 없음
        if (!NowTurnUnit.IsCharacter) return;

        // 핸드의 남은 카드를 묘지로 이동 
        foreach (var g in m_hand[NowTurnUnit])
        {
            m_graveyard[NowTurnUnit].Add(g);
        }
        m_hand[NowTurnUnit].Clear();

        // 임시 추가 카드도 묘지로 이동
        foreach (var gt in m_tempQueueForCardsToBeAdded[NowTurnUnit])
        {
            m_graveyard[NowTurnUnit].Add(gt);
        }
        m_tempQueueForCardsToBeAdded[NowTurnUnit].Clear();

        DrawCard(NowTurnUnit, 5);

        m_activeCardNum = m_hand[NowTurnUnit].Count;
    }

    public void DrawCard(Unit unit, int amount)
    {
        for (int i = 0; i < amount; i++) 
        {
            // 덱에 카드 없으면 묘지의 카드 모두 이동
            if (m_deck[unit].Count == 0)
            {
                foreach (var g in m_graveyard[unit])
                {
                    m_deck[unit].Add(g);
                }
                m_graveyard[unit].Clear();
            }
            // 랜덤으로 1장 뽑아 핸드에 넣고 덱에서 제외
            int address = UnityEngine.Random.Range(0, m_deck[unit].Count);
            var a = m_deck[unit][address];
            m_tempQueueForCardsToBeAdded[unit].Add(m_deck[unit][address]);
            m_deck[unit].RemoveAt(address);
        }
    }

    public override void SetTurn()
    {
        HandShaker();
        AddTemtQueueCardsToHand();
    }

    public override void SetRound()
    {
        
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
        Unit key = card.Unit;

        m_activeCardNum--;
        if (m_hand.ContainsKey(key))
        {
            m_hand[key].Remove(card);
        }
        if (m_graveyard.ContainsKey(key))
        {
            m_graveyard[key].Add(card);
        } 
    }

    public void AddTemtQueueCardsToHand()
    {
        Unit key = m_turnManager.CurrentTurnUnit;

        foreach (var card in m_tempQueueForCardsToBeAdded[key])
        {
            m_hand[key].Add(card);
        }
        m_tempQueueForCardsToBeAdded[m_turnManager.CurrentTurnUnit].Clear();
    }

    public void DrawNewHandCard()
    {
        m_cardUIManager.DrawNewHandCard();
    }
}
