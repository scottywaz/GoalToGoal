using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : NetworkBehaviour
{
	[SerializeField] private UIController uiController;

	public NetworkVariable<int> numPlayAgain = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	private UnityTransport _unityTransport;

	public static GameManager Singleton { get; private set; }

	public float TimeoutInSeconds { get; private set; }
	public int NumberOfConnections { get; private set; }

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
	}

	// Start is called before the first frame update
	void Start()
    {
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
		NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;

		_unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
		TimeoutInSeconds = (_unityTransport.ConnectTimeoutMS * _unityTransport.MaxConnectAttempts) / 1000f;
	}

	public void StartHost(string ipAddressString, string portString)
	{
		SetConnection(ipAddressString, portString);
		NetworkManager.Singleton.StartHost();
	}

	public void ConnectToClient(string ipAddressString, string portString)
	{
		SetConnection(ipAddressString, portString);
		NetworkManager.Singleton.StartClient();
	}

	[Rpc(SendTo.Everyone)]
	public void QuitGameRpc()
	{
		NetworkManager.Singleton.Shutdown();
		uiController.ShowMainMenu();
		NumberOfConnections = 0;
	}

	[Rpc(SendTo.Everyone)]
	public void RestartGameRpc()
	{
		PlayerController playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
		playerController.Reset();
		StartGameRpc();
	}

	[Rpc(SendTo.Everyone)]
	public void PlayerScoredRpc(string playerName, int score)
	{
		uiController.UpdateScore(playerName, score);
		PlayerController playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
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

	[Rpc(SendTo.Server)]
	public void PlayerToPlayAgainRpc()
	{
		numPlayAgain.Value += 1;
		// We have 2 players ready to play again
		if(numPlayAgain.Value > 1)
		{
			RestartGameRpc();
		}
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
		if(NumberOfConnections > 2)
		{
			response.Approved = false;
			return;
		}

		NumberOfConnections++;
		response.CreatePlayerObject = true;

		// Start the host on the left side
		if(NumberOfConnections == 1)
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
		// As soon as we get 1 connection we can start the game
		if (!NetworkManager.Singleton.IsHost)
		{
			StartGameRpc();
		}
	}

	[Rpc(SendTo.Everyone)]
	private void StartGameRpc()
	{
		uiController.ShowInGameHUD();
		uiController.StartRound(5, RoundStarted);
	}

	private void RoundStarted()
	{
		PlayerController playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
		if(playerController != null)
		{
			playerController.StartGame();
		}
	}

	private void OnClientDisconnected(ulong clientId)
	{
		QuitGameRpc();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (NetworkManager.Singleton != null)
		{
			NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
		}
	}
}
