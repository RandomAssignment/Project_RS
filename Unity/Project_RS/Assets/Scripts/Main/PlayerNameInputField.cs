using Photon.Pun;

using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInputField : MonoBehaviour
{
    private const string PlayerNamePrefKey = "PlayerName";

    private void Start()
    {
        string defaultName = string.Empty;
        InputField _inputField = GetComponent<InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(PlayerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(PlayerNamePrefKey);
                _inputField.text = defaultName;
            }
        }
        PhotonNetwork.NickName = defaultName;
    }

    public void SetPlayerName(string value)
    {
        // #Important
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }
        PhotonNetwork.NickName = value;
        PlayerPrefs.SetString(PlayerNamePrefKey, value);
    }
}
