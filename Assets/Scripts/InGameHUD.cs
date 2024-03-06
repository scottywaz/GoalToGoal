using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameHUD : MonoBehaviour
{
    [SerializeField] private GameObject countdownContainer;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] protected TextMeshProUGUI player2ScoreText;

    public void OnClickQuit()
    {
        GameManager.Singleton.QuitGame();
    }

    public void UpdateScore(int playerNum, int score)
    {
        if(playerNum == 1) player1ScoreText.text = score.ToString();
        else player2ScoreText.text = score.ToString();
    }

    public void StartCountdown(int startValue, Action callback)
    {
        countdownContainer.SetActive(true);
        StartCoroutine(StartCountdownCo(startValue, callback));
    }

    private IEnumerator StartCountdownCo(int startValue, Action callback)
    {
        for(int i = startValue; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

		countdownText.text = "0";
        yield return new WaitForSeconds(.5f);
		countdownContainer.SetActive(false);

		if (callback != null) callback.Invoke();      
	}
}
