using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform Target;
    public Vector3 offset = new Vector3(0f, 10f, -8f);
    public float followSpeed = 5f;
    public float lookDownAngle = 45f;
    void Update()
    {
        if (Target == null) return;

        Vector3 desiredPosition = Target.position + offset;

        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        
        Quaternion targetRotation = Quaternion.Euler(lookDownAngle, 0f, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
    }
}
