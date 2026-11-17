using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    public int Number { get; private set; }
    public bool IsWaiting { get; set; }   // stopped at the pickup point
    public bool Locked { get; set; }      // true while the game is busy with it (cache hit)
    public bool Returning { get; set; }   // moving back to the belt after a mistake

    Collider col;
    void Awake() { col = GetComponent<Collider>(); }

    public void Init(int number)
    {
        Number = number;
        label.text = number.ToString();
    }

    public void SetPhysical(bool on) { col.enabled = on; }   // off = gaze ray passes through
}