using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Text PlayerNameText;
    public Image HealthBarImage;
    public Vector3 ScreenOffset = new Vector3(0f, 30f, 0f);

    private BaseMonster _target;

    private void Awake()
    {
        transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
    }

    public void SetTarget(BaseMonster target)
    {
        _target = target;

        if (PlayerNameText != null)
        {
            PlayerNameText.text = target.photonView.Owner.NickName;
        }
    }

    private void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (_target.Health < 0)
        {
            return;
        }

        HealthBarImage.fillAmount = (float)_target.Health / _target.MaxHealth;
    }

    private void LateUpdate()
    {
        if (_target != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(_target.transform.position) + ScreenOffset;
        }
    }
}
