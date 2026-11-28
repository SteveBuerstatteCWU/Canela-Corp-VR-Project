using UnityEngine;

public class GazeInteractor : MonoBehaviour
{
    [SerializeField] Transform head;            // the Main Camera
    [SerializeField] BlockSpawner spawner;
    [SerializeField] GameManager game;
    [SerializeField] float maxDistance = 15f;

    Block held;

    static bool Tapped()
    {
        #if UNITY_EDITOR
                return Input.GetMouseButtonDown(0);     // left click = Cardboard button
        #else
                return Google.XR.Cardboard.Api.IsTriggerPressed;
        #endif
    }

    void Update()
    {
        Physics.Raycast(head.position, head.forward, out RaycastHit hit, maxDistance);
        bool tap = Tapped();

        if (held == null)
        {
            Block b = hit.collider ? hit.collider.GetComponentInParent<Block>() : null;

            if (tap && b != null && b == spawner.Front)
            {
                held = spawner.TakeFront();
                held.SetPhysical(false);         // gaze ray passes through it
                held.SetHighlight(true);         // highlight while holding
            }
        }
        else
        {
            Vector3 hover = head.position + head.forward * 1.2f + Vector3.down * 0.35f;
            held.transform.position = Vector3.Lerp(
                held.transform.position, hover, 12f * Time.deltaTime);

            CacheSlot slot = hit.collider
                ? hit.collider.GetComponentInParent<CacheSlot>()
                : null;

            if (tap && slot != null)
            {
                // true  = the block left the player's hands (placed, or sent back to the belt)
                // false = keep holding it (the player still has to choose a block to evict)
                if (game.TryPlace(held, slot))
                {
                    held.SetHighlight(false);    // remove highlight once it is placed
                    held = null;
                }
            }
        }
    }
}
