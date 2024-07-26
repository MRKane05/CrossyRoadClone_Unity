using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCoinDistributor : MonoBehaviour {
	public GameObject coinPrefab;
	float coinOdds = 0.15f; //Feels about correct :)
	public LayerMask stopperLayerMask;
	IEnumerator Start()
    {
		yield return null;
		DoCoinPopulate();
    }

	//We need to make sure this happens after we've done things like trees...
	void DoCoinPopulate () {
		if (Random.value < coinOdds)
		{
			int cycles = 5;
			bool bCleared = false;
			Vector3 position = Vector3.zero;
			while (!bCleared && cycles > 0)
            {
				position = new Vector3(Random.RandomRange(-9.5f, 9.5f), 0.77f, 0);
				if (!Physics.CheckSphere(position, 0.1f, stopperLayerMask))
                {
					bCleared = true;
                }
				cycles--;
            }
			GameObject newCoin = Instantiate(coinPrefab, transform);
			newCoin.transform.localScale = Vector3.one;
			newCoin.transform.localPosition = position;	//Who cares if it gets stuck in a tree or something
		}
	}
}
