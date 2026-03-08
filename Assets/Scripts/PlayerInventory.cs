using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private readonly HashSet<string> keys = new HashSet<string>();

    public bool AddKey(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            Debug.LogWarning("PlayerInventory: keyId vacio.");
            return false;
        }

        bool added = keys.Add(keyId);
        if (added)
        {
            Debug.Log("Llave recogida: " + keyId);
        }
        else
        {
            Debug.Log("La llave ya estaba en inventario: " + keyId);
        }

        return added;
    }

    public bool HasKey(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return false;
        }

        return keys.Contains(keyId);
    }
}
