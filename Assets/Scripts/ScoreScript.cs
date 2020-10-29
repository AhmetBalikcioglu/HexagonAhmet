using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public Text scoreText;
    public int score;
    private int scoreMultiplier;

    private void Start()
    {
        score = 0;
        scoreMultiplier = 5;
    }

    //ChangeScore function adds 5 points to the score and updates scoreText.
    //It is called everytime a tile is destroyed. It is called from DestroyMatchesAt function in BoardScript.
    public void ChangeScore()
    {
        score += scoreMultiplier;
        scoreText.text = "" + score;
    }
}
