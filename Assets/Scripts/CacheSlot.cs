using System.Collections;
using TMPro;
using UnityEngine;

public class CacheSlot : MonoBehaviour
{
    public int SetIndex;                      // set in the Inspector
    public int WayIndex;
    [SerializeField] Renderer tray;           // the flat Tray cube
    [SerializeField] TMP_Text tagLabel;       // optional: small text under the slot

    public Block Occupant { get; private set; }
    public int ArrivalTick { get; private set; }    // when the block was placed (used by FIFO)
    public int LastUsedTick { get; private set; }   // placed or hit most recently (used by LRU)

    Color idleColor, restColor;
    Coroutine flashing;

    void Awake()
    {
        idleColor = restColor = tray.material.color;
        SetTag("");
    }

    public void Place(Block b, int tick)
    {
        Occupant = b;
        ArrivalTick = LastUsedTick = tick;

        b.transform.SetParent(transform);

        StartCoroutine(Snap(b));
    }

    public void Touch(int tick) { LastUsedTick = tick; }   // cache hit on this slot

    public void Evict()
    {
        if (Occupant != null) Destroy(Occupant.gameObject, 0.4f);   // add a pop or fade here later
        Occupant = null;
    }

    public void SetTag(string text)
    {
        if (tagLabel != null) tagLabel.text = text;
    }

    // Keeps the tray colored until ClearHighlight (used while the player chooses what to evict)
    public void Highlight(Color c) { restColor = c; tray.material.color = c; }
    public void ClearHighlight() { restColor = idleColor; tray.material.color = idleColor; }

    public void Flash(Color c)
    {
        if (flashing != null) StopCoroutine(flashing);
        flashing = StartCoroutine(FlashRoutine(c));
    }

    IEnumerator FlashRoutine(Color c)
    {
        tray.material.color = c;
        yield return new WaitForSeconds(0.8f);
        tray.material.color = restColor;
    }

    IEnumerator Snap(Block b)
    {
        Vector3 p0 = b.transform.position;
        Quaternion r0 = b.transform.rotation;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
        {
            if (b == null) yield break;
            b.transform.SetPositionAndRotation(Vector3.Lerp(p0, transform.position, t),
                                               Quaternion.Slerp(r0, transform.rotation, t));
            yield return null;
        }
        if (b != null) b.transform.SetPositionAndRotation(transform.position, transform.rotation);
    }
}
