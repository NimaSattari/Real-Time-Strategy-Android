using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] Transform playerCameraTransform = null;
    [SerializeField] float speed = 20f;
    [SerializeField] float screenBorderTickness = 10f;
    [SerializeField] Vector2 screenXLimits = Vector2.zero;
    [SerializeField] Vector2 screenZLimits = Vector2.zero;

    private Vector2 previousInput;

    private Controls controls;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);
        controls = new Controls();

        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;
        controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority || !Application.isFocused) { return; }
        UpdateCameraPositon();
    }

    public void MoveUp(bool doo)
    {
        if (doo)
        {
            previousInput = Vector2.up;
        }
        else
        {
            previousInput = Vector2.zero;
        }
    }
    public void MoveDown(bool doo)
    {
        if (doo)
        {
            previousInput = Vector2.down;
        }
        else
        {
            previousInput = Vector2.zero;
        }
    }
    public void MoveLeft(bool doo)
    {
        if (doo)
        {
            previousInput = Vector2.left;
        }
        else
        {
            previousInput = Vector2.zero;
        }
    }
    public void MoveRight(bool doo)
    {
        if (doo)
        {
            previousInput = Vector2.right;
        }
        else
        {
            previousInput = Vector2.zero;
        }
    }

    private void UpdateCameraPositon()
    {
        Vector3 pos = playerCameraTransform.position;
        if (previousInput == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;
            Vector2 cursorPosition = Mouse.current.position.ReadValue();
            if (cursorPosition.y >= Screen.height - screenBorderTickness)
            {
                cursorMovement.z += 1;
            }
            else if (cursorPosition.y <= screenBorderTickness)
            {
                cursorMovement.z -= 1;
            }
            if (cursorPosition.x >= Screen.width - screenBorderTickness)
            {
                cursorMovement.x += 1;
            }
            else if (cursorPosition.x <= screenBorderTickness)
            {
                cursorMovement.x -= 1;
            }
            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else
        {
            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }
        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = pos;

    }

    private void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previousInput = ctx.ReadValue<Vector2>();
    }
}
