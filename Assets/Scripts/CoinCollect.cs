using UnityEngine;

public class CoinCollect : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip soundCoin;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Coin trigger hit by: " + other.name);

        if (!other.CompareTag("Player"))
            return;
        
        if (soundCoin != null){
            AudioSource.PlayClipAtPoint(soundCoin, transform.position);
        }

        Debug.Log("Collected coin!");

        if (CoinCounter.Instance != null)
            CoinCounter.Instance.Add(1);
        else
            Debug.LogError("CoinCounter.Instance is NULL (no CoinCounter in scene).");

        Destroy(gameObject);
    }
}
