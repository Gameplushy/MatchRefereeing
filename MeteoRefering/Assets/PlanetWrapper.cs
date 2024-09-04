using UnityEngine;

public class PlanetWrapper : MonoBehaviour
{
    public Animator animator;
    public KMSelectable selectable;
    public SpriteRenderer sr;

    public Material material
    {
        get { return sr.material; }
    }
}
