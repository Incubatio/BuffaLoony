using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMono : MonoBehaviour
{
    private const int ORB_POOL_SIZE = 1000;

    [Header("Gameplay")]
    public int StartingOrbNumber = 3; // Add number of orb to spawning player
    public float PlayerSpeed = 8f;
    public float OrbitSpeed = 140f;
    public float OrbitRadius = 1.6f;
    
    [Header("Assets")]
    public GameObject PlayerPrefab;
    public GameObject OrbPrefab;
    
    public InputActionReference MoveLeft, MoveRight, MoveForward, MoveBackward;

    private Stack<GameObject> _FreeOrbs;
    private HashSet<GameObject> _OrbsInUse;
    private List<GameObject> _Players;
    private List<int> _AlivePlayers;
    private GameObject _LocalPlayer;
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
        _ActorsParent = gameObjects.First(go => go.name == SceneHelper.ACTORS).transform;
        _Players = new List<GameObject>();
        _AlivePlayers = new List<int>();
        _FreeOrbs = new Stack<GameObject>();
        _OrbsInUse = new HashSet<GameObject>();
        
        for (int i = 0; i < ORB_POOL_SIZE; i++)
        {
            GameObject orb = Instantiate(OrbPrefab, _ActorsParent);
            orb.AddComponent<OrbComponent>();
            MainHelper.FreeOrb(orb, _FreeOrbs);
        }
        
        CreateCharacter();
        _LocalPlayer = _Players.First();
        _RootUI.Q<Button>(UIHelper.SPAWN_BTN).clicked += CreateCharacter;
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

            var playerComponent = player.GetComponent<PlayerComponent>();
            MainHelper.AnimateOrbs(playerComponent.Orbs, playerPosition, OrbitSpeed, OrbitRadius);
        }
        
        // 4. Collision Detection
        foreach (var orb in _OrbsInUse)
        {
            var orbComponent = orb.GetComponent<OrbComponent>();
            foreach (int playerIndex in _AlivePlayers)
            {
                if (orbComponent.PlayerIndex == playerIndex) continue;

                var attacker = _Players[orbComponent.PlayerIndex];
                var attackerComponent = attacker.GetComponent<PlayerComponent>();
                //if (attackerComponent.Form == EForms.GHOST ) continue;

                var victim = _Players[playerIndex];
                var victimComponent = victim.GetComponent<PlayerComponent>();
                //if (victimComponent.Form == EForms.GHOST ) continue;

                var pos1 = victim.transform.position;
                var pos2 = orb.transform.position;
                if (MainHelper.CheckCircleCollision(new Vector2(pos1.x, pos1.z), victim.transform.localScale.x / 2, 
                        new Vector2(pos2.x, pos2.z), orb.transform.localScale.x / 2))
                {
                    Debug.Log("Collision from " + orbComponent.PlayerIndex + " to " + playerIndex);
                }
            }
        }


            //

    }

    public void CreateCharacter()
    {
        var player = MainHelper.CreatePlayer(PlayerPrefab, _ActorsParent, _Waypoints);
        var playerIndice = _Players.Count;
        var isLocalPlayer = playerIndice == 0;
        _Players.Add(player);
        _AlivePlayers.Add(playerIndice);
        
        if(!isLocalPlayer)
            MainHelper.AddAI(player, _Waypoints);
        
        var playerComponent = player.GetComponent<PlayerComponent>();
        playerComponent.Orbs = new List<GameObject>();
            
        for (var i = 0; i < StartingOrbNumber; i++)
        {
            var orb = _FreeOrbs.Pop();
            var projectileComponent = orb.GetComponent<OrbComponent>();
            projectileComponent.PlayerIndex = playerIndice;
            _OrbsInUse.Add(orb);
            playerComponent.Orbs.Add(orb);
        }
        
        var color = isLocalPlayer
            ? new Color(0.2f, 0.2f, 0.8f, 1f)
            : new Color(0.5f, 0.1f, 0.1f, 1f);
        MainHelper.SetColor(player, color);
    }

}
