using UnityEngine;
using UnityEngine.InputSystem;

public class MainMono : MonoBehaviour
{
    public GameObject PlayerPrefab;
    
    public float PlayerSpeed = 8f;
    
    public InputActionReference MoveLeft, MoveRight, MoveForward, MoveBackward;

    private GameObject _LocalPlayer;
    private Vector3 _MovementInput;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _LocalPlayer = Instantiate(PlayerPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        // 1. Update Game Data
        
        // 2. Input Management
        _MovementInput.x = _MovementInput.y = _MovementInput.z = 0f;
        if (MoveLeft.action.IsPressed()) _MovementInput.x -= 1f;
        if (MoveRight.action.IsPressed()) _MovementInput.x += 1f;
        if (MoveBackward.action.IsPressed()) _MovementInput.z -= 1f;
        if (MoveForward.action.IsPressed()) _MovementInput.z += 1f;
        if (_MovementInput.magnitude > 1f)
            _MovementInput.Normalize();   
        
        _LocalPlayer.transform.Translate(_MovementInput * PlayerSpeed * Time.deltaTime);
        
        // 3. Process Movement
        // 3. Collision Detection
        
        //

    }

}
