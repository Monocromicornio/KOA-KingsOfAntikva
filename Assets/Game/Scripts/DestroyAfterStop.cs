using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterStop : MonoBehaviour
{
    // Start is called before the first frame update

    AudioSource au;
    
    void Start()
    {
        au = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!au.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
