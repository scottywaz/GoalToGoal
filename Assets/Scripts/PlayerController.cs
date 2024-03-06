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
	[SerializeField] private TextMeshPro playerNameText;

	public NetworkVariable<int> score = new NetworkVariable<int>(0);

	private Rigidbody2D _rigidBody;
	private string playerName;
	private Vector2 _startingPos;
	private bool _gameStarted = false;
	private float _currentSpeed = 0f;
	private float _currentSpin = 0f;
	private float _oldSpeed = 0f;
	private float _oldSpin = 0f;
	private int _numPlayersReadyToPlayAgain = 0;

	private const float ACCEL = 5f;
	private const float ROTATE_SPEED = 100f;
	private const float MAX_SPEED = 20f;	

	private void Awake()
	{
		_rigidBody = GetComponent<Rigidbody2D>();
	}

	void Start()
    {
		DontDestroyOnLoad(gameObject);
	}

	public override void OnNetworkSpawn()
	{
		playerName = "Player" + OwnerClientId;
		if (!IsLocalPlayer)
		{
			playerImage.color = Color.red;
		}

		playerNameText.text = playerName.ToString();
		_startingPos = transform.position;
	}

	public void StartGame()
	{
  		_gameStarted = true;
	}

	[ClientRpc]
	public void ResetClientRpc()
	{
		transform.position = _startingPos;
		_gameStarted = false;
		_numPlayersReadyToPlayAgain = 0;
		_currentSpeed = 0f;
		_rigidBody.velocity = Vector3.zero;
		_rigidBody.SetRotation(0f);
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

	[ServerRpc]
	public void UpdateMovementServerRpc(float speed, float spin)
	{
		_currentSpeed = speed;
		_currentSpin = spin;
	}

	private void FixedUpdate()
	{
		if(IsServer)
		{
			UpdateServer();
		}
		else
		{
			UpdateClient();
		}
	}

	private void UpdateClient()
	{
		if (!IsLocalPlayer || !_gameStarted) return;

		int spin = 0;
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			spin += 1;
		}

		if (Input.GetKey(KeyCode.RightArrow))
		{
			spin -= 1;
		}

		int speed = 0;
		if (Input.GetKey(KeyCode.UpArrow))
		{
			speed += 1;
		}

		if (Input.GetKey(KeyCode.DownArrow))
		{
			speed -= 1;
		}

		if(_oldSpeed != speed || _oldSpin != spin)
		{
			UpdateMovementServerRpc(speed, spin);
			_oldSpeed = speed;
			_oldSpin = spin;
		}
	}

	private void UpdateServer()
	{
		// update rotation 
		float rotate = _currentSpin * ROTATE_SPEED;
		_rigidBody.angularVelocity = rotate;

		// update velocity
		if (_currentSpeed != 0)
		{
			Vector3 speedVector = transform.right * (_currentSpeed * ACCEL);
			_rigidBody.AddForce(speedVector);

			// restrict max speed
			if (_rigidBody.velocity.magnitude > MAX_SPEED)
			{
				_rigidBody.velocity = _rigidBody.velocity.normalized * MAX_SPEED;
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
