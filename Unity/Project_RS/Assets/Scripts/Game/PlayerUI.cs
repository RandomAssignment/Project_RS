using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    #region Unity Field
    [SerializeField]
    private Text _playerNameText = null;

    [SerializeField]
    private Image _healthBarImage = null;

    [SerializeField]
    private Vector3 _screenOffset = new Vector3(0f, 30f, 0f);
    #endregion

    private BaseMob _target = null;

    private void Awake()
    {
        transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
    }

    public void SetTarget(BaseMob target)
    {
        _target = target;

        if (_playerNameText != null)
        {
            _playerNameText.text = target.photonView.Owner.NickName;
        }
    }

    private void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        _healthBarImage.fillAmount = (float)_target.Health / _target.MaxHealth;
    }

    private void LateUpdate()
    {
        if (_target != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(_target.transform.position) + _screenOffset;
        }
    }
}
