using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoadCarGenerator : MonoBehaviour {
    public enum Direction { Left = -1, Right = 1 };

    public bool randomizeValues = false;

    public Direction direction;
    public Vector2 speedRange = new Vector2(5f, 15f);
    public Vector2 hazardDensityRange = new Vector2(2f, 5f);
    public Vector2 gapOddsRange = new Vector2(0.1f, 0.3f);   //What are the odds of a car not being spawned and having a gap in the traffic?
    public float gapOdds = 0.3f;
    public float hazardDensity = 3;
    public float speed = 2.0f;
    //public Vector2 intervalRange = new Vector2(2, 5);
    public float interval = 6.0f;
    public float leftX = -20.0f;
    public float rightX = 20.0f;

    public GameObject[] carPrefabs;

    private float elapsedTime;

    private List<GameObject> cars = new List<GameObject>();

    public Color[] CarColors;
    public float Difficulty = 0.5f;

    public void Start() {
        //So basically we need a difficulty measure here
        float LineValue = transform.position.z / 3f;
        Difficulty = LineValue / GameStateControllerScript.Instance.maxDifficultyLine;
        float Difficulty_Min = Mathf.Lerp(Difficulty, 0f, 0.25f);
        float Difficulty_Max = Mathf.Lerp(Difficulty, 1f, 0.25f);
        if (randomizeValues) {
            direction = Random.value < 0.5f ? Direction.Left : Direction.Right;

            //So these values need some sort of bias for the difficulty...
            speed = Random.Range(Mathf.Lerp(speedRange.x, speedRange.y, Difficulty_Min),
                Mathf.Lerp(speedRange.x, speedRange.y, Difficulty_Max));
            hazardDensity = Random.Range(Mathf.Lerp(hazardDensityRange.x, hazardDensityRange.y, Difficulty_Min),
                Mathf.Lerp(hazardDensityRange.x, hazardDensityRange.y, Difficulty_Max));

            gapOdds = Random.Range(Mathf.Lerp(gapOddsRange.y, gapOddsRange.x, Difficulty_Min),
                Mathf.Lerp(gapOddsRange.y, gapOddsRange.x, Difficulty_Max));

            float distance = rightX - leftX;
            float travelTime = distance / speed;
            interval = travelTime / hazardDensity;
        }

        elapsedTime = interval; //As we'll be pre-populating
        cars = new List<GameObject>();

        //We could do with pre-populating our cars
        prepopulateLine();
    }

    void prepopulateLine()
    {
        float distance = rightX - leftX;
        float travelTime = distance / speed;
        int hazardDensity = Mathf.CeilToInt(travelTime / interval);

        for (int i=0; i<hazardDensity; i++)
        {
            float span = (float)i / (float)hazardDensity;
            Vector3 position = transform.position + new Vector3(Mathf.Lerp(rightX, leftX, span) * (direction == Direction.Right ? 1f : -1f), 0.6f, 0);
            if (Random.value > gapOdds)
            {
                SpawnVehicle(position);
            }
        }
    }

    public void Update() {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > interval) {
            elapsedTime = 0.0f;
            Vector3 position = transform.position + new Vector3(direction == Direction.Left ? rightX : leftX, 0.6f, 0);
            if (Random.value > gapOdds)
            {
                SpawnVehicle(position);
            }
        }

        foreach (GameObject o in cars.ToArray()) {
            if (direction == Direction.Left && o.transform.position.x < leftX || direction == Direction.Right && o.transform.position.x > rightX) {
                Destroy(o);
                cars.Remove(o);
            }
        }
    }

    void SpawnVehicle(Vector3 position)
    {

        //Vector3 position = transform.position + new Vector3(direction == Direction.Left ? rightX : leftX, 0.6f, 0);
        GameObject o = (GameObject)Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], position, Quaternion.Euler(-90, 90, 0));

        CarScript ourCarScript = o.GetComponent<CarScript>();
        ourCarScript.SetSpeed((int)direction * speed);

        if (CarColors.Length > 0)
        {
            ourCarScript.SetMaterialColor(CarColors[Random.Range(0, CarColors.Length)]);
        }

        if (direction < 0)
            o.transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            o.transform.rotation = Quaternion.Euler(0, 180, 0);

        cars.Add(o);
    }

    public void OnDestroy() {
        foreach (GameObject o in cars) {
            Destroy(o);
        }
    }
}
