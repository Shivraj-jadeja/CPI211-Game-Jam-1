using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    private Rigidbody body;

    [SerializeField] private float maxSpeed = .5f;
    public float jumpPower = 1;
    [SerializeField] private float moveForce = 15f;
    public float brakeStrength = 0.85f;

    private Transform camTf;
    public float voidHeight = -20f;

    [Header("Dash")]
    [SerializeField] private bool hasDash = false;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.75f;

    private bool isDashing = false;
    private float nextDashTime = 0f;

    [Header("Ground Stick (Fix slope micro-jumps)")]
    [SerializeField] private float groundCheckDistance = 0.75f;
    [SerializeField] private float groundStickForce = 35f;     // 20-45 usually good
    [SerializeField] private float microHopUpVelCap = 0.20f;   // only caps tiny hops, not real jumps
    [SerializeField] private float stickDisableAfterJump = 0.12f;

    private float lastJumpTime = -999f;

    public Vector3 facing;
    public Vector3 perpendicular;

    public AudioSource audioPlayer;
    public AudioClip soundDash;

    [SerializeField] private bool isJumping = false;

    private float xInput;
    private float zInput;
    private bool braking;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        if (body == null) Debug.LogError("Player is missing a Rigidbody!");

        camTf = GetComponentInChildren<UnityEngine.Camera>()?.transform;
        if (camTf == null && UnityEngine.Camera.main != null)
            camTf = UnityEngine.Camera.main.transform;

        facing = transform.forward;
        perpendicular = GetPerpendicular(facing);

        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.SetCheckpoint(transform.position);

        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void UnlockDash() { hasDash = true; }

    public void Respawn()
    {
        if (body == null) return;

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        if (CheckpointManager.Instance != null)
            transform.position = CheckpointManager.Instance.GetCheckpoint();
    }

    void Update()
    {
        // Jump
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            isJumping = true;
            lastJumpTime = Time.time;
            body.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }

        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");
        braking = Input.GetKey(KeyCode.B);

        if (hasDash && Input.GetKeyDown(KeyCode.LeftShift))
            TryDash();

        if (transform.position.y < voidHeight)
            Respawn();
    }

    private void TryDash()
    {
        if (isDashing) return;
        if (Time.time < nextDashTime) return;
        if (body == null) return;

        if (audioPlayer != null && soundDash != null){
            audioPlayer.PlayOneShot(soundDash);
        }
        
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (camTf != null)
        {
            forward = Vector3.ProjectOnPlane(camTf.forward, Vector3.up).normalized;
            right   = Vector3.ProjectOnPlane(camTf.right, Vector3.up).normalized;
        }
        else
        {
            forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            right   = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        }

        Vector3 inputDir = (forward * zInput) + (right * xInput);
        Vector3 dir = (inputDir.sqrMagnitude > 0.001f) ? inputDir.normalized : forward;

        StartCoroutine(DashRoutine(dir));
    }

    private IEnumerator DashRoutine(Vector3 dir)
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        float dashSpeed = dashDistance / Mathf.Max(0.01f, dashDuration);
        float originalY = body.linearVelocity.y;

        float t = 0f;
        while (t < dashDuration)
        {
            Vector3 v = dir * dashSpeed;
            body.linearVelocity = new Vector3(v.x, originalY, v.z);

            t += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        if (body == null) return;

        // Camera-relative directions on XZ plane
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (camTf != null)
        {
            forward = Vector3.ProjectOnPlane(camTf.forward, Vector3.up).normalized;
            right   = Vector3.ProjectOnPlane(camTf.right, Vector3.up).normalized;
        }

        Vector3 inputDir = (forward * zInput) + (right * xInput);

        // Grounded check
        bool grounded = false;
        Ray ray = new Ray(transform.position + Vector3.up * 0.15f, Vector3.down);
        if (Physics.Raycast(ray, groundCheckDistance))
            grounded = true;

        // Only apply stick if grounded AND not within the "just jumped" window
        bool stickAllowed = grounded && (Time.time - lastJumpTime) > stickDisableAfterJump;

        if (stickAllowed && inputDir.sqrMagnitude > 0.001f)
        {
            body.AddForce(Vector3.down * groundStickForce, ForceMode.Acceleration);

            // Cap ONLY tiny upward micro-hops (never affects real jumps because it's disabled right after jump)
            Vector3 v = body.linearVelocity;
            if (!isJumping && v.y > microHopUpVelCap)
                body.linearVelocity = new Vector3(v.x, microHopUpVelCap, v.z);
        }

        if (braking)
        {
            Vector3 v0 = body.linearVelocity;
            Vector3 lateral0 = new Vector3(v0.x, 0f, v0.z);
            lateral0 *= (1f - brakeStrength);
            body.linearVelocity = new Vector3(lateral0.x, v0.y, lateral0.z);
        }

        body.AddForce(inputDir * moveForce, ForceMode.Force);

        Vector3 v2 = body.linearVelocity;
        Vector3 lateral = new Vector3(v2.x, 0f, v2.z);
        lateral = Vector3.ClampMagnitude(lateral, maxSpeed);
        body.linearVelocity = new Vector3(lateral.x, v2.y, lateral.z);
    }

    private Vector3 GetPerpendicular(Vector3 inVec)
    {
        return new Vector3(inVec.z, 0, -inVec.x);
    }

    public void OnCollisionEnter(Collision col)
    {
        Vector3 delta = Vector3.zero;
        List<ContactPoint> list = new List<ContactPoint>();
        col.GetContacts(list);

        for (int i = 0; i < col.contactCount; i++)
            delta += transform.position - list[i].point;

        delta /= col.contactCount;

        if (Mathf.Abs(delta.y) > 0.25f)
            isJumping = false;
    }
}
