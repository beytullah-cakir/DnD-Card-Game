using UnityEngine;
using TMPro;

public class AbilityTooltip : MonoBehaviour
{
    public static AbilityTooltip Instance;

    [Header("UI Objects")]
    public GameObject panel; // Arkaplan paneli
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI manaText;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 100, 0); // Butonun ne kadar üstünde çıksın (Screen Space)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Hide();
    }

    public void Show(AbilityData data, Vector3 worldPos)
    {
        if (data == null || panel == null) return;

        panel.SetActive(true);

        if (titleText) titleText.text = data.abilityName;
        if (descText) descText.text = data.description;
        if (damageText) damageText.text = $"Hasar: {data.damage}";
        // if (manaText) manaText.text = $"Mana: {data.manaCost}"; // İstenirse eklenir

        // Dünya pozisyonunu ekran pozisyonuna çevir
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        transform.position = screenPos + offset;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}
