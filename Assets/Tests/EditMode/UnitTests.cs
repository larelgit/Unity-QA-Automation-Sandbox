using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode (Unit) тесты — проверяют чистую логику без физики.
/// Запускаются мгновенно, не входя в Play Mode.
/// </summary>
public class UnitTests
{
    private Player CreatePlayerSimple()
    {
        GameObject obj = new GameObject("TestPlayer");
        Player player = obj.AddComponent<Player>();
        // Вручную вызываем Awake-инициализацию
        // (В EditMode Awake не вызывается автоматически)
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        return player;
    }

    [Test]
    public void Player_StartsWithFullHealth()
    {
        Player player = CreatePlayerSimple();

        Assert.AreEqual(100, player.Health);
        Assert.IsTrue(player.IsAlive);

        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void TakeDamage_ReducesHealth()
    {
        Player player = CreatePlayerSimple();

        player.TakeDamage(25);

        Assert.AreEqual(75, player.Health);

        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void TakeDamage_NegativeValue_IsIgnored()
    {
        Player player = CreatePlayerSimple();

        player.TakeDamage(-10);

        Assert.AreEqual(100, player.Health,
            "Отрицательный урон не должен изменять здоровье");

        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void TakeDamage_HealthNeverGoesBelowZero()
    {
        Player player = CreatePlayerSimple();

        player.TakeDamage(999);

        Assert.AreEqual(0, player.Health,
            "HP не может быть отрицательным");

        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void Player_DiesWhenHealthReachesZero()
    {
        Player player = CreatePlayerSimple();
        bool died = false;
        player.OnDeath += () => died = true;

        player.TakeDamage(100);

        Assert.IsFalse(player.IsAlive);
        Assert.IsTrue(died, "Событие OnDeath должно сработать");

        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void Heal_RestoresHealth()
    {
        Player player = CreatePlayerSimple();
        player.TakeDamage(50);

        player.Heal(30);

        Assert.AreEqual(80, player.Health);

        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void Heal_CannotExceedMaxHealth()
    {
        Player player = CreatePlayerSimple();
        player.TakeDamage(10);

        player.Heal(999);

        Assert.AreEqual(100, player.Health);

        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void DeadPlayer_CannotTakeDamage()
    {
        Player player = CreatePlayerSimple();
        player.TakeDamage(100); // убиваем

        player.TakeDamage(50); // пытаемся добить

        Assert.AreEqual(0, player.Health,
            "Мёртвый игрок не должен получать дополнительный урон");

        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void DeadPlayer_CannotHeal()
    {
        Player player = CreatePlayerSimple();
        player.TakeDamage(100);

        player.Heal(50);

        Assert.AreEqual(0, player.Health,
            "Мёртвый игрок не должен исцеляться");

        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void OnDamageTaken_EventFires_WithCorrectValues()
    {
        Player player = CreatePlayerSimple();
        int receivedHealth = -1;
        int receivedDamage = -1;

        player.OnDamageTaken += (health, damage) =>
        {
            receivedHealth = health;
            receivedDamage = damage;
        };

        player.TakeDamage(35);

        Assert.AreEqual(65, receivedHealth,
            "Событие должно передавать текущее здоровье");
        Assert.AreEqual(35, receivedDamage,
            "Событие должно передавать количество урона");

        Object.DestroyImmediate(player.gameObject);
    }
}