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

    // Start is called before the first frame update
    void Start()
    {
        bombTimer = 5;
        gameManager = FindObjectOfType<GameManager>();
        bombText = GetComponentInChildren<TextMeshProUGUI>();
        bombText.SetText(bombTimer.ToString());
    }

    public void TickBombTimer()
    {
        if (bombTimer > 0)
        {
            bombTimer -= 1;
        }
        if (bombTimer == 0)
        {
            gameManager.GameOver();
        }
        Debug.Log("TickBombTimer");
        bombText.SetText(bombTimer.ToString());
    }
}
