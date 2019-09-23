using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    const float START_GAME_TIME = 120f;
    const float TIME_ADD_PER_CLEAR = 3f;

    float _timeLeft;

    [SerializeField]
    GameObject MenuScreen;
    Text MenuText;

    [SerializeField]
    Image TimerFill;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        MenuText = MenuScreen.transform.Find("TextResult").GetComponent<Text>();
        MenuText.text = "Pairing Number Game";
    }

    public void Restart()
    {
        _timeLeft = START_GAME_TIME;
        StartCoroutine("GameTimer");
        MenuScreen.SetActive(false);
        TimerFill.fillAmount = 1f;
    }

    IEnumerator GameTimer()
    {
        while (_timeLeft > 0f)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            _timeLeft -= 0.1f;
            TimerFill.fillAmount = _timeLeft / START_GAME_TIME;
        }

        LoseGame();
    }

    public void WinGame()
    {
        StopCoroutine("GameTimer");

        MenuText.text = "You Win";

        MenuScreen.SetActive(true);
    }

    public void LoseGame()
    {
        MenuText.text = "You Lose";
        MenuScreen.SetActive(true);
    }

    public void AddTime()
    {
        _timeLeft += TIME_ADD_PER_CLEAR;
        if (_timeLeft > START_GAME_TIME)
            _timeLeft = START_GAME_TIME;
    }

    
}
