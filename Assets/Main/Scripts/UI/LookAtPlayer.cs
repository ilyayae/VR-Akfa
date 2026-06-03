using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    Camera player;
    private float startRotationX;
    private float startRotationZ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = Camera.main; 
        startRotationX = transform.eulerAngles.x;
        startRotationZ = transform.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        float rotationSpeed = 5f;
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            direction *= -1f;
            Quaternion targetHorizontalRotation = Quaternion.LookRotation(direction);
            float targetRotationY = targetHorizontalRotation.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(startRotationX, targetRotationY, startRotationZ);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
