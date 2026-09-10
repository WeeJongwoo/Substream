using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFacade 
{
    public readonly MasterManager MasterManager;

    public readonly CharacterUIManager CharacterUIManager;
    public readonly MonsterUIManager MonsterUIManager;
    public readonly CardUIManager CardUIManager;
    public readonly TurnUIManager TurnUIManager;

    private Dictionary<EResultType, UIEventStrategy> m_resultExecuteStrategies;

    public IEnumerator Execute(Flow flow)
    {
        foreach (var fr in flow.Collector.Results)
        {
            m_resultExecuteStrategies[fr.ResultType].Execute(fr);
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }

    public UIFacade(MasterManager masterManager,
        CharacterUIManager characterUIManager,
        MonsterUIManager monsterUIManager,
        CardUIManager cardUIManager,
        TurnUIManager turnUIManager)
    {
        /// 필요 수정사항 정리
        /// 1. 상태 이상 관련(changeStatusEffectEvent, ChangeStackEvent)
        /// 2. CardUse처리는 굳이 안쓸거 같음
        m_resultExecuteStrategies = new Dictionary<EResultType, UIEventStrategy> {
            { EResultType.E_DEFAULT, new UIDefaultEvent(this)},

            { EResultType.E_ATTACK, new UIAttackEvent(this)},
            { EResultType.E_CAST, new UICastEvent(this)},
            { EResultType.E_SKILL, new UICastEvent(this)},

            { EResultType.E_CHANGESTACK, new UIChangeStackEvent(this)},// 수정 필요

            { EResultType.E_CHANGEHP, new UIChangeHPEvent(this)},
            { EResultType.E_ADDSHIELD, new UIAddShieldEvent(this)},
            { EResultType.E_CHANGEAETHER, new UIChangeAetherEvent(this)},

            { EResultType.E_CARDDRAW, new UIDrawCardEvent(this)},
            { EResultType.E_CARDUSE, new UIUseCardEvent(this)}, // 안쓸거 같음

            { EResultType.E_UNITDYING, new UIUnitDeathEvent(this)},
            { EResultType.E_TURNEND, new UIEndTurnEvent(this)},
            { EResultType.E_ROUNDEND, new UIEndRoundEvent(this)},
            { EResultType.E_PAUSE, new UIPausaeEvent(this)},
        };

        CharacterUIManager = characterUIManager;
        MonsterUIManager = monsterUIManager;
        CardUIManager = cardUIManager;
        TurnUIManager = turnUIManager;
        MasterManager = masterManager;
    }
}
