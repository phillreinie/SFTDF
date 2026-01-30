using UnityEngine;

[CreateAssetMenu(menuName = "Game/Resources/Resource")]
public class ResourceData : ScriptableObject
{
    [Header("Identity")]
    public string id;              // e.g. "res_scrap"
    public string displayName;     // e.g. "Scrap"
    public Tier tier = Tier.T1;

    [Header("UI")]
    public Sprite icon;
    public Color uiColor = Color.white;
}