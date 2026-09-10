using System.Collections.Generic;
using UnityEngine;

public class CardManager : BaseManager
{
    struct UnitCardPair
    {
        public Unit s_unit;
        public List<Card> s_card;
    };

    private UnitCardPair m_hand;
    private Dictionary<Unit,List<Card>> m_deck;
    private Dictionary<Unit,List<Card>> m_graveyard;

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
    public override void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;
    }

    public override void DataInitialize(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager)
    {
        /***************test Code about Struct**************/
        m_hand = new UnitCardPair();
        m_hand.s_card = new List<Card>();

        m_deck = new Dictionary<Unit,List<Card>>();
        m_graveyard = new Dictionary<Unit,List<Card>>();

        foreach (var character in characterManager.Character)
        {
            UnitCardPair pair = new UnitCardPair();
            pair.s_unit = character;
            pair.s_card = new List<Card>();
            if (character is CharacterTableData CTD)
            {
                foreach (var uc in DataBase.UseCardDataBase.UseCard)
                {
                    if(CTD.ID == uc.Value.PrototypeUnitID)
                    {
                        Card newCard = new Card();
                        newCard.Initialize(pair.s_unit, DataBase.CardTable(uc.Value.CardID), m_masterManager.SkillScheduleManager);
                        pair.s_card.Add(newCard);
                    }
                }
            }
            m_deck.Add(pair.s_unit,pair.s_card);
            m_graveyard.Add(pair.s_unit,new List<Card>());
        }
        HandShaker(turnManager,characterManager);
    }

    public void HandShaker(TurnManager turnManager, CharacterManager characterManager)
    {
        foreach(var g in m_hand.s_card)
        {
            m_graveyard[m_hand.s_unit].Add(g);
        }
        m_hand.s_card.Clear();

        if (turnManager.CurrentTurnUnit is not CharacterTableData c)
        {
            return;
        }

        m_hand.s_unit = turnManager.CurrentTurnUnit;
        Unit key = m_hand.s_unit;

        if (m_deck[key].Count < 6)
        {
            foreach (var d in m_deck[key])
            {
                m_hand.s_card.Add(d);
            }
            m_deck[key].Clear();
            foreach (var g in m_graveyard[key])
            {
                m_deck[key].Add(g);
            }
            m_graveyard[key].Clear();
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                int address = Random.Range(0, m_deck[key].Count);
                m_hand.s_card.Add(m_deck[key][address]);
                m_deck[key].RemoveAt(address);
            }
        }

        if (m_deck[key].Count < 1)
        {
            foreach (var g in m_graveyard[key])
            {
                m_deck[key].Add(g);
            }
            m_graveyard[key].Clear();
        }
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager, CardManager cardManager)
    {
        HandShaker(turnManager, characterManager);
    }

    public void CharacterDying(Unit unit)
    {
        m_deck[unit].Clear();
        m_graveyard[unit].Clear();

        m_deck.Remove(unit);
        m_graveyard.Remove(unit);
    }
}
