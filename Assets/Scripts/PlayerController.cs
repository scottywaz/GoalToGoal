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
	private Quaternion _startingRotation;
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
		playerName = "Player" + (OwnerClientId+1);
		if (IsOwner)
		{
			playerImage.color = Color.blue;
		}

		playerNameText.text = playerName.ToString();
		_startingPos = transform.position;
		_startingRotation = transform.rotation;
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
		_rigidBody.angularVelocity = 0f;
		transform.rotation = _startingRotation;
		score.Value = 0;
	}

	// Movement is client authoritative
	// We should validate the movement on the server
	private void FixedUpdate()
	{
		if (!IsOwner || !_gameStarted) return;

		int spin = 0;
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			spin += 1;
		}

		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			spin -= 1;
		}

		int speed = 0;
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			speed += 1;
		}

		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
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
