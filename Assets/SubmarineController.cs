using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineController : MonoBehaviour
{
    [SerializeField] private float acceleration;
    [SerializeField] private float speedDump;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxSpeed;

    private float rotationX;
    private float rotationY;
    
    private bool isAccelerating = false;
    private float speed;

    // Update is called once per frame
    void Update()
    {
        var mousePosition = Input.mousePosition;
        var horizontal = (mousePosition.x / Screen.width) * 2 - 1;
        var vertical = -((mousePosition.y / Screen.height) * 2 - 1);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isAccelerating ^= true;
            Cursor.lockState = isAccelerating ? CursorLockMode.Confined : CursorLockMode.None;
            Cursor.visible = !isAccelerating;
        }

        if (isAccelerating)
        {
            speed += Time.deltaTime * acceleration;
            speed = Mathf.Clamp(speed, 0, maxSpeed);
            speed = maxSpeed;
            
            rotationY += horizontal * rotationSpeed * Time.deltaTime;
            rotationX += vertical * rotationSpeed * Time.deltaTime * 0.5f;
            rotationX = Mathf.Clamp(rotationX, -Mathf.PI / 3, Mathf.PI / 3);

            var x = Mathf.Cos(-rotationY) * Mathf.Cos(-rotationX);
            var z = Mathf.Sin(-rotationY) * Mathf.Cos(-rotationX);
            var y = Mathf.Sin(-rotationX);

            transform.rotation = Quaternion.LookRotation(new Vector3(x, y, z), Vector3.up);
        }
        else
        {
            speed = Mathf.Lerp(speed, 0, speedDump * Time.deltaTime);
        }

        transform.Translate(0, 0, speed);
    }
}
