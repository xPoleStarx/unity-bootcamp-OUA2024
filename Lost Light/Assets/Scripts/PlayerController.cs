using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Serialize
    [SerializeField] float walkSpeed = 5;
    [SerializeField] float runSpeed = 10;
    [SerializeField] float jumpForce = 5;
    [SerializeField] float sneakSpeed = 2;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private GameObject newspaperPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwHeightOffset = 1.5f;
    [SerializeField] private GameObject cube;
    #endregion

    #region Variables
    PlayerInput playerInput;
    InputAction moveAction, runAction, jumpAction, sneakAction, throwAction, toggleLampAction, drawAction;
    Animator anim_;
    Rigidbody rb;
    StreetLampController currentStreetLamp;

    private Vector2 move_Direction;
    private IState current_State;

    private bool isWalking, isRunning, isJumping, isSneaking = false;
    private bool isGrounded;
    private Texture2D texture;
    private Vector2 previousMousePosition;
    private bool isDrawing;
    private NumberRecognizer recognizer;
    #endregion

    #region Main
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        anim_ = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        recognizer = GetComponent<NumberRecognizer>(); // NumberRecognizer bileşenini alın
        if (recognizer == null)
        {
            recognizer = gameObject.AddComponent<NumberRecognizer>(); // NumberRecognizer bileşenini ekleyin
        }
    }

    void Start()
    {
        moveAction = playerInput.actions.FindAction("Move");
        runAction = playerInput.actions.FindAction("Run");
        jumpAction = playerInput.actions.FindAction("Jump");
        sneakAction = playerInput.actions.FindAction("Sneak");
        throwAction = playerInput.actions.FindAction("Throw");
        toggleLampAction = playerInput.actions.FindAction("ToggleLamp");
        drawAction = playerInput.actions.FindAction("Draw");

        if (rb == null) { Debug.LogError("No Rigidbody component found on " + gameObject.name); }
        toggleLampAction.performed += ctx => ToggleNearestLamp();

        texture = new Texture2D(28, 28);
        previousMousePosition = Vector2.zero;
        isDrawing = false;
    }

    void Update()
    {
        HandleInput();
        MovePlayer();
        HandleJump();
        Look();
        UpdateThrowPoint();
        HandleThrow();
        HandleDraw();
    }
    #endregion

    #region Functions
    void HandleInput()
    {
        isRunning = runAction.ReadValue<float>() > 0;
        isSneaking = sneakAction.ReadValue<float>() > 0;

        if (isRunning)
        {
            isSneaking = false;
        }
        else if (isSneaking)
        {
            isRunning = false;
        }
    }

    public bool IsSneaking()
    {
        return isSneaking;
    }

    void MovePlayer()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        float currentSpeed = walkSpeed;

        if (isSneaking)
        {
            currentSpeed = sneakSpeed;
        }
        else if (isRunning)
        {
            currentSpeed = runSpeed;
        }

        Vector3 movement = new Vector3(-direction.x, 0, -direction.y) * currentSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);

        bool isMoving = movement != Vector3.zero;

        anim_.SetBool("isWalking", isMoving && !isRunning && !isSneaking);
        anim_.SetBool("isRunning", isMoving && isRunning);
        anim_.SetBool("isSneaking", isMoving && isSneaking);
    }

    private void Look()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        if (moveInput.sqrMagnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(-moveInput.x, -moveInput.y) * Mathf.Rad2Deg;

            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, 20f * Time.deltaTime);
        }
        else
            rb.angularVelocity = Vector3.zero;
    }

    void HandleJump()
    {
        if (jumpAction.triggered && !isJumping && rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
            anim_.SetBool("isJumping", true);
        }
    }

    void UpdateThrowPoint()
    {
        if (throwPoint != null)
        {
            throwPoint.position = transform.position + transform.forward * 1f + Vector3.up * throwHeightOffset; // 1 birim önde ve yukarıda
            throwPoint.rotation = transform.rotation;
        }
    }

    void HandleThrow()
    {
        if (throwAction.triggered && newspaperPrefab != null && throwPoint != null)
        {
            GameObject newspaper = Instantiate(newspaperPrefab, throwPoint.position, throwPoint.rotation);
            Rigidbody newspaperRb = newspaper.GetComponent<Rigidbody>();
            if (newspaperRb != null)
            {
                newspaperRb.AddForce(throwPoint.forward * throwForce, ForceMode.Impulse);
            }
        }
    }

    void ToggleNearestLamp()
    {
        StreetLampController[] lamps = FindObjectsOfType<StreetLampController>();
        float closestDistance = Mathf.Infinity;
        StreetLampController closestLamp = null;

        foreach (StreetLampController lamp in lamps)
        {
            float distanceToLamp = Vector3.Distance(transform.position, lamp.transform.position);
            if (distanceToLamp < closestDistance && distanceToLamp <= lamp.activationDistance)
            {
                closestDistance = distanceToLamp;
                closestLamp = lamp;
            }
        }

        if (closestLamp != null)
        {
            closestLamp.ToggleLamp();
        }
    }

    void HandleDraw()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDrawing = true;
            previousMousePosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.leftButton.isPressed && isDrawing)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            DrawLine(previousMousePosition, currentMousePosition, Color.white);
            previousMousePosition = currentMousePosition;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDrawing)
        {
            isDrawing = false;
            int number = RecognizeDrawnNumber();
            ChangeCubeColor(number);
            ClearTexture();
        }
    }

    void DrawLine(Vector2 start, Vector2 end, Color color)
    {
        for (float t = 0.0f; t < 1.0f; t += 0.01f)
        {
            int x = (int)Mathf.Lerp(start.x, end.x, t);
            int y = (int)Mathf.Lerp(start.y, end.y, t);
            texture.SetPixel(x, y, color);
        }
        texture.Apply();
    }

    int RecognizeDrawnNumber()
    {
        Tensor<float> inputTensor = new DenseTensor<float>(new[] { 1, 1, 28, 28 });
        for (int y = 0; y < 28; y++)
        {
            for (int x = 0; x < 28; x++)
            {
                inputTensor[0, 0, y, x] = texture.GetPixel(x, y).grayscale;
            }
        }
        return recognizer.RecognizeNumber(inputTensor);
    }

    void ChangeCubeColor(int number)
    {
        Color color = Color.white;
        switch (number)
        {
            case 1:
                color = Color.yellow;
                break;
            case 2:
                color = Color.blue;
                break;
            case 3:
                color = Color.red;
                break;
        }
        cube.GetComponent<Renderer>().material.color = color;
    }

    void ClearTexture()
    {
        Color[] clearColors = new Color[28 * 28];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = Color.black;
        }
        texture.SetPixels(clearColors);
        texture.Apply();
    }
    #endregion

    #region Misc
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
            anim_.SetBool("isJumping", false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        StreetLampController lamp = other.GetComponent<StreetLampController>();
        if (lamp != null)
        {
            currentStreetLamp = lamp;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (currentStreetLamp != null && other.GetComponent<StreetLampController>() == currentStreetLamp)
        {
            currentStreetLamp = null;
        }
    }
    #endregion
}
