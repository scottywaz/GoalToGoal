using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
	[SerializeField] private UIController uiController;

    private NetworkManager _networkManager;
    private UnityTransport _unityTransport;

	public static GameManager Singleton { get; private set; }

	public float TimeoutInSeconds { get; private set; }

	private int  _numberOfConnections;

	private void Awake()
	{
		if(Singleton != null && Singleton != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			Singleton = this;
		}

		_networkManager = GetComponent<NetworkManager>();
		_networkManager.OnClientConnectedCallback += OnClientConnected;
		_networkManager.OnClientDisconnectCallback += OnClientDisconnected;
		_networkManager.ConnectionApprovalCallback += ConnectionApproval;
	}

	// Start is called before the first frame update
	void Start()
    {
        _unityTransport = (UnityTransport)_networkManager.NetworkConfig.NetworkTransport;
		TimeoutInSeconds = (_unityTransport.ConnectTimeoutMS * _unityTransport.MaxConnectAttempts) / 1000f;
	}

	public void StartHost(string ipAddressString, string portString)
	{
		SetConnection(ipAddressString, portString);
		_networkManager.StartHost();
	}

	public void ConnectToClient(string ipAddressString, string portString)
	{
		SetConnection(ipAddressString, portString);
		_networkManager.StartClient();
	}

	public void QuitGame()
	{
		_networkManager.Shutdown();
		uiController.ShowMainMenu();
	}

	public void RestartGame()
	{
		PlayerController playerController = _networkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
		playerController.Reset();
		uiController.ShowInGameHUD();
		uiController.StartRound(5, RoundStarted);
	}

	public void PlayerScored(string playerName, int score)
	{
		uiController.UpdateScore(playerName, score);
		PlayerController playerController = _networkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
		if (playerController != null)
		{
			playerController.Reset();
		}

		if (score >= 3) // Player Won
		{
			uiController.ShowEndGame(playerName);
		}
		else
		{
			uiController.StartRound(3, RoundStarted);
		}
	}

	public void PlayerToPlayAgain()
	{
		PlayerController playerController = _networkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
		playerController.UpdateNumberOfReadyPlayersServerRpc();
	}

	private void SetConnection(string ipAddressString, string portString)
	{
		if (ushort.TryParse(portString, out ushort port))
		{
			_unityTransport.SetConnectionData(ipAddressString, port);
		}
		else
		{
			_unityTransport.SetConnectionData(ipAddressString, 7777);
		}
	}

	private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
		// 2 Player Game
		// If someone else tries to connect we don't accept
		if(_numberOfConnections == 2)
		{
			response.Approved = false;
			return;
		}

		_numberOfConnections++;
		response.CreatePlayerObject = true;

		// Start the host on the left side
		if(_networkManager.IsHost)
		{
			response.Position = new Vector3(-20, 0, 0);
			response.Rotation = Quaternion.identity;
		}
		// Start the person trying to connect on the right side
		else 
		{
			response.Position = new Vector3(20, 0, 0);
			response.Rotation = Quaternion.Euler(0, 0, -180);
		}
		
		response.Approved = true;
	}

	private void OnClientConnected(ulong obj)
	{
		if (_numberOfConnections == 2)
		{
			uiController.ShowInGameHUD();
			uiController.StartRound(5, RoundStarted);
		}
	}

	private void RoundStarted()
	{
		PlayerController playerController = _networkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
		if(playerController != null)
		{
			playerController.StartGame();
		}
	}

	private void OnClientDisconnected(ulong clientId)
	{
		if (_networkManager.IsServer && clientId != NetworkManager.ServerClientId)
		{
			return;
		}
		uiController.ShowMainMenu();
	}	

	private void OnDestroy()
	{
		_networkManager.OnClientConnectedCallback -= OnClientConnected;
		_networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
	}
}
