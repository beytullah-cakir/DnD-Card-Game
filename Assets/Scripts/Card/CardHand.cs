using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CardHand : NetworkBehaviour
{
    [Header("Dizilim Ayarları")]
    [Header("Dizilim Ayarları")]
    public int cardCount = 4; // Resimde 4 kart var
    public float spacing = 0.6f;        // Benim kartlarım arası mesafe
    public float remoteSpacing = 0.6f;  // Rakip kartlar arası mesafe (Aynı olsun diye ekledim)

    public float zOffsetAmount = 0.1f;  // Kartların derinlik farkı

    [Header("Online Pozisyonlar")]
    [Header("Online Pozisyonlar")]
    public float onlineY_Local = -4.0f;  // Benim kartlarımın Y yüksekliği
    public float onlineY_Remote = 4.0f;  // Rakibin kartlarının Y yüksekliği
    
    [Header("Referanslar")]
    public GameObject cardPrefab;
    public Transform handContainer;

    private List<GameObject> cards = new List<GameObject>();

    private void Start()
    {
        base.OnNetworkSpawn();
        
        // 1. Pozisyonu Ayarla
        if (IsOwner)
        {
            transform.position = new Vector3(0, onlineY_Local, 0);
            name = "MyHand";
        }
        else
        {
            transform.position = new Vector3(0, onlineY_Remote, 0);
            name = "EnemyHand";
        }

        // 2. Kartları Doğur (Sadece Sahibi)
        if (IsOwner)
        {
            SpawnInitialHandServerRpc();
        }
        
        // 3. Client'lar için de dizilimi zorla (Gecikmeli)
        Invoke(nameof(ForceArrange), 0.5f);
        Invoke(nameof(ForceArrange), 1.0f);
    }
    
    private void ForceArrange()
    {
        ArrangeCards();
    }

    [ServerRpc]
    private void SpawnInitialHandServerRpc()
    {
        for (int i = 0; i < cardCount; i++)
        {
            GameObject newCard = Instantiate(cardPrefab);
            var netObj = newCard.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.SpawnWithOwnership(OwnerClientId);
                netObj.TrySetParent(transform);
            }
        }
    }

    private void Update()
    {
        // Kart sayısı değiştiyse listeyi güncelle ve yeniden diz
        if (transform.childCount != cards.Count)
        {
            ArrangeCards();
        }
        
        // Emniyet: Eğer hiç kart listem yoksa ama çocuklarım varsa, listeyi doldur
        if (cards.Count == 0 && transform.childCount > 0)
        {
            RefreshCardList();
            ArrangeCards();
        }
    }

    private void OnTransformChildrenChanged()
    {
        // Child eklendiğinde hemen diz
        ArrangeCards();
    }

    [ContextMenu("Kartları Diz")]
    public void ArrangeCards()
    {
        RefreshCardList();
        int count = cards.Count;
        if (count == 0) return;

        // Sahibi ben isem 'spacing', değilsem 'remoteSpacing' kullan
        float currentSpacing = IsOwner ? spacing : remoteSpacing;

        float totalWidth = (count - 1) * currentSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            if(cards[i] == null) continue;

            float x = startX + (i * currentSpacing);
            float y = 0f;
            float z = -i * zOffsetAmount; 

            // Local Position kullanarak Hand'e göre konumlandır
            cards[i].transform.localPosition = new Vector3(x, y, z);
            cards[i].transform.localRotation = Quaternion.identity;
        }
    }

    private void RefreshCardList()
    {
        if (handContainer == null) return;
        cards.Clear();
        foreach (Transform child in handContainer) cards.Add(child.gameObject);
    }

    // Eski SpawnCards metodunu Offline için tutuyoruz
    public void SpawnCardsOffline()
    {
        if (cardPrefab == null || handContainer == null) return;

        // Temizle
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in handContainer) children.Add(child.gameObject);
        foreach (var child in children) { if (Application.isPlaying) Destroy(child); else DestroyImmediate(child); }
        
        cards.Clear();

        // Kartları oluştur
        for (int i = 0; i < cardCount; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, handContainer);
            newCard.name = "Card_" + i;
            cards.Add(newCard);
        }
        
        ArrangeCards();
    }

    private void OnValidate()
    {
        if (handContainer != null && (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)) 
            ArrangeCards();
    }
}
