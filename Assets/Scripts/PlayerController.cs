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

	public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(new FixedString32Bytes(""));
	public NetworkVariable<int> Score = new NetworkVariable<int>(0);

	private Vector2 _startingPos;
	private bool _gameStarted = false;
	private float _currentSpeed = 0f;
	private const float ACCEL = .5f;
	private const float MAX_SPEED = 15f;

	// Start is called before the first frame update
	void Start()
    {
		DontDestroyOnLoad(gameObject);
	}

	public override void OnNetworkSpawn()
	{
		if(IsHost)
        {
			playerImage.color = Color.blue;
			PlayerName.Value = "Player1";
		}
		else
		{
			playerImage.color = Color.red;
			PlayerName.Value = "Player2";
		}

		playerNameText.text = PlayerName.Value.ToString();
		_startingPos = transform.position;
	}

	public override void OnNetworkDespawn()
	{

	}

	public void StartGame()
	{
		_gameStarted = true;
	}

	public void Reset()
	{
		transform.position = _startingPos;
		_gameStarted = false;
		_currentSpeed = 0f;
		playerRigidBody.velocity = Vector3.zero;
		playerRigidBody.SetRotation(0f);
	}

	private void UpdateServer()
    {
		// Logic to actually move the player with the current speed
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

	private void UpdateClient()
	{
		if (!IsLocalPlayer || !_gameStarted) return;

		// accelerating
		if(Input.GetKey(KeyCode.Space))
		{
			_currentSpeed = Mathf.Min(_currentSpeed + ACCEL, MAX_SPEED);		
		}
		else if(_currentSpeed > 0)// deaccelerate
		{
			_currentSpeed = Mathf.Max(_currentSpeed - ACCEL, 0);
		}		
	}

	private void FixedUpdate()
	{
		if(IsServer)
		{
			UpdateServer();
		}
		
		if(IsClient)
		{
			UpdateClient();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		// Checking if we entered the goal
		if(collision.tag == "Goal")
		{
			// Player Scored
			if((collision.gameObject.name == "Goal1" && !IsHost) ||
				(collision.gameObject.name == "Goal2" && IsHost))
			{
				Score.Value += 1;
				int playerNum = IsHost ? 1 : 2;
				GameManager.Singleton.PlayerScored(playerNum, Score.Value);
			}
		}
	}
}
