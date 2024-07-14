using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCoinDistributor : MonoBehaviour {
	public GameObject coinPrefab;
	float coinOdds = 0.2f;	//For the moment :)
	// Use this for initialization
	void Start () {
		if (Random.value < coinOdds)
		{
			GameObject newCoin = Instantiate(coinPrefab, transform);
			newCoin.transform.localScale = Vector3.one;
			newCoin.transform.localPosition = new Vector3(Random.RandomRange(-9.5f, 9.5f), 0.77f, 0);	//Who cares if it gets stuck in a tree or something
		}
	}
}
