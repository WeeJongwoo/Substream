using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlot : MonoBehaviour
{
    [SerializeField]
    private StatusEffectPanel m_statusEffectUIPanel;
    [SerializeField]
    private GameObject m_unitSpine;
    [SerializeField]
    private Transform m_nowTurnIndicator;
    [SerializeField]
    private Transform m_shieldSliderBGI;
    [SerializeField]
    private Slider m_healthPointSlider;

    public Slider HealthPointSlider
    {
        get { return m_healthPointSlider; }
    }
    public Transform NowTurnIndicator
    {
        get { return m_nowTurnIndicator; }
    }
    public Transform ShieldSliderBGI
    {
        get { return m_shieldSliderBGI; }
    }

    public GameObject UnitSpine
    {
        get { return m_unitSpine; }
    }

    public void Initialize()
    {
        m_statusEffectUIPanel.Initialize();
        TurnOn();
    }

    public void TurnOff()
    {
        // 스파인 애니메이션 출력
        m_unitSpine.gameObject.SetActive(false);
        // 스파인 애니메이션 출력 종료시 코루틴 시작
        StartCoroutine(TurnOffCoroutine());
    }

    private IEnumerator TurnOffCoroutine()
    {
        yield return null;
        m_healthPointSlider.gameObject.SetActive(false);
        m_statusEffectUIPanel.gameObject.SetActive(false);
    }

    public void TurnOn()
    {
        m_unitSpine.gameObject.SetActive(true);
        m_healthPointSlider.gameObject.SetActive(true);
        m_statusEffectUIPanel.gameObject.SetActive(true);
    }

    public void ChangeStatusEffect(Sprite uiSprite, ESkillStatusType statusType, int duration, int stack, bool isNew)
    {
        m_statusEffectUIPanel.ChangeStatusEffect(uiSprite, statusType, duration, stack, isNew);
    }
}
