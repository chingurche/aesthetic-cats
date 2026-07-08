using System;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AtmosphericFishSwarm swarmPrefab;
    private Transform player;

    [Header("Spawn")]
    [SerializeField] private int maxGroups = 6;
    
    [SerializeField]
    private Vector2Int fishCountRange = new(40, 80);

    [SerializeField] private float despawnRadius = 130f;

    [SerializeField] private float minSpawnDistance = 70f;
    [SerializeField] private float maxSpawnDistance = 110f;

    private float spawnTimer;
    [SerializeField] private float spawnInterval = 0.7f;

    private readonly System.Random random = new();

    private ObjectPool pool;

    private readonly List<FishGroup> groups = new();

    private Rigidbody playerRb;

    class FishGroup
    {
        public Vector3 Center;
        public AtmosphericFishSwarm Swarm;
    }

    public void Initialize(Transform player)
    {
        pool = GetComponent<ObjectPool>();

        if (player != null)
            this.player = player;
            playerRb = player.GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (player == null)
            return;

        RemoveFarGroups();

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval && groups.Count < maxGroups)
        {
            spawnTimer = 0f;
            SpawnGroupAroundPlayer();
        }
    }

    private Vector3 GetMovementDirection()
    {
        if (playerRb != null && playerRb.linearVelocity.sqrMagnitude > 1f)
        {
            Vector3 dir = playerRb.linearVelocity;
            dir.y = 0f;
            return dir.normalized;
        }

        Vector3 forward = player.forward;
        forward.y = 0f;

        return forward.normalized;
    }
    private Vector3 GetSpawnDirection(System.Random random)
    {
        Vector3 forward = GetMovementDirection();

        Vector3 right = Vector3.Cross(Vector3.up, forward);

        float roll = (float)random.NextDouble();

        if (roll < 0.7f)
        {
            return Quaternion.AngleAxis(
                RandomRange(random, -35f, 35f),
                Vector3.up) * forward;
        }

        if (roll < 0.9f)
        {
            return RandomRange(random, 0f, 1f) < 0.5f
                ? right
                : -right;
        }

        return -forward;
    }
  private void SpawnGroupAroundPlayer()
    {

        Vector3 direction = GetSpawnDirection(random);

        float distance =
            RandomRange(random, minSpawnDistance, maxSpawnDistance);

        Vector3 target = player.position + direction * distance;

        Ray ray = new Ray(target + Vector3.up * 100f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
        target.y = hit.point.y + 3f;
        }

        GameObject obj = pool.Get(swarmPrefab.gameObject, transform);

        AtmosphericFishSwarm swarm = obj.GetComponent<AtmosphericFishSwarm>();

        swarm.transform.position = target;
        swarm.playerTransform = player;
        groups.Add(new FishGroup
        {
            Center = target,
            Swarm = swarm
        });

        swarm.Initialize(random.Next(fishCountRange.x, fishCountRange.y + 1));

    }

    private void RemoveFarGroups()
    {
        for (int i = groups.Count - 1; i >= 0; i--)
        {
            FishGroup group = groups[i];

            float distance =
                Vector3.Distance(player.position, group.Center);

            if (distance < despawnRadius)
                continue;

            group.Swarm.ResetSwarm();
            pool.Release(group.Swarm.gameObject);

            groups.RemoveAt(i);
        }
    }

    private float RandomRange(System.Random random, float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }
}