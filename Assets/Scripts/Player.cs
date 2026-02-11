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
    [SerializeField] private bool hasDash = false;      // locked until mystery box
    [SerializeField] private float dashDistance = 5f;   // how far dash should go
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.75f;

    private bool isDashing = false;
    private float nextDashTime = 0f;

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
    }

    public void UnlockDash()
    {
        hasDash = true;
    }

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
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            isJumping = true;
            if (body != null)
                body.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }

        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");
        braking = Input.GetKey(KeyCode.B);

        if (hasDash && Input.GetKeyDown(KeyCode.LeftShift))
        {
            TryDash();
        }

        if (transform.position.y < voidHeight)
        {
            Respawn();
        }
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
            right = Vector3.ProjectOnPlane(camTf.right, Vector3.up).normalized;
        }
        else
        {
            forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
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

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (camTf != null)
        {
            forward = Vector3.ProjectOnPlane(camTf.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(camTf.right, Vector3.up).normalized;
        }

        Vector3 inputDir = (forward * zInput) + (right * xInput);

        if (braking)
        {
            Vector3 v0 = body.linearVelocity;
            Vector3 lateral0 = new Vector3(v0.x, 0f, v0.z);
            lateral0 *= (1f - brakeStrength);
            body.linearVelocity = new Vector3(lateral0.x, v0.y, lateral0.z);
        }

        body.AddForce(inputDir * moveForce, ForceMode.Force);

        Vector3 v = body.linearVelocity;
        Vector3 lateral = new Vector3(v.x, 0f, v.z);
        lateral = Vector3.ClampMagnitude(lateral, maxSpeed);
        body.linearVelocity = new Vector3(lateral.x, v.y, lateral.z);
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
        {
            delta += transform.position - list[i].point;
        }
        delta /= col.contactCount;

        if (Mathf.Abs(delta.y) > 0.25f)
            isJumping = false;
    }
}
