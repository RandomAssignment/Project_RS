using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillController : JoysticController
{
    [SerializeField]
    private Image _background = null;

    protected override void Awake()
    {
        base.Awake();
        Debug.Assert(_background);
        _background.enabled = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        _background.enabled = true;
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        // 스틱의 LocalPosition에서 y값과 z값을 바꾸기
        var pos = Stick.localPosition.normalized;
        var yPos = pos.y;
        pos.y = pos.z;
        pos.z = yPos;

        Target.UseSkill(nameof(Punch), pos);

        base.OnEndDrag(eventData);
        _background.enabled = false;
    }
}
