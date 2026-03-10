using UnityEngine;

/// <summary>
/// Simple player movement (WASD / arrows).
/// Needed for manual testing; not used in autotests.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float MoveForce = 10f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(h, 0f, v) * MoveForce;
        _rb.AddForce(movement, ForceMode.Force);
    }
}