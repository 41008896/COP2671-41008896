using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Initialize Variables
    public GameObject player;
    public GameObject projectilePrefab;
    public float horizontalInput = 0.0f;
    public float verticalInput = 0.0f;
    public float speed = 20.0f;
    private float rotationSpeed = 70.0f;
    private int horizontalBounds = 20;
    private int verticalBounds = 25; // Added vertical bounds
    
    public int isFire = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get player input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical"); // Added vertical input
        // Move Player
        MovePlayer();
        // Fire Projectile
        FireProjectile();
    }

void MovePlayer()
{
    // Rotation
    float rotation = horizontalInput * rotationSpeed * Time.deltaTime;
    player.transform.Rotate(Vector3.up * rotation);

    // Forward/Backward Movement
    Vector3 movement = player.transform.forward * verticalInput;
    if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // Move player forward/backward with sprint.
            player.transform.position += movement * (speed * 3) * Time.deltaTime;
        }
    else
        {
            // Move player forward/backward, no sprint.
            player.transform.position += movement * speed * Time.deltaTime;
        }

    // Restrict player to world bounds
    if (player.transform.position.x < -horizontalBounds)
        {
            // Set player position to left bounds
            player.transform.position = new Vector3(-horizontalBounds, player.transform.position.y, player.transform.position.z);
        }
    else if (player.transform.position.x > horizontalBounds)
        {
            // Set player position to right bounds
            player.transform.position = new Vector3(horizontalBounds, player.transform.position.y, player.transform.position.z);
        }

    // Forward/backward bounds checking
    if (player.transform.position.z < -verticalBounds)
        {
            // Set player position to backward bounds
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -verticalBounds);
        }
    else if (player.transform.position.z > verticalBounds)
        {
            // Set player position to forward bounds
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, verticalBounds);
        }
}

    void FireProjectile()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Instantiate a projectile
            Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
            isFire = 1;
        }
    }
}