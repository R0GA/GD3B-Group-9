using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform camTarget;
    public float pLerp = .02f;
    public float rLerp = .01f;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, camTarget.position, pLerp);
        transform.rotation = Quaternion.Lerp(transform.rotation, camTarget.rotation, rLerp);
    }
}
