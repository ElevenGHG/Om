using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [Header("Waggon Prefabs")]
    public List<GameObject> wagonPrefabs = new List<GameObject>();

    [Header("Train Settings")]
    [Min(1)]
    public int trainCount = 3;

    [Min(1)]
    public int wagonsPerTrain = 15;

    [Header("Dimensions")]
    public float wagonWidth = 15f;
    public float wagonLength = 40f;

    public float wagonGap = 3f;
    public float trainGap = 3f;

    [Header("Movement")]
    public bool randomDirection = true;
    public float minSpeed = 2f;
    public float maxSpeed = 8f;

    [Header("Endless System")]
    public float recycleDistance = 400f;

    private class Train
    {
        public List<GameObject> wagons = new List<GameObject>();

        public float direction;
        public float speed;
        public float xPosition;
    }

    private readonly List<Train> trains = new List<Train>();

    private float WagonSpacing => wagonLength + wagonGap;

    private void Start()
    {
        if (wagonPrefabs == null || wagonPrefabs.Count == 0)
        {
            Debug.LogError(
                "TrainManager: Bitte mindestens ein Waggon-Prefab zuweisen!"
            );

            enabled = false;
            return;
        }

        if (trainCount < 1 || wagonsPerTrain < 1)
        {
            Debug.LogError(
                "TrainManager: Zuganzahl und Waggonanzahl müssen mindestens 1 sein."
            );

            enabled = false;
            return;
        }

        CreateTrains();
    }

    private void CreateTrains()
    {
        trains.Clear();

        // Die Züge werden gleichmäßig um X = 0 verteilt.
        float trainSpacing = wagonWidth + trainGap;

        for (int t = 0; t < trainCount; t++)
        {
            Train train = new Train();

            train.xPosition =
                (t - (trainCount - 1) / 2f) * trainSpacing;

            train.direction = randomDirection
                ? (Random.value < 0.5f ? -1f : 1f)
                : 1f;

            train.speed = Random.Range(minSpeed, maxSpeed);

            for (int w = 0; w < wagonsPerTrain; w++)
            {
                GameObject prefab = GetRandomWagonPrefab();

                if (prefab == null)
                {
                    Debug.LogWarning(
                        "TrainManager: Ein Waggon-Prefab ist leer."
                    );

                    continue;
                }

                float zPosition =
                    (w - (wagonsPerTrain - 1) / 2f) * WagonSpacing;

                Vector3 position = new Vector3(
                    train.xPosition,
                    0f,
                    zPosition
                );

                GameObject wagon = Instantiate(
                    prefab,
                    position,
                    Quaternion.identity,
                    transform
                );

                train.wagons.Add(wagon);
            }

            trains.Add(train);
        }
    }

    private GameObject GetRandomWagonPrefab()
    {
        if (wagonPrefabs.Count == 0)
            return null;

        int index = Random.Range(0, wagonPrefabs.Count);

        return wagonPrefabs[index];
    }

    private void Update()
    {
        foreach (Train train in trains)
        {
            MoveTrain(train);
        }
    }

    private void MoveTrain(Train train)
    {
        if (train.wagons.Count == 0)
            return;

        float movement =
            train.direction * train.speed * Time.deltaTime;

        // Zuerst alle Waggons bewegen.
        foreach (GameObject wagon in train.wagons)
        {
            if (wagon == null)
                continue;

            wagon.transform.position += new Vector3(
                0f,
                0f,
                movement
            );
        }

        // Danach die äußersten Positionen bestimmen.
        float minZ = float.PositiveInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (GameObject wagon in train.wagons)
        {
            if (wagon == null)
                continue;

            float z = wagon.transform.position.z;

            minZ = Mathf.Min(minZ, z);
            maxZ = Mathf.Max(maxZ, z);
        }

        // Waggons hinter dem Zug werden nach vorne versetzt.
        foreach (GameObject wagon in train.wagons)
        {
            if (wagon == null)
                continue;

            Vector3 position = wagon.transform.position;

            if (train.direction > 0f &&
                position.z < -recycleDistance)
            {
                position.z = maxZ + WagonSpacing;
                wagon.transform.position = position;

                maxZ = position.z;
            }
            else if (train.direction < 0f &&
                     position.z > recycleDistance)
            {
                position.z = minZ - WagonSpacing;
                wagon.transform.position = position;

                minZ = position.z;
            }
        }
    }
}