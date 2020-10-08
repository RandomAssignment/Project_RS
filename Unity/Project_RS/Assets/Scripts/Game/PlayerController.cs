using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    Transform playertrans;
    RectTransform stick;
    SpriteRenderer playerSR;

    public void OnDrag(PointerEventData eventData)
    {
        stick.position = eventData.position;
        float value = stick.rect.width / 2 - 30;
        if (stick.localPosition.magnitude > value)
        {
            stick.localPosition = stick.localPosition.normalized * value;
        }
        playerSR.flipX = stick.localPosition.x < 0;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        stick.localPosition = Vector3.zero;
    }

    void Start()
    {
        playertrans = GameObject.FindGameObjectWithTag("Player").transform;
        playerSR = playertrans.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        stick = transform.GetChild(0).GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        playertrans.Translate(new Vector3(stick.localPosition.normalized.x,0,stick.localPosition.normalized.y) * Time.deltaTime * 10);
    }

}
