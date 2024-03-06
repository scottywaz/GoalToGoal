using System;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private InGameHUD inGameHud;

    private MainMenu _mainMenu;
    private InGameHUD _inGameHud;

	// Start is called before the first frame update
	void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        ClearUI();
        _mainMenu = GameObject.Instantiate(mainMenu, transform);
	}

    public void ShowInGameHUD()
    {
        ClearUI();
		_inGameHud = GameObject.Instantiate(inGameHud, transform);
    }

    public void ShowEndGame(bool show, int playerWhoWon)
    {

    }

    public void UpdateScore(int playerNumber, int score)
    {
		_inGameHud.UpdateScore(playerNumber, score);
    }

    public void StartRound(int countdownTime, Action callback)
    {
		_inGameHud.StartCountdown(countdownTime, callback);
    }

    private void ClearUI()
    {
		foreach (Transform child in transform)
		{
			GameObject.Destroy(child.gameObject);
		}
	}
}
