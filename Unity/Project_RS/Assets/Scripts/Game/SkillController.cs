﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform Stick { get; private set; }
    private BaseMob _target;
    private GameObject _backGround;

    public void OnDrag(PointerEventData eventData)
    {
        _backGround.SetActive(true);
        Stick.position = eventData.position;
        float value = Stick.rect.width / 2 - 30;
        if (Stick.localPosition.magnitude > value)
        {
            Stick.localPosition = Stick.localPosition.normalized * value;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _target.Skills["punch"].Use(Stick.localPosition.normalized);
        Stick.localPosition = Vector3.zero;
        _backGround.SetActive(false);
    }

    public void SetTarget(BaseMob target)
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
