using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject inGameUI;
    public GameObject gameOverUI;

    //GameOver function stops all coroutines and calls DestroyAll Coroutine from BoardScript.
    //It is used for end game. It is called if bomb tick is reached 0 or there is a deadlock.
    public void GameOver()
    {
        BoardScript board = FindObjectOfType<BoardScript>();
        StopAllCoroutines();
        StartCoroutine(board.DestroyAll());
        inGameUI.SetActive(false);
        board.gameObject.SetActive(false);
        gameOverUI.SetActive(true);
        gameOverUI.transform.FindChild("ScoreText").GetComponent<Text>().text = FindObjectOfType<ScoreScript>().score.ToString();
    }
    
    //Restart function is for loading the scene again. It is called from restart button when the game is over.
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
