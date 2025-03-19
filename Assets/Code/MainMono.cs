using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class MainMono : MonoBehaviour
{
    private const int ORB_POOL_SIZE = 1000;

    [Header("Gameplay")]
    public int StartingOrbNumber = 3; // Add number of orb to spawning player
    public float PlayerSpeed = 8f;
    public float OrbitSpeed = 140f;
    public float OrbitRadius = 1.6f;
    public float GhostFormDuration = 3f;
    public float TitanFormDuration = 5f;
    public float TitanOrbRequirement = 6f;
    public int TitanAttacksPerSecond = 2;
    public int TitanProjectileNumber = 12;
    public float TitanProjectileSpeed = 30f;
    
    public float MaxOrbDistance = 20f;

    
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
    
    private List<(int attackerIndex, int victimIndex)> _Collisions; 
    private List<GameObject> _OrbsToCleanup;

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
        _Collisions = new List<(int playerIndex1, int playerIndex2)>();
        _OrbsToCleanup = new List<GameObject>();
        
        for (int i = 0; i < ORB_POOL_SIZE; i++)
        {
            GameObject orb = Instantiate(OrbPrefab, _ActorsParent);
            orb.AddComponent<OrbComponent>();
            MainHelper.FreeOrb(orb, _FreeOrbs);
        }
        
        CreateCharacter();
        _LocalPlayer = _Players.First();
        _RootUI.Q<Button>(UIHelper.REVIVE_BTN).style.display = DisplayStyle.None;
        _RootUI.Q<Button>(UIHelper.SPAWN_BTN).clicked += CreateCharacter;
        _RootUI.Q<Button>(UIHelper.TITAN_BTN).clicked += SwitchToTitanCheat;
        _RootUI.Q<Button>(UIHelper.GHOST_BTN).clicked += SwitchToGhostCheat;
    }

    // Update is called once per frame
    void Update()
    {
        // 1. Update Game Data
        foreach (int playerIndex in _AlivePlayers)
        {
            var player = _Players[playerIndex];
            var playerComponent = player.GetComponent<PlayerComponent>();
            if (playerComponent.IsGhostFormOver(GhostFormDuration))
                MainHelper.SwitchForm(player, EForms.WARRIOR);
            
            if (playerComponent.IsTitanFormOver(TitanFormDuration))
                MainHelper.SwitchForm(player, EForms.WARRIOR);
            
            if (playerComponent.Orbs.Count >= TitanOrbRequirement) // check for orb number for potential ascension
                MainHelper.SwitchForm(player, EForms.TITAN);
            
            if (playerComponent.Form == EForms.TITAN) // Check attack timer and spawn hell
            {
                var attackCount = (Time.time - playerComponent.TitanFormStart) * TitanAttacksPerSecond;
                if (playerComponent.TitanAttackCount < attackCount)
                {
                    playerComponent.TitanAttackCount++;
                    Debug.Log("stack size:" + _FreeOrbs.Count);
                    Debug.Log("titan attacking, current count:" + playerComponent.TitanAttackCount);
                    var projectiles = new List<GameObject>();
                    for (var j = 0; j < TitanProjectileNumber; j++)
                    {
                        var projectile = _FreeOrbs.Pop();
                        projectiles.Add(projectile);
                        _OrbsInUse.Add(projectile);
                    }

                    var playerPosition = player.transform.position;
                    var playerColor = player.GetComponent<Renderer>().material.color;
                    MainHelper.PlaceOrbs(projectiles, playerPosition, OrbitSpeed, OrbitRadius);
                    foreach (var projectile in projectiles)
                    {
                        var orbComponent = projectile.GetComponent<OrbComponent>();
                        orbComponent.Direction = (projectile.transform.position - playerPosition).normalized;
                        orbComponent.PlayerIndex = playerIndex;
                        projectile.GetComponent<Renderer>().material.color = playerColor;
                    }
                }
            }

        }
        
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
        
        foreach (int playerIndex in _AlivePlayers)
        { 
            var player = _Players[playerIndex];
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
            if (playerComponent.Form == EForms.TITAN)
                playerPosition.y = player.GetComponent<Renderer>().bounds.size.y;
            MainHelper.PlaceOrbs(playerComponent.Orbs, playerPosition, OrbitSpeed, OrbitRadius);
        }
        
        foreach (var projectile in _OrbsInUse)
        {
            var orbComponent = projectile.GetComponent<OrbComponent>();
            projectile.transform.Translate(orbComponent.Direction * TitanProjectileSpeed * Time.deltaTime); // move projectile
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
                if (attackerComponent.Form == EForms.GHOST ) continue;

                var victim = _Players[playerIndex];
                var victimComponent = victim.GetComponent<PlayerComponent>();
                if (victimComponent.Form == EForms.GHOST ) continue;

                var pos1 = victim.transform.position;
                var pos2 = orb.transform.position;
                if (MainHelper.CheckCircleCollision(new Vector2(pos1.x, pos1.z), victim.transform.localScale.x / 2, 
                        new Vector2(pos2.x, pos2.z), orb.transform.localScale.x / 2))
                {
                    Debug.Log("Collision from " + orbComponent.PlayerIndex + " to " + playerIndex);
                    _Collisions.Add((orbComponent.PlayerIndex, playerIndex));
                }
            }
        }

        // 5. Apply Damage
        foreach ((int attackerIndex, int victimIndex) in _Collisions)
        {
            //Transformation to a ghost (invincibility for 3 seconds), also avoid re-triggering collision instantly
            MainHelper.SwitchForm(_Players[victimIndex], EForms.GHOST);
            
            var attacker = _Players[attackerIndex];
            var victim = _Players[victimIndex];
            var attackerComponent = attacker.GetComponent<PlayerComponent>();
            var victimComponent = victim.GetComponent<PlayerComponent>();
            var lastOrbIndex = victimComponent.Orbs.Count - 1;
            var orbitingObject = victimComponent.Orbs[lastOrbIndex];
            victimComponent.Orbs.RemoveAt(lastOrbIndex); // losing an orbitObject
            if( attackerComponent.Form == EForms.TITAN )
                _OrbsToCleanup.Add(orbitingObject);
            else
                attackerComponent.Orbs.Add(orbitingObject); // Add a new orbitObject
            MainHelper.SetColor(attacker, default);

            // Death
            if (victimComponent.Orbs.Count == 0)
            {
                attackerComponent.Kills++;
                victimComponent.Deaths++;
                _AlivePlayers.Remove(victimIndex);
                if (victimIndex == 0) // if LocalPlayer
                {
                    var reviveBtn = _RootUI.Q<Button>(UIHelper.REVIVE_BTN);
                    reviveBtn.clicked += _ReviveLocalPlayer;
                    reviveBtn.style.display = DisplayStyle.Flex;
                }
            }
        }
        _Collisions.Clear();
        
        // 6. Cleanup projectile going to far
        
        foreach (var orb in _OrbsInUse)
        {
            var pos = orb.transform.position;
            if (MaxOrbDistance < Math.Abs(pos.x) + Math.Abs(pos.y) + Math.Abs(pos.z))
                _OrbsToCleanup.Add(orb);
        }

        foreach (var orb in _OrbsToCleanup)
        {
            _OrbsInUse.Remove(orb);
            MainHelper.FreeOrb(orb, _FreeOrbs);
        }
        _OrbsToCleanup.Clear();
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

        _InitOrbs(playerIndice);
        
        var color = isLocalPlayer
            ? new Color(0.2f, 0.2f, 0.8f, 1f)
            : new Color(0.5f, 0.1f, 0.1f, 1f);
        MainHelper.SetColor(player, color);
    }

    private void _InitOrbs(int pPlayerIndice)
    {
        for (var i = 0; i < StartingOrbNumber; i++)
        {
            var orb = _FreeOrbs.Pop();
            var orbComponent = orb.GetComponent<OrbComponent>();
            orbComponent.PlayerIndex = pPlayerIndice;
            _OrbsInUse.Add(orb);
            _Players[pPlayerIndice].GetComponent<PlayerComponent>().Orbs.Add(orb);
        }
    }

    private void _ReviveLocalPlayer()
    {
        var reviveBtn = _RootUI.Q<Button>(UIHelper.REVIVE_BTN);
        reviveBtn.clicked -= _ReviveLocalPlayer;
        reviveBtn.style.visibility = Visibility.Hidden;
        _AlivePlayers.Add(0);

        var renderer = _LocalPlayer.GetComponent<Renderer>();
        var playerComponent = _LocalPlayer.GetComponent<PlayerComponent>();
        _InitOrbs(0);
    }

    public void SwitchToTitanCheat() => MainHelper.SwitchForm(_LocalPlayer, EForms.TITAN);
    public void SwitchToGhostCheat() => MainHelper.SwitchForm(_LocalPlayer, EForms.GHOST);

}
