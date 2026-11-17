using System.Collections;
using UnityEngine;

public class CacheSlot : MonoBehaviour
{
    public int SetIndex;                  // set in the Inspector
    public int WayIndex;
    [SerializeField] Renderer tray;       // the flat Tray cube
    public Block Occupant { get; private set; }

    Color idleColor;
    Coroutine flashing;

    void Awake() { idleColor = tray.material.color; }

    public void Place(Block b)
    {
        Occupant = b;
        StartCoroutine(Snap(b));
    }

    public void Evict()
    {
        if (Occupant != null) Destroy(Occupant.gameObject, 0.4f);   // add a pop or fade here later
        Occupant = null;
    }

    public void Flash(Color c)
    {
        if (flashing != null) StopCoroutine(flashing);
        flashing = StartCoroutine(FlashRoutine(c));
    }

    IEnumerator FlashRoutine(Color c)
    {
        tray.material.color = c;
        yield return new WaitForSeconds(0.8f);
        tray.material.color = idleColor;
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