using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private Camera player;
    private float startRotationZ;

    void Awake()
    {
        player = Camera.main;

        startRotationZ = transform.eulerAngles.z;
    }

    private void OnEnable()
    {
        if (player != null)
        {
            Vector3 direction = player.transform.position - transform.position;

            if (direction.sqrMagnitude > 0.01f)
            {
                direction *= -1f;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                Quaternion targetRotation = Quaternion.Euler(lookRotation.eulerAngles.x, lookRotation.eulerAngles.y, startRotationZ);

                transform.rotation = targetRotation;
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float rotationSpeed = 5f;
        Vector3 direction = player.transform.position - transform.position;

        if (direction.sqrMagnitude > 0.01f)
        {
            direction *= -1f;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion targetRotation = Quaternion.Euler(lookRotation.eulerAngles.x, lookRotation.eulerAngles.y, startRotationZ);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}