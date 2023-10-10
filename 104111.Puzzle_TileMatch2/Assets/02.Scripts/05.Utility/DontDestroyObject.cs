using UnityEngine;
using System.Collections;

public class DontDestroyObject : MonoBehaviour {
    
    void Awake()
    {
        DontDestroyOnLoad (gameObject);
    }

    void OnDestroy()
    {
        Debug.Log("Destroy : [" + this.name + "]");
    }
}
