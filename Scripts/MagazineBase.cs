using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MagazineBase : MonoBehaviour //, IMagazine
{
    [SerializeField, Tooltip("Automatically adds all children colliders if colliders is empty")]
    private bool autoManageColliders = false;
    [SerializeField]
    private List<Collider> colliders;

    public IEnumerable<Collider> Colliders { get { return colliders; } }
    
    private void OnValidate()
    {
        if (autoManageColliders && colliders.Count == 0)
        {
            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                colliders.Add(collider);
            }
        }
    }

    public void SetCollidersEnabled(bool isEnabled)
    {
        foreach(var collider in Colliders)
        {
            collider.enabled = isEnabled;
        }
    }
}
