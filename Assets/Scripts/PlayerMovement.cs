using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5f;
    public Transform cameraOfPlayer;
    CharacterController controller;
    private Vector3 velocity;
    private float gravity = -9.81f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(Vector3.up * velocity.y);
        if (DialogueManager.instance.isDialogueActive) return;
        if(Cursor.lockState != CursorLockMode.Locked) return;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 forward = cameraOfPlayer.forward;
        //Debug.Log("forward = " + forward);
        Vector3 right = cameraOfPlayer.right;
        //Debug.Log("right = " + right);
        forward.y = 0;
        right.y = 0;
        Vector3 movement = forward * z + right * x;
        controller.Move(movement * speed * Time.deltaTime);
    }
}
