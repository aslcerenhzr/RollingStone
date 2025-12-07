using UnityEngine;
using System.Collections.Generic;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Aynı ID'ye sahip portal'lar birbirine bağlıdır")]
    public int portalID = 1;

    private static Dictionary<int, List<Portal>> portalsByID = new Dictionary<int, List<Portal>>();

    void Awake()
    {
        // Portal'ı ID'sine göre dictionary'ye ekle
        if (!portalsByID.ContainsKey(portalID))
        {
            portalsByID[portalID] = new List<Portal>();
        }
        portalsByID[portalID].Add(this);
    }

    void OnDestroy()
    {
        // Portal destroy edildiğinde dictionary'den çıkar
        if (portalsByID.ContainsKey(portalID))
        {
            portalsByID[portalID].Remove(this);
            if (portalsByID[portalID].Count == 0)
            {
                portalsByID.Remove(portalID);
            }
        }
    }

    // Aynı ID'ye sahip diğer portalı bul (kendisi hariç)
    public Portal GetOtherPortal()
    {
        if (!portalsByID.ContainsKey(portalID))
        {
            Debug.LogWarning("[PORTAL] Portal ID " + portalID + " için başka portal bulunamadı!");
            return null;
        }

        List<Portal> portals = portalsByID[portalID];
        
        // Kendisi hariç başka portal var mı?
        foreach (Portal portal in portals)
        {
            if (portal != this && portal != null)
            {
                return portal;
            }
        }

        Debug.LogWarning("[PORTAL] Portal ID " + portalID + " için başka portal bulunamadı!");
        return null;
    }

    // Portal'ın çıkış yönünü al (transform.up kullanarak)
    public Vector2 GetExitDirection()
    {
        return transform.right;
    }
}
