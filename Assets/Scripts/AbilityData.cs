using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Card Game/Ability Data")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    [TextArea] public string description;
    public int damage;
    public int manaCost;
    public Sprite icon; // İstenirse yetenek ikonu
}
