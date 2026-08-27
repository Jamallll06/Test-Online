using Unity.Netcode;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    private Vector3 velocity;

    private CharacterController controller;
    private Camera playerCamera;

    public NetworkVariable<ulong> networkClientId = new NetworkVariable<ulong>();
    public NetworkVariable<Color32> playerColor = new NetworkVariable<Color32>();

    [Header("UI Referensi")]
    public TextMeshProUGUI idText;

    [Header("Visual Referensi")]
    public Renderer playerRenderer;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (IsServer)
        {
            networkClientId.Value = OwnerClientId;
            playerColor.Value = GetColorForClientId(OwnerClientId);
        }

        ApplyPlayerColor(playerColor.Value);
        UpdatePlayerIDText(networkClientId.Value);

        playerColor.OnValueChanged += (Color32 oldColor, Color32 newColor) =>
        {
            ApplyPlayerColor(newColor);
        };

        networkClientId.OnValueChanged += (ulong oldId, ulong newId) =>
        {
            UpdatePlayerIDText(newId);
        };

        if (!IsOwner)
        {
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            enabled = false;
        }
        else
        {
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
                TPSCamera tpsCam = playerCamera.GetComponent<TPSCamera>();
                if (tpsCam != null) tpsCam.SetTarget(transform);
            }

            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.gameObject != playerCamera.gameObject)
            {
                mainCam.gameObject.SetActive(false);
            }
        }
    }

    private Color32 GetColorForClientId(ulong clientId)
    {
        switch (clientId % 4)
        {
            case 0: return Color.red;
            case 1: return Color.blue;
            case 2: return Color.green;
            case 3: return Color.yellow;
            default: return Color.magenta;
        }
    }

    private void ApplyPlayerColor(Color color)
    {
        if (playerRenderer != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            playerRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", color);
            playerRenderer.SetPropertyBlock(propBlock);
        }
    }

    private void UpdatePlayerIDText(ulong id)
    {
        if (idText != null)
        {
            idText.text = "Player " + id;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // Reset kecepatan jatuh jika menyentuh tanah
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Transform camTransform = playerCamera != null ? playerCamera.transform : transform;
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * moveZ + right * moveX).normalized;
        controller.Move(move * speed * Time.deltaTime);

        // Putar badan ke arah jalan
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }

        // Logika Loncat (Tombol Spasi) - Memastikan perintah loncat terbaca dengan baik
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Menerapkan gravitasi
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}