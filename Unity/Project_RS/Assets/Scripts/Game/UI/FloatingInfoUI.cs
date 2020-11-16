using UnityEngine;
using UnityEngine.UI;

public class FloatingInfoUI : MonoBehaviour
{
    #region Unity Field
    [SerializeField]
    private Text _playerNameText = null;

    [SerializeField]
    private Image _healthBarImage = null;
    #endregion

    private Vector2 _screenOffset = new Vector2(0f, 30f);
    private Mob _target = null;

    private void Awake()
    {
        transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
    }

    public void SetTarget(Mob target, Vector2 offset)
    {
        _target = target;
        _screenOffset = offset;

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
            transform.position = Camera.main.WorldToScreenPoint(_target.transform.position) + (Vector3)_screenOffset;
        }
    }
}
