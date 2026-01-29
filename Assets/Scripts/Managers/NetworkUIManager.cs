using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI; // UI kullanacaksan gerekebilir ama fonksiyonlar için şart değil

public class NetworkUIManager : MonoBehaviour
{
    // Bu paneli bağlantı kurulunca gizlemek istersen referans ver
    [SerializeField] private GameObject connectionPanel;

    public void StartHostGame()
    {
        Debug.Log("Host Başlatılıyor...");
        NetworkManager.Singleton.StartHost();
        HidePanel();
    }

    public void StartClientGame()
    {
        Debug.Log("Client Başlatılıyor...");
        NetworkManager.Singleton.StartClient();
        HidePanel();
    }

    public void StartServerGame()
    {
        Debug.Log("Server Başlatılıyor...");
        NetworkManager.Singleton.StartServer();
        HidePanel();
    }

    private void HidePanel()
    {
        if (connectionPanel != null)
        {
            connectionPanel.SetActive(false);
        }
    }
}
