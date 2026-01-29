using UnityEngine;
using DG.Tweening;

public class WorldAbilityButton : MonoBehaviour
{
    // Text kaldırıldı, sadece ikon/sprite olacak
    public SpriteRenderer iconRenderer;
    
    private int abilityIndex;
    private CardInteraction parentCard;
    private bool isSelected = false;

    public void Setup(int index, CardInteraction parent)
    {
        // İsim ataması yok
        abilityIndex = index;
        parentCard = parent;
        
        // Başlangıç rengi (Pasif)
        if (iconRenderer != null) iconRenderer.color = Color.white; 
    }

    public void SetIcon(Sprite icon)
    {
        if (iconRenderer != null) iconRenderer.sprite = icon;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (iconRenderer != null)
        {
            // Seçilince Yeşil, Seçilmezse Beyaz (veya gri)
            iconRenderer.color = isSelected ? Color.green : Color.white;
        }
    }

    public int GetIndex()
    {
        return abilityIndex;
    }

    public CardInteraction GetParent()
    {
        return parentCard;
    }
}
