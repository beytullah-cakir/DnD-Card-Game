using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance; 

    [Header("UI Referansları")]
    public GameObject abilityPanel; 
    public TextMeshProUGUI cardNameText;
    
    [Header("Yetenek Butonları")]
    public Button ability1Btn;
    public Button ability2Btn;
    public Button ability3Btn;
    public Button attackBtn;

    [Header("Yetenek İsimleri (Text)")]
    public TextMeshProUGUI ability1Text;
    public TextMeshProUGUI ability2Text;
    public TextMeshProUGUI ability3Text;

    private int selectedAbilityIndex = -1;
    private CardData currentCardData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Panelleri ve butonları başlangıçta gizle
        if(abilityPanel != null) abilityPanel.SetActive(false);
        if(attackBtn != null) 
        {
            attackBtn.gameObject.SetActive(false); // Tamamen gizle
            attackBtn.onClick.AddListener(OnAttack);
        }

        // UI Yetenek buton dinleyicileri (Artık kullanılmıyor ama kod bütünlüğü için kalsın)
        if(ability1Btn) ability1Btn.onClick.AddListener(() => SelectAbility(0));
        if(ability2Btn) ability2Btn.onClick.AddListener(() => SelectAbility(1));
        if(ability3Btn) ability3Btn.onClick.AddListener(() => SelectAbility(2));
    }

    public void ShowAbilities(CardData data)
    {
        if (data == null) return;
        currentCardData = data;
        selectedAbilityIndex = -1; 
        
        // Attack butonunu gizle (henüz saldırı seçilmedi)
        if(attackBtn != null) attackBtn.gameObject.SetActive(false);

        // Not: abilityPanel'i artık açmıyoruz çünkü yetenekler dünyada (World Space) gösteriliyor.
        // Sadece veriyi tutuyoruz.
    }

    public void HidePanel()
    {
        if(abilityPanel != null) abilityPanel.SetActive(false);
        if(attackBtn != null) attackBtn.gameObject.SetActive(false); // Saldırı butonunu da gizle
        
        currentCardData = null;
        selectedAbilityIndex = -1;

        if (AbilityTooltip.Instance != null) AbilityTooltip.Instance.Hide();
    }

    public void SelectAbility(int index)
    {
        selectedAbilityIndex = index;
        
        // Saldırı butonunu GÖSTER ve AKTİF ET
        if(attackBtn != null) 
        {
            attackBtn.gameObject.SetActive(true);
            attackBtn.interactable = true;
        }

        // Tooltip logic moved to CardInteraction internal overlay
    }

    private void ResetButtonColors()
    {
        // Tüm butonları beyaza (varsayılan) döndür
        RestoreColor(ability1Btn);
        RestoreColor(ability2Btn);
        RestoreColor(ability3Btn);
    }

    private void RestoreColor(Button btn)
    {
        if(btn == null) return;
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.selectedColor = Color.white;
        btn.colors = cb;
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

        AbilityData selectedAbility = null;
        if(selectedAbilityIndex == 0) selectedAbility = currentCardData.ability1;
        else if(selectedAbilityIndex == 1) selectedAbility = currentCardData.ability2;
        else if(selectedAbilityIndex == 2) selectedAbility = currentCardData.ability3;

        if (selectedAbility != null)
        {
            Debug.Log($"Saldırı Yapıldı! Kart: {currentCardData.cardName}, Yetenek: {selectedAbility.abilityName}, Hasar: {selectedAbility.damage}");
        }
        else
        {
            Debug.LogWarning("Seçilen slotta bir Yetenek Verisi (Ability Data) yok!");
        }
        
        // Saldırı yapıldıktan sonra butonu gizle (veya isteğe bağlı paneli kapat)
        if(attackBtn != null) attackBtn.gameObject.SetActive(false);

        // Saldırıdan sonra tüm kartları eski haline (yerine) gönder
        var allCards = FindObjectsOfType<CardInteraction>();
        foreach (var card in allCards)
        {
            card.Deselect();
        }
    }
}
