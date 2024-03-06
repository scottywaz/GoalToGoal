using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
	[SerializeField] private SpriteRenderer playerImage;
	[SerializeField] private ParticleSystem playerParticleSystem;
	[SerializeField] private Rigidbody2D playerRigidBody;
	[SerializeField] private TextMeshPro playerNameText;

	public NetworkVariable<int> score = new NetworkVariable<int>(0);

	private string playerName;
	private Vector2 _startingPos;
	private bool _gameStarted = false;
	private float _currentSpeed = 0f;
	private const float ACCEL = .75f;
	private const float MAX_SPEED = 20f;
	private int _numPlayersReadyToPlayAgain = 0;

	private void Awake()
	{
	}

	void Start()
    {
		DontDestroyOnLoad(gameObject);
	}

	public override void OnNetworkSpawn()
	{
		if(IsServer)
		{
			playerName = "Player" + OwnerClientId;
			playerImage.color = Color.blue;

			if(!IsHost)
			{
				playerImage.color = Color.red;
			}
		}

		playerNameText.text = playerName.ToString();
		_startingPos = transform.position;
	}

	public void StartGame()
	{
  		_gameStarted = true;
	}

	[Rpc(SendTo.ClientsAndHost)]
	public void ResetClientRpc()
	{
		transform.position = _startingPos;
		_gameStarted = false;
		_numPlayersReadyToPlayAgain = 0;
		_currentSpeed = 0f;
		playerRigidBody.velocity = Vector3.zero;
		playerRigidBody.SetRotation(0f);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateNumberOfReadyPlayersServerRpc()
	{
		_numPlayersReadyToPlayAgain++;
		if(_numPlayersReadyToPlayAgain > 1) RestartGameClientRpc();
	}

	[ClientRpc]
	public void RestartGameClientRpc()
	{
		GameManager.Singleton.RestartGame();
	}

	[ClientRpc]
	public void PlayerScoreClientRpc()
	{
		GameManager.Singleton.PlayerScored(playerName.ToString(), score.Value);
	}

	[ServerRpc]
	public void PlayerScoredServerRpc()
	{
		score.Value += 1;
		PlayerScoreClientRpc();
	}

	private void FixedUpdate()
	{
		if (!IsOwner || !_gameStarted) return;

		// accelerating
		if (Input.GetKey(KeyCode.Space))
		{
			_currentSpeed = Mathf.Min(_currentSpeed + ACCEL, MAX_SPEED);
		}
		else if (_currentSpeed > 0)// deaccelerate
		{
			_currentSpeed = Mathf.Max(_currentSpeed - ACCEL, 0);
		}

		if (_currentSpeed > 0)
		{
			// Use the mouse as the direction to go
       		Vector3 screenPosition = Input.mousePosition;
			screenPosition.z = 1f;
			Vector3 targetPos = Camera.main.ScreenToWorldPoint(screenPosition);
				
			// Update Rotation first
			float angle = Mathf.Atan2(targetPos.y - transform.position.y, targetPos.x - transform.position.x) * Mathf.Rad2Deg;
			Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
			playerRigidBody.MoveRotation(angle);

			// Now update direction
			playerRigidBody.AddForce(transform.right * _currentSpeed);
			if (playerRigidBody.velocity.magnitude > MAX_SPEED)
			{
				playerRigidBody.velocity = playerRigidBody.velocity.normalized * MAX_SPEED;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (IsOwner)
		{
			// Checking if we entered the goal
			if (collision.tag == "Goal")
			{
				// Player Scored
				if ((collision.gameObject.name == "Goal1" && !IsServer) ||
					(collision.gameObject.name == "Goal2" && IsServer))
				{
					PlayerScoredServerRpc();
				}
			}
		}
	}
}
