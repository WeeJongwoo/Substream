
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Resources;
using Spine;
using Unity.VisualScripting;


public class CardUIManager : BaseUI<CardManager>
{
    [SerializeField]
    private UnitSelectHelper m_unitSelectHelper;
    [SerializeField]
    private Transform m_cardUIManager2;

    private TurnManager m_turnManager;

    private int m_thisTurnMaxCardCount;

    [Header("NotEnough")]
    [SerializeField]
    private Image m_textMessageImage;
    [SerializeField]
    private Text m_textMessageText;

    [Header("Hand")]
    [SerializeField]
    private HandPanelSlot m_handPanelSlot;
    [SerializeField]
    private GridLayoutGroup m_layoutGroup;

    [Header("UIButton")]
    [SerializeField]
    private Button m_viewDeckButton;
    [SerializeField]
    private Button m_closeCardPanelButton;

    [Header("DeckAndGraveyard")]
    [SerializeField]
    private CardPanelSlot m_deckGraveyardCardPanelSlotPrefab;
    [SerializeField]
    private Transform m_deckGraveyardPanel;

    [Header("Deck")]
    [SerializeField]
    private Button m_selectDeckButton;
    [SerializeField]
    private Transform m_deckPanel;
    [SerializeField]
    private Transform m_deckSlot;
    private Dictionary<Unit,DeckGraveyardCardPanelSlot> m_deckPanelSlot;

    [Header("Graveyard")]
    [SerializeField]
    private Button m_selectGraveyardButton;
    [SerializeField]
    private Transform m_graveyardPanel;
    [SerializeField]
    private Transform m_graveyardSlot;
    private Dictionary<Unit,DeckGraveyardCardPanelSlot> m_graveyardPanelSlot;

    private Coroutine m_selectUnitCor = null;

    public override void Initialize()
    {
        m_handPanelSlot.gameObject.SetActive(true);

        m_handPanelSlot.gameObject.name = "HandPanelSlot";
        m_handPanelSlot.Initialize();
        
        DeckAndGraveyardPanelInitialize();

        m_unitSelectHelper.Initialize();
    }

    private void DeckAndGraveyardPanelInitialize()
    {
        m_deckPanelSlot = new Dictionary<Unit, DeckGraveyardCardPanelSlot>();
        m_graveyardPanelSlot = new Dictionary<Unit, DeckGraveyardCardPanelSlot>();

        m_deckGraveyardPanel.gameObject.SetActive(false);

        m_viewDeckButton.onClick.RemoveAllListeners();
        m_selectDeckButton.onClick.RemoveAllListeners();
        m_selectGraveyardButton.onClick.RemoveAllListeners();
        m_closeCardPanelButton.onClick.RemoveAllListeners();

        m_viewDeckButton.onClick.AddListener(() => {
            m_deckGraveyardPanel.gameObject.SetActive(true);
            m_deckPanel.gameObject.SetActive(true);
            m_graveyardPanel.gameObject.SetActive(false);
        });

        m_closeCardPanelButton.onClick.AddListener(() => {
            m_deckGraveyardPanel.gameObject.SetActive(false);
        });

        m_selectDeckButton.onClick.AddListener(() =>
        {
            m_deckPanel.gameObject.SetActive(true);
            m_graveyardPanel.gameObject.SetActive(false);
        });
        m_selectGraveyardButton.onClick.AddListener(() =>
        {
            m_deckPanel.gameObject.SetActive(false);
            m_graveyardPanel.gameObject.SetActive(true);
        });
    }

    private Coroutine m_notEnoughAetherTextMessageEnumerator = null;
    private Coroutine m_itIsAWrongSelectTextMessageEnumerator = null;

    public override void DataInitialize()
    {
        DeckAndGraveyardPanelDataInitialize();
        m_unitSelectHelper.DataInitialize();
        m_turnManager = m_masterManager.TurnManager;
    }

    private void DeckAndGraveyardPanelDataInitialize()
    {
        foreach(var c in m_model.Deck)
        {
            if (!c.Key.IsCharacter) continue;
            var newSlot = Instantiate(m_deckGraveyardCardPanelSlotPrefab).GetComponent<DeckGraveyardCardPanelSlot>();
            newSlot.transform.SetParent(m_deckSlot, true);
            m_deckPanelSlot.Add(c.Key, newSlot);
        }
        foreach (var c in m_model.Graveyard)
        {
            if (!c.Key.IsCharacter) continue;
            var newSlot = Instantiate(m_deckGraveyardCardPanelSlotPrefab).GetComponent<DeckGraveyardCardPanelSlot>();
            newSlot.transform.SetParent(m_graveyardSlot, true);
            m_graveyardPanelSlot.Add(c.Key, newSlot);
        }
        
        foreach (var c in m_deckPanelSlot)
        {
            c.Value.Initialize();
        }
        foreach (var c in m_graveyardPanelSlot)
        {
            c.Value.Initialize();
        }
    }

    public override void Synchronization()
    {
        HandGridInit(m_model.NowHand, this.ResourcesManager);

        foreach (var d in m_deckPanelSlot)
        {
            d.Value.Synchronization();
        }
        foreach (var g in m_graveyardPanelSlot)
        {
            g.Value.Synchronization();
        }

        DeckAndGraveyardGridUpdate();
    }

    public override void UseCard(Card card)
    {
        DeckAndGraveyardGridUpdate();
    }

    public override void SetTurn()
    {
        HandGridInit(m_model.NowHand, this.ResourcesManager);
        DeckAndGraveyardGridUpdate();

        if(m_selectUnitCor != null)
        {
            StopCoroutine(m_selectUnitCor);
            m_unitSelectHelper.SetSelector();
            m_unitSelectHelper.TurnOffSelectPanel();
        }
    }

    public void DrawNewHandCard()
    {
        foreach(var card in m_model.TemtQueueForCardsToBeAdded)
        {
            DrawCard(card);
        }
    }

    private void DrawCard(Card card)
    {
        CardSlot newSlot = m_handPanelSlot.GetObject();
        newSlot.gameObject.name = "새로 드로우한 카드";
        newSlot.ReInit(m_thisTurnMaxCardCount, card, card.CardData.Cost % 3,
            ResourcesManager.Card_Cost(card.CardData.Cost % 3),
            ResourcesManager.Card_Image(1001),
            ResourcesManager.Card_Frame(card.CardData.Cost % 3)
            );
        m_handPanelSlot.AddSlot(newSlot);
        m_thisTurnMaxCardCount++;

        // 카드 버튼의 이벤트 트리거 로드
        EventTrigger eventTrigger = newSlot.Event;

        // 마우스가 버튼 위에 올라가는 이벤트
        EventTrigger.Entry Entry = new EventTrigger.Entry();
        Entry.eventID = EventTriggerType.PointerEnter;
        Entry.callback.AddListener((data) => {
            m_handPanelSlot.OnMouseCardEvent(newSlot);
        });

        // 마우스가 버튼 위에서 내려가는 이벤트
        EventTrigger.Entry Exit = new EventTrigger.Entry();
        Exit.eventID = EventTriggerType.PointerExit;
        Exit.callback.AddListener((data) => {
            if (!newSlot.s_isReady)
            {
                m_handPanelSlot.SetCardEvent(newSlot);
            }
        });

        // 버튼을 클릭하는 이벤트
        EventTrigger.Entry ClickEnter = new EventTrigger.Entry();
        ClickEnter.eventID = EventTriggerType.PointerDown;
        ClickEnter.callback.AddListener((UnityEngine.Events.UnityAction<BaseEventData>)((data) => {
        }));

        EventTrigger.Entry ClickExit = new EventTrigger.Entry();
        ClickExit.eventID = EventTriggerType.PointerUp;
        ClickExit.callback.AddListener((UnityEngine.Events.UnityAction<BaseEventData>)((data) => {
            if (newSlot.s_isReady)
            {
                StopCoroutine(m_selectUnitCor);
                m_unitSelectHelper.SetSelector();
                m_unitSelectHelper.TurnOffSelectPanel();
                newSlot.s_isReady = false;
                return;
            }

            m_handPanelSlot.SetCardEvent();
            m_handPanelSlot.OnMouseCardEvent(newSlot);
            newSlot.s_isReady = true;
            if (m_selectUnitCor != null)
            {
                StopCoroutine(m_selectUnitCor);
                m_selectUnitCor = StartCoroutine(SelectUnit(newSlot));
            }
            else
            {
                m_selectUnitCor = StartCoroutine(SelectUnit(newSlot));
            }
        }));

        eventTrigger.triggers.Add(ClickEnter);
        eventTrigger.triggers.Add(ClickExit);
        eventTrigger.triggers.Add(Entry);
        eventTrigger.triggers.Add(Exit);

        m_thisTurnMaxCardCount++;
    }

    public void HandGridInit(List<Card> cards, ResourceManager resourceManager)
    {
        m_handPanelSlot.Synchronization();

        for (int i = 0; i < cards.Count; i++)
        {
            CardSlot newSlot = m_handPanelSlot.GetObject();
            newSlot.ReInit(i, cards[i], cards[i].CardData.Cost % 3,
                resourceManager.Card_Cost(cards[i].CardData.Cost % 3),
                resourceManager.Card_Image(1001),
                resourceManager.Card_Frame(cards[i].CardData.Cost % 3)
                );
            m_handPanelSlot.AddSlot(newSlot);
            m_thisTurnMaxCardCount++;

            // 카드 버튼의 이벤트 트리거 로드
            EventTrigger eventTrigger = newSlot.Event;

            // 마우스가 버튼 위에 올라가는 이벤트
            EventTrigger.Entry Entry = new EventTrigger.Entry();
            Entry.eventID = EventTriggerType.PointerEnter;
            Entry.callback.AddListener((data) => {
                m_handPanelSlot.OnMouseCardEvent(newSlot);
            });

            // 마우스가 버튼 위에서 내려가는 이벤트
            EventTrigger.Entry Exit = new EventTrigger.Entry();
            Exit.eventID = EventTriggerType.PointerExit;
            Exit.callback.AddListener((data) => {
                if (!newSlot.s_isReady)
                {
                    m_handPanelSlot.SetCardEvent(newSlot);
                }
            });

            // 버튼을 클릭하는 이벤트
            EventTrigger.Entry ClickEnter = new EventTrigger.Entry();
            ClickEnter.eventID = EventTriggerType.PointerDown;
            ClickEnter.callback.AddListener((UnityEngine.Events.UnityAction<BaseEventData>)((data) => {
            }));

            EventTrigger.Entry ClickExit = new EventTrigger.Entry();
            ClickExit.eventID = EventTriggerType.PointerUp;
            ClickExit.callback.AddListener((UnityEngine.Events.UnityAction<BaseEventData>)((data) => {
                if (newSlot.s_isReady)
                {
                    StopCoroutine(m_selectUnitCor);
                    m_unitSelectHelper.SetSelector();
                    m_unitSelectHelper.TurnOffSelectPanel();
                    newSlot.s_isReady = false;
                    return;
                }

                m_handPanelSlot.SetCardEvent();
                m_handPanelSlot.OnMouseCardEvent(newSlot);
                newSlot.s_isReady = true;
                if (m_selectUnitCor != null)
                {
                    StopCoroutine(m_selectUnitCor);
                    m_selectUnitCor = StartCoroutine(SelectUnit(newSlot));
                }
                else
                {
                    m_selectUnitCor = StartCoroutine(SelectUnit(newSlot));
                }
            }));

            eventTrigger.triggers.Add(ClickEnter);
            eventTrigger.triggers.Add(ClickExit);
            eventTrigger.triggers.Add(Entry);
            eventTrigger.triggers.Add(Exit);
        }
    }

    public IEnumerator SelectUnit(CardSlot cardSlot)
    {
        bool done = false;
        m_unitSelectHelper.TurnOffSelectScope();
        while (!done)
        {
            if (cardSlot.s_card.CardData.TargetType != ETargetType.E_NONE)
            {

            }
            m_unitSelectHelper.TurnOnSelectPanel();
            m_unitSelectHelper.KeyInput(cardSlot.s_card.CardData.TargetType);
            if (m_unitSelectHelper.GetSelector != 401)
            {
                m_handPanelSlot.SetCardEvent(cardSlot);

                done = UseCardEvent(cardSlot);
                if(done)
                {
                    m_unitSelectHelper.SetSelector();
                }
                else
                {
                    m_handPanelSlot.OnMouseCardEvent(cardSlot);
                }
            }
            yield return null;
        }
        yield return null;
    }

    public void GridInit(List<Card> cards, ResourceManager resourceManager, CardPanelSlot cardPanelSlot)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            CardSlot newSlot = cardPanelSlot.GetObject();
            newSlot.ReInit(i, cards[i], cards[i].CardData.Cost % 3,
                resourceManager.Card_Cost(cards[i].CardData.Cost % 3),
                resourceManager.Card_Image(1001),
                resourceManager.Card_Frame(cards[i].CardData.Cost % 3)
                );
            cardPanelSlot.AddSlot(newSlot);
            newSlot.Button.transform.localPosition = (new Vector3(0, 0, 0));
            newSlot.Button.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

            // 카드 버튼의 이벤트 트리거 로드
            EventTrigger EventTrigger = newSlot.Event;

            // 마우스가 버튼 위에 올라가는 이벤트
            EventTrigger.Entry Entry = new EventTrigger.Entry();
            Entry.eventID = EventTriggerType.PointerEnter;
            Entry.callback.AddListener((data) => {
                cardPanelSlot.OnMouseCardEvent(newSlot);
            });

            // 마우스가 버튼 위에서 내려가는 이벤트
            EventTrigger.Entry Exit = new EventTrigger.Entry();
            Exit.eventID = EventTriggerType.PointerExit;
            Exit.callback.AddListener((data) => {
                cardPanelSlot.SetCardEvent(newSlot);
            });

            EventTrigger.triggers.Add(Entry);
            EventTrigger.triggers.Add(Exit);
        }
    }

    public override void UnitDying(Unit unit)
    {
        
    }

    public bool UseCardEvent(CardSlot cardSlot)
    {
        bool result = false;
        var select = m_unitSelectHelper.GetSelector;

        if (select == 401) return false;

        if (select == 402 && !m_turnManager.IsTurnInputLocked) // 버리기 동작
        {
            m_model.UseCard(cardSlot.s_card);
            UseCard(cardSlot.s_card);
            m_handPanelSlot.ReleaseObject(cardSlot);
            m_unitSelectHelper.SetSelector();
            m_unitSelectHelper.TurnOffSelectPanel();
            return true;
        }
        else if(select == 0)
        {
            if (m_masterManager.UseCard(cardSlot.s_card, select))
            {
                m_handPanelSlot.ReleaseObject(cardSlot);
                result = true;
            }
        }
        else
        {
            int a = cardSlot.s_card.Unit.IsCharacter ? 1 : -1;
            int b = cardSlot.s_card.CardData.TargetType == ETargetType.E_ENEMY ? -1 : 1;

            bool thisSkillIsTargetAllies = a * b == 1 ? true : false;

            if (thisSkillIsTargetAllies != (select > 0))
            {
                if (m_itIsAWrongSelectTextMessageEnumerator != null) StopCoroutine(m_itIsAWrongSelectTextMessageEnumerator);
                m_itIsAWrongSelectTextMessageEnumerator = StartCoroutine(ItIsAWrongSelectTextMessage(m_textMessageImage, m_textMessageText));

                return false;
            }

            if (m_masterManager.UseCard(cardSlot.s_card, select))
            {
                m_handPanelSlot.ReleaseObject(cardSlot);
                result = true;
            }
            else
            {
                if (m_notEnoughAetherTextMessageEnumerator != null) StopCoroutine(m_notEnoughAetherTextMessageEnumerator);
                m_notEnoughAetherTextMessageEnumerator = StartCoroutine(NotEnoughAetherTextMessage(m_textMessageImage, m_textMessageText));
            }
            m_unitSelectHelper.SetSelector();
        }

        m_handPanelSlot.SetCardEvent(cardSlot);
        
        m_unitSelectHelper.TurnOffSelectPanel();

        m_selectUnitCor = null;
        cardSlot.s_isReady = false;
        return result;
    }

    public IEnumerator NotEnoughAetherTextMessage(Image image, Text text)
    {
        float _timer = -0.30f;
        bool trigger = true;
        image.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        text.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        text.text = "에테르가 부족합니다";

        float t;
        while (trigger)
        {
            _timer += Time.deltaTime;
            t = Mathf.Clamp01(_timer / 2.0f);
            float angle = t / 2 * Mathf.PI;

            float at = Mathf.Sin(angle) * Mathf.Sin(angle) * Mathf.Sin(angle);
            float alpha = Mathf.Lerp(1, 0, at);
            alpha *= alpha * alpha * alpha * alpha * alpha * alpha;
            image.color = new Color(0.0f, 0.0f, 0.0f, alpha);
            text.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            
            if (t >= 1.0f)
            {
                _timer = 0f;
                trigger = false;
            }
            yield return null;
        }
        m_notEnoughAetherTextMessageEnumerator = null;
    }

    public void TurnInputLockOn()
    {
        m_handPanelSlot.Synchronization();
        if (m_selectUnitCor != null)
        {
            StopCoroutine(m_selectUnitCor);
            m_unitSelectHelper.SetSelector();
            m_unitSelectHelper.TurnOffSelectPanel();
        }
    }

    public IEnumerator ItIsAWrongSelectTextMessage(Image image, Text text)
    {
        float _timer = -0.30f;
        bool trigger = true;
        image.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        text.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        text.text = "잘못된 타겟입니다";

        float t;
        while (trigger)
        {
            _timer += Time.deltaTime;
            t = Mathf.Clamp01(_timer / 2.0f);
            float angle = t / 2 * Mathf.PI;

            float at = Mathf.Sin(angle) * Mathf.Sin(angle) * Mathf.Sin(angle);
            float alpha = Mathf.Lerp(1, 0, at);
            alpha *= alpha * alpha * alpha * alpha * alpha * alpha;
            image.color = new Color(0.0f, 0.0f, 0.0f, alpha);
            text.color = new Color(1.0f, 1.0f, 1.0f, alpha);

            if (t >= 1.0f)
            {
                _timer = 0f;
                trigger = false;
            }
            yield return null;
        }
        m_itIsAWrongSelectTextMessageEnumerator = null;
    }

    public void SetCardEvent(CardPanelSlot panel)
    {
        panel.SetCardEvent();
    }

    public void SetCardEvent(CardPanelSlot panel, CardSlot cardSlot)
    {
        panel.SetCardEvent(cardSlot);
    }

    private void DeckAndGraveyardGridUpdate()
    {
        foreach (var c in m_deckPanelSlot)
        {
            GridInit(m_model.Deck[c.Key], this.ResourcesManager, c.Value);
        }
        foreach (var c in m_graveyardPanelSlot)
        {
            GridInit(m_model.Graveyard[c.Key], this.ResourcesManager, c.Value);
        }
    }
}
