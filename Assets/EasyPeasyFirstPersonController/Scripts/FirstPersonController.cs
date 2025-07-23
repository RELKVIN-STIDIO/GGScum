namespace EasyPeasyFirstPersonController
{
    using System;
    using System.Collections;
    using UnityEngine;

    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Look Settings")]
        [Range(0, 100)] public float mouseSensitivity = 25f;
        [Range(0f, 200f)] private float snappiness = 100f;
        [Range(70f, 120f)] public float normalFov = 90f;
        [Range(70f, 120f)] public float sprintFov = 100f;
        public float fovChangeSpeed = 5f;

        [Header("Movement Settings")]
        [Range(0f, 20f)] public float walkSpeed = 10f;
        [Range(0f, 30f)] public float sprintSpeed = 15f;
        [Range(0f, 10f)] public float crouchSpeed = 6f;
        [Range(0f, 15f)] public float jumpSpeed = 3f;
        [Range(0f, 50f)] public float gravity = 9.81f;
        public float acceleration = 10f;
        public float deceleration = 15f;

        [Header("Crouch & Slide Settings")]
        public float crouchHeight = 1f;
        public float crouchCameraHeight = 0.5f;
        public float crouchTransitionSpeed = 10f;
        public float slideSpeed = 9f;
        public float slideDuration = 0.7f;
        public float slideFovBoost = 5f;
        public float slideTiltAngle = 5f;
        public float slideCooldown = 0.5f;
        private float lastSlideTime;

        [Header("Advanced Settings")]
        public bool coyoteTimeEnabled = true;
        public float coyoteTimeDuration = 0.25f;
        public float groundCheckRadius = 0.3f;
        public LayerMask groundMask;
        public float ceilingCheckDistance = 0.2f;

        [Header("Head Bobbing")]
        public float walkingBobbingSpeed = 14f;
        public float bobbingAmount = 0.05f;
        private float sprintBobMultiplier = 1.2f;

        [Header("References")]
        public Transform groundCheck;
        public Transform playerCamera;
        public Transform cameraParent;
        public Camera cam;

        [Header("Features Toggle")]
        public bool canSlide = true;
        public bool canJump = true;
        public bool canSprint = true;
        public bool canCrouch = true;

        // Private variables
        private CharacterController characterController;
        private float rotX, rotY;
        private float xVelocity, yVelocity;
        private Vector3 moveDirection = Vector3.zero;
        private Vector2 moveInput;
        private bool isGrounded;
        private bool isSprinting;
        private bool isCrouching;
        private bool isSliding;
        private float slideTimer;
        private Vector3 slideDirection;
        private float originalHeight;
        private float originalCameraParentHeight;
        private float coyoteTimer;
        private float bobTimer;
        private float defaultPosY;
        private Vector3 recoil = Vector3.zero;
        private bool isLook = true, isMove = true;
        private float currentFov;
        private float fovVelocity;
        private float currentTiltAngle;
        private float tiltVelocity;
        private float currentSpeed;
        private Vector3 currentVelocity;
        private float currentCameraHeight;
        private float currentBobOffset;
        private float lastGroundedTime;
        private bool wasGrounded;
        private RaycastHit cameraWallHit;
        private float cameraWallDistance;
        private const float cameraCollisionOffset = 0.1f;
        private const float cameraCollisionSmoothSpeed = 15f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            cam = playerCamera.GetComponent<Camera>();
            originalHeight = characterController.height;
            originalCameraParentHeight = cameraParent.localPosition.y;
            defaultPosY = cameraParent.localPosition.y;

            Cursor.lockState = CursorLockMode.Locked;
            currentFov = normalFov;
            currentCameraHeight = originalCameraParentHeight;
        }

        private void Update()
        {
            HandleGroundCheck();
            HandleLook();
            HandleCrouchAndSlide();
            HandleMovement();
            HandleHeadBob();
            UpdateCamera();
        }

        private void HandleGroundCheck()
        {
            wasGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

            if (isGrounded && !wasGrounded)
            {
                lastGroundedTime = Time.time;
            }

            if (isGrounded && moveDirection.y < 0)
            {
                moveDirection.y = -2f;
                coyoteTimer = coyoteTimeEnabled ? coyoteTimeDuration : 0f;
            }
            else if (coyoteTimeEnabled)
            {
                coyoteTimer -= Time.deltaTime;
            }
        }

        private void HandleLook()
        {
            if (!isLook) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 10f * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 10f * Time.deltaTime;

            rotX += mouseX;
            rotY -= mouseY;
            rotY = Mathf.Clamp(rotY, -90f, 90f);

            xVelocity = Mathf.Lerp(xVelocity, rotX, snappiness * Time.deltaTime);
            yVelocity = Mathf.Lerp(yVelocity, rotY, snappiness * Time.deltaTime);

            transform.rotation = Quaternion.Euler(0f, xVelocity, 0f);
        }

        private void HandleCrouchAndSlide()
        {
            // Check for ceiling
            bool hasCeiling = Physics.Raycast(transform.position, Vector3.up,
                originalHeight + ceilingCheckDistance, groundMask);

            // Handle slide
            if (canSlide && isSprinting && Input.GetKeyDown(KeyCode.LeftControl) &&
                isGrounded && Time.time > lastSlideTime + slideCooldown)
            {
                StartSlide();
            }

            if (isSliding)
            {
                UpdateSlide();
            }
            else
            {
                // Handle crouch only when not sliding
                bool wantsToCrouch = canCrouch && Input.GetKey(KeyCode.LeftControl);
                isCrouching = wantsToCrouch || (hasCeiling && !isSliding);
            }

            // Update character height
            float targetHeight = isCrouching || isSliding ? crouchHeight : originalHeight;
            characterController.height = Mathf.Lerp(
                characterController.height,
                targetHeight,
                crouchTransitionSpeed * Time.deltaTime);
            characterController.center = new Vector3(0f, characterController.height * 0.5f, 0f);
        }

        private void StartSlide()
        {
            isSliding = true;
            isCrouching = true;
            slideTimer = slideDuration;
            lastSlideTime = Time.time;

            slideDirection = moveInput.magnitude > 0.1f ?
                (transform.right * moveInput.x + transform.forward * moveInput.y).normalized :
                transform.forward;

            currentSpeed = sprintSpeed;
        }

        private void UpdateSlide()
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f || !isGrounded)
            {
                isSliding = false;
                return;
            }

            float slideProgress = slideTimer / slideDuration;
            float targetSlideSpeed = slideSpeed * Mathf.Lerp(0.7f, 1f, slideProgress);
            currentSpeed = Mathf.Lerp(currentSpeed, targetSlideSpeed, Time.deltaTime * 5f);

            Vector3 slideMovement = slideDirection * currentSpeed * Time.deltaTime;
            characterController.Move(slideMovement);
        }

        private void HandleMovement()
        {
            if (!isMove)
            {
                moveInput = Vector2.zero;
                return;
            }

            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            isSprinting = canSprint && Input.GetKey(KeyCode.LeftShift) &&
                         moveInput.y > 0.1f && isGrounded && !isCrouching && !isSliding;

            float targetSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
            if (isSliding) targetSpeed = currentSpeed;

            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            Vector3 targetVelocity = transform.TransformDirection(inputDirection) * targetSpeed;

            // Apply acceleration/deceleration
            float accelerationRate = inputDirection.magnitude > 0.1f ? acceleration : deceleration;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, accelerationRate * Time.deltaTime);

            // Handle jumping
            if ((isGrounded || coyoteTimer > 0f) && canJump && Input.GetKeyDown(KeyCode.Space) && !isSliding)
            {
                moveDirection.y = jumpSpeed;
                coyoteTimer = 0f;
            }

            // Apply gravity
            moveDirection.y -= gravity * Time.deltaTime;

            if (!isSliding)
            {
                moveDirection = new Vector3(currentVelocity.x, moveDirection.y, currentVelocity.z);
                characterController.Move(moveDirection * Time.deltaTime);
            }
        }

        private void HandleHeadBob()
        {
            Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
            bool isMoving = horizontalVelocity.magnitude > 0.1f && isGrounded;

            if (isMoving && !isSliding)
            {
                float bobSpeed = walkingBobbingSpeed * (isSprinting ? sprintBobMultiplier : 1f);
                bobTimer += Time.deltaTime * bobSpeed;
                currentBobOffset = Mathf.Sin(bobTimer) * bobbingAmount;

                // Small recoil effect based on movement
                recoil.z = moveInput.x * -2f;
            }
            else
            {
                bobTimer = 0f;
                currentBobOffset = Mathf.Lerp(currentBobOffset, 0f, Time.deltaTime * 10f);
                recoil = Vector3.Lerp(recoil, Vector3.zero, Time.deltaTime * 10f);
            }
        }

        private void UpdateCamera()
        {
            // Update camera height
            float targetCameraHeight = isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;
            currentCameraHeight = Mathf.Lerp(
                currentCameraHeight,
                targetCameraHeight,
                crouchTransitionSpeed * Time.deltaTime);

            // Handle camera collision with walls
            float targetCameraZ = -0.2f; // Default camera local position z
            float cameraCollisionRadius = 0.2f;

            if (Physics.SphereCast(
                transform.position + Vector3.up * currentCameraHeight,
                cameraCollisionRadius,
                playerCamera.forward,
                out cameraWallHit,
                2f,
                groundMask))
            {
                targetCameraZ = -Vector3.Distance(transform.position, cameraWallHit.point) + cameraCollisionOffset;
            }

            // Smoothly adjust camera position
            Vector3 targetCameraPos = new Vector3(
                0f,
                currentCameraHeight + currentBobOffset,
                Mathf.Lerp(playerCamera.localPosition.z, targetCameraZ, Time.deltaTime * cameraCollisionSmoothSpeed));

            playerCamera.localPosition = targetCameraPos;

            // Update camera rotation
            playerCamera.localRotation = Quaternion.Euler(yVelocity, 0f, 0f);
            cameraParent.localRotation = Quaternion.Euler(recoil);

            // Update FOV
            float targetFov = isSprinting ? sprintFov : normalFov;
            if (isSliding)
            {
                float slideProgress = slideTimer / slideDuration;
                targetFov = sprintFov + (slideFovBoost * Mathf.Lerp(0f, 1f, 1f - slideProgress));
            }

            currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovVelocity, 1f / fovChangeSpeed);
            cam.fieldOfView = currentFov;

            // Update camera tilt for sliding
            float targetTiltAngle = isSliding ? slideTiltAngle : 0f;
            currentTiltAngle = Mathf.SmoothDamp(
                currentTiltAngle,
                targetTiltAngle,
                ref tiltVelocity,
                0.2f);

            playerCamera.localRotation *= Quaternion.Euler(-currentTiltAngle, 0f, 0f);
        }

        public void SetControl(bool newState)
        {
            SetLookControl(newState);
            SetMoveControl(newState);
        }

        public void SetLookControl(bool newState)
        {
            isLook = newState;
        }

        public void SetMoveControl(bool newState)
        {
            isMove = newState;
        }

        public void SetCursorVisibility(bool newVisibility)
        {
            Cursor.lockState = newVisibility ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = newVisibility;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}