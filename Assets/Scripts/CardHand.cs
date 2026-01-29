using System.Collections.Generic;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    [Header("Dizilim Ayarları")]
    public int cardCount = 5;
    public float spacing = 1.3f;        // Kartlar arası yatay mesafe
    public float selectionLiftAmount = 1.5f; // Seçilen kartın ne kadar yukarı çıkacağı
    public float zOffsetAmount = 0.1f;  // Kartların derinlik farkı

    [Header("Referanslar")]
    public GameObject cardPrefab;
    public Transform handContainer;

    private List<GameObject> cards = new List<GameObject>();

    void Start()
    {
        if (handContainer == null) handContainer = this.transform;
        if (handContainer.childCount == 0 && cardPrefab != null) SpawnCards();
        ArrangeCards();
    }

    [ContextMenu("Kartları Diz")]
    [ContextMenu("Kartları Diz")]
    public void ArrangeCards()
    {
        RefreshCardList();
        int count = cards.Count;
        if (count == 0) return;

        // Linear Layout: Yan yana dizilim
        // Kartlar merkezden eşit uzaklıkta sola ve sağa dağıtılır.
        
        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + (i * spacing);
            float y = 0f;
            
            float z = -i * zOffsetAmount; 

            cards[i].transform.localPosition = new Vector3(x, y, z);
            cards[i].transform.localRotation = Quaternion.identity;

            // Kartın seçim yükselme ayarını Hand üzerinden güncelle
            CardInteraction interaction = cards[i].GetComponent<CardInteraction>();
            if (interaction != null)
            {
                interaction.liftAmount = selectionLiftAmount;
            }
        }
    }

    private void RefreshCardList()
    {
        if (handContainer == null) return;
        cards.Clear();
        foreach (Transform child in handContainer) cards.Add(child.gameObject);
    }

    public void SpawnCards()
    {
        if (cardPrefab == null || handContainer == null) return;

        // Önce temizle
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
        if (handContainer != null) ArrangeCards();
    }
}
