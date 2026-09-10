using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlot : MonoBehaviour
{
    [SerializeField]
    private StatusEffectUI m_statusEffectUI;
    [SerializeField]
    private GameObject m_unitSpine;
    [SerializeField]
    private Transform m_HPSliderBGI;
    [SerializeField]
    private Slider m_healthPointSlider;

    public Transform StatusEffectTransform
    {
        get { return m_statusEffectUI.PoolTransform; }
    }

    public Slider HealthPointSlider
    {
        get { return m_healthPointSlider; }
    }
    public Transform HPSliderBGI
    {
        get { return m_HPSliderBGI; }
    }
    public GameObject UnitSpine
    {
        get { return m_unitSpine; }
    }

    public void Initialize()
    {
        m_statusEffectUI.Initialize();
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
        m_statusEffectUI.gameObject.SetActive(false);
    }

    public void TurnOn()
    {
        m_unitSpine.gameObject.SetActive(true);
        m_healthPointSlider.gameObject.SetActive(true);
        m_statusEffectUI.gameObject.SetActive(true);
    }
}
