using Unity.Netcode.Components;
using UnityEngine;

// Wajib inherit dari NetworkTransform
public class ClientNetworkTransform : NetworkTransform
{
    // Memaksa Unity untuk memberikan izin pergerakan kepada Client (pemilik objek)
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}