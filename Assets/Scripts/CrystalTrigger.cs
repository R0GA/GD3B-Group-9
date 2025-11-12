using UnityEngine;
using System.Collections;

public class CrystalTrigger : MonoBehaviour
{
    public GameObject singleCrystal;
    public GameObject multipleCrystals;

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(CrystalFall());
    }
    private IEnumerator CrystalFall()
    {
        singleCrystal.SetActive(true);

        yield return new WaitForSeconds(2f);

        multipleCrystals.SetActive(true);
    }
}
