using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomChildSelector : MonoBehaviour
{
    private void OnEnable()
    {
        int rndIndex = Random.Range(0, transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == rndIndex);
        }
    }
}
