using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
    public List<GameObject> Orbs;
    public EForms Form;
}
public enum EForms
{
    WARRIOR, GHOST, TITAN
}
