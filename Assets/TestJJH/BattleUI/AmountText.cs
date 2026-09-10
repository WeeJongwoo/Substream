using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private ObjectPool<AmountText> ObjectPool;

    public void Initialize(string txt, ESkillType type, Vector3 pos, ObjectPool<AmountText> objectPool)
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
            case ESkillType.E_CONDITIONAL_DAMAGE:
                StartCoroutine(FadeOut(DamageColor));
                break;
            case ESkillType.E_HEAL:
                StartCoroutine(FadeOut(HealColor));
                break;
            case ESkillType.E_SHIELD:
                StartCoroutine(FadeOut(ShieldColor));
                break;

            case ESkillType.E_DRAW:
                StartCoroutine(FadeOut(DrawColor));
                break;
        }
    }

    public IEnumerator FadeOut(Color color)
    {
        float _timer = -0.25f;
        bool trigger = true;
        Color BaseColor = color;
        transform.position += Vector3.up * 20;

        float t;
        float alpha;
        while (trigger)
        {
            _timer += Time.deltaTime;
            if(_timer < 0)
            {
                t = Mathf.Clamp01((_timer + 1) / 0.25f);
                alpha = t;
                text.color = new Color(BaseColor.r, BaseColor.g, BaseColor.b, alpha);
                transform.position += Vector3.up * t * 0.1f;
            }
            else
            {
                t = Mathf.Clamp01(_timer / 2.0f);

                float angle = t / 2 * Mathf.PI;

                float at = Mathf.Sin(angle) * Mathf.Sin(angle) * Mathf.Sin(angle);
                alpha = Mathf.Lerp(1, 0, at);
                alpha *= alpha * alpha * alpha * alpha;
                text.color = new Color(BaseColor.r, BaseColor.g, BaseColor.b, alpha);
                transform.position += Vector3.up * t ;
                
                if (t >= 1.0f)
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
