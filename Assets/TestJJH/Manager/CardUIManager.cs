using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using static CardUIManager;
using System;

public class CardUIManager : BaseManager, IsynchronizeUI
{
    public class CardButton
    {
        public int s_num; // 버튼 번호
        public Button s_button; // 카드 버튼 
        public Card s_card;
        /*
        {
            ID = cardData.ID,
            Name = cardData.Name,
            Type = cardData.Type,
            Rank = cardData.Rank,
            Cost = cardData.Cost,
            TargetCount = cardData.TargetCount,
            Explanation = cardData.Explanation
        }
        */
        public bool s_isDrag;
        public bool s_isClick;
        public string s_key;
    }

    private bool m_isDrag;
    private bool m_isClick;

    [SerializeField]
    private GameObject m_cardGrid;

    [SerializeField]
    private Button m_openDeckButton;
    [SerializeField]
    private Button m_closeDeckButton;
    [SerializeField]
    private Button m_openGraveyardButton;
    [SerializeField]
    private Button m_closeGraveyardButton;

    [SerializeField]
    private GameObject m_deckPanel;
    [SerializeField]
    private GameObject m_graveyardPanel;

    [SerializeField]
    private GameObject m_deckButtonGrid;
    [SerializeField]
    private GameObject m_graveyardButtonGrid;

    [SerializeField]
    private Button[] m_cardButtons;
    private Dictionary<int, CardButton> m_cardButtonsDic;

    [SerializeField]
    private int m_activeCardNum;

    public int ActiveCardNum
    {
        get { return m_activeCardNum; }
    }

    public override void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;
        m_cardButtonsDic = new Dictionary<int, CardButton>();
        m_activeCardNum = 5;
        m_isDrag = false;
        m_isClick = false;

        Debug.Log("CardUIManagerInitialize");

        m_openDeckButton.onClick.AddListener(() =>
        {
            OpenDeck();
        });
        m_closeDeckButton.onClick.AddListener(() =>
        {
            CloseDeck();
        });
        m_openGraveyardButton.onClick.AddListener(() =>
        {
            OpenGraveyard();
        });
        m_closeGraveyardButton.onClick.AddListener(() =>
        {
            CloseGraveyard();
        });
    }

    public override void DataInitialize(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager)
    {
        
    }

    public void Synchronization(BaseManager baseManager)
    {
        string key = "";
        if (baseManager is CardManager cardManager)
        {
            if (cardManager.Hand.Count == 0)
            {
                for (int i = 0; i < m_cardButtons.Length; i++)
                {
                    m_cardButtons[i].gameObject.SetActive(false);
                }
                return;
            }
            else
            {
                for (int i = 0; i < m_cardButtons.Length; i++)
                {
                    m_cardButtons[i].gameObject.SetActive(true);
                }
            }

            //Deck 열람 초기화
            for (int i = 0; i < m_deckButtonGrid.transform.childCount; i++)
            {
                m_deckButtonGrid.transform.GetChild(i).gameObject.SetActive(true);
            }
            for (int i = 0; i < cardManager.Deck.Count; i++)
            {
                for (int k = 3; k > -1; k--)
                {
                    int cost = cardManager.Deck[i].CardData.Cost;
                    int p = (int)Math.Pow(10, k);
                    int c = cost / p;
                    if (c == 0) key += "0";
                    else
                    {
                        key += ((cardManager.Deck[i].CardData.Cost - 1) % 3).ToString();
                        Debug.Log("key : " + key);
                        break;
                    }
                }
                m_deckButtonGrid.transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Cost[key];
                m_deckButtonGrid.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Frame[key];
                key = "";
            }
            for (int i = cardManager.Deck.Count; i < m_deckButtonGrid.transform.childCount; i++)
            {
                m_deckButtonGrid.transform.GetChild(i).gameObject.SetActive(false);
            }

            //Graveyard 열람 초기화
            for (int i = 0; i < m_graveyardButtonGrid.transform.childCount; i++)
            {
                m_graveyardButtonGrid.transform.GetChild(i).gameObject.SetActive(true);
            }
            for (int i = 0; i < cardManager.Graveyard.Count; i++)
            {
                for (int k = 3; k > -1; k--)
                {
                    int cost = cardManager.Graveyard[i].CardData.Cost;
                    int p = (int)Math.Pow(10, k);
                    int c = cost / p;
                    if (c == 0) key += "0";
                    else
                    {
                        key += (cardManager.Graveyard[i].CardData.Cost % 3).ToString();
                        Debug.Log("key : " + key);
                        break;
                    }
                }
                m_graveyardButtonGrid.transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Cost[key];
                m_graveyardButtonGrid.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Frame[key];
                key = "";
            }
            for (int i = cardManager.Graveyard.Count; i < m_graveyardButtonGrid.transform.childCount; i++)
            {
                m_graveyardButtonGrid.transform.GetChild(i).gameObject.SetActive(false);
            }

            //Hand 기준으로 카드 기능 적용
            for (int i = 0; i < m_cardButtons.Length; i++)
            {
                CardButton newCardButton = new CardButton();
                newCardButton.s_num = i;
                newCardButton.s_button = m_cardButtons[i];
                newCardButton.s_card = cardManager.Hand[i];

                newCardButton.s_key = "";
                for (int k = 3; k > -1; k--)
                {
                    int cost = cardManager.Hand[i].CardData.Cost;
                    int p = (int)Math.Pow(10, k);
                    int c = cost / p;
                    if (c == 0) newCardButton.s_key += "0";
                    else
                    {
                        newCardButton.s_key += (cardManager.Hand[i].CardData.Cost % 3).ToString();
                        Debug.Log("key : " + newCardButton.s_key);
                        break;
                    }
                }
                newCardButton.s_button.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Cost[newCardButton.s_key];
                newCardButton.s_button.transform.GetComponent<Image>().sprite = GameManager.Instance.Card_Frame[newCardButton.s_key];
                newCardButton.s_button.transform.GetChild(1).GetComponent<Text>().text = newCardButton.s_card.CardData.Name;
                newCardButton.s_isDrag = false;
                newCardButton.s_isClick = false;
                m_cardButtonsDic.Add(i, newCardButton);

                EventTrigger EventTrigger = newCardButton.s_button.GetComponent<EventTrigger>();

                EventTrigger.Entry Entry = new EventTrigger.Entry();
                Entry.eventID = EventTriggerType.PointerEnter;
                Entry.callback.AddListener((data) => {
                    if (m_isClick || m_isDrag)
                    {
                        return;
                    }
                    MouseOver(m_cardButtonsDic[newCardButton.s_num].s_button);
                });

                EventTrigger.Entry Exit = new EventTrigger.Entry();
                Exit.eventID = EventTriggerType.PointerExit;
                Exit.callback.AddListener((data) => {
                    if (m_cardButtonsDic[newCardButton.s_num].s_isClick)
                    {
                        return;
                    }
                    MouseExit(m_cardButtonsDic[newCardButton.s_num].s_button);
                });

                EventTrigger.Entry Click = new EventTrigger.Entry();
                Click.eventID = EventTriggerType.PointerClick;
                Click.callback.AddListener((data) => {
                    if (m_cardButtonsDic[newCardButton.s_num].s_isDrag || m_isDrag)
                    {
                        return;
                    }

                    SetCardEvent(newCardButton);
                    m_isClick = true;
                    if (!m_cardButtonsDic[newCardButton.s_num].s_isClick)
                    {
                        m_cardButtonsDic[newCardButton.s_num].s_isClick = true;
                        MouseOver(m_cardButtonsDic[newCardButton.s_num].s_button);
                        return;
                    }
                    else
                    {
                        UseCard(cardManager, newCardButton);
                        m_cardButtonsDic[newCardButton.s_num].s_isClick = false;
                        m_isClick = false;
                        SetCardEvent();
                    }
                });
                EventTrigger.triggers.Add(Click);


                EventTrigger.triggers.Add(Entry);
                EventTrigger.triggers.Add(Exit);

                
                EventTrigger.Entry Drag = new EventTrigger.Entry();
                Drag.eventID = EventTriggerType.Drag;
                Drag.callback.AddListener((data) => {
                    SetCardEvent(newCardButton);
                    MouseOver(m_cardButtonsDic[newCardButton.s_num].s_button);
                    m_isDrag = true;
                    m_cardButtonsDic[newCardButton.s_num].s_isDrag = true;
                    m_cardButtonsDic[newCardButton.s_num].s_isClick = false;
                    Vector3 mousePostion = Input.mousePosition;
                    mousePostion.x -= newCardButton.s_button.transform.parent.localPosition.x;
                    newCardButton.s_button.gameObject.transform.localPosition = mousePostion;
                });

                EventTrigger.Entry EndDrag = new EventTrigger.Entry();
                EndDrag.eventID = EventTriggerType.EndDrag;
                EndDrag.callback.AddListener((data) => {
                    SetCardEvent(newCardButton);
                    if (newCardButton.s_button.gameObject.transform.localPosition.y < 400)
                    {
                        MouseExit(newCardButton.s_button);
                        m_cardButtonsDic[newCardButton.s_num].s_isDrag = false;
                        return;
                    }
                    UseCard(cardManager, newCardButton);
                    SetCardEvent();
                });
                EventTrigger.triggers.Add(Drag);
                EventTrigger.triggers.Add(EndDrag);
            }

            m_activeCardNum = m_cardButtons.Length;
        }
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager, CardManager cardManager)
    {
        string key = "";
        foreach (var cardButton in m_cardButtonsDic)
        {
            MouseExit(cardButton.Value.s_button);
            cardButton.Value.s_button.GetComponent<EventTrigger>().triggers.Clear();
            cardButton.Value.s_button.onClick.RemoveAllListeners();
            cardButton.Value.s_button.gameObject.SetActive(true);
        }
        m_cardButtonsDic.Clear();

        //Deck 열람 초기화
        for (int i = 0; i < m_deckButtonGrid.transform.childCount; i++)
        {
            m_deckButtonGrid.transform.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = 0; i < cardManager.Deck.Count; i++)
        {
            for (int k = 3; k > -1; k--)
            {
                int cost = cardManager.Deck[i].CardData.Cost;
                int p = (int)Math.Pow(10, k);
                int c = cost / p;
                if (c == 0) key += "0";
                else
                {
                    key += (cardManager.Deck[i].CardData.Cost % 3).ToString();
                        Debug.Log("key : " + key);
                    break;
                }
            }
            m_deckButtonGrid.transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Cost[key];
            m_deckButtonGrid.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Frame[key];
            key = "";
        }
        for (int i = cardManager.Deck.Count; i < m_deckButtonGrid.transform.childCount; i++)
        {
            m_deckButtonGrid.transform.GetChild(i).gameObject.SetActive(false);
        }

        //Graveyard 열람 초기화
        for (int i = 0; i < m_graveyardButtonGrid.transform.childCount; i++)
        {
            m_graveyardButtonGrid.transform.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = 0; i < cardManager.Graveyard.Count; i++)
        {
            for (int k = 3; k > -1; k--)
            {
                int cost = cardManager.Graveyard[i].CardData.Cost;
                int p = (int)Math.Pow(10, k);
                int c = cost / p;
                if (c == 0) key += "0";
                else
                {
                    key += (cardManager.Graveyard[i].CardData.Cost % 3).ToString();
                        Debug.Log("key : " + key);
                    break;
                }
            }
            m_graveyardButtonGrid.transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Cost[key];
            m_graveyardButtonGrid.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Frame[key];
            key = "";
        }
        for (int i = cardManager.Graveyard.Count; i < m_graveyardButtonGrid.transform.childCount; i++)
        {
            m_graveyardButtonGrid.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void MouseOver(Button Card)
    {
        Card.gameObject.transform.localPosition = new Vector3(0, 100, 0);
        Card.gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
    }

    public void MouseExit(Button Card)
    {
        Card.gameObject.transform.localPosition = (new Vector3(0, 0, 0));
        Card.gameObject.transform.localScale = new Vector3(1, 1, 1);
    }

    public void OpenDeck()
    {
        m_deckPanel.SetActive(true);
    }

    public void CloseDeck()
    {
        m_deckPanel.SetActive(false);
    }

    public void OpenGraveyard()
    {
        m_graveyardPanel.SetActive(true);
    }

    public void CloseGraveyard()
    {
        m_graveyardPanel.SetActive(false);
    }

    public void UseCard(CardManager cardManager, CardButton newCardButton)
    {
        m_masterManager.UseCard(newCardButton.s_button, newCardButton.s_card.CardData.Cost);
        m_cardButtonsDic[newCardButton.s_num].s_button.gameObject.transform.localPosition = (new Vector3(0, 0, 0));
        m_cardButtonsDic[newCardButton.s_num].s_card.Execute();
        // graveyard에 추가
        cardManager.Graveyard.Add(m_cardButtonsDic[newCardButton.s_num].s_card);
        m_graveyardButtonGrid.transform.GetChild(cardManager.Graveyard.Count).gameObject.SetActive(true);
        m_graveyardButtonGrid.transform.GetChild(cardManager.Graveyard.Count).GetChild(0).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Cost[newCardButton.s_key];
        m_graveyardButtonGrid.transform.GetChild(cardManager.Graveyard.Count).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.Card_Frame[newCardButton.s_key];
        // hand에서 삭제
        cardManager.Hand.Remove(m_cardButtonsDic[newCardButton.s_num].s_card);

        m_activeCardNum--;
    }

    public void SetCardEvent()
    {
        m_isClick = false;
        m_isDrag = false;
        foreach (var cardButton in m_cardButtonsDic)
        {
            cardButton.Value.s_isClick = false;
            cardButton.Value.s_isDrag = false;
            MouseExit(cardButton.Value.s_button);
        }
    }

    public void SetCardEvent(CardButton newCardButton)
    {
        m_isClick = false;
        m_isDrag = false;
        foreach (var cardButton in m_cardButtonsDic)
        {
            if(cardButton.Value == newCardButton)
            {
                continue;
            }
            cardButton.Value.s_isClick = false;
            cardButton.Value.s_isDrag = false;
            MouseExit(cardButton.Value.s_button);
        }
    }
}
