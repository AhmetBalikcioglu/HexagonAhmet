using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour
{

    public bool dontSelect;
    public float swipeAngle = 0f;
    public float swipeResist = 0.5f;

    private BoardScript board;
    private TileBehavior tile;
    public Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Touch touch;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<BoardScript>();
        tile = FindObjectOfType<TileBehavior>();
        dontSelect = false;
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && board.currentState == GameState.move)
            {
                firstTouchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                tile.ClosestTiles();
            }

            if (touch.phase == TouchPhase.Ended && !dontSelect && board.currentState == GameState.move)
            {
                finalTouchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                CalculateAngle();
            }
        }
    }
    
    /*private void OnMouseDown()
    {
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tile.ClosestTiles();
        }
    }

    private void OnMouseUp()
    {
        if (!dontSelect && board.currentState == GameState.move)
        {
            //board.currentState = GameState.wait;
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }*/

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.x, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            UserInput();
        }
        else
        {
            tile.selectedTiles[0] = tile.selectedTiles[1] = tile.selectedTiles[2] = null;
            board.currentState = GameState.move;
        }
    }

    void UserInput()
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
