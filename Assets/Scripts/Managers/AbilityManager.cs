using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance; 

    [Header("Zar Kontrolcüsü")]
    public DiceController diceController;

    private int selectedAbilityIndex = -1;
    private CardData currentCardData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowAbilities(CardData data)
    {
        if (data == null) return;
        currentCardData = data;
        selectedAbilityIndex = -1; 
    }

    public void HidePanel()
    {
        currentCardData = null;
        selectedAbilityIndex = -1;
    }

    public void SelectAbility(int index)
    {
        selectedAbilityIndex = index;
    }

    public void OnAttack()
    {
        if(selectedAbilityIndex == -1)
        {
            Debug.LogWarning("HATA: Bir yetenek seçilmedi! Lütfen önce yetenek butonuna tıklayın.");
            return;
        }
        
        if (currentCardData == null)
        {
            Debug.LogWarning("HATA: Kart Verisi (Card Data) bulunamadı! Lütfen karta bir ScriptableObject atayın.");
            return;
        }

        // Verileri yedekle (çünkü kartları indirince currentCardData null olabilir)
        var cardData = currentCardData;
        var abilityIndex = selectedAbilityIndex;

        // 1. ÖNCE KARTLARI ESKİ HALİNE DÖNDÜR (Hemen)
        DeselectAllCards();

        // 2. SONRA ZARI AT
        if (diceController != null)
        {
            diceController.Roll((rollResult) => {
                // Zar animasyonu bitince sonucu işle
                PerformAttack(cardData, abilityIndex, rollResult);
                
                // Zarı bir süre sonra gizle (Sonucu okumak için süre)
                StartCoroutine(HideDiceWithDelay(1.5f));
            });
        }
        else
        {
            Debug.LogWarning("Zar Kontrolcüsü atanmamış! Direkt saldırı yapılıyor.");
            PerformAttack(cardData, abilityIndex, 0); 
        }
    }

    private System.Collections.IEnumerator HideDiceWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if(diceController != null) diceController.HideDice();
    }

    private void PerformAttack(CardData card, int abilityIdx, int diceResult)
    {
        AbilityData selectedAbility = null;
        if(abilityIdx == 0) selectedAbility = card.ability1;
        else if(abilityIdx == 1) selectedAbility = card.ability2;
        else if(abilityIdx == 2) selectedAbility = card.ability3;

        if (selectedAbility != null)
        {
            string logMsg = $"Saldırı Yapıldı! Kart: {card.cardName}, Yetenek: {selectedAbility.abilityName}, Hasar: {selectedAbility.damage}";
            if(diceResult > 0) logMsg += $", Zar Sonucu: {diceResult}";
            
            Debug.Log(logMsg);
        }
        else
        {
            Debug.LogWarning("Seçilen slotta bir Yetenek Verisi (Ability Data) yok!");
        }
    }

    private void DeselectAllCards()
    {
        var allCards = FindObjectsOfType<CardInteraction>();
        foreach (var card in allCards)
        {
            card.Deselect();
        }
    }
}
