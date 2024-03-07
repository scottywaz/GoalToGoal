using System;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private InGameHUD inGameHud;
    [SerializeField] private EndGameDialog endGameDialog;

    private MainMenu _mainMenu;
    private InGameHUD _inGameHud;
    private EndGameDialog _endGameDialog;

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

    public void ShowEndGame(string playerNameOfWinner)
    {
        ClearUI();
        _endGameDialog = GameObject.Instantiate(endGameDialog, transform);
        _endGameDialog.SetPlayerWon(playerNameOfWinner);
    }

    public void UpdateScore(string playerName, int score)
    {
		_inGameHud.UpdateScore(playerName, score);
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
