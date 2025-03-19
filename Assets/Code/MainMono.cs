using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMono : MonoBehaviour
{
    public GameObject PlayerPrefab;
    
    public float PlayerSpeed = 8f;
    
    public InputActionReference MoveLeft, MoveRight, MoveForward, MoveBackward;

    private List<GameObject> _Players;
    private GameObject _LocalPlayer => _Players[0];
    private Vector3 _MovementInput;
    private Transform[] _Waypoints;
    private Transform _ActorsParent;
    private VisualElement _RootUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        var currentScene = SceneManager.GetActiveScene();
        var gameObjects = currentScene.GetRootGameObjects();
        _RootUI = gameObjects.First(go => go.name == SceneHelper.UI_DOCUMENT).GetComponent<UIDocument>().rootVisualElement;
        _Waypoints = gameObjects.First(go => go.name == SceneHelper.WAYPOINTS).GetComponentsInChildren<Transform>();
        _Players = new List<GameObject>();
        
        var actorsParent = gameObjects.First(go => go.name == SceneHelper.ACTORS).transform;
        _Players.Add(MainHelper.CreatePlayer(_Players.Count, PlayerPrefab, actorsParent, _Waypoints));
        _RootUI.Q<Button>(UIHelper.SPAWN_BTN).clicked += CreateAI;
    }

    void CreateAI()
    {
        var aiPlayer = MainHelper.CreateAI(_Players.Count, PlayerPrefab, _ActorsParent, _Waypoints);
        _Players.Add(aiPlayer);
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
        
        
        // 3. Process Movement
        _LocalPlayer.transform.Translate(_MovementInput * PlayerSpeed * Time.deltaTime);
        
        foreach (var player in _Players)
        { 
            var playerPosition = player.transform.position;
            if (player.TryGetComponent(out AIComponent aiComponent))
            {
                var targetPosition = _Waypoints[aiComponent.WaypointIndice].position;
                Vector3 direction = (targetPosition - playerPosition).normalized;

                player.transform.Translate(direction * PlayerSpeed * Time.deltaTime); // move players

                if (Vector3.Distance(playerPosition, targetPosition) < 0.1f) // check if AI reached waypoint
                    aiComponent.WaypointIndice = Random.Range(0, _Waypoints.Length);
            }
        }

        
        
        // 3. Collision Detection
        
        //

    }


}
