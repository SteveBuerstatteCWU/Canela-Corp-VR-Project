using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] int setCount = 4;           // power of two; must match the number of racks
    [SerializeField] int maxStrikes = 3;
    [SerializeField] BlockSpawner spawner;
    [SerializeField] CacheSlot[] allSlots;       // drag all 8 slots in
    [SerializeField] TMP_Text monitorText, scoreText, strikesText, answerText;

    readonly Dictionary<int, Queue<CacheSlot>> fifo = new Dictionary<int, Queue<CacheSlot>>();
    Block current;
    int score, strikes;
    bool gameOver;
    

    void Start()
    {
        for (int s = 0; s < setCount; s++) fifo[s] = new Queue<CacheSlot>();
        monitorText.text = "Set Index = Block % " + setCount;
        answerText.gameObject.SetActive(false);
        UpdateHud();
    }

    IEnumerable<CacheSlot> SlotsInSet(int setIndex) => allSlots.Where(s => s.SetIndex == setIndex);

    void Update()
    {
        if (gameOver) return;
        Block front = spawner.Front;
        if (front == current) return;
        current = front;
        if (current == null) return;

        
        answerText.gameObject.SetActive(false);

        monitorText.text = "Set Index = Block % " + setCount;

        int n = current.Number;
        int setIndex = n % setCount;

        if (SlotsInSet(setIndex).Any(s => s.Occupant != null && s.Occupant.Number == n))
            StartCoroutine(CacheHit(current, setIndex));
    }

    IEnumerator CacheHit(Block b, int setIndex)
    {
        b.Locked = true;
        monitorText.text += "\nCACHE HIT! Already in the cache.";
        foreach (var s in SlotsInSet(setIndex))
            if (s.Occupant != null && s.Occupant.Number == b.Number) s.Flash(Color.green);
        score += 5;
        UpdateHud();
        yield return new WaitForSeconds(1.5f);
        spawner.Discard(b);
    }

    // Called by GazeInteractor when the player taps a slot while holding a block
    public void TryPlace(Block block, CacheSlot tapped)
    {
        int correct = block.Number % setCount;

        if (tapped.SetIndex != correct)              // wrong set: strike and explain
        {
            
            answerText.gameObject.SetActive(true);
            tapped.Flash(Color.red);
            strikes++;
            string bits = System.Convert.ToString(block.Number, 2).PadLeft(4, '0');
            int idx = Mathf.RoundToInt(Mathf.Log(setCount, 2));

            string hi = bits.Substring(0, bits.Length - idx);
            string lo = bits.Substring(bits.Length - idx);

            answerText.text =
                $"Wrong!\n\nBlock {block.Number}\n" +
                $"{block.Number} % {setCount} = {correct}\n" +
                $"Bits: {hi}<color=#5ADCFF>{lo}</color>\n\n" +
                $"It belongs in Cache {correct}.";

            spawner.ReturnToFront(block);
            UpdateHud();
            if (strikes >= maxStrikes) EndGame();
            return;
        }

        CacheSlot target = SlotsInSet(correct).FirstOrDefault(s => s.Occupant == null);
        if (target == null)                          // set is full: FIFO replacement
        {
            target = fifo[correct].Dequeue();
            monitorText.text = $"Set full! FIFO evicts Block {target.Occupant.Number}.";
            target.Evict();
        }
        target.Place(block);
        fifo[correct].Enqueue(target);
        target.Flash(Color.green);
        score += 10;
        UpdateHud();
    }

    void UpdateHud()
    {
        scoreText.text = $"Score: {score}";
        strikesText.text = string.Concat(Enumerable.Range(0, maxStrikes)
            .Select(i => i < strikes ? "<color=red>X</color> " : "<color=grey>-</color> "));
    }

    void EndGame()
    {
        gameOver = true;
        spawner.enabled = false;
        monitorText.text = $"You're fired!\nFinal score: {score}";
    }
}