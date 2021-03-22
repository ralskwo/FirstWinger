using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateInput();
        UpdateMouse();
    }

    void UpdateInput()
    {
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            // Debug.Log("Up");
            moveDirection.y = 1;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // Debug.Log("Down");
            moveDirection.y = -1;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            // Debug.Log("Left");
            moveDirection.x = -1;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            // Debug.Log("Right");
            moveDirection.x = 1;
        }

        SystemManager.Instance.Hero.ProcessInput(moveDirection);
    }

    void UpdateMouse()
    {
        // [0, 1, 2, ...] : 마우스 왼쪽 버튼, 오른쪽 버튼, 중간(휠)버튼, 여타 추가 마우스 버튼들...
        if (Input.GetMouseButtonDown(0))
        {
            // Debug.Log("click");
            SystemManager.Instance.Hero.Fire();
        }
    }
}
