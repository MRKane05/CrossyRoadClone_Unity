using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrunkGeneratorScript : MonoBehaviour {
    public enum Direction { Left = -1, Right = 1 };

    public bool randomizeValues = false;
    public Vector2 speedRange = new Vector2(2f, 5f);
    public Vector2 hazardDensityRange = new Vector2(4f, 1f);
    public Vector2 gapOddsRange = new Vector2(0.1f, 0.5f);   //What are the odds of a car not being spawned and having a gap in the traffic?
    public Vector2 lengthRange = new Vector2(2f, 4f);

    public float gapOdds = 0.3f;
    public float hazardDensity = 3;
    public float Difficulty = 0.5f;

    public Direction direction;
    public float speed = 2.0f;
    public float length = 2.0f;
    public float interval = 2.0f;
    public float leftX = -20.0f;
    public float rightX = 20.0f;

    public GameObject trunkPrefab;

    private float elapsedTime;

    private List<GameObject> trunks = new List<GameObject>();

    public GameObject EffectLeft, EffectRight;

    public void Start() {
        float LineValue = transform.position.z / 3f;
        Difficulty = LineValue / GameStateControllerScript.Instance.maxDifficultyLine;
        float Difficulty_Min = Mathf.Lerp(Difficulty, 0f, 0.25f);
        float Difficulty_Max = Mathf.Lerp(Difficulty, 1f, 0.25f);
        if (randomizeValues)
        {
            direction = Random.value < 0.5f ? Direction.Left : Direction.Right;

            //So these values need some sort of bias for the difficulty...
            //speed = Random.Range(Mathf.Lerp(speedRange.x, speedRange.y, Difficulty_Min),
            //    Mathf.Lerp(speedRange.x, speedRange.y, Difficulty_Max));
            speed = Random.Range(speedRange.x, speedRange.y);
            hazardDensity = Random.Range(Mathf.Lerp(hazardDensityRange.x, hazardDensityRange.y, Difficulty_Min),
                Mathf.Lerp(hazardDensityRange.x, hazardDensityRange.y, Difficulty_Max));
            //hazardDensity = hazardDensityRange.y - hazardDensity; //Invert the hazard density so that things become more sparse as we go on

            gapOdds = Random.Range(Mathf.Lerp(gapOddsRange.y, gapOddsRange.x, Difficulty_Min),
                Mathf.Lerp(gapOddsRange.y, gapOddsRange.x, Difficulty_Max));

            length = Random.Range(lengthRange.x, lengthRange.y);

            float distance = rightX - leftX;
            float travelTime = distance / speed;
            interval = travelTime / hazardDensity;

            EffectLeft.SetActive(direction == Direction.Right);
            EffectRight.SetActive(direction == Direction.Left);
        }
        /*
        if (randomizeValues) {
            direction = Random.value < 0.5f ? Direction.Left : Direction.Right;
            speed = Random.Range(2.0f, 4.0f);
            length = Random.Range(2, 4);
            interval = length / speed + Random.Range(2.0f, 4.0f);
            EffectLeft.SetActive(direction == Direction.Right);
            EffectRight.SetActive(direction == Direction.Left);
        }*/

        elapsedTime = interval; //As we'll be pre-populating
        trunks = new List<GameObject>();

        prepopulateLine();
    }

    void prepopulateLine()
    {
        float distance = rightX - leftX;
        float travelTime = distance / speed;
        int hazardCount = Mathf.CeilToInt(travelTime / interval);

        for (int i = 0; i < hazardCount; i++)
        {
            float span = (float)i / (float)hazardCount;
            Vector3 position = transform.position + new Vector3(Mathf.Lerp(rightX, leftX, span) * (direction == Direction.Right ? 1f : -1f), 0, 0);
            SpawnLog(position);
        }
    }

    void SpawnLog(Vector3 position)
    {
        var o = (GameObject)Instantiate(trunkPrefab, position, Quaternion.identity);
        o.GetComponent<TrunkFloatingScript>().speedX = (int)direction * speed;

        var scale = o.transform.localScale;
        o.transform.localScale = new Vector3(scale.x * length, scale.y, scale.z * 3);

        trunks.Add(o);
    }

    public void Update() {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > interval) {
            elapsedTime = 0.0f;

            var position = transform.position + new Vector3(direction == Direction.Left ? rightX : leftX, 0, 0);
            SpawnLog(position);
        }

        foreach (var o in trunks.ToArray()) {
            if (direction == Direction.Left && o.transform.position.x < leftX || direction == Direction.Right && o.transform.position.x > rightX) {
                Destroy(o);
                trunks.Remove(o);
            }
        }
	}

    void OnDestroy() {
        foreach (var o in trunks) {
            Destroy(o);
        }
    }
}
