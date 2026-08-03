using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using WebSocketSharp;

public class HandleLobbyUI : MonoBehaviour
{
    public static HandleLobbyUI instance;

    [Header("Main References")]
    [SerializeField] private GameObject mainUIObj;

    [Header("Create Server UI References")]
    [SerializeField] private GameObject createServerUIObj;
    [SerializeField] private TMP_InputField serverNameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField maxPlayersInputField;

    [Header("Join Server UI References")]
    [SerializeField] private GameObject joinServerListUIObj;
    [SerializeField] private Transform serverListParent;
    [SerializeField] private GameObject serverListCardPrefab;

    [Header("Join Session Password UI References")]
    [SerializeField] private GameObject joinSessionPasswordUIObj;
    [SerializeField] private TMP_InputField joinSessionPasswordInputField;

    private ISessionInfo currentSelectedSessionInfo;

    private void Start()
    {
        instance = this;

        mainUIObj.SetActive(true);
        createServerUIObj.SetActive(false);
        joinServerListUIObj.SetActive(false);
        joinSessionPasswordUIObj.SetActive(false);
        SettingsManager.instance.CloseSettings();
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void EnableMainUIObj(bool condition)
    {
        mainUIObj.SetActive(condition);
    }

    public void OpenSettings()
    {
        SettingsManager.instance.OpenSettings();
        EnableMainUIObj(false);
    }

    public void OpenCreateServerList()
    {
        createServerUIObj.SetActive(true);
        EnableMainUIObj(false);
    }

    public void CloseCreateServerList()
    {
        createServerUIObj.SetActive(false);
        EnableMainUIObj(true);
    }

    public void OpenJoinServerList()
    {
        joinServerListUIObj.SetActive(true);
        EnableMainUIObj(false);

        // populate the list every time it's opened
        HandleLobby.instance.RefreshSessionList();
    }

    public void CloseJoinServerList()
    {
        joinServerListUIObj.SetActive(false);
        EnableMainUIObj(true);
    }

    public void ClearServerList()
    {
        foreach (Transform child in serverListParent)
            Destroy(child.gameObject);
    }

    public void CreateServerListCard(ISessionInfo sessionInfo)
    {
        GameObject card = Instantiate(serverListCardPrefab, serverListParent);

        if (card.TryGetComponent<SessionItemData>(out var item))
        {
            item.SetSession(sessionInfo);
        }
    }

    public void CreateSession()
    {
        if (int.TryParse(maxPlayersInputField.text, out int maxPlayer))
        {
            HandleLobby.instance.CreateServerAsHost(serverNameInputField.text, passwordInputField.text, maxPlayer, !string.IsNullOrEmpty(passwordInputField.text));
        }
    }

    public void OpenJoinSessionPasswordObj(ISessionInfo session)
    {
        currentSelectedSessionInfo = session;
        joinSessionPasswordUIObj.SetActive(true);
    }

    public void JoinSession()
    {
        HandleLobby.instance.JoinSelectedSession(currentSelectedSessionInfo, joinSessionPasswordInputField.text);
    }

    public void CloseJoinSessionPasswordObj()
    {
        currentSelectedSessionInfo = null;
        joinSessionPasswordUIObj.SetActive(false);
    }

    public void ClearJoinPasswordField()
    {
        joinSessionPasswordInputField.text = "";
    }

    public bool IsJoinSessionPasswordObjActive()
    {
        return joinSessionPasswordUIObj.activeInHierarchy;
    }
}