using UnityEngine;

public class Collectibles : MonoBehaviour
{
    public enum CollectibleType
    { Default, Shield }

    public CollectibleType collectibleType = CollectibleType.Default;
}
