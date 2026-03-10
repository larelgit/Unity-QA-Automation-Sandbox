using UnityEngine;

/// <summary>
/// Dangerous zone (lava). Deals damage to the player on physical contact.
/// </summary>
public class Lava : MonoBehaviour
{
    [Header("Lava Settings")]
    public int DamageAmount = 20;

    [Tooltip("Can lava deal damage multiple times on repeated touch?")]
    public bool CanDamageMultipleTimes = true;

    private bool _hasDamaged = false;

    /// <summary>
    /// Called by Unity upon physical collision.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // If we already dealt damage and don't want to repeat
        if (!CanDamageMultipleTimes && _hasDamaged)
            return;

        // Try to get the Player component from the object that touched us
        Player player = collision.gameObject.GetComponent<Player>();

        if (player != null)
        {
            Debug.Log($"[Lava] Player touched the lava! Dealing {DamageAmount} damage.");
            player.TakeDamage(DamageAmount);
            _hasDamaged = true;
        }
    }

    /// <summary>
    /// Resets the state (useful for tests).
    /// </summary>
    public void ResetState()
    {
        _hasDamaged = false;
    }
}