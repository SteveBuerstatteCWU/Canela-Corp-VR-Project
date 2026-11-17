using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] Block blockPrefab;
    [SerializeField] Transform spawnPoint, pickupPoint;
    [SerializeField] Renderer beltTop;               // for the scrolling texture
    [SerializeField] float beltSpeed = 0.6f;         // metres per second
    [SerializeField] float spacing = 0.6f;           // gap between queued blocks
    [SerializeField] float metersPerTexture = 0.5f;  // belt length / Tiling X
    [SerializeField] int maxQueued = 3;
    [SerializeField] int maxBlockNumber = 16;
    [SerializeField] int[] lessonSequence = { 4, 7, 0, 1, 8, 7 };

    readonly List<Block> queue = new List<Block>();  // index 0 = front of the line
    Material beltMat;
    int spawned;

    // The block waiting at the pickup point, or null
    public Block Front =>
        queue.Count > 0 && queue[0].IsWaiting && !queue[0].Locked ? queue[0] : null;

    void Awake() { beltMat = beltTop.material; }

    void Update()
    {
        // 1. Keep the line topped up
        if (queue.Count < maxQueued && SpawnPointIsFree()) Spawn();

        // 2. Scroll the belt texture (flip the sign if it slides the wrong way)
        beltMat.mainTextureOffset -= new Vector2(beltSpeed * Time.deltaTime / metersPerTexture, 0f);

        // 3. Move every block toward its place in line
        for (int i = 0; i < queue.Count; i++)
        {
            Block b = queue[i];
            Vector3 target = pickupPoint.position + Vector3.left * (spacing * i);
            float speed = b.Returning ? beltSpeed * 5f : beltSpeed;
            b.transform.position = Vector3.MoveTowards(b.transform.position, target, speed * Time.deltaTime);
            bool arrived = b.transform.position == target;
            if (arrived) b.Returning = false;
            b.IsWaiting = arrived && i == 0;
        }
    }

    bool SpawnPointIsFree() =>
        queue.Count == 0 ||
        Vector3.Distance(queue[queue.Count - 1].transform.position, spawnPoint.position) >= spacing;

    void Spawn()
    {
        Block b = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        b.Init(NextNumber());
        queue.Add(b);
    }

    int NextNumber() =>
        spawned < lessonSequence.Length ? lessonSequence[spawned++] : Random.Range(0, maxBlockNumber);

    // Player grabbed the front block
    public Block TakeFront()
    {
        Block b = queue[0];
        queue.RemoveAt(0);
        b.IsWaiting = false;
        return b;
    }

    // Wrong placement: the block goes back to the front of the line
    public void ReturnToFront(Block b)
    {
        b.SetPhysical(true);
        b.Returning = true;
        queue.Insert(0, b);
    }

    // Cache hit: the block rides off the end of the belt
    public void Discard(Block b)
    {
        queue.Remove(b);
        StartCoroutine(SlideOff(b));
    }

    IEnumerator SlideOff(Block b)
    {
        float end = Time.time + 3f;
        while (b != null && Time.time < end)
        {
            b.transform.position += Vector3.right * beltSpeed * 2f * Time.deltaTime;
            yield return null;
        }
        if (b != null) Destroy(b.gameObject);
    }
}