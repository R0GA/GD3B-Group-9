using UnityEngine;
using System.Collections;

public class CrystalTrigger : MonoBehaviour
{
    public GameObject singleCrystal;
    public GameObject multipleCrystals;

    private AudioSource crystalAudio;

    private void Start()
    {
        crystalAudio.GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(CrystalFall());
    }
    private IEnumerator CrystalFall()
    {
        crystalAudio.Play();
        singleCrystal.SetActive(true);

        yield return new WaitForSeconds(2f);

        multipleCrystals.SetActive(true);

        yield return new WaitForSeconds(1f);
        crystalAudio.Stop();
    }
}
