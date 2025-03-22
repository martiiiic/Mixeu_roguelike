using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMouseMovement : MonoBehaviour
{
    public float sensitivity = 0.1f; // Adjust how much the object moves
    public float smoothing = 5f; // Smoothness of movement
    public float rotationAmount = 5f; // Adjust how much the object rotates
    public float idleMoveAmount = 0.05f; // Amount of idle movement
    public float idleRotationAmount = 2f; // Amount of idle rotation
    public float idleTime = 3f; // Time before idle movement starts

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 lastMousePosition;
    private float idleTimer;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        lastMousePosition = Input.mousePosition;
        idleTimer = 0f;
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;

        if (mousePosition != lastMousePosition)
        {
            idleTimer = 0f;
            lastMousePosition = mousePosition;
        }
        else
        {
            idleTimer += Time.deltaTime;
        }

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Normalize mouse position (-1 to 1)
        float xOffset = (mousePosition.x / screenWidth - 0.5f) * 2f;
        float yOffset = (mousePosition.y / screenHeight - 0.5f) * 2f;

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (idleTimer >= idleTime)
        {
            float idleX = Mathf.Sin(Time.time) * idleMoveAmount;
            float idleY = Mathf.Cos(Time.time) * idleMoveAmount;
            targetPosition = initialPosition + new Vector3(idleX, idleY, 0);
            targetRotation = initialRotation * Quaternion.Euler(Mathf.Sin(Time.time) * idleRotationAmount, Mathf.Cos(Time.time) * idleRotationAmount, 0);
        }
        else
        {
            targetPosition = initialPosition + new Vector3(xOffset * sensitivity, yOffset * sensitivity, 0);
            targetRotation = initialRotation * Quaternion.Euler(yOffset * -rotationAmount, xOffset * rotationAmount, 0);
        }

        // Smoothly move towards target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothing);
    }
}
