using UnityEngine;
using System; // for event

/// <summary>
/// The main player component.
/// Manages health and handles taking damage.
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Health Settings")]
    public int MaxHealth = 100;
    public int Health;

    /// <summary>
    /// Event triggered when taking damage.
    /// Parameters: current health, amount of damage taken.
    /// </summary>
    public event Action<int, int> OnDamageTaken;

    /// <summary>
    /// Player death event.
    /// </summary>
    public event Action OnDeath;

    /// <summary>
    /// Flag: is the player alive.
    /// </summary>
    public bool IsAlive => Health > 0;

    private void Awake()
    {
        Health = MaxHealth;
    }

    /// <summary>
    /// Deals damage to the player.
    /// </summary>
    /// <param name="amount">Amount of damage (positive number).</param>
    public void TakeDamage(int amount)
    {
        if (!IsAlive)
            return;

        if (amount < 0)
        {
            Debug.LogWarning($"[Player] TakeDamage received a negative value: {amount}. Ignoring.");
            return;
        }

        Health -= amount;
        Health = Mathf.Max(Health, 0); // don't go below zero

        Debug.Log($"[Player] Damage taken: {amount}. Current HP: {Health}/{MaxHealth}");

        OnDamageTaken?.Invoke(Health, amount);

        if (Health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Restores health.
    /// </summary>
    /// <param name="amount">Amount of health points to restore.</param>
    public void Heal(int amount)
    {
        if (!IsAlive)
            return;

        Health = Mathf.Min(Health + amount, MaxHealth);
        Debug.Log($"[Player] Restored {amount} HP. Current HP: {Health}/{MaxHealth}");
    }

    private void Die()
    {
        Debug.Log("[Player] Player died!");
        OnDeath?.Invoke();
    }
}