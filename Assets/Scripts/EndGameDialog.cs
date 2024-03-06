using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winTitleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;

    public void SetPlayerWon(string playerName)
    {
        winTitleText.text = $"{playerName} Wins!";
    }

    public void OnClickPlayAgain()
    {
        statusText.gameObject.SetActive(true);
        playAgainButton.enabled = false;
        quitButton.enabled = false; 
        GameManager.Singleton.PlayerToPlayAgain();
    }

    public void OnClickQuit()
    {
        GameManager.Singleton.QuitGame();
    }
}
