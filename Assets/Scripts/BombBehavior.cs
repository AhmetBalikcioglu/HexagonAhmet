using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BombBehavior : MonoBehaviour
{
    private int bombTimer;
    private TextMeshProUGUI bombText;
    private GameManager gameManager;


    void Start()
    {
        bombTimer = 5;
        gameManager = FindObjectOfType<GameManager>();
        bombText = GetComponentInChildren<TextMeshProUGUI>();
        bombText.SetText(bombTimer.ToString());
    }

    //TickBombTimer function decreases bombTimer for the called bomb.
    //It checks if the bombTimer is 0 and if it is it calls GameOver function from GameManager.
    //If it is still above 0 it updates the text on the bomb.
    public void TickBombTimer()
    {
        if (bombTimer > 0)
        {
            bombTimer--;
        }
        if (bombTimer == 0)
        {
            gameManager.GameOver();
        }
        Debug.Log("TickBombTimer");
        bombText.SetText(bombTimer.ToString());
    }
}
