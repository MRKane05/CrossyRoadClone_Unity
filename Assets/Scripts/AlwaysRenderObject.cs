using UnityEngine;

public class AlwaysRenderObject : MonoBehaviour
{
    // The large bounds size to ensure the object is always rendered
    private Vector3 largeBoundsMin = new Vector3(-10000, -10000, -10000);
    private Vector3 largeBoundsMax = new Vector3(10000, 10000, 10000);

    void Start()
    {
        SetLargeBounds();
    }

    // Function to set the bounds of the target object to a large value
    void SetLargeBounds()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Create a new bounds object with large size
            Bounds newBounds = new Bounds();
            newBounds.SetMinMax(largeBoundsMin, largeBoundsMax);

            // Apply the new bounds to the renderer's local bounds
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.mesh.bounds = newBounds;
                Debug.Log("Large bounds set to ensure the object is always rendered.");
            }
            else
            {
                Debug.LogError("No MeshFilter component found on the target object.");
            }
        }
        else
        {
            Debug.LogError("No Renderer component found on the target object.");
        }
    }
}