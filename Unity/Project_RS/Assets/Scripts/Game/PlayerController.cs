using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform Stick { get; private set; }
    private BaseMonster _target;

    public void OnDrag(PointerEventData eventData)
    {
        Stick.position = eventData.position;
        float value = Stick.rect.width / 2 - 30;
        if (Stick.localPosition.magnitude > value)
        {
            Stick.localPosition = Stick.localPosition.normalized * value;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Stick.localPosition = Vector3.zero;
    }

    public void SetTarget(BaseMonster target)
    {
        _target = target;
    }

    private void Awake()
    {
        transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);
        Stick = transform.GetChild(0).GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        _target.Move(Stick.localPosition.normalized);
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
