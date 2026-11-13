using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class araç_gidişi : MonoBehaviour
{
    [SerializeField] float motorSpeed = 1000f;
    [SerializeField, Min(0f)] float minimumMotorSpeed = 200f;
    [SerializeField, Min(0f)] float maximumMotorSpeed = 2000f;
    [SerializeField, Min(0f)] float maxMotorTorque = 10000f;
    [SerializeField, Min(0f)] float brakeMotorTorque = 15000f;
    [SerializeField, Min(0f)] float bodyRotationTorque = 5f;
    [SerializeField, Min(0f)] float jumpForce = 8f;
    [SerializeField, Range(0.1f, 1f)] float secondJumpForceMultiplier = 0.85f;
    [SerializeField] LayerMask groundLayers = ~0;
    [SerializeField] Camera followCamera;
    [SerializeField, Min(0.01f)] float cameraSmoothTime = 0.25f;

    Rigidbody2D body;
    WheelJoint2D[] wheelJoints;
    Rigidbody2D[] wheelBodies;
    Transform cameraTransform;
    Vector3 cameraOffset;
    Vector3 cameraVelocity;
    int jumpsRemaining = 2;
    bool jumpRequested;
    bool movementEnabled = true;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        wheelJoints = GetComponents<WheelJoint2D>();
        wheelBodies = new Rigidbody2D[wheelJoints.Length];

        if (wheelJoints.Length == 0)
            Debug.LogError("Araç gövdesinde WheelJoint2D bulunamadı.", this);

        for (int i = 0; i < wheelJoints.Length; i++)
        {
            wheelJoints[i].enableCollision = false;
            wheelBodies[i] = wheelJoints[i].connectedBody;
            if (wheelBodies[i] == null)
            {
                Debug.LogError("Her WheelJoint2D için tekerleğin Rigidbody2D bileşenini Connected Body alanına atayın.", wheelJoints[i]);
                continue;
            }

            wheelBodies[i].bodyType = RigidbodyType2D.Dynamic;
            wheelBodies[i].interpolation = RigidbodyInterpolation2D.Interpolate;
            wheelBodies[i].WakeUp();
        }

        IgnoreWheelBodyCollisions();
    }

    void IgnoreWheelBodyCollisions()
    {
        HashSet<Rigidbody2D> wheelBodySet = new HashSet<Rigidbody2D>();
        foreach (Rigidbody2D wheelBody in wheelBodies)
        {
            if (wheelBody != null)
                wheelBodySet.Add(wheelBody);
        }

        Collider2D[] vehicleColliders = GetComponentsInChildren<Collider2D>(true);
        foreach (Rigidbody2D wheelBody in wheelBodySet)
        {
            Collider2D[] wheelColliders = wheelBody.GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D vehicleCollider in vehicleColliders)
            {
                if (wheelBodySet.Contains(vehicleCollider.attachedRigidbody))
                    continue;

                foreach (Collider2D wheelCollider in wheelColliders)
                {
                    if (vehicleCollider != wheelCollider)
                        Physics2D.IgnoreCollision(vehicleCollider, wheelCollider, true);
                }
            }
        }
    }

    void Start()
    {
        if (followCamera == null)
            followCamera = Camera.main;

        if (followCamera == null)
        {
            Debug.LogWarning("Aracı takip etmesi için bir kamera atayın veya MainCamera etiketli kamera ekleyin.", this);
            return;
        }

        cameraTransform = followCamera.transform;
        cameraOffset = cameraTransform.position - transform.position;
    }

    void FixedUpdate()
    {
        if (!movementEnabled)
        {
            foreach (WheelJoint2D wheelJoint in wheelJoints)
                wheelJoint.useMotor = false;

            return;
        }

        if (IsGrounded() && body.linearVelocity.y <= 0.1f)
            jumpsRemaining = 2;

        if (jumpRequested)
        {
            jumpRequested = false;
            TryJump();
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            for (int i = 0; i < wheelJoints.Length; i++)
            {
                if (wheelBodies[i] == null)
                    continue;

                JointMotor2D brakeMotor = wheelJoints[i].motor;
                brakeMotor.motorSpeed = 0f;
                brakeMotor.maxMotorTorque = brakeMotorTorque;
                wheelJoints[i].motor = brakeMotor;
                wheelJoints[i].useMotor = brakeMotorTorque > 0f;
                wheelBodies[i].WakeUp();
            }

            return;
        }

        float input = GetHorizontalInput();
        if (Mathf.Approximately(input, 0f))
        {
            foreach (WheelJoint2D wheelJoint in wheelJoints)
                wheelJoint.useMotor = false;

            return;
        }

        for (int i = 0; i < wheelJoints.Length; i++)
        {
            WheelJoint2D wheelJoint = wheelJoints[i];
            if (wheelBodies[i] == null)
                continue;

            JointMotor2D motor = wheelJoint.motor;
            motor.motorSpeed = -input * motorSpeed;
            motor.maxMotorTorque = maxMotorTorque;
            wheelJoint.motor = motor;
            wheelJoint.useMotor = true;
            wheelBodies[i].WakeUp();
        }

        body.AddTorque(-input * bodyRotationTorque, ForceMode2D.Force);
    }

    void Update()
    {
        if (movementEnabled && Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame)
            jumpRequested = true;
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 targetPosition = transform.position + cameraOffset;
        cameraTransform.position = Vector3.SmoothDamp(
            cameraTransform.position,
            targetPosition,
            ref cameraVelocity,
            cameraSmoothTime);
    }

    float GetHorizontalInput()
    {
        if (Keyboard.current == null)
            return 0f;

        float input = 0f;
        if (Keyboard.current.aKey.isPressed)
            input -= 1f;
        if (Keyboard.current.dKey.isPressed)
            input += 1f;

        return input;
    }

    bool IsGrounded()
    {
        foreach (Rigidbody2D wheelBody in wheelBodies)
        {
            if (wheelBody != null && wheelBody.IsTouchingLayers(groundLayers))
                return true;
        }

        return false;
    }

    void TryJump()
    {
        if (jumpsRemaining <= 0)
            return;

        float force = jumpsRemaining == 2 ? jumpForce : jumpForce * secondJumpForceMultiplier;
        Vector2 velocity = body.linearVelocity;
        velocity.y = 0f;
        body.linearVelocity = velocity;
        body.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        jumpsRemaining--;
    }

    public void AdjustMotorSpeed(float amount)
    {
        float minimum = Mathf.Min(minimumMotorSpeed, maximumMotorSpeed);
        float maximum = Mathf.Max(minimumMotorSpeed, maximumMotorSpeed);
        motorSpeed = Mathf.Clamp(motorSpeed + amount, minimum, maximum);
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (enabled)
            return;

        jumpRequested = false;
        foreach (WheelJoint2D wheelJoint in wheelJoints)
            wheelJoint.useMotor = false;
    }
}
