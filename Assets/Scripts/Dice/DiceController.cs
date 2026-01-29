using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DiceController : MonoBehaviour
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
        // 1. Zarı aktif et
        gameObject.SetActive(true); 
        
        // 2. Callback'i sakla (Animasyon bitince çağıracağız)
        storedCallback = onRollComplete;

        // 3. Yazıyı temizle
        if(resultText != null) resultText.text = "";

        // 4. Animasyonu tetikle
        if(diceAnimator != null) diceAnimator.SetTrigger("Roll");
    }

    // BU FONKSİYONU ANIMATION EVENT ÇAĞIRACAK
    public void OnAnimationFinished()
    {
        // 1. Sonucu belirle
        int result = Random.Range(1, 21);
        
        // 2. Yazıyı güncelle (Kameraya dönük)
        if(resultText != null) 
        {
            resultText.text = result.ToString();
            // Metnin rotasyonunu kamerayla aynı yap (Billboard effect)
            if (Camera.main != null)
                resultText.transform.rotation = Camera.main.transform.rotation;
        }

        Debug.Log("Zar animasyonu bitti (Event), sonuç: " + result);

        // 3. Callback'i çalıştır ve temizle
        storedCallback?.Invoke(result);
        storedCallback = null;
    }

    public void HideDice()
    {
         gameObject.SetActive(false);
    }
}
