using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarScript : MonoBehaviour {
    public float speedX = 1.0f;

    public GameObject LeftShadow, RightShadow;
    public MeshRenderer VehicleMesh;

    private Dictionary<Material, Material> materialMap = new Dictionary<Material, Material>();
    public Material[] newMaterials;
    public Color carColor;


    void Start()
    {
        if (VehicleMesh) { CreateUniqueMaterials(); }
    }

    void CreateUniqueMaterials()
    {
        MeshRenderer meshRenderer = VehicleMesh.GetComponent<MeshRenderer>();
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

        DoSetMaterialColor(carColor);
    }

    public void SetSpeed(float newSpeed)
    {
        speedX = newSpeed;
        if (RightShadow) { RightShadow.SetActive(speedX > 0); }
        if (LeftShadow) { LeftShadow.SetActive(speedX < 0); }
    }

    public void SetMaterialColor(Color newColor)
    {
        carColor = newColor;
    }

    void DoSetMaterialColor(Color newColor)
    {
        for (int i = 0; i<newMaterials.Length; i++)
        {
            newMaterials[i].SetColor("_Color", newColor);
        }
    }

    public void Update() {
        transform.position += new Vector3(speedX * Time.deltaTime, 0.0f, 0.0f);
    }

    void OnTriggerEnter(Collider other) {
        // When collide with player, flatten it!
        if (other.gameObject.tag == "Player") {
            Vector3 scale = other.gameObject.transform.localScale;
            other.gameObject.transform.localScale = new Vector3(scale.x, scale.y * 0.1f, scale.z);
            //other.gameObject.SendMessage("GameOver");   //PROBLEM: Not the best way of doing this
            PlayerMovementScript PlayerMove = other.GetComponent<PlayerMovementScript>();
            if (PlayerMove)
            {
                PlayerMove.GameOver();
            }
        }
    }
}
