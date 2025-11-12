using System.Collections;
using UnityEngine;

public class LavaGeyser : MonoBehaviour
{
    public GameObject lavaProjectile;
    public Transform firePoint;
    public float fireForce = 10f;
    public float fireInterval = 3f;
    public AudioSource fireSound;

    private void Start()
    {
        StartCoroutine(FireLava());
    }

    private IEnumerator FireLava()
    {
        while (true)
        {
            
            yield return new WaitForSeconds(fireInterval);

            GameObject lava = Instantiate(lavaProjectile, firePoint.position, firePoint.rotation);
            Rigidbody rb = lava.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(firePoint.up * fireForce, ForceMode.Impulse);

            fireSound.Play();
        }
    }
}
