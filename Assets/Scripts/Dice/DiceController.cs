using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class DiceController : NetworkBehaviour
{
    [Header("Referanslar")]
    public TextMeshProUGUI resultText; 

    private Animator diceAnimator;

    // Callback'i saklamak için değişken
    private System.Action<int> storedCallback;

    private void Start()
    {
        diceAnimator = GetComponent<Animator>();
    }

    public void Roll(System.Action<int> onRollComplete)
    {
        // 1. Callback'i sakla (Sadece çağıran client'ta çalışacak)
        storedCallback = onRollComplete;

        // 2. Server'dan zar atmasını iste
        RequestRollServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRollServerRpc()
    {
        // 1. Sonucu belirle (Server otoritesi)
        int result = Random.Range(1, 21);

        // 2. Tüm client'lara sonucu ve animasyonu bildir
        PerformRollClientRpc(result);
    }

    [ClientRpc]
    private void PerformRollClientRpc(int result)
    {
        // 1. Zarı aktif et
        gameObject.SetActive(true);

        // 2. Yazıyı temizle
        if(resultText != null) resultText.text = "";

        // 3. Animasyonu tetikle
        if(diceAnimator != null) diceAnimator.SetTrigger("Roll");

        // 4. Sonucu sakla (Animasyon bitince kullanmak için)
        // Not: Animasyon event'i OnAnimationFinished'i çağıracak
        // Ancak sonucu buraya parametre olarak geçemiyoruz çünkü OnAnimationFinished parametresiz.
        // Bu yüzden sonucu geçici bir değişkende saklayabiliriz.
        currentResult = result;
    }

    private int currentResult;

    // BU FONKSİYONU ANIMATION EVENT ÇAĞIRACAK
    public void OnAnimationFinished()
    {
        // 1. Sonucu kullan (ClientRpc'den gelen)
        int result = currentResult;
        
        // 2. Yazıyı güncelle (Kameraya dönük)
        if(resultText != null) 
        {
            resultText.text = result.ToString();
            // Metnin rotasyonunu kamerayla aynı yap (Billboard effect)
            if (Camera.main != null)
                resultText.transform.rotation = Camera.main.transform.rotation;
        }

        Debug.Log("Zar animasyonu bitti (Event), sonuç: " + result);

        // 3. Callback'i çalıştır ve temizle (Sadece çağıran client'ta dolu olacak)
        storedCallback?.Invoke(result);
        storedCallback = null;
    }

    public void HideDice()
    {
         gameObject.SetActive(false);
    }
}
