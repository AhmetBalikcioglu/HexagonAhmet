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
    public void ChangeScore()
    {
        score += scoreMultiplier;
        scoreText.text = "" + score;
    }
}
