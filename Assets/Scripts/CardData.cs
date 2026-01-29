using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Card", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite artwork;
    [TextArea] public string description;

    [Header("Yetenekler (Scriptable Objects)")]
    public AbilityData ability1;
    public AbilityData ability2;
    public AbilityData ability3;
}
