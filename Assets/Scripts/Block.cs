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
    Renderer blockRenderer;
    Color originColor;
    void Awake() 
    {
        col = GetComponent<Collider>();
        blockRenderer = GetComponent<Renderer>();
        originColor = blockRenderer.material.color;
    }

    public void Init(int number)
    {
        Number = number;
        label.text = number.ToString();
    }

    public void SetPhysical(bool on) { col.enabled = on; }   // off = gaze ray passes through

    public void SetHighlight(bool on)
    {
        blockRenderer.material.color = on ? Color.yellow : originColor;
    }

  //  void Update()
   // {
    //    blockRenderer.material.color = IsWaiting && !Locked && !Returning ? Color.yellow : originColor;        If we want to have first block already seleected and highlighted
   // }
}