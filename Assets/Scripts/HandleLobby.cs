using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Qos;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HandleLobby : MonoBehaviour
{
    public static HandleLobby instance;

    public ISession activeSession;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName;

    private QuerySessionsResults sessions;

    private bool isHost;

    private void Awake()
    {
        // for singleton
        instance = this;
    }

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private async Task<string> GetBestRegion()
    {
        try
        {
            var qosResults = await QosService.Instance.GetSortedQosResultsAsync("relay", new List<string>());

            if (qosResults != null && qosResults.Count > 0)
            {
                return qosResults[0].Region;
            }

            return "Unknown";
        }

        catch (Exception e)
        {
            Debug.Log(e);
        }

        return "Unknown";
    }

    #region Server Creation

    public async void CreateServerAsHost(string name, string password, int maxPlayers, bool hasPassword)
    {
        try
        {
            isHost = true;

            // get our current region
            string myRegion = await GetBestRegion();

            SessionOptions options = new SessionOptions
            {
                MaxPlayers = maxPlayers,
                Name = name,
                Password = hasPassword ? password : null,
                IsPrivate = false,
            }.WithRelayNetwork();

            options.SessionProperties = new Dictionary<string, SessionProperty>
            {
                { "Region", new SessionProperty(myRegion, VisibilityPropertyOptions.Public)},
            };

            // create the new session
            activeSession = await MultiplayerService.Instance.CreateSessionAsync(options);

            if (activeSession == null)
                return;

            JoinNetworkGame();
        }

        catch (RequestFailedException e)
        {
            Debug.LogException(e);
        }

        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    #endregion

    #region Server List / Selection

    private async void UpdateSessions()
    {
        QuerySessionsOptions options = new QuerySessionsOptions
        {
            SortOptions = new List<SortOption>
            {
                new SortOption(SortOrder.Descending, SortField.AvailableSlots)
            }
        };

        sessions = await MultiplayerService.Instance.QuerySessionsAsync(options);

        HandleLobbyUI.instance.ClearServerList();

        if (sessions == null)
            return;

        foreach (var sessionInfo in sessions.Sessions)
            HandleLobbyUI.instance.CreateServerListCard(sessionInfo);
    }

    public void RefreshSessionList()
    {
        UpdateSessions();
    }

    #endregion

    #region Joining Game

    public async void JoinSelectedSession(ISessionInfo sessionInfo, string password = default)
    {
        if (sessionInfo == null)
            return;

        try
        {
            isHost = false;

            if (sessionInfo.HasPassword)
            {
                JoinSessionOptions options = new JoinSessionOptions
                {
                    Password = password
                };

                activeSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionInfo.Id, options);
            }

            else
            {
                activeSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionInfo.Id);
            }

            if (activeSession == null)
                return;

            HandleLobbyUI.instance.ClearJoinPasswordField();
            HandleLobbyUI.instance.CloseJoinServerList();

            JoinNetworkGame();
        }

        catch (RequestFailedException e)
        {
            Debug.LogException(e);

            RefreshSessionList();
        }

        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void JoinNetworkGame()
    {
        if (isHost)
        {
            Debug.Log("joining");

            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        else
        {
            NetworkManager.Singleton.StartClient();
        }

        isHost = false;
    }

    #endregion
}