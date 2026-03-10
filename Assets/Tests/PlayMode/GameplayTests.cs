using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for checking gameplay mechanics.
/// Compatible with URP (Universal Render Pipeline).
/// </summary>
public class GameplayTests
{
    // List of all created objects for guaranteed cleanup
    private List<GameObject> _spawnedObjects = new List<GameObject>();

    // =====================================================
    //  SETUP / TEARDOWN — called BEFORE and AFTER each test
    // =====================================================

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _spawnedObjects.Clear();

        // Create camera and light for EACH test
        CreateCamera();
        CreateDirectionalLight();

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // Destroy ALL objects after each test
        foreach (GameObject obj in _spawnedObjects)
        {
            if (obj != null)
                Object.Destroy(obj);
        }
        _spawnedObjects.Clear();

        // Wait a frame for Destroy to process
        yield return null;
    }

    // =====================================================
    //  HELPERS
    // =====================================================

    /// <summary>
    /// Registers an object for automatic deletion after the test.
    /// </summary>
    private T Register<T>(T component) where T : Component
    {
        _spawnedObjects.Add(component.gameObject);
        return component;
    }

    private GameObject RegisterObj(GameObject obj)
    {
        _spawnedObjects.Add(obj);
        return obj;
    }

    /// <summary>
    /// Creates a camera that looks at the polygon from above.
    /// Fixes the "Display 1 No cameras rendering" issue.
    /// </summary>
    private Camera CreateCamera()
    {
        GameObject camObj = new GameObject("TestCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // dark blue background

        camObj.transform.position = new Vector3(0, 12, -6);
        camObj.transform.rotation = Quaternion.Euler(55, 0, 0);

        // URP will automatically add UniversalAdditionalCameraData
        _spawnedObjects.Add(camObj);
        return cam;
    }

    /// <summary>
    /// Creates a directional light.
    /// Without it, everything will be black in URP.
    /// </summary>
    private Light CreateDirectionalLight()
    {
        GameObject lightObj = new GameObject("TestLight");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1.5f;
        light.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        _spawnedObjects.Add(lightObj);
        return light;
    }

    /// <summary>
    /// Creates a URP-compatible material.
    /// The standard Shader.Find("Standard") gives a pink color in URP!
    /// </summary>
    private Material CreateMaterial(Color color)
    {
        // Try URP shader
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        // Fallback to Standard (for the built-in render pipeline)
        if (shader == null)
            shader = Shader.Find("Standard");

        Material mat = new Material(shader);

        // In URP the main color is set via _BaseColor
        mat.SetColor("_BaseColor", color);
        // Fallback for Standard pipeline
        mat.SetColor("_Color", color);

        return mat;
    }

    /// <summary>
    /// Applies a color to an object.
    /// </summary>
    private void SetColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = CreateMaterial(color);
        }
    }

    /// <summary>
    /// Creates a player with physics and a Player script.
    /// </summary>
    private Player CreatePlayer(Vector3 position)
    {
        GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerObj.name = "TestPlayer";
        playerObj.transform.position = position;

        Rigidbody rb = playerObj.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.useGravity = true;

        Player player = playerObj.AddComponent<Player>();

        SetColor(playerObj, Color.blue);

        _spawnedObjects.Add(playerObj);
        return player;
    }

    /// <summary>
    /// Creates lava — a red cube that deals damage on touch.
    /// </summary>
    private Lava CreateLava(Vector3 position, Vector3 scale)
    {
        GameObject lavaObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lavaObj.name = "TestLava";
        lavaObj.transform.position = position;
        lavaObj.transform.localScale = scale;

        Lava lava = lavaObj.AddComponent<Lava>();
        lava.DamageAmount = 20;
        lava.CanDamageMultipleTimes = false;

        SetColor(lavaObj, Color.red);

        _spawnedObjects.Add(lavaObj);
        return lava;
    }

    /// <summary>
    /// Creates a wall.
    /// </summary>
    private GameObject CreateWall(Vector3 position, Vector3 scale)
    {
        GameObject wallObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallObj.name = "TestWall";
        wallObj.transform.position = position;
        wallObj.transform.localScale = scale;

        SetColor(wallObj, new Color(0.6f, 0.5f, 0.4f)); // brown

        _spawnedObjects.Add(wallObj);
        return wallObj;
    }

    /// <summary>
    /// Creates a floor.
    /// </summary>
    private GameObject CreateFloor(Vector3 position)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "TestFloor";
        floor.transform.position = position;
        floor.transform.localScale = new Vector3(3, 1, 3);

        SetColor(floor, new Color(0.3f, 0.3f, 0.3f)); // gray

        _spawnedObjects.Add(floor);
        return floor;
    }

    // =====================================================
    //  TEST 1: Check lava damage
    // =====================================================

    [UnityTest]
    public IEnumerator DamageTest_PlayerFallsOnLava_LosesHealth()
    {
        // --- ARRANGE ---
        GameObject floor = CreateFloor(Vector3.zero);

        Lava lava = CreateLava(
            position: new Vector3(0, 0.15f, 0),
            scale: new Vector3(3, 0.2f, 3)
        );

        Player player = CreatePlayer(
            position: new Vector3(0, 3f, 0)
        );

        int initialHealth = player.Health;

        // --- ACT ---
        yield return new WaitForSeconds(2f);

        // --- ASSERT ---
        Assert.AreEqual(100, initialHealth,
            "Initial health should be 100");

        Assert.AreEqual(80, player.Health,
            $"After falling into lava, HP should be 80, but it is: {player.Health}");

        Assert.IsTrue(player.IsAlive,
            "Player should be alive after 20 damage");

        Debug.Log($"[TEST PASSED] DamageTest: HP {initialHealth} -> {player.Health}");
    }

    // =====================================================
    //  TEST 2: Check wall collision
    // =====================================================

    [UnityTest]
    public IEnumerator WallCollisionTest_PlayerDoesNotPassThroughWall()
    {
        // --- ARRANGE ---
        GameObject floor = CreateFloor(Vector3.zero);

        float wallZ = 8f;
        GameObject wall = CreateWall(
            position: new Vector3(0, 1.5f, wallZ),
            scale: new Vector3(10, 3, 1)
        );

        Player player = CreatePlayer(
            position: new Vector3(0, 1f, 0)
        );

        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // --- ACT ---
        rb.AddForce(Vector3.forward * 50f, ForceMode.Impulse);

        yield return new WaitForSeconds(2f);

        // --- ASSERT ---
        float playerZ = player.transform.position.z;
        float wallFrontZ = wallZ - 0.5f;

        Assert.Less(playerZ, wallFrontZ,
            $"Player (Z={playerZ:F2}) should be IN FRONT of the wall (Z={wallFrontZ:F2}), " +
            "and not pass through it!");

        Debug.Log($"[TEST PASSED] WallCollisionTest: Player Z={playerZ:F2}, Wall front Z={wallFrontZ:F2}");
    }

    // =====================================================
    //  TEST 3: Multiple damage — death
    // =====================================================

    [UnityTest]
    public IEnumerator MultiDamageTest_PlayerDiesAfterEnoughDamage()
    {
        // --- ARRANGE ---
        GameObject floor = CreateFloor(new Vector3(20, 0, 0));

        Player player = CreatePlayer(new Vector3(20, 1, 0));
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        bool deathEventFired = false;
        player.OnDeath += () => { deathEventFired = true; };

        // --- ACT ---
        for (int i = 0; i < 5; i++)
        {
            player.TakeDamage(20);
        }

        yield return null;

        // --- ASSERT ---
        Assert.AreEqual(0, player.Health,
            "After 5 hits of 20 damage each, HP should be 0");

        Assert.IsFalse(player.IsAlive,
            "Player should be dead");

        Assert.IsTrue(deathEventFired,
            "OnDeath event should have fired");

        player.TakeDamage(50);
        Assert.AreEqual(0, player.Health,
            "HP should not go below zero after death");

        Debug.Log("[TEST PASSED] MultiDamageTest: Player dies correctly");
    }

    // =====================================================
    //  TEST 4: Gravity works
    // =====================================================

    [UnityTest]
    public IEnumerator GravityTest_PlayerFallsDown()
    {
        // --- ARRANGE ---
        Player player = CreatePlayer(new Vector3(0, 10, 15));

        float startY = player.transform.position.y;

        // --- ACT ---
        yield return new WaitForSeconds(1f);

        // --- ASSERT ---
        float endY = player.transform.position.y;

        Assert.Less(endY, startY,
            $"Player should fall! Start Y={startY:F2}, Current Y={endY:F2}");

        Debug.Log($"[TEST PASSED] GravityTest: Y {startY:F2} -> {endY:F2}");
    }

    // =====================================================
    //  TEST 5: Healing does not exceed maximum
    // =====================================================

    [UnityTest]
    public IEnumerator HealTest_HealthCannotExceedMax()
    {
        // --- ARRANGE ---
        GameObject floor = CreateFloor(new Vector3(-15, 0, 0));

        Player player = CreatePlayer(new Vector3(-15, 1, 0));
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        // --- ACT ---
        player.TakeDamage(30);
        player.Heal(50);

        yield return null;

        // --- ASSERT ---
        Assert.AreEqual(100, player.Health,
            $"HP should not exceed MaxHealth. Current: {player.Health}");

        Debug.Log("[TEST PASSED] HealTest: Health capped at MaxHealth");
    }
}