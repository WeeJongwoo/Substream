using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ESelectType
{
    None,       // 선택중이지만 버튼 위에 없음
    Selecting,  // 선택중에 버튼 위에 있음
    Selected    // 선택 끝
}

public class UnitSelectHelper : MonoBehaviour
{
    [SerializeField]
    private UnitManagingSystem m_characterManager;
    [SerializeField]
    private UnitManagingSystem m_monsterManager;
    [SerializeField]
    private CharacterUIManager m_characterUIManager;
    [SerializeField]
    private MonsterUIManager m_monsterUIManager;

    private Dictionary<UnitSlot, Unit> m_unitSlotList;

    [SerializeField]
    private int SelectUnitPosition;

    [SerializeField]
    private GameObject m_selectPanel;

    [SerializeField]
    private GameObject m_selectScope;

    [SerializeField]
    private Button m_selectNoneButton;
    [SerializeField]
    private Button m_selectAbandonButton;

    public void TurnOnSelectPanel()
    {
        m_selectNoneButton.gameObject.SetActive(true);
        m_selectPanel.SetActive(true);
    }

    public void TurnOffSelectPanel()
    {
        m_selectNoneButton.gameObject.SetActive(false);
        m_selectPanel.SetActive(false);
        m_selectScope.SetActive(false);
    }

    public void TurnOnSelectScope(UnitSlot unitSlot)
    {
        m_selectScope.SetActive(true);
        m_selectScope.transform.position = new Vector3(unitSlot.transform.position.x, unitSlot.transform.position.y - 250, unitSlot.transform.position.z);
    }

    public void TurnOffSelectScope()
    {
        m_selectScope.SetActive(false);
    }

    public void SetSelector()
    {
        SelectUnitPosition = 401;
    }

    /// <summary>
    /// /- 오류 -/
    /// 선택되지 않음 401
    /// /- 선택 -/
    /// 일반 선택 1,2,3,4, 적 선택 -1,-2,-3,-4
    /// 선택 없이 시전 0
    /// 버리기 402
    /// </summary>
    public int GetSelector
    {
        get { return SelectUnitPosition; }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        List<RaycastResult> results = new();

        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            UnitSlot slot = r.gameObject.GetComponentInParent<UnitSlot>();

            if (slot != null)
            {
                SelectUnitPosition = m_unitSlotList[slot].Position;
                break;
            }
        }
    }

    public void Initialize()
    {
        m_selectPanel.gameObject.SetActive(false);
        m_selectScope.gameObject.SetActive(false);
        m_unitSlotList = new Dictionary<UnitSlot, Unit>();
        SetSelector();
    }

    public void DataInitialize()
    {
        for (int i = 0; i < m_characterManager.Units.Count; i++)
        {
            m_unitSlotList.Add(m_characterUIManager.UnitSlots[i], m_characterManager.Units[i]);
        }
        for (int i = 0; i < m_monsterManager.Units.Count; i++)
        {
            m_unitSlotList.Add(m_monsterUIManager.UnitSlots[i], m_monsterManager.Units[i]);
        }
        foreach(var USL in m_unitSlotList)
        {
            USL.Key.SelectButton.onClick.RemoveAllListeners();

            UnitSlot key = USL.Key;
            // 유닛 버튼의 이벤트 트리거 로드
            EventTrigger EventTrigger = USL.Key.EventTrigger;
            USL.Key.EventTrigger.triggers.Clear();

            // 마우스가 버튼 위에 올라가면 UI 켜기
            EventTrigger.Entry Entry = new EventTrigger.Entry();
            Entry.eventID = EventTriggerType.PointerEnter;
            Entry.callback.AddListener((UnityEngine.Events.UnityAction<BaseEventData>)((data) => {
                TurnOnSelectScope(key);
            }));

            // 마우스가 버튼 위에서 내려가는면 UI끄기
            EventTrigger.Entry Exit = new EventTrigger.Entry();
            Exit.eventID = EventTriggerType.PointerExit;
            Entry.callback.AddListener((UnityEngine.Events.UnityAction<BaseEventData>)((data) => {
                //TurnOffSelectScope();
            }));

            USL.Key.SelectButton.onClick.AddListener(()=> {
                SelectUnitPosition = m_unitSlotList[key].Position * (m_unitSlotList[key].IsCharacter ? 1 : -1);
            });

            EventTrigger.triggers.Add(Entry);
            EventTrigger.triggers.Add(Exit);
        }
        m_selectNoneButton.onClick.RemoveAllListeners();
        m_selectNoneButton.onClick.AddListener(() =>
        {
            SelectUnitPosition = 0;
        });

        m_selectAbandonButton.onClick.RemoveAllListeners();
        m_selectAbandonButton.onClick.AddListener(() =>
        {
            SelectUnitPosition = 402;
        });
    }

    public void KeyInput(ETargetType needInputType)
    {
        int pos = 401;
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            pos = 1;
        }
        if (Input.GetKeyUp(KeyCode.Alpha2)) 
        {
            pos = 2;
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            pos = 3;
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            pos = 4;
        }

        if (needInputType == ETargetType.E_NONE)
        {
            if (pos != 401) SelectUnitPosition = 0;
            return;
        }
        else
        {
            if (needInputType == ETargetType.E_ENEMY)
            {
                foreach (var u in m_unitSlotList)
                {
                    if (u.Value.IsCharacter == true) continue;

                    if (u.Value.Position == pos)
                    {
                        SelectUnitPosition = u.Value.Position * -1;
                    }
                }
            }
            else if (needInputType == ETargetType.E_ALLIES)
            {
                foreach (var u in m_unitSlotList)
                {
                    if (u.Value.IsCharacter != true) continue;

                    if (u.Value.Position == pos)
                    {
                        SelectUnitPosition = u.Value.Position;
                    }
                }
            }
        }
    }
}
