using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private GameObject connectionContainer;
	[SerializeField] private Button HostAndPlayButton;
	[SerializeField] private Button ConnectButton;
	[SerializeField] private TextMeshProUGUI ipAddressInput;
	[SerializeField] private TextMeshProUGUI portInput;
	[SerializeField] private TextMeshProUGUI statusText;

	private string _ipAddress;
	private string _port;

	void OnEnable()
	{
		statusText.gameObject.SetActive(false);
		HostAndPlayButton.enabled = true;
		ConnectButton.enabled = true;
		ipAddressInput.text = "127.0.0.1";
		portInput.text = "7777";
	}

	public void OnClickHostAndPlay()
	{
		HostAndPlayButton.enabled = false;
		ConnectButton.enabled = false;

		if (CheckConnectionData())
		{
			GameManager.Singleton.StartHost(_ipAddress, _port);
			StopAllCoroutines();
			ShowStatus("Waiting for Opponent");
		}
		else
		{
			HostAndPlayButton.enabled = true;
			ConnectButton.enabled = true;
		}
	}

	public void OnClickConnectToClient()
	{
		if (CheckConnectionData())
		{
			GameManager.Singleton.ConnectToClient(_ipAddress, _port);
			StopAllCoroutines();
			StartCoroutine(ShowConnectionStatus());
		}
	}

	private bool CheckConnectionData()
	{
		_ipAddress = SanitizeInput(ipAddressInput.text);
		_port = SanitizeInput(portInput.text);

		if (_ipAddress == "")
		{
			StopAllCoroutines();
			StartCoroutine(ShowInvalidStatus("IP Address Invalid"));
			return false;
		}

		if (_port == "")
		{
			StopAllCoroutines();
			StartCoroutine(ShowInvalidStatus("Port Invalid"));
			return false;
		}

		return true;
	}

	private void ShowStatus(string status)
	{
		statusText.text = status;
		statusText.gameObject.SetActive(true);
	}

	private IEnumerator ShowConnectionStatus()
	{
		ShowStatus("Connecting...");
		HostAndPlayButton.enabled = false;
		ConnectButton.enabled = false;

		yield return new WaitForSeconds(GameManager.Singleton.TimeoutInSeconds);

		yield return new WaitForSeconds(1f);

		statusText.text = "Connection Failed!";
		HostAndPlayButton.enabled = true;
		ConnectButton.enabled = true;

		yield return new WaitForSeconds(5f);

		statusText.gameObject.SetActive(false);
	}

	private IEnumerator ShowInvalidStatus(string status)
	{
		ShowStatus(status);

		yield return new WaitForSeconds(5f);

		statusText.gameObject.SetActive(false);
	}

	static string SanitizeInput(string dirtyString)
	{
		// sanitize the input for the ip address
		return Regex.Replace(dirtyString, "[^0-9.]", "");
	}
}
