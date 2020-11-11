using UnityEngine;
using UnityEngine.EventSystems;

public abstract class JoysticController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform Stick { get; private set; }
    protected Character Target { get; private set; }

    protected virtual void Awake()
    {
        transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);
        Stick = transform.GetChild(0).GetComponent<RectTransform>();
    }

    protected virtual void Update()
    {
        if (Target == null)
        {
            Destroy(gameObject);
            return;
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        Stick.position = eventData.position;
        var value = Stick.rect.width / 2 - 30;
        if (Stick.localPosition.magnitude > value)
        {
            Stick.localPosition = Stick.localPosition.normalized * value;
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        Stick.localPosition = Vector3.zero;
    }

    public void SetTarget(Character target)
    {
        Target = target;
    }
}
