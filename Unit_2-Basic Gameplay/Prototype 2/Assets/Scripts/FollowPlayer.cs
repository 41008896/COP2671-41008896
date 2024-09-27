using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public float distance = 7.0f; // Distance behind the player
    public float height = 5.0f;   // Height above the player

    // Late Update is called after each Update frame
    void LateUpdate()
    {
        // Get the player's rotation
        Quaternion playerRotation = player.transform.rotation;

        // Calculate the camera's new position behind and above the player
        Vector3 offset = playerRotation * new Vector3(0, height, -distance);

        // Set the camera's position
        transform.position = player.transform.position + offset;

        // Manually rotate the camera to face the same direction as the player
        transform.rotation = playerRotation;
        transform.Rotate(10f, 0f, 0f);  // Adjust this angle for a better view from above if needed
    }
}
