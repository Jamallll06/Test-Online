using Unity.Netcode;
using UnityEngine;

public class TPSCamera : MonoBehaviour
{
    private Transform target;
    [Header("Pengaturan Kamera")]
    public float distance = 5.0f;
    public float mouseSensitivity = 3.0f;
    public float minY = -20f;
    public float maxY = 60f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Mengambil input pergerakan mouse
        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Membatasi sudut pandang vertikal agar tidak terlalu menengadah atau kebalik
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // Menghitung rotasi dan posisi kamera terhadap target
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 targetPosition = target.position + Vector3.up * 1.5f;
        Vector3 position = targetPosition - (rotation * Vector3.forward * distance);

        transform.rotation = rotation;
        transform.position = position;
    }
}