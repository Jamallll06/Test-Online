using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;
    private CharacterController controller;
    private Camera playerCamera;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (!IsOwner)
        {
            // Matikan kamera untuk player lain
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            enabled = false; // Matikan script update untuk player lain agar hemat performa
        }
        else
        {
            // Aktifkan kamera untuk player lokal
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
                TPSCamera tpsCam = playerCamera.GetComponent<TPSCamera>();
                if (tpsCam != null) tpsCam.SetTarget(transform);
            }

            // Matikan Main Camera scene utama agar tidak bentrok
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.gameObject != playerCamera.gameObject)
            {
                mainCam.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Mengambil arah kamera yang aktif
        Transform camTransform = playerCamera != null ? playerCamera.transform : transform;
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;

        // Abaikan perbedaan sumbu Y agar karakter tidak terbang saat melihat ke atas/bawah
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * moveZ + right * moveX).normalized;

        // Menggerakkan karakter menggunakan Character Controller
        controller.Move(move * speed * Time.deltaTime);

        // Membuat badan karakter menghadap ke arah jalannya
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }

        // Gravitasi sederhana
        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }
}