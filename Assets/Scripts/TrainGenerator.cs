using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainGenerator : MonoBehaviour
{
    public enum Direction { Left = -1, Right = 1 };

    public bool randomizeValues = false;

    public Direction direction;
    public float speed = 2.0f;
    public Vector2 intervalRange = new Vector2(6, 12);
    private float interval = 6f;
    public float leftX = -20.0f;
    public float rightX = 20.0f;

    public GameObject trainPrefab;

    private float nextTime;
    private bool bDoingTrain = false;

    private List<GameObject> cars;

    public GameObject railLine;

    private Dictionary<Material, Material> materialMap = new Dictionary<Material, Material>();
    Material[] newMaterials;

    public void Start()
    {
        if (randomizeValues)
        {
            direction = Random.value < 0.5f ? Direction.Left : Direction.Right;
            //speed = Random.Range(2.0f, 4.0f);
            interval = Random.Range(intervalRange.x, intervalRange.y);
        }

        nextTime = Time.time + interval;
        cars = new List<GameObject>();

        CreateUniqueLineMaterials();
    }

    public void Update()
    {
        if (Time.time > nextTime && !bDoingTrain)
        {
            bDoingTrain = true;
            StartCoroutine(DoTrainPass());
        }

        foreach (GameObject o in cars.ToArray())
        {
            if (direction == Direction.Left && o.transform.position.x < leftX || direction == Direction.Right && o.transform.position.x > rightX)
            {
                Destroy(o);
                cars.Remove(o);
            }
        }
    }

    void CreateUniqueLineMaterials()
    {
        MeshRenderer meshRenderer = railLine.GetComponent<MeshRenderer>();
        Material[] originalMaterials = meshRenderer.sharedMaterials;
        newMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            Material originalMaterial = originalMaterials[i];

            if (materialMap.ContainsKey(originalMaterial))
            {
                // If we've already created a new material instance, reuse it
                newMaterials[i] = materialMap[originalMaterial];
            }
            else
            {
                // Create a new material instance and store it in the dictionary
                Material newMaterial = new Material(originalMaterial);
                materialMap[originalMaterial] = newMaterial;
                newMaterials[i] = newMaterial;
            }
        }

        meshRenderer.materials = newMaterials;
    }

    void SetMaterialRed(float toThis)
    {
        if (railLine)
        {
            foreach (Material thisMat in newMaterials)
            {
                thisMat.SetFloat("_LightMix", toThis);
            }
        }
    }

    IEnumerator DoTrainPass() {
        SetMaterialRed(0.5f);
        yield return new WaitForSeconds(1f);
        

        // TODO extract 0.375f and -0.5f to outside -- probably along with genericization
        Vector3 position = transform.position + new Vector3(direction == Direction.Left ? rightX : leftX, 2.4f, 0);
        GameObject o = (GameObject)Instantiate(trainPrefab, position, Quaternion.identity); // Quaternion.Euler(-90, 90, 0));
        o.GetComponent<CarScript>().speedX = (int)direction * speed;

        if (direction < 0)
            o.transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            o.transform.rotation = Quaternion.Euler(0, 180, 0);

        cars.Add(o);

        yield return new WaitForSeconds(1f);
        SetMaterialRed(0f);
        nextTime = Time.time + interval;
    }

    public void OnDestroy()
    {
        foreach (GameObject o in cars)
        {
            Destroy(o);
        }
    }
}
