using UnityEngine;

public class ExplosionDestroy : MonoBehaviour
{
    private ParticleSystem ps;
    
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }
    
    void Update()
    {
        if (ps != null && !ps.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}