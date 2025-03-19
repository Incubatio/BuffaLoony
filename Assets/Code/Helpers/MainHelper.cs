using UnityEngine;

public static class MainHelper
{
    public static GameObject CreatePlayer(int pIndex, GameObject pPrefab, Transform pParent, Transform[] pWaypoints)
    {
        var player = Object.Instantiate(pPrefab, pParent);

        var renderer = player.GetComponent<Renderer>();
        renderer.material.color = pIndex == 0
            ? new Color(0.2f, 0.2f, 0.8f, 1f)
            : new Color(0.5f, 0.1f, 0.1f, 1f);

        var transformIndex = Random.Range(0, pWaypoints.Length);
        player.transform.position = pWaypoints[transformIndex].position;

        player.AddComponent<PlayerComponent>();
        var playerComponent = player.GetComponent<PlayerComponent>();
        return player;
    }

    public static GameObject CreateAI(int pIndex, GameObject pPrefab, Transform pParent, Transform[] pWaypoints)
    {
        var player = CreatePlayer(pIndex, pPrefab, pParent, pWaypoints);
        player.AddComponent<AIComponent>();
        var aiComponent = player.GetComponent<AIComponent>();
        aiComponent.WaypointIndice = Random.Range(0, pWaypoints.Length);
        
        return player;
    }
}
