using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AmountText : UIObject
{
    [SerializeField]
    private Text text;

    [SerializeField]
    private Color DefaultColor;

    [SerializeField]
    private Color DamageColor;
    [SerializeField]
    private Color HealColor;
    [SerializeField]
    private Color DrawColor;
    [SerializeField]
    private Color ShieldColor;
    [SerializeField]
    private Color BleedDamageColor;
    [SerializeField]
    private Color ShockDamageColor;
    [SerializeField]
    private Color OverloadDamageColor;


    [SerializeField]
    private ObjectPool<AmountText> ObjectPool;

    public void Initialize(string txt, ESkillType type, ESkillStatusType statusType, Vector3 pos, ObjectPool<AmountText> objectPool)
    {
        ObjectPool = objectPool;

        text.text = txt;
        transform.position = pos;
        switch(type)
        {
            case ESkillType.E_DEFAULT:
                StartCoroutine(FadeOut(DefaultColor));
                break;
            case ESkillType.E_DAMAGE:
                StartCoroutine(FadeOut(DamageColor));
                break;
            case ESkillType.E_CONDITIONAL_DAMAGE:
                ConditionalDamage(statusType);
                break;
            case ESkillType.E_HEAL:
                text.text = "+" + txt;
                StartCoroutine(FadeOut(HealColor));
                break;
            case ESkillType.E_SHIELD:
                text.text = "+" + txt;
                StartCoroutine(FadeOut(ShieldColor));
                break;
            case ESkillType.E_DRAW:
                StartCoroutine(FadeOut(DrawColor));
                break;
        }
    }

    public void ConditionalDamage(ESkillStatusType statusType)
    {
        switch(statusType)
        {
            case ESkillStatusType.E_NONE:
                break;
            case ESkillStatusType.E_BLEED:
                StartCoroutine(FadeOut(BleedDamageColor));
                break;
            case ESkillStatusType.E_SHOCK:
                StartCoroutine(FadeOut(ShockDamageColor));
                break;
            case ESkillStatusType.E_OVERLOAD:
                StartCoroutine(FadeOut(OverloadDamageColor));
                break;
        }
    }

    public IEnumerator FadeOut(Color color)
    {
        float _timer = -0.25f;
        bool trigger = true;
        Color BaseColor = color;
        transform.position -= Vector3.up * 40;

        float t;
        float alpha;
        while (trigger)
        {
            _timer += Time.deltaTime;
            if(_timer < 0)
            {
                t = Mathf.Clamp01(_timer / 0.25f);
                alpha = t;
                text.color = new Color(BaseColor.r, BaseColor.g, BaseColor.b, alpha);
                transform.position += Vector3.up * t * 0.2f + Vector3.up;
            }
            else
            {
                t = Mathf.Clamp01(_timer / 2.0f);

                float angle = t * Mathf.PI / 2;

                float at = Mathf.Sin(angle);// * Mathf.Sin(angle);// * Mathf.Sin(angle);
                alpha = Mathf.Lerp(1, 0, at);
                alpha *= alpha * alpha * alpha;
                text.color = new Color(BaseColor.r, BaseColor.g, BaseColor.b, alpha);
                transform.position -= Vector3.up * t * 2.5f - Vector3.up;

                if (t >= 0.999f)
                {
                    _timer = 0f;
                    trigger = false;
                }
            }

            yield return null;
        }

        ObjectPool.ReleaseObject(this);
    }

}
