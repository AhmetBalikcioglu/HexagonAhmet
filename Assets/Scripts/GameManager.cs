using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject inGameUI;
    public GameObject gameOverUI;

    public void GameOver()
    {
        StopAllCoroutines();
        StartCoroutine(GameObject.Find("Board").GetComponent<BoardScript>().DestroyAll());
        inGameUI.SetActive(false);
        GameObject.Find("Board").gameObject.SetActive(false);
        gameOverUI.SetActive(true);
        gameOverUI.transform.FindChild("ScoreText").GetComponent<Text>().text = FindObjectOfType<ScoreScript>().score.ToString();
        
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
