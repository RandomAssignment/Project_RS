using UnityEngine;
using UnityEngine.EventSystems;

public class SkillController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform Stick { get; private set; }
    private Character _target;
    private GameObject _backGround;

    public void OnDrag(PointerEventData eventData)
    {
        _backGround.SetActive(true);
        Stick.position = eventData.position;
        var value = Stick.rect.width / 2 - 30;
        if (Stick.localPosition.magnitude > value)
        {
            Stick.localPosition = Stick.localPosition.normalized * value;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 스틱의 LocalPosition에서 y값과 z값을 바꾸기
        var pos = Stick.localPosition.normalized;
        var yPos = pos.y;
        pos.y = pos.z;
        pos.z = yPos;

        _target.UseSkill("Punch", pos);

        Stick.localPosition = Vector3.zero;
        _backGround.SetActive(false);
    }

    public void SetTarget(Character target)
    {
        _target = target;
    }

    private void Awake()
    {
        transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);
        Stick = transform.GetChild(1).GetComponent<RectTransform>();
        _backGround = transform.GetChild(0).gameObject;
        _backGround.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (_target != null)
        {

        }
    }

    private void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }
    }
}
