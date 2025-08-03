using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class CardManager : BaseManager
{
    private List<Card> m_hand;
    private List<List<Card>> m_deck;
    private List<List<Card>> m_graveyard;
    private int m_turnCounter;
    public List<Card> Hand
    {
        get { return m_hand; }
    }
    public List<Card> Deck
    {
        get { return m_deck[m_turnCounter]; }
    }
    public List<Card> Graveyard
    {
        get { return m_graveyard[m_turnCounter]; }
    }
    public override void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;
        m_turnCounter = 1;
    }

    public override void DataInitialize(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager)
    {
        m_hand = new List<Card>();

        m_deck = new List<List<Card>>(characterManager.Character.Count);
        m_graveyard = new List<List<Card>>(characterManager.Character.Count);

        for (int a = 0; a < characterManager.Character.Count; a++)
        {
            List<Card> Deck = new List<Card>();
            foreach(var c in DontDestroyOnLoadManager.Instance.UseCard.UseCard)
            {
                if (turnManager.CurrentTurnUnit is CharacterData character)
                {
                    if (character.PrototypeUnitID == c.Value.PrototypeUnitID)
                    {
                        Card newCard = new Card();
                        newCard.Initialize(DontDestroyOnLoadManager.Instance.Card.Card[c.Value.CardID]);
                        Deck.Add(newCard);
                    }
                }
            }
            m_deck.Add(Deck);
            List<Card> graveyard = new List<Card>();
            m_graveyard.Add(graveyard);
        }
        HandShake(turnManager, characterManager);
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager, CardManager cardManager)
    {
        HandShake(turnManager, characterManager);
    }

    private void HandShake(TurnManager turnManager, CharacterManager characterManager)
    {
        // 기존 핸드 패 삭제
        foreach(var h in m_hand)
        {
            m_graveyard[(m_turnCounter % characterManager.Character.Count)].Add(h);
        }
        m_hand.Clear();

        int position = 0;
        if (!(turnManager.CurrentTurnUnit is CharacterData)) {
            Debug.Log("몬스터 턴");
            return;
        }
        else
        {
            if (turnManager.CurrentTurnUnit is CharacterData currentTurnCharacter)
            {
                var key = (currentTurnCharacter.UserID, currentTurnCharacter.PrototypeUnitID, currentTurnCharacter.InstanceID);
                Debug.Log(key);
                int i = 0;
                foreach (var character in characterManager.Character)
                {
                    var characterkey = (character.UserID, character.PrototypeUnitID, character.InstanceID);
                    if (key.Equals(characterkey))
                    {
                        position = i;
                        break;
                    }
                    i++;    
                }
            }
        }

        // 핸드 패 새로 생성
        if (m_deck[position].Count < 5 && m_deck[position].Count >0)
        {
            foreach(var d in m_deck[position])
            {
                m_hand.Add(d);
            }
            m_deck[position].Clear();
            foreach (var g in m_graveyard[position])
            {
                m_deck[position].Add(g);
            }
            m_graveyard[position].Clear();
        }
        else
        {
            for(int i=0;i<5;i++)
            {
                int address = Random.Range(0, m_deck[position].Count);
                m_hand.Add(m_deck[position][address]);
                m_deck[position].RemoveAt(address);
            }
        }

        // 패 배분이 끝나고 덱이 비어있으면 다시 채움
        if(m_deck[position].Count < 1)
        {
            foreach(var g in m_graveyard[position])
            {
                m_deck[position].Add(g);
            }
            m_graveyard[position].Clear();
        }
        m_turnCounter = position;
    }

    public void CharacterDying(int poisition)
    {
        m_deck.RemoveAt(poisition);
        m_graveyard.RemoveAt(poisition);
    }
}
