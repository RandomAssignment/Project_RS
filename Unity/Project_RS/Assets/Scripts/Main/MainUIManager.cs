using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    #region Unity Field
    [SerializeField]
    private GameObject[] _ui = null;
    #endregion

    public void ViewPanel(int type)
    {
        _ui[type].SetActive(true);
    }
    public void ResetAndViewPanel(int type)
    {
        for (var i = 0; i < _ui.Length; i++)
        {
            _ui[i].SetActive(false);
        }
        _ui[type].SetActive(true);
    }
}
