using UnityEngine;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine.Events;

public class SessionItemData : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI sessionPlayersCount;
    [SerializeField] private TextMeshProUGUI sessionRegionAndUsernameText;
    [SerializeField] private TextMeshProUGUI sessionPrivateStatusText;

    [Header("Color Settings")]
    [SerializeField] private Color redColor;
    [SerializeField] private Color greenColor;

    public UnityEvent<ISessionInfo> OnSessionSelected;
    public UnityEvent OnSessionDeselected;

    private ISessionInfo sessionInfo;


    public void SetSession(ISessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;
        SetSessionNameText(sessionInfo.Name);
        SetSessionPlayerCountText(sessionInfo.MaxPlayers - sessionInfo.AvailableSlots, sessionInfo.MaxPlayers);
        SetSessionRegionText();
    }

    public void SetSessionNameText(string sessionName) => sessionNameText.text = sessionName;

    public void SetSessionPlayerCountText(int currentPlayers, int maxPlayers) => sessionPlayersCount.text = $"{currentPlayers}/{maxPlayers}";

    public void SetSessionRegionText() => sessionRegionAndUsernameText.text = $"{sessionInfo.Properties["Region"].Value}";

    public void SetSessionPrivateStatusText()
    {
        if (sessionInfo.HasPassword)
        {
            sessionPrivateStatusText.text = "PRIVATE";
            sessionPrivateStatusText.color = redColor;
        }

        else
        {
            sessionPrivateStatusText.text = "PUBLIC";
            sessionPrivateStatusText.color = greenColor;
        }
    }

    public void JoinSession()
    {
        if (sessionInfo.HasPassword)
        {
            HandleLobbyUI.instance.OpenJoinSessionPasswordObj(sessionInfo);
        }

        else
        {
            HandleLobby.instance.JoinSelectedSession(sessionInfo);
        }
    }
}