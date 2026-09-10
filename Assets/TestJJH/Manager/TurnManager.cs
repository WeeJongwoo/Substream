using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.UI.CanvasScaler;

public class TurnManager : BaseSystem
{
    private CharacterManager m_characterManager;
    private MonsterManager m_monsterManager;

    private int m_roundCount;
    private int m_turnCount;
    [SerializeField]
    private int m_currentAetherCount;
    private int m_currentTurnMaxAetherCount;

    private LinkedList<Unit> m_unitFlow;
    private Unit m_currentTurnUnit;
    private Dictionary<Unit, int> m_unitTurnRecorder;

    private bool m_turnInputLock;

    public bool IsTurnInputLocked
    {
        get { return m_turnInputLock? true : false; }
    }

    public void TurnInputLockOn()
    {
        m_turnInputLock = true;
    }

    public int RoundCount
    {
        get { return m_roundCount; }
    }
    public int TurnCount
    {
        get { return m_turnCount; }
    }

    public int CurrentAetherCount
    {
        get { return m_currentAetherCount; }
    }
    public int CurrentTurnMaxEtherCount
    {
        get { return m_currentTurnMaxAetherCount; }
    }

    public Unit CurrentTurnUnit
    {
        get { return m_currentTurnUnit; }
    }

    public LinkedList<Unit> Units
    {
        get { return m_unitFlow; }
    }

    private const int AETHERCOUNT = 7;


    public void ExpendMaxAetherCount(int amount)
    {
        m_currentAetherCount += amount;
        m_currentTurnMaxAetherCount += amount;
    }

    public override void Initialize()
    {
        m_turnInputLock = false;
        m_roundCount = 1;
        m_turnCount = 1;
        m_currentTurnMaxAetherCount = AETHERCOUNT;
        m_currentAetherCount = m_currentTurnMaxAetherCount;
        m_unitFlow = new LinkedList<Unit>();
        m_unitTurnRecorder = new Dictionary<Unit, int>();
    }
    public override void InitializeReference(MasterManager masterManager)
    {
        m_masterManager = masterManager;
        m_characterManager = masterManager.CharacterManager;
        m_monsterManager = masterManager.MonsterManager;
    }

    /// 유닛의 속도 용어 정리
    /// Speed는 기본 속도
    /// UnitSpeed는 인플레이 속도(즉, 가변 가능한 변수)
    /// </summary>
    /// <param name="turnManager"></param>
    /// <param name="characterManager"></param>
    /// <param name="monsterManager"></param>
    public override void DataInitialize()
    {
        m_unitFlow.Clear();
        foreach(var character in m_characterManager.Units)
        {
            m_unitFlow.AddLast(character);
        }
        foreach (var monster in m_monsterManager.Units)
        {
            m_unitFlow.AddLast(monster);
        }
        
        List<Unit> units = new List<Unit>();
        foreach (var unit in m_unitFlow)
        {
            int speed = (int)unit.SpeedValue.Now;
            units.Add(unit);
        }
        units.Sort((a, b) =>
        {
            int cmp = b.SpeedValue.Now.CompareTo(a.SpeedValue.Now);
            if (cmp == 0)
            {
                b.IsCharacter.CompareTo(a.IsCharacter);
                return cmp;
            }
            return cmp;
        });


        m_unitFlow.Clear();
        foreach (var unit in units)
        {
            m_unitFlow.AddLast((unit));
        }

        m_currentTurnUnit = m_unitFlow.First.Value;
        m_unitFlow.RemoveFirst();

        m_unitTurnRecorder.Clear();
        foreach (var character in m_characterManager.Units)
        {
            m_unitTurnRecorder.Add(character, 0);
        }
        foreach (var monster in m_monsterManager.Units)
        {
            m_unitTurnRecorder.Add(monster, 0);
        }
    }

    public override void SetTurn()
    {
        // 25.12.09 기준. +1 이 필수적 현 속도 테스트 기준으로는 +1 없이는 너무 느려서 턴이 안옴
        // 상수 값 15 기준으로 이상이면 4번째 재배치, 아니면 무조건 마지막으로 감
        // int pos = m_unitFlow.Count - (int)(m_currentTurnUnit.SpeedPoint.Now + 15) / 15 + 1;

        // 26.08.11 기준.
        //  Speed   |   Position
        //  ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
        //  0 ~ 6   |   NextTurn
        //  7 ~ 14  |   Count - 1
        //  15 ~ 22 |   Count - 2
        //  23 ~ 30 |   Count - 3
        //  31 ~ 38 |   Count - 4
        const int MIN_SPEED = 6;
        const int SPEED_PER_SLOT = 8;
        const int MIN_POS = 2;

        int speed = (int)m_currentTurnUnit.SpeedValue.Now;

        int advance = Mathf.Max(
            0,
            Mathf.CeilToInt((speed - MIN_SPEED) / (float)SPEED_PER_SLOT)
        );

        int pos = Mathf.Clamp(
            m_unitFlow.Count - advance,
            MIN_POS,
            m_unitFlow.Count
        );

        List<Unit> units = new List<Unit>();
        foreach (var unit in m_unitFlow)
        {
            units.Add(unit);
        }
        units.Insert(pos, m_currentTurnUnit);

        m_unitFlow.Clear();
        foreach (var unit in units)
        {
            m_unitFlow.AddLast((unit));
        }

        m_unitTurnRecorder[m_currentTurnUnit]++;

        m_currentTurnUnit = m_unitFlow.First.Value;
        m_unitFlow.RemoveFirst();

        m_turnCount++;
        m_currentAetherCount = AETHERCOUNT + (int)(m_turnCount / 3);
        m_currentTurnMaxAetherCount = m_currentAetherCount;

        units.Clear();

        if (m_unitTurnRecorder.All(r => r.Value > 0))
        {
            m_masterManager.SetRound();
        }
        else
        {
            m_turnInputLock = false;
        }
    }

    public override void SetRound()
    {
        m_turnCount = 1;
        m_roundCount++;
        foreach (var key in m_unitTurnRecorder.Keys.ToList())
        {
            m_unitTurnRecorder[key] = 0;
        }
        m_turnInputLock = false;
    }

    public override void UseCard(Card card)
    {

    }

    public bool UseAether(int AetherCount)
    {
        if (m_currentAetherCount < AetherCount)
            return false;
        m_currentAetherCount -= AetherCount;
        return true;
    }

    public override void Synchronization()
    {

    }

    public override void UnitDying(Unit unit)
    {
        LinkedList<Unit> list = new LinkedList<Unit>();
        foreach(var uf in m_unitFlow)
        {
            if (unit != uf) list.AddLast(uf);
            else continue;
        }
        m_unitFlow.Clear();
        m_unitFlow = list;

        if(m_currentTurnUnit == unit)
        {
            m_masterManager.SetTurn();
        }
    }
}
