using System.Collections.Generic;
using UnityEngine;

public static class MainHelper
{
    public static GameObject CreatePlayer( GameObject pPrefab, Transform pParent, Transform[] pWaypoints)
    {
        var player = Object.Instantiate(pPrefab, pParent);

        var transformIndex = Random.Range(0, pWaypoints.Length);
        player.transform.position = pWaypoints[transformIndex].position;

        player.AddComponent<PlayerComponent>();
        return player;
    }

    public static void AddAI(GameObject player, Transform[] pWaypoints)
    {
        player.AddComponent<AIComponent>();
        var aiComponent = player.GetComponent<AIComponent>();
        aiComponent.WaypointIndice = Random.Range(0, pWaypoints.Length);
    }

    public static void FreeOrb(GameObject pOrb, Stack<GameObject> pFreeOrbs)
    {
        pOrb.transform.position = new Vector3(-1000, -1000, -1000);
        var projectileComponent = pOrb.GetComponent<OrbComponent>();
        projectileComponent.PlayerIndex = -1;
        pFreeOrbs.Push(pOrb);

    }

    public static void SetColor(GameObject pPlayer, Color pColor = default)
    {
        if (pColor == default)
            pColor = pPlayer.GetComponent<Renderer>().material.color;
        else
            pPlayer.GetComponent<Renderer>().material.color = pColor;
        
        var playerComponent = pPlayer.GetComponent<PlayerComponent>();
        foreach (var orb in playerComponent.Orbs)
            orb.GetComponent<Renderer>().material.color = pColor;
    }

    public static void AnimateOrbs(List<GameObject> pOrbitObjects, Vector3 pCenterPosition, float pOrbitSpeed, float pOrbitRadius)
    {
        float angleOffset = 360f / pOrbitObjects.Count;

        // add offset so rotations are not in sync
        // TODO: Maybe variable speed ?
        var offset = pOrbitObjects[0].GetComponent<OrbComponent>().PlayerIndex * 10;
        for (int j = 0; j < pOrbitObjects.Count; j++)
        {
            float angle = (pOrbitSpeed * Time.time + offset + angleOffset * j) * Mathf.Deg2Rad;

            float x = pCenterPosition.x + Mathf.Cos(angle) * pOrbitRadius;
            float z = pCenterPosition.z + Mathf.Sin(angle) * pOrbitRadius;

            pOrbitObjects[j].transform.position = new Vector3(x, pCenterPosition.y, z);
        }
    }

    public static bool CheckCircleCollision(Vector2 pPosition1, float pRadius1, Vector2 pPosition2, float pRadius2)
    {
        float distance = Vector2.Distance(pPosition1, pPosition2);
        return distance < pRadius1 + pRadius2;
    }

    public static void SwitchForm(GameObject pPlayer, EForms pForm)
    {
        var playerComponent = pPlayer.GetComponent<PlayerComponent>();
        if (playerComponent.Form == pForm) return;
        var renderer = pPlayer.GetComponent<Renderer>();

        switch (playerComponent.Form)
        {
            case EForms.GHOST:
                playerComponent.GhostFormStart = 0f;
                var oldColor = renderer.material.color;
                MainHelper.SetColor(pPlayer, new Color(oldColor.r, oldColor.g, oldColor.b, 1f));
                Debug.Log("Cleaning Ghost");
                break;
            case EForms.TITAN:
                Debug.Log("Cleaning Titan");
                playerComponent.TitanFormStart = 0f;
                pPlayer.transform.localScale /= 2f;
                break;
        }

        switch (pForm)
        {
            case EForms.GHOST:
                playerComponent.GhostFormStart = Time.time;
                var oldColor = renderer.material.color;
                MainHelper.SetColor(pPlayer, new Color(oldColor.r, oldColor.g, oldColor.b, .5f));
                Debug.Log("initiating Ghost");
                break;
            case EForms.TITAN:
                playerComponent.TitanFormStart = Time.time;
                pPlayer.transform.localScale *= 2f;
                Debug.Log("initiating Titan");
                break;
        }
        
        playerComponent.Form = pForm;
    }
}
