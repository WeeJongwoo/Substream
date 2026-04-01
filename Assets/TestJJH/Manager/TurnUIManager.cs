using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.TextCore.Text;

public class TurnUIManager : BaseUI<TurnManager>
{
    [SerializeField]
    private GameObject m_portraitPrefab;
    [SerializeField]
    private List<PortraitSlot> m_portraits;
    [SerializeField]
    private Transform m_portraitsParent;

    [SerializeField]
    private PortraitSlot m_currentTurnUnitPortrait;



    [SerializeField]
    private Button m_turnEndButton;
    [SerializeField]
    private Text m_turnText;
    [SerializeField]
    private Text m_aetherText1;
    [SerializeField]
    private Text m_aetherText2;
    [SerializeField]
    private Text m_aetherText3;
    private StringBuilder m_stringBuilder = new StringBuilder(32);


    public override void Initialize() 
    {
        m_portraits = new List<PortraitSlot>();

        m_turnEndButton.onClick.AddListener(() => {
            m_masterManager.SetTurn();
        });
    }

    public override void DataInitialize()
    {
        m_portraits.Clear();

        foreach (var unit in m_model.Units)
        {
            PortraitSlot NPS = Instantiate(m_portraitPrefab).GetComponent<PortraitSlot>();
            NPS.transform.parent = m_portraitsParent;
            NPS.transform.localScale = Vector3.one;
            NPS.transform.localPosition = Vector3.zero;
            m_portraits.Add(NPS);
        }

        SetTurnEtherInfo();
        SetPortrait();
    }

    public override void Synchronization()
    {

    }

    public void SetTurnEtherInfo()
    {
        m_stringBuilder.Clear();
        m_stringBuilder
            .Append("Turn")
            .Append(m_model.TurnCount);
        m_turnText.text = m_stringBuilder.ToString();

        m_stringBuilder.Clear();
        m_stringBuilder
            .Append("\n")
            .Append(m_model.CurrentAetherCount);
        m_aetherText2.text = m_stringBuilder.ToString();
        
        m_stringBuilder.Clear();
        m_stringBuilder    
            .Append("\n")
            .Append(m_model.CurrentTurnMaxEtherCount);
        m_aetherText3.text = m_stringBuilder.ToString();
    }


    public void SetPortrait()
    {
        for (int i = m_model.Units.Count ; i < m_portraits.Count; i++)
        {
            m_portraits[i].gameObject.SetActive(false);
        }
                
        m_currentTurnUnitPortrait.Portrait.sprite = ResourcesManager.Unit_Portrait(m_model.CurrentTurnUnit.IngameUnitID());

        int j = 0;
        foreach (var unit in m_model.Units)
        {
            m_portraits[j].Portrait.sprite = ResourcesManager.Unit_Portrait(unit.IngameUnitID());
            if (unit.IsCharacter) m_portraits[j].Arrow.color = Color.blue;
            else m_portraits[j].Arrow.color = Color.red;
            j++;
        }
    }

    public override void SetTurn()
    {
        SetTurnEtherInfo();
        SetPortrait();
    }

    public override void UseCard(Card card)
    {
        SetTurnEtherInfo();
    }

    public override void UnitDying(Unit unit)
    {
        SetPortrait();
    }
}
