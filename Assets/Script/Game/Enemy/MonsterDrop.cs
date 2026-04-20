using UnityEngine;

public class MonsterDrop : MonoBehaviour
{
    [Header("드랍 프리팹")]
    public GameObject expOrbPrefab;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private float dropRadius = 2f;

    [Header("드랍 설정")]
    [SerializeField] private float itemDropChance = 0.3f; // 30%

    [Header("스테이지 범위")]
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;
    [SerializeField] private float minZ = -20f;
    [SerializeField] private float maxZ = 20f;
    [SerializeField] private float spawnY = 0.5f;
    [SerializeField] private float edgePadding = 1.0f;

    public void DropItems()
    {
        // 경험치 구슬은 확정 드랍
        if (expOrbPrefab != null)
        {
            Vector3 expPos = GetClampedDropPosition();
            Instantiate(expOrbPrefab, expPos, Quaternion.identity);
        }

        // 아이템은 확률 드랍
        if (itemPrefab != null && Random.value <= itemDropChance)
        {
            Vector3 itemPos = GetClampedDropPosition();
            Instantiate(itemPrefab, itemPos, Quaternion.identity);
        }
    }

    public void Setup(GameObject expPrefab, GameObject item, float chance)
    {
        expOrbPrefab = expPrefab;
        itemPrefab = item;
        itemDropChance = chance;
    }

    Vector3 GetClampedDropPosition()
    {
        Vector3 pos = transform.position + GetRandomOffset();

        pos.x = Mathf.Clamp(pos.x, minX + edgePadding, maxX - edgePadding);
        pos.z = Mathf.Clamp(pos.z, minZ + edgePadding, maxZ - edgePadding);
        pos.y = spawnY;

        return pos;
    }

    Vector3 GetRandomOffset()
    {
        return new Vector3(
            Random.Range(-dropRadius, dropRadius),
            0f,
            Random.Range(-dropRadius, dropRadius)
        );
    }
}