using System;

[Serializable]
public class PlayerEventArgs : EventArgs
{
    public string PlayerName { get; set; }
    public PlayerEventArgs(string name)
    {
        PlayerName = name;
    }
}
