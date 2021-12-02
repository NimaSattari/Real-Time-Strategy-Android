using Mirror;
using UnityEngine;
using Steamworks;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject landingPagePanel;
    [SerializeField] bool useSteam = false;

    public void Quit()
    {
        AudioManagerMainMenu.instance.PlayJoinSound();
        Application.Quit();
    }

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Start()
    {
        if (!useSteam) { return; }
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobyyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobyyEntered);
    }

    public void HostLobby()
    {
        landingPagePanel.SetActive(false);
        if (useSteam)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
            return;
        }

        NetworkManager.singleton.StartHost();
    }
    void OnLobyyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            landingPagePanel.SetActive(true);
            return;
        }
        NetworkManager.singleton.StartHost();
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "HostAddress", SteamUser.GetSteamID().ToString());
    }
    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    void OnLobyyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) { return; }
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "HostAddress");
        NetworkManager.singleton.networkAddress = hostAddress;
        NetworkManager.singleton.StartClient();
        landingPagePanel.SetActive(false);
    }
}
