using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class CameraScalar : MonoBehaviour
{
    [Header("Board Related")]
    private BoardScript board;
    private float xOffSet = 0.914f;
    private float yOffSet = 1.06f;

    [Header("Camera Related")]
    private float camOffSet;
    private float aspectRatio = 0.625f;
    private float padding = 4f;
    private float screenWidth;
    private float screenHeight;


    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        aspectRatio = screenWidth / screenHeight;
        board = FindObjectOfType<BoardScript>();
        camOffSet = -10f;
        if (board != null)
        {
            RepositionCamera(board.width - 1, board.height - 1);
        }
    }

    //RepositionCamera function gets 2 float values and uses them to position the main camera
    //and configure Orthographic Size.
    //This function is used for setting the best camera view for different grid values.

    void RepositionCamera(float x, float y)
    {
        Vector3 tempPosition = new Vector3(x * xOffSet / 2, y * yOffSet / 2 + 1f, camOffSet);
        transform.position = tempPosition;
        if (board.width >= board.height)
        {
            Camera.main.orthographicSize = (board.width / 2 + padding) / aspectRatio;
        }
        else
        {
            Camera.main.orthographicSize = 2 * board.height / 3 + padding;
        }
    }
}
