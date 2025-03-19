using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
    public List<GameObject> Orbs;
    public EForms Form;
    public int Kills;
    public int Deaths;
    public float GhostFormStart;
    public float TitanFormStart;

    public bool IsGhostFormOver(float duration) => Form == EForms.GHOST && duration < Time.time - GhostFormStart;
    public bool IsTitanFormOver(float duration) => Form == EForms.TITAN && duration < Time.time - TitanFormStart;
}
public enum EForms
{
    WARRIOR, GHOST, TITAN
}
