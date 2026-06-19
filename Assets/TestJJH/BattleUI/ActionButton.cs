using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class ActionButton : BaseSystem
{
    [SerializeField]
    private Transform m_actionButtonGrid;

    [SerializeField]
    private Transform m_actionButtonSet1;
    [SerializeField]
    private Transform m_actionButtonSet2;
    
    [SerializeField]
    private Button m_characterSkill;
    [SerializeField]
    private Button m_viewInfo;
    [SerializeField]
    private Button m_returnButton;

    public override void Initialize()
    {
        //m_actionButtonGrid.transform.position = Vector3.zero;
        Debug.Log("position : " + m_actionButtonGrid.position + "local : " + m_actionButtonGrid.localPosition);

        m_characterSkill.onClick.RemoveAllListeners();
        m_characterSkill.onClick.AddListener(() => 
        {
            StartCoroutine(GoTo(m_actionButtonSet2.localPosition, m_actionButtonSet1.localPosition));
        });
        m_returnButton.onClick.AddListener(() =>
        {
            StartCoroutine(GoTo(m_actionButtonSet1.localPosition, m_actionButtonSet2.localPosition));
        });
    }

    public override void InitializeReference(MasterManager masterManager)
    {
        
    }

    public override void DataInitialize()
    {

    }

    public override void SetTurn()
    {

    }

    public override void UnitDying(Unit unit)
    {
        int a = unit.Position;
    }

    public IEnumerator GoTo(Vector3 A, Vector3 ToB)
    {
        float duration = 0.2f;
        float timer = 0f;

        Vector3 start = A;
        Vector3 end = ToB;

        Vector3 prevPos = start;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            Vector3 currentPos = Vector3.Lerp(start, end, t);

            Vector3 delta = currentPos - prevPos;

            m_actionButtonGrid.Translate(delta);

            prevPos = currentPos;

            yield return null;
        }

        m_actionButtonGrid.Translate(end - prevPos);
    }
}
