using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;
using UnityEngine.TextCore.Text;

public class MonsterUIManager : BaseUI<MonsterManager>
{
    [SerializeField]
    private AmountText m_amountTextPrefab;
    [SerializeField]
    private Transform m_amountTextParent;

    [SerializeField]
    private UnitSlot m_unitPrefab;
    [SerializeField]
    private RectTransform[] m_unitUIPosition;

    private UnitSlot[] m_unitUISlot;

    private ObjectPool<AmountText> m_textPool;
    public override void Initialize()
    {
        m_textPool = new ObjectPool<AmountText>(m_amountTextPrefab, 64, m_amountTextParent);
    }

    public override void DataInitialize()
    {
        m_unitUISlot = new UnitSlot[m_model.Units.Count];
        //create
        for (int i = 0; i < m_model.Units.Count; i++)
        {
            UnitSlot newUnitUI = Instantiate(m_unitPrefab);
            newUnitUI.transform.parent = this.transform;
            //유닛 스파인 지정 필요
            //newUnitUI.UnitSpine;
            newUnitUI.Initialize();
            newUnitUI.GetComponent<RectTransform>().position = m_unitUIPosition[i].position;
            newUnitUI.gameObject.name = newUnitUI.gameObject.name + i.ToString();
            newUnitUI.UnitSpine.transform.rotation = Quaternion.Euler(0,180,0);

            m_unitUISlot[i] = newUnitUI;
        }

        InitHP();
    }

    public void InitHP()
    {
        int c = 0;
        foreach (var character in m_model.Units)
        {
            m_unitUISlot[c].HealthPointSlider.maxValue = character.HealthPoint.Max;
            m_unitUISlot[c].HealthPointSlider.value = character.HealthPoint.Now;
            c++;
        }
    }

    public void InitHP(int pos)
    {
        Debug.Log("체력 : " + m_model.Units[pos].HealthPoint.Now);
        m_unitUISlot[pos].HealthPointSlider.maxValue = m_model.Units[pos].HealthPoint.Max + m_model.Units[pos].ShieldPoint.Now;
        m_unitUISlot[pos].HealthPointSlider.value = m_model.Units[pos].HealthPoint.Now;
        if (m_model.Units[pos].ShieldPoint.Now > 0) m_unitUISlot[pos].ShieldSliderBGI.gameObject.SetActive(true);
        else m_unitUISlot[pos].ShieldSliderBGI.gameObject.SetActive(false);
    }

    public override void UseCard(Card card)
    {

    }

    public override void Synchronization()
    {
        InitHP();
    }

    public void SetNowTurnIndicator(bool isCharacter, int exceptionPosition)
    {
        foreach (var unitUI in m_unitUISlot)
        {
            unitUI.NowTurnIndicator.gameObject.SetActive(false);
        }
        if (isCharacter)
        {
            return;
        }
        m_unitUISlot[exceptionPosition].NowTurnIndicator.gameObject.SetActive(true);
    }

    public override void UnitDying(Unit unit)
    {
        Debug.Log("몬스터 사망 이벤트 출력");

        if (!unit.IsCharacter)
        {
            m_unitUISlot[unit.Position].gameObject.SetActive(false);
        }
    }

    public void AttackEvent(int sourceUnitpos,
        bool targetUnitIsCharacter, int targetUnitpos)
    {
        // m_unitUISlot[sourceUnitpos];
    }
    public void CastEvent(int sourceUnitpos)
    {
        // m_unitUISlot[sourceUnitpos];
    }


    public void ChangeStatusEffectEvent(int targetUnitPos)
    {
        // 상태 이상 애니메이션 출력
    }

    public void ChangeHPEvent(int targetUnitPos, bool isDamage, ESkillStatusType statusType, int amount)
    {
        InitHP(targetUnitPos);
        var text = m_textPool.GetObject();
        if (isDamage)
        {
            text.Initialize(amount.ToString(), ESkillType.E_DAMAGE, statusType, m_unitUIPosition[targetUnitPos].position, m_textPool);
        }
        else
        {
            text.Initialize(amount.ToString(), ESkillType.E_HEAL, ESkillStatusType.E_NONE, m_unitUIPosition[targetUnitPos].position, m_textPool);
        }
    }

    public void ShieldEvent(int targetUnitPos, int amount)
    {
        InitHP(targetUnitPos);
        var text = m_textPool.GetObject();
        text.Initialize(amount.ToString(), ESkillType.E_SHIELD, ESkillStatusType.E_NONE, m_unitUIPosition[targetUnitPos].position, m_textPool);
    }

    public void ChangeStack(int targetUnitPos, ESkillStatusType statusType, int duration, int stack, bool isNew)
    {
        m_unitUISlot[targetUnitPos].ChangeStatusEffect(ResourcesManager.Status_Effect_Image((int)statusType), statusType, duration, stack, isNew);
    }
}
