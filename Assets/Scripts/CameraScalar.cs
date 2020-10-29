using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class CameraScalar : MonoBehaviour
{
    private BoardScript board;
    private float camOffSet;
    private float aspectRatio = 0.625f;
    private float padding = 4f;
    private float xOffSet = 0.914f;
    private float yOffSet = 1.06f;
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<BoardScript>();
        camOffSet = -10f;
        if (board != null)
        {
            RepositionCamera(board.width - 1, board.height - 1);
        }
    }

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
            Camera.main.orthographicSize = board.height / 2 + padding;
        }
    }
}
