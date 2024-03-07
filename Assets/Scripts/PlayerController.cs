using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
	[SerializeField] private SpriteRenderer playerImage;
	[SerializeField] private ParticleSystem playerParticleSystem;
	[SerializeField] private TextMeshPro playerNameText;

	public NetworkVariable<int> score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

	private Rigidbody2D _rigidBody;
	private string playerName;
	private Vector2 _startingPos;
	private bool _gameStarted = false;

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

	public void Reset()
	{
		transform.position = _startingPos;
		_gameStarted = false;
		_rigidBody.velocity = Vector3.zero;
		_rigidBody.SetRotation(0f);
	}

	public void RestartGame()
	{
		GameManager.Singleton.RestartGameRpc();
	}

	private void FixedUpdate()
	{
		if (!IsOwner || !_gameStarted) return;

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

		// update rotation 
		float rotate = spin * ROTATE_SPEED;
		_rigidBody.angularVelocity = rotate;

		// update velocity
		if (speed != 0)
		{
			Vector3 speedVector = transform.right * (speed * ACCEL);
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
					score.Value += 1;
					GameManager.Singleton.PlayerScoredRpc(playerName.ToString(), score.Value);
				}
			}
		}
	}
}
