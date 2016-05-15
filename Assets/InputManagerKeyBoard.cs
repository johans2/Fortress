using UnityEngine;
using System.Collections.Generic;


public class InputManagerKeyBoard : MonoBehaviour
{
    private enum MoveDiretion {
        Up,
        Down,
        Left,
        Right
    }
    
    public Camera mainCamera;
    public float moveSpeed = 10f;
       
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveCamera(MoveDiretion.Up);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveCamera(MoveDiretion.Down);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveCamera(MoveDiretion.Left);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveCamera(MoveDiretion.Right);
        }
    }


    void MoveCamera(MoveDiretion direction)
    {
        switch (direction)
        {
            case MoveDiretion.Up:
                mainCamera.transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);
                break;
            case MoveDiretion.Down:
                mainCamera.transform.position += new Vector3(0, -moveSpeed * Time.deltaTime, 0);
                break;
            case MoveDiretion.Left:
                mainCamera.transform.position += new Vector3(-moveSpeed * Time.deltaTime, 0, 0);
                break;
            case MoveDiretion.Right:
                mainCamera.transform.position += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
                break;
            default:
                break;
        }

    }
    
}
