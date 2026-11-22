using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum ReplacementPolicy { FIFO, LRU }

public class GameManager : MonoBehaviour
{
    [SerializeField] int setCount = 4;           // power of two; must match the number of racks
    [SerializeField] int maxStrikes = 3;
    [SerializeField] ReplacementPolicy policy = ReplacementPolicy.FIFO;
    [SerializeField] bool strikeOnBadEviction = false;   // tick for a harder game
    [SerializeField] BlockSpawner spawner;
    [SerializeField] CacheSlot[] allSlots;       // drag all 8 slots in
    [SerializeField] TMP_Text monitorText, scoreText, strikesText, answerText;
    [SerializeField] WrongSlotSound wrongSlotSound;      // optional (Eli's sound); leave empty if unused

    static readonly Color Orange = new Color(1f, 0.6f, 0.2f);

    Block current;
    int score, strikes;
    int tick;                                    // goes up by 1 on every placement and every cache hit
    bool gameOver;
    int pendingSet = -1;                         // -1 = normal play; otherwise the full set the player must choose a victim from
    bool AwaitingEviction => pendingSet >= 0;

    void Start()
    {
        monitorText.text = $"Set Index = Block % {setCount}";

        answerText.gameObject.SetActive(false);

        UpdateHud();
    }

    IEnumerable<CacheSlot> SlotsInSet(int setIndex) =>
        allSlots.Where(s => s.SetIndex == setIndex);

    void Update()
    {
        if (gameOver || AwaitingEviction) return;    // pause belt logic while the player is choosing

        Block front = spawner.Front;

        if (front == current) return;

        current = front;

        if (current == null) return;

        // Show the block's set index and binary explanation.
        // Do not hide answerText here, because a wrong block is returned to the front.

        int n = current.Number;
        int setIndex = n % setCount;

        string bits = System.Convert.ToString(n, 2).PadLeft(4, '0');
        int idx = Mathf.RoundToInt(Mathf.Log(setCount, 2));

        string hi = bits.Substring(0, bits.Length - idx);
        string lo = bits.Substring(bits.Length - idx);

        monitorText.text = $"Set Index = Block % {setCount}\n\n";

        if (SlotsInSet(setIndex).Any(s =>
            s.Occupant != null && s.Occupant.Number == n))
        {
            StartCoroutine(CacheHit(current, setIndex));
        }
    }

    IEnumerator CacheHit(Block b, int setIndex)
    {
        b.Locked = true;

        monitorText.text += "\nCACHE HIT! Already in the cache.";

        foreach (var s in SlotsInSet(setIndex))
        {
            if (s.Occupant != null && s.Occupant.Number == b.Number)
            {
                s.Flash(Color.green);
                s.Touch(++tick);                     // a hit counts as a "use" (matters for LRU)
            }
        }

        RefreshTags();

        score += 5;
        UpdateHud();

        yield return new WaitForSeconds(1.5f);

        spawner.Discard(b);
    }

    // Called by GazeInteractor when the player taps a slot while holding a block.
    // Returns true when the block has left the player's hands, false if they keep holding it.
    public bool TryPlace(Block block, CacheSlot tapped)
    {
        // Hide the previous explanation when another placement is attempted.
        answerText.gameObject.SetActive(false);

        // Second tap of a replacement: the player is choosing which block to evict.
        if (AwaitingEviction)
        {
            if (tapped.SetIndex != pendingSet || tapped.Occupant == null)
            {
                ShowAnswer($"\nCache {pendingSet} is full!\nTap one of ITS blocks to evict.\n{PolicyHint()}");
                return false;                        // nothing changes, keep holding the block
            }

            ResolveEviction(block, tapped);
            return true;
        }

        int correct = block.Number % setCount;

        if (tapped.SetIndex != correct)              // wrong set: strike and explain
        {
            answerText.gameObject.SetActive(true);

            tapped.Flash(Color.red);

            if (wrongSlotSound != null) wrongSlotSound.PlayWrongSlotSound();

            strikes++;

            string bits = System.Convert.ToString(block.Number, 2).PadLeft(4, '0');
            int idx = Mathf.RoundToInt(Mathf.Log(setCount, 2));

            string hi = bits.Substring(0, bits.Length - idx);
            string lo = bits.Substring(bits.Length - idx);

            answerText.text =
                $"\nWrong!\nBlock {block.Number}\n" +
                $"{block.Number} % {setCount} = {correct}\n" +
                $"It belongs in Cache {correct}.";

            spawner.ReturnToFront(block);

            UpdateHud();

            if (strikes >= maxStrikes)
                EndGame();

            return true;
        }

        CacheSlot free = SlotsInSet(correct).FirstOrDefault(s => s.Occupant == null);

        if (free != null)                            // room in the set: simple placement
        {
            PlaceInto(free, block);
            score += 10;
            UpdateHud();
            return true;
        }

        // First tap of a replacement: the set is full, so the player must pick a victim.
        pendingSet = correct;
        foreach (var s in SlotsInSet(correct)) s.Highlight(Orange);
        ShowAnswer($"\nCache {correct} is full!\nTap the block you want to evict.\n{PolicyHint()}");
        return false;                                // the player keeps holding the block
    }

    void ResolveEviction(Block block, CacheSlot chosen)
    {
        // The block the policy would evict: lowest arrival tick (FIFO) or lowest last-used tick (LRU).
        CacheSlot best = SlotsInSet(pendingSet)
            .OrderBy(s => policy == ReplacementPolicy.FIFO ? s.ArrivalTick : s.LastUsedTick)
            .First();

        bool correct = chosen == best;
        int chosenNumber = chosen.Occupant.Number;
        int bestNumber = best.Occupant.Number;

        foreach (var s in SlotsInSet(pendingSet)) s.ClearHighlight();
        pendingSet = -1;

        chosen.Evict();                              // the player's choice is carried out either way
        PlaceInto(chosen, block);

        if (correct)
        {
            score += 10;
            ShowAnswer($"\nCorrect!\n{policy} evicts Block {chosenNumber}.");
        }
        else
        {
            score += 3;
            ShowAnswer($"\nNot quite.\n{policy} would evict Block {bestNumber}.\nYou evicted Block {chosenNumber}.");

            if (strikeOnBadEviction)
            {
                strikes++;
                if (strikes >= maxStrikes) EndGame();
            }
        }

        UpdateHud();
    }

    void PlaceInto(CacheSlot slot, Block block)
    {
        slot.Place(block, ++tick);
        slot.Flash(Color.green);
        RefreshTags();
    }

    void ShowAnswer(string text)
    {
        answerText.gameObject.SetActive(true);
        answerText.text = text;
    }

    string PolicyHint() => policy == ReplacementPolicy.FIFO
        ? "FIFO: evict the block that arrived first."
        : "LRU: evict the block used longest ago.";

    // Small labels under each slot give the player the information they need to decide.
    void RefreshTags()
    {
        // Process each cache set independently.
    foreach (var group in allSlots.GroupBy(s => s.SetIndex))
    {
        // Get occupied slots and sort them according to the replacement policy.
        var occupied = group
            .Where(s => s.Occupant != null)
            .OrderBy(s => policy == ReplacementPolicy.FIFO
                ? s.ArrivalTick
                : s.LastUsedTick)
            .ToList();

        // Assign labels starting from #1 for each cache set.
        for (int i = 0; i < occupied.Count; i++)
        {
            occupied[i].SetTag(policy == ReplacementPolicy.FIFO
                ? $"arrived #{i + 1}"
                : $"used #{i + 1}");
        }

        // Clear labels for empty slots.
        foreach (var slot in group.Where(s => s.Occupant == null))
        {
            slot.SetTag("");
        }
    }
    }

    void UpdateHud()
    {
        scoreText.text = $"Score: {score}";

        strikesText.text = string.Concat(
            Enumerable.Range(0, maxStrikes)
                .Select(i => i < strikes
                    ? "<color=red>X</color> "
                    : "<color=grey>-</color> ")
        );
    }

    void EndGame()
    {
        gameOver = true;

        spawner.enabled = false;

        monitorText.text = $"You're fired!\nFinal score: {score}";
        answerText.text = "";
    }
}
