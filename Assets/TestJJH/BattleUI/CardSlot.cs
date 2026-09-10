using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlot : UIObject
{
    [SerializeField]
    private Button m_cardButton;
    [SerializeField]
    private Image m_costImage;
    [SerializeField]
    private Text m_cardText;
    [SerializeField]
    private Image m_cardIllust;
    [SerializeField]
    private Image m_cardFrame;
    [SerializeField]
    private EventTrigger m_eventTrigger;

    private int m_num;
    [SerializeField]
    private Card m_card;
    public bool s_isDrag;
    public bool s_isReady;
    private int m_cost;

    public bool m_inPool = false;

    public void MouseOver()
    {
        m_cardButton.transform.localPosition = new Vector3(0, 100, 0);
        m_cardButton.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
    }

    public void MouseExit()
    {
        m_cardButton.transform.localPosition = (new Vector3(0, 0, 0));
        m_cardButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    public int s_num
    {
        get { return m_num; }
    }
    public Card s_card
    {
        get { return m_card; }
    }

    public int s_cost
    {
        get { return m_cost; }
    }

    public Button Button
    {
        get { return m_cardButton; }
    }

    public EventTrigger Event
    {
        get { return m_eventTrigger; }
    }

    public Sprite Cost
    {
        get { return m_costImage.sprite; }
    }

    public Text CardText
    { 
        get { return m_cardText; }
    }

    public Sprite CardIllust
    {
        get { return m_cardIllust.sprite; }
    }

    public Sprite CardFrame
    {
        get { return m_cardFrame.sprite; }
    }

    public void ReInit(int num, Card card, int cost,
        Sprite costSprite, string text, Sprite cardIllust, Sprite cardFrame
        )
    {
        m_num = num;
        m_card = card;
        s_isDrag = false;
        s_isReady = false;
        m_cost = cost;

        m_costImage.sprite = costSprite;
        m_cardText.text = text;
        m_cardIllust.sprite = cardIllust;
        m_cardFrame.sprite = cardFrame;

        m_costImage.gameObject.SetActive(true);
        m_cardText.gameObject.SetActive(true);
        m_cardIllust.gameObject.SetActive(true);
        m_cardFrame.gameObject.SetActive(true); 
        this.gameObject.SetActive(true);
        MouseExit();

        m_eventTrigger.triggers.Clear(); 
        m_eventTrigger = m_cardButton.GetComponent<EventTrigger>();

        transform.localPosition = (new Vector3(0, 0, 0));
        transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
        
        m_cardButton.onClick.RemoveAllListeners();
    }
}
