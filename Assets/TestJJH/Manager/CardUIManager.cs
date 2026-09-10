
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;


public class CardUIManager : BaseUI<CardManager>
{
    [SerializeField]
    private Transform m_cardUIManager2;

    private bool m_isDrag;
    private bool m_isClick;
    private int m_thisTurnMaxCardCount;

    [SerializeField]
    private CardPanelSlot m_cardPanelSlotPrefab;
    [SerializeField]
    private Image m_notEnoughAetherImage;
    [SerializeField]
    private Text m_notEnoughAetherText;

    [SerializeField]
    private CardPanelSlot m_handPanelSlot;
    [SerializeField]
    private CardPanelSlot m_deckPanelSlot;
    [SerializeField]
    private CardPanelSlot m_graveyardPanelSlot;

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

    public override void Initialize()
    {
        m_deckPanelSlot = Instantiate(m_cardPanelSlotPrefab, m_cardUIManager2);
        m_graveyardPanelSlot = Instantiate(m_cardPanelSlotPrefab, m_cardUIManager2);

        m_deckPanel = m_deckPanelSlot.gameObject;
        m_graveyardPanel = m_graveyardPanelSlot.gameObject;

        m_closeDeckButton = m_deckPanelSlot.CloseButton;
        m_closeGraveyardButton = m_graveyardPanelSlot.CloseButton;

        m_handPanelSlot.SetBGI(new Color(1, 1, 1, 0));

        m_handPanelSlot.TurnOn();
        m_deckPanelSlot.TurnOff();
        m_graveyardPanelSlot.TurnOff();

        m_handPanelSlot.gameObject.name = "HandPanelSlot";
        m_deckPanelSlot.gameObject.name = "DeckPanelSlot";
        m_graveyardPanelSlot.gameObject.name = "GraveyardPanelSlot";

        m_handPanelSlot.Initialize();
        m_deckPanelSlot.Initialize();
        m_graveyardPanelSlot.Initialize();

        m_deckPanelSlot.ScrollRect.horizontal = false;
        m_deckPanelSlot.ScrollRect.vertical = true;

        m_graveyardPanelSlot.ScrollRect.horizontal = false;
        m_graveyardPanelSlot.ScrollRect.vertical = true;

        m_isDrag = false;
        m_isClick = false;

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

    private Coroutine enumerator;

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.A) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(enumerator != null) StopCoroutine(enumerator);
            enumerator = StartCoroutine(NotEnoughAetherTextMessage(m_notEnoughAetherImage, m_notEnoughAetherText));
        }
    }

    public override void DataInitialize()
    {
        m_handPanelSlot.SetTurn();
        m_deckPanelSlot.SetTurn();
        m_graveyardPanelSlot.SetTurn();

        GridInit(m_model.Deck, this.ResourcesManager, m_deckPanelSlot);
        GridInit(m_model.Graveyard, this.ResourcesManager, m_graveyardPanelSlot);
        HandGridInit(m_model.Hand, this.ResourcesManager);
    }

    public override void Synchronization()
    {

    }

    public void MouseOver(Button Card)
    {
        Card.transform.localPosition = new Vector3(0, 100, 0);
        Card.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
    }

    public void MouseExit(Button Card)
    {
        Card.transform.localPosition = (new Vector3(0, 0, 0));
        Card.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
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

    public override void UseCard(Card card)
    {
        
    }

    public override void SetTurn()
    {
        bool setActive = m_model.Hand.Count == 0 ? false : true;

        m_handPanelSlot.SetTurn();
        m_deckPanelSlot.SetTurn();
        m_graveyardPanelSlot.SetTurn();
        
        m_thisTurnMaxCardCount = 0;

        GridInit(m_model.Deck, this.ResourcesManager, m_deckPanelSlot);
        GridInit(m_model.Graveyard, this.ResourcesManager, m_graveyardPanelSlot);
        HandGridInit(m_model.Hand, this.ResourcesManager);
    }

    public void DrawCard()
    {
        foreach(var card in m_model.TemtQueueForCardsToBeAdded)
        {
            DrawCard(card);
        }
        m_model.AddTemtQueueCardsToHand();
    }

    private void DrawCard(Card card)
    {
        CardSlot newSlot = m_handPanelSlot.GetObject();
        newSlot.gameObject.name = "새로 드로우한 카드";
        newSlot.ReInit(m_thisTurnMaxCardCount, card, card.CardData.Cost % 3,
            ResourcesManager.Card_Cost(card.CardData.Cost % 3),
            card.CardData.Name,
            ResourcesManager.Card_Image(1001),
            ResourcesManager.Card_Frame(card.CardData.Cost % 3)
            );
        newSlot.transform.SetParent(m_handPanelSlot.GridTransform, true);
        m_handPanelSlot.AddSlot(newSlot);

        // 카드 버튼의 이벤트 트리거 로드
        EventTrigger EventTrigger = newSlot.Event;

        // 마우스가 버튼 위에 올라가는 이벤트
        EventTrigger.Entry Entry = new EventTrigger.Entry();
        Entry.eventID = EventTriggerType.PointerEnter;
        Entry.callback.AddListener((data) => {
            if (m_isClick || m_isDrag)
            {
                return;
            }
            MouseOver(newSlot.Button);
        });

        // 마우스가 버튼 위에서 내려가는 이벤트
        EventTrigger.Entry Exit = new EventTrigger.Entry();
        Exit.eventID = EventTriggerType.PointerExit;
        Exit.callback.AddListener((data) => {
            m_isClick = false;
            newSlot.s_isReady = false;
            MouseExit(newSlot.Button);
        });

        // 버튼을 클릭하는 이벤트
        EventTrigger.Entry ClickEnter = new EventTrigger.Entry();
        ClickEnter.eventID = EventTriggerType.PointerDown;
        ClickEnter.callback.AddListener((data) => {
            //드래그 중이면 종료
            if (newSlot.s_isDrag || m_isDrag)
            {
                return;
            }

            SetCardEvent(this.m_handPanelSlot, newSlot);
            m_isClick = true;
            MouseOver(newSlot.Button);
        });

        EventTrigger.Entry ClickExit = new EventTrigger.Entry();
        ClickExit.eventID = EventTriggerType.PointerUp;
        ClickExit.callback.AddListener((data) => {
            //드래그 중이면 종료
            if (newSlot.s_isDrag || m_isDrag)
            {
                return;
            }

            SetCardEvent(this.m_handPanelSlot, newSlot);
            m_isClick = false;
            if (newSlot.s_isReady)
            {
                UseCardEvent(newSlot);
                InitCardEvent(this.m_handPanelSlot);
                //m_cardButtonsDic[newCardButton.s_num].s_isClick = false;
                return;
            }
            newSlot.s_isReady = true;
        });

        EventTrigger.triggers.Add(ClickEnter);
        EventTrigger.triggers.Add(ClickExit);
        EventTrigger.triggers.Add(Entry);
        EventTrigger.triggers.Add(Exit);

        EventTrigger.Entry Drag = new EventTrigger.Entry();
        Drag.eventID = EventTriggerType.Drag;
        Drag.callback.AddListener((data) => {
            SetCardEvent(this.m_handPanelSlot, newSlot);
            MouseOver(newSlot.Button);
            m_isDrag = true;
            newSlot.s_isDrag = true;
            newSlot.s_isReady = false;
            Vector3 mousePostion = Input.mousePosition;
            mousePostion.x -= newSlot.Button.transform.parent.localPosition.x;
            newSlot.Button.transform.localPosition = mousePostion;
        });

        EventTrigger.Entry EndDrag = new EventTrigger.Entry();
        EndDrag.eventID = EventTriggerType.EndDrag;
        EndDrag.callback.AddListener((data) => {
            SetCardEvent(this.m_handPanelSlot, newSlot);
            if (newSlot.Button.transform.localPosition.y < 400)
            {
                MouseExit(newSlot.Button);
                newSlot.s_isDrag = false;
                return;
            }
            UseCardEvent(newSlot);
            InitCardEvent(this.m_handPanelSlot);
        });
        EventTrigger.triggers.Add(Drag);
        EventTrigger.triggers.Add(EndDrag);
        
        m_thisTurnMaxCardCount++;
    }

    public void HandGridInit(List<Card> cards, ResourceManager resourceManager)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            CardSlot newSlot = m_handPanelSlot.GetObject();
            newSlot.ReInit(i, cards[i], cards[i].CardData.Cost % 3,
                resourceManager.Card_Cost(cards[i].CardData.Cost % 3),
                cards[i].CardData.Name + "\n" + cards[i].CardData.ID,
                resourceManager.Card_Image(1001),
                resourceManager.Card_Frame(cards[i].CardData.Cost % 3)
                );
            newSlot.transform.SetParent(m_handPanelSlot.GridTransform, true);
            m_handPanelSlot.AddSlot(newSlot);
            m_thisTurnMaxCardCount++;

            // 카드 버튼의 이벤트 트리거 로드
            EventTrigger EventTrigger = newSlot.Event;

            // 마우스가 버튼 위에 올라가는 이벤트
            EventTrigger.Entry Entry = new EventTrigger.Entry();
            Entry.eventID = EventTriggerType.PointerEnter;
            Entry.callback.AddListener((data) => {
                if (m_isClick || m_isDrag)
                {
                    return;
                }
                MouseOver(newSlot.Button);
            });

            // 마우스가 버튼 위에서 내려가는 이벤트
            EventTrigger.Entry Exit = new EventTrigger.Entry();
            Exit.eventID = EventTriggerType.PointerExit;
            Exit.callback.AddListener((data) => {
                m_isClick = false;
                newSlot.s_isReady = false;
                MouseExit(newSlot.Button);
            });

            // 버튼을 클릭하는 이벤트
            EventTrigger.Entry ClickEnter = new EventTrigger.Entry();
            ClickEnter.eventID = EventTriggerType.PointerDown;
            ClickEnter.callback.AddListener((data) => {
                //드래그 중이면 종료
                if (newSlot.s_isDrag || m_isDrag)
                {
                    return;
                }

                SetCardEvent(this.m_handPanelSlot, newSlot);
                m_isClick = true;
                MouseOver(newSlot.Button);
            });

            EventTrigger.Entry ClickExit = new EventTrigger.Entry();
            ClickExit.eventID = EventTriggerType.PointerUp;
            ClickExit.callback.AddListener((data) => {
                //드래그 중이면 종료
                if (newSlot.s_isDrag || m_isDrag)
                {
                    return;
                }

                SetCardEvent(this.m_handPanelSlot, newSlot);
                m_isClick = false;
                if (newSlot.s_isReady)
                {
                    UseCardEvent(newSlot);
                    InitCardEvent(this.m_handPanelSlot);
                    //m_cardButtonsDic[newCardButton.s_num].s_isClick = false;
                    return;
                }
                newSlot.s_isReady = true;
            });

            EventTrigger.triggers.Add(ClickEnter);
            EventTrigger.triggers.Add(ClickExit);
            EventTrigger.triggers.Add(Entry);
            EventTrigger.triggers.Add(Exit);

            EventTrigger.Entry Drag = new EventTrigger.Entry();
            Drag.eventID = EventTriggerType.Drag;
            Drag.callback.AddListener((data) => {
                SetCardEvent(this.m_handPanelSlot, newSlot);
                MouseOver(newSlot.Button);
                m_isDrag = true;
                newSlot.s_isDrag = true;
                newSlot.s_isReady = false;
                Vector3 mousePostion = Input.mousePosition;
                mousePostion.x -= newSlot.Button.transform.parent.localPosition.x;
                newSlot.Button.transform.localPosition = mousePostion;
            });

            EventTrigger.Entry EndDrag = new EventTrigger.Entry();
            EndDrag.eventID = EventTriggerType.EndDrag;
            EndDrag.callback.AddListener((data) => {
                SetCardEvent(this.m_handPanelSlot, newSlot);
                if (newSlot.Button.transform.localPosition.y < 400)
                {
                    MouseExit(newSlot.Button);
                    newSlot.s_isDrag = false;
                    return;
                }
                UseCardEvent(newSlot);
                InitCardEvent(this.m_handPanelSlot);
            });
            EventTrigger.triggers.Add(Drag);
            EventTrigger.triggers.Add(EndDrag);
        }
    }

    public void GridInit(List<Card> cards, ResourceManager resourceManager, CardPanelSlot cardPanelSlot)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            CardSlot newSlot = cardPanelSlot.GetObject();
            newSlot.ReInit(i, cards[i], cards[i].CardData.Cost % 3,
                resourceManager.Card_Cost(cards[i].CardData.Cost % 3),
                cards[i].CardData.Name,
                resourceManager.Card_Image(1001),
                resourceManager.Card_Frame(cards[i].CardData.Cost % 3)
                );
            newSlot.transform.SetParent(cardPanelSlot.GridTransform, true);
            cardPanelSlot.AddSlot(newSlot);
        }
    }

    public override void UnitDying(Unit unit)
    {
        
    }

    public void UseCardEvent(CardSlot cardSlot)
    {
        if(m_masterManager.UseCard(cardSlot.s_card))
        {
            m_handPanelSlot.ReleaseObject(cardSlot);
        }
        else
        {
            if (enumerator != null) StopCoroutine(enumerator);
            enumerator = StartCoroutine(NotEnoughAetherTextMessage(m_notEnoughAetherImage, m_notEnoughAetherText));
        }
        MouseExit(cardSlot.Button);
    }

    public IEnumerator NotEnoughAetherTextMessage(Image image, Text text)
    {
        float _timer = -0.30f;
        bool trigger = true;
        image.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        text.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

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
    }

    public void InitCardEvent(CardPanelSlot panel)
    {
        m_isClick = false;
        m_isDrag = false;
        panel.SetCardEvent();
    }

    public void SetCardEvent(CardPanelSlot panel, CardSlot cardSlot)
    {
        m_isClick = false;
        m_isDrag = false;
        panel.SetCardEvent(cardSlot);
    }
}
