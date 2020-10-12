using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Text PlayerNameText;
    public Image HealthBarImage;
    public Vector3 ScreenOffset = new Vector3(0f, 30f, 0f);

    private BaseMonster target;

    private void Awake()
    {
        transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
    }

    public void SetTarget(BaseMonster target)
    {
        this.target = target;

        if (PlayerNameText != null)
        {
            PlayerNameText.text = target.photonView.Owner.NickName;
        }
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (target.Health < 0)
        {
            return;
        }

        HealthBarImage.fillAmount = (float)target.Health / target.MaxHealth;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(target.transform.position) + ScreenOffset;
        }
    }
}
