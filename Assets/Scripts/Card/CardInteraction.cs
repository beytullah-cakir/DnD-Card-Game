using UnityEngine;
using UnityEngine.InputSystem; 
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Unity.Netcode;

public class CardInteraction : NetworkBehaviour
{
    [Header("Animation Settings")]
    public float liftAmount = 1.5f;
    public float moveDuration = 0.3f;

    [Header("Data")]
    public CardData cardData;
    public Sprite cardBackSprite; // Arka yüz görseli
    public GameObject abilityButtonPrefab;

    [Header("Ability Button Settings")]
    public float abilityHeight = 2.2f;  // Kartın merkezinden yüksekliği
    public float abilitySpread = 0.8f;  // Yanların merkeze uzaklığı

    [Header("Ability Info Overlay")]
    public GameObject infoOverlay; // Kartın üzerini kapatacak panel/canvas
    public TMPro.TextMeshProUGUI infoNameText;
    public TMPro.TextMeshProUGUI infoDescText;
    public TMPro.TextMeshProUGUI infoDamageText;

    private bool isSelected = false;
    private Vector3 originalScale;
    private static Camera mainCamera; // Static yaparak performansı artır

    private SpriteRenderer spriteRenderer;
    private Vector3 originalPos;
    private int originalOrder;

    private List<WorldAbilityButton> spawnedButtons = new List<WorldAbilityButton>();
    private int selectedAbilityIndex = -1;


    // --- STATIC INPUT MANAGER ---
    // Her kartta Update çalışır ama sadece biri o kare işlemini yapmalı.
    private static int lastProcessedFrame = -1;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        UpdateCardVisuals();
    }

    private void Start()
    {
        originalScale = transform.localScale;
        if (mainCamera == null) mainCamera = Camera.main;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) originalOrder = spriteRenderer.sortingOrder;
        
        HideAbilityInfo(); // Başlangıçta gizle
    }

    private void UpdateCardVisuals()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (IsOwner)
        {
            // Sahibiysen ön yüzü gör
            if (cardData != null && cardData.artwork != null) 
                spriteRenderer.sprite = cardData.artwork;
        }
        else
        {
            // Rakip isen arka yüzü gör
            if (cardBackSprite != null) 
                spriteRenderer.sprite = cardBackSprite;
        }
    }

    private void Update()
    {
        // 1. Sadece 'Sol Tık' anında çalış
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame) return;

        // 2. Bu karede zaten işlem yapıldı mı?
        if (lastProcessedFrame == Time.frameCount) return;
        lastProcessedFrame = Time.frameCount; // İşlem başlıyor, kilitle

        // 3. Global Input İşleme Fonksiyonunu Çağır
        HandleGlobalInput();
    }

    private static void HandleGlobalInput()
    {
        // UI tarafından engelleme kontrolü (Sadece dış UI, kartın kendi panelleri değil)
        if (IsBlockedByExternalUI()) return;

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Mouse pozisyonundan Ray at
        Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
        Vector2 pointerWorldPos = mainCamera.ScreenToWorldPoint(pointerScreenPos);
        
        // Sadece tek bir noktaya değil, altındaki her şeye bak
        RaycastHit2D[] hits = Physics2D.RaycastAll(pointerWorldPos, Vector2.zero);

        // Adayları belirle
        WorldAbilityButton bestButton = null;
        CardInteraction bestCard = null;

        // Sorting için değişkenler
        int maxButtonSort = int.MinValue;
        int maxCardSort = int.MinValue;
        float minCardZ = float.MaxValue;
        int maxCardSibling = int.MinValue;

        foreach (var hit in hits)
        {
            // -- BUTON KONTROLÜ --
            var btn = hit.transform.GetComponent<WorldAbilityButton>();
            if (btn != null)
            {
                // Butonlar UI olduğu için veya üstte olduğu için genelde önceliklidir.
                // Eğer birden fazla buton üst üste ise (pek olmaz ama) sorting bakılabilir.
                // Şimdilik ilk bulduğu butonu veya Canvas sorting'i yüksek olanı alabiliriz.
                
                // Basitçe: İlk bulduğun butonu al (Genelde butonlar çakışmaz)
                bestButton = btn; 
                // Buton bulunduysa, kart aramaya gerek yok. Buton en üsttedir.
                break; 
            }

            // -- KART KONTROLÜ --
            if (bestButton == null) // Henüz buton bulamadıysak kartlara bak
            {
                var card = hit.transform.GetComponent<CardInteraction>();
                if (card != null)
                {
                    // Sadece kendi kartlarımızı seçebiliriz
                    if (card.IsSpawned && !card.IsOwner) continue;

                    SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
                    int sortOrder = (sr != null) ? sr.sortingOrder : 0;
                    float zPos = card.transform.position.z;
                    int siblingIndex = card.transform.GetSiblingIndex();

                    // Kim daha önde? Algoritma:
                    // 1. Order (Büyük iyi)
                    // 2. Z (Küçük iyi)
                    // 3. Sibling (Büyük iyi)

                    bool isBetter = false;

                    if (bestCard == null)
                    {
                        isBetter = true;
                    }
                    else
                    {
                        if (sortOrder > maxCardSort) isBetter = true;
                        else if (sortOrder == maxCardSort)
                        {
                            if (zPos < minCardZ - 0.001f) isBetter = true; // Z daha önde
                            else if (Mathf.Abs(zPos - minCardZ) < 0.001f) // Z eşit
                            {
                                if (siblingIndex > maxCardSibling) isBetter = true; // Hiyerarşide altta
                            }
                        }
                    }

                    if (isBetter)
                    {
                        bestCard = card;
                        maxCardSort = sortOrder;
                        minCardZ = zPos;
                        maxCardSibling = siblingIndex;
                    }
                }
            }
        }

        // --- SONUÇLARI UYGULA ---

        if (bestButton != null)
        {
            // Bir butona tıklandı
            var parentCard = bestButton.GetParent();
            if (parentCard != null)
            {
                parentCard.OnAbilityClicked(bestButton.GetIndex());
            }
        }
        else if (bestCard != null)
        {
            // Bir karta tıklandı
            if (bestCard.isSelected)
            {
                // Zaten seçiliyse
                // EĞER BİR YETENEK SEÇİLİYSE -> Karta tıklayınca da SALDIR
                if (bestCard.selectedAbilityIndex != -1 && AbilityManager.Instance != null)
                {
                    AbilityManager.Instance.OnAttack();
                }
                else
                {
                    // Yetenek seçili değilse kapat (toggle)
                    bestCard.Deselect();
                }
            }
            else
            {
                // Değilse seç (ve diğerlerini kapat)
                bestCard.SelectCard();
            }
        }
        else
        {
            // Boşluğa tıklandı -> Her şeyi kapat
            DeselectAllCards();
        }
    }

    private static void DeselectAllCards()
    {
        var allCards = FindObjectsOfType<CardInteraction>();
        foreach (var card in allCards)
        {
            card.Deselect();
        }
    }

    // --- INSTANCE METHODS (Eski mantık aynen korunuyor) ---

    public void SelectCard()
    {
        // Diğerlerini kapat
        var allCards = FindObjectsOfType<CardInteraction>();
        foreach (var card in allCards)
        {
            if (card != this) card.Deselect();
        }

        if (!isSelected)
        {
            transform.DOKill(true);

            originalPos = transform.localPosition;
            if (spriteRenderer) originalOrder = spriteRenderer.sortingOrder;

            isSelected = true;

            // 1. En öne gelme
            if (spriteRenderer) spriteRenderer.sortingOrder = 100;

            // 2. Sadece yukarı kay
            Vector3 targetPos = new Vector3(originalPos.x, originalPos.y + liftAmount, originalPos.z);
            transform.DOLocalMove(targetPos, moveDuration);

            // Dünyada Yetenek butonlarını göster
            ShowWorldAbilities();
            
            // AbilityManager'a veriyi gönder
            if (AbilityManager.Instance != null) 
                AbilityManager.Instance.ShowAbilities(cardData);

            Debug.Log(gameObject.name + " seçildi.");
        }
    }

    public void Deselect()
    {
        if (isSelected)
        {
            isSelected = false;

            transform.localScale = originalScale;
            if (spriteRenderer) spriteRenderer.sortingOrder = originalOrder;
            transform.DOLocalMove(originalPos, moveDuration);

            // Butonları ve paneli gizle
            HideWorldAbilities();
            HideAbilityInfo();

            // UI Manager ile bağlantıyı kes
            if (AbilityManager.Instance != null)
                AbilityManager.Instance.HidePanel();
        }
    }

    private void ShowWorldAbilities()
    {
        if (abilityButtonPrefab == null) return;

        HideWorldAbilities(); // Temizle

        Vector3[] offsets = {
            new Vector3(-abilitySpread, abilityHeight - 0.3f, -0.1f), // Sol
            new Vector3(0f, abilityHeight, -0.1f),                    // Orta
            new Vector3(abilitySpread, abilityHeight - 0.3f, -0.1f)   // Sağ
        };

        for (int i = 0; i < 3; i++)
        {
            GameObject obj = Instantiate(abilityButtonPrefab, transform);
            obj.transform.localPosition = Vector3.zero; 

            WorldAbilityButton btnScript = obj.GetComponent<WorldAbilityButton>();
            if (btnScript != null)
            {
                btnScript.Setup(i, this);
                if (cardData != null)
                {
                    AbilityData ad = null;
                    if (i == 0) ad = cardData.ability1;
                    else if (i == 1) ad = cardData.ability2;
                    else if (i == 2) ad = cardData.ability3;

                    if (ad != null && ad.icon != null) btnScript.SetIcon(ad.icon);
                }
            }

            spawnedButtons.Add(btnScript);

            // Order Ayarla
            if (spriteRenderer)
            {
                var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
                foreach (var r in renderers) r.sortingOrder = spriteRenderer.sortingOrder + 1;

                var canvases = obj.GetComponentsInChildren<Canvas>();
                foreach (var c in canvases) { c.overrideSorting = true; c.sortingOrder = spriteRenderer.sortingOrder + 1; }
            }

            obj.transform.DOLocalMove(offsets[i], 0.4f).SetEase(Ease.OutBack);
        }
    }

    private void HideWorldAbilities()
    {
        foreach (var btn in spawnedButtons)
        {
            if (btn != null)
            {
                btn.transform.DOKill();
                Destroy(btn.gameObject);
            }
        }
        spawnedButtons.Clear();
        selectedAbilityIndex = -1;
    }

    private void OnAbilityClicked(int index)
    {
        // Eğer zaten seçili olan yeteneğe tekrar tıklanırsa -> SALDIR
        if (selectedAbilityIndex == index)
        {
            if (AbilityManager.Instance != null)
            {
                AbilityManager.Instance.OnAttack();
            }
            return;
        }

        // --- YENİ SEÇİM ---
        selectedAbilityIndex = index;

        
        // Buton parlama
        foreach (var btn in spawnedButtons)
        {
            if (btn != null) btn.SetSelected(btn.GetIndex() == index);
        }

        // Overlay Göster
        if (cardData != null)
        {
            AbilityData ad = null;
            if (index == 0) ad = cardData.ability1;
            else if (index == 1) ad = cardData.ability2;
            else if (index == 2) ad = cardData.ability3;

            ShowAbilityInfo(ad);
        }

        // Manager Haber
        if (AbilityManager.Instance != null)
        {
             AbilityManager.Instance.SelectAbility(index); 
        }
    }

    private void ShowAbilityInfo(AbilityData data)
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(true);
            if (infoNameText) infoNameText.text = data.abilityName;
            if (infoDescText) infoDescText.text = data.description;
            if (infoDamageText) infoDamageText.text = (data.damage > 0) ? $"Hasar: {data.damage}" : "";

            if (spriteRenderer)
            {
                var c = infoOverlay.GetComponent<Canvas>();
                if (c) { c.overrideSorting = true; c.sortingOrder = spriteRenderer.sortingOrder + 2; }
            }
        }
    }

    private void HideAbilityInfo()
    {
        if (infoOverlay != null) infoOverlay.SetActive(false);
    }

    // --- STATIC HELPER FOR UI BLOCKING ---
    private static bool IsBlockedByExternalUI()
    {
        if (EventSystem.current == null || Pointer.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Pointer.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            GameObject clickedUI = results[0].gameObject;
            
            // Eğer tıklanan UI bir CardInteraction'ın parçasıysa (Overlay vb.) BLOKLAMA
            if (clickedUI.GetComponentInParent<CardInteraction>() != null) return false;

            // Değilse (Saldırı butonu vb.) BLOKLA
            Debug.Log($"UI Blokladı: {clickedUI.name}");
            return true;
        }

        return false;
    }
}
