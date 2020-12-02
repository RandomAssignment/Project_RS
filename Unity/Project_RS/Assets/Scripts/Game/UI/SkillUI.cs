using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField]
    private Image[] _skillSlot = new Image[4];
    [SerializeField]
    private Color[] _skillSprite;
    //private Sprite[] _skillSprite;
    private Text[] _coolDownText = new Text[4];
    private Character _player;

    private void Awake()
    {
        for(int i = 0; i < 4; i++)
        {
            _coolDownText[i] = _skillSlot[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>();
        }
    }

    public void SetSkillUI(int slotcount, int skillnumber)
    {
        _skillSlot[slotcount].color = _skillSprite[skillnumber];
        //_skillSlot[slotcount].sprite = _skillSprite[skillnumber];
    }

    public void SetCoolDownText(int slotcount, int cooldown)
    {
        if (cooldown > 0)
            _coolDownText[slotcount].text = cooldown.ToString();
        else
            _coolDownText[slotcount].text = " ";
    }
}
