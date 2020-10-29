using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour
{
    [Header("Input Related")]
    public bool dontSelect;
    public float swipeAngle = 0f;
    public float swipeResist = 0.5f;
    public Vector2 firstTouchPosition;

    private Vector2 finalTouchPosition;

    [Header("Board Related")]
    private BoardScript board;
    private TileBehavior tile;
    private float moveTimer = 0f;
    private float timerLimit = 2f;


    void Start()
    {
        board = FindObjectOfType<BoardScript>();
        tile = FindObjectOfType<TileBehavior>();
        dontSelect = false;
    }

    //Update function checks if Game State is in wait and FillBoardCo is working.
    //If somehow the Game State is stuck in wait it calculates 2 seconds and changes
    //the Current State from wait to move so user can continue to play
    private void Update()
    {
        if (board.currentState == GameState.wait && !board.fillBoardWorking)
        {
            moveTimer += Time.deltaTime;
        }
        else
        {
            moveTimer = 0;
        }
        if (moveTimer >= timerLimit)
        {
            board.currentState = GameState.move;
        }
    }

    //MouseDown function first checks the Current State and if it is on move it gets the first touch position
    //and calls ClosestTiles function from TileBehavior Class.
    public void MouseDown(TileBehavior tile)
    {
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tile.ClosestTiles();
        }
    }

    //MouseUp function first checks the Current State and dontSelect bool. dontSelect bool is for odd behaviour 
    //when ClosestTiles is calculated. If it is on move and dontSelect is false(means there is none odd behavior
    //it gets the final touch position and calls CalculateAngle function.
    public void MouseUp(TileBehavior tile)
    {
        if (!dontSelect && board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle(tile);
        }
    }

    //CalculateAngle function checks if the first and final touch positions difference is greater than swipeResist.
    //If it is, puts Current State to wait and calculates the tan value for swipe and calls UserInput function.
    //If it is not, it deselects the selected tiles and puts Current State to move.
    void CalculateAngle(TileBehavior tile)
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.x, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            UserInput(tile);
        }
        else
        {
            tile.selectedTiles[0] = tile.selectedTiles[1] = tile.selectedTiles[2] = null;
            board.currentState = GameState.move;
        }
    }

    //UserInput function checks swipeAngle(tan value for swipe) and determines isTurning and turningWay values
    //for the selected tiles.
    void UserInput(TileBehavior tile)
    {
        if (swipeAngle > -45 && swipeAngle <= 45)
        {
            //Right Swipe
            tile.turningWay = -1;
            tile.isTurning = true;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135)
        {
            //Up Swipe
            tile.turningWay = 1;
            tile.isTurning = true;
        }
        else if (swipeAngle > 135 || swipeAngle <= -135)
        {
            //Left Swipe
            tile.turningWay = 1;
            tile.isTurning = true;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135)
        {
            //Down Swipe
            tile.turningWay = -1;
            tile.isTurning = true;
        }
    }
}
