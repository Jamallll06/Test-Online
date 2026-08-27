using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RelayUIManager : MonoBehaviour
{
    [Header("Referensi UI Canvas")]
    public Button startHostButton;
    public Button joinClientButton;
    public TMP_InputField joinCodeInput;
    public TextMeshProUGUI joinCodeDisplay;

    async void Start()
    {
        // Inisialisasi layanan cloud Unity
        await UnityServices.InitializeAsync();

        // Login secara anonim jika belum terautentikasi
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void StartRelayHost()
    {
        try
        {
            // Meminta alokasi untuk 4 pemain (1 Host + 3 Client)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            // Mendapatkan Join Code untuk dibagikan
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Tampilkan kode di layar Host
            joinCodeDisplay.text = "Join Code Anda: " + joinCode;
            Debug.Log("Relay Join Code: " + joinCode);

            // CARA MANUAL: Ekstraksi IP dan Port dari server Relay
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Memulai Host
            NetworkManager.Singleton.StartHost();

            SembunyikanMenuAwal();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void StartRelayClient()
    {
        try
        {
            // Mengambil teks dari kolom input UI
            string codeToJoin = joinCodeInput.text;
            if (string.IsNullOrEmpty(codeToJoin)) return;

            // Meminta izin bergabung menggunakan kode
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(codeToJoin);

            // CARA MANUAL: Ekstraksi data koneksi untuk masuk ke sesi Host
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // Memulai Client
            NetworkManager.Singleton.StartClient();

            SembunyikanMenuAwal();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private void SembunyikanMenuAwal()
    {
        // Sembunyikan tombol dan input field agar tidak mengganggu gameplay
        startHostButton.gameObject.SetActive(false);
        joinClientButton.gameObject.SetActive(false);
        joinCodeInput.gameObject.SetActive(false);
    }
}