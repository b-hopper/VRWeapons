using UnityEngine;
using System.Collections;

public class timeTilDeath : MonoBehaviour {

    public float timeTillDeath;
    
	IEnumerator Start () {
        yield return new WaitForSeconds(timeTillDeath);
        Destroy(gameObject);
    }
    
}
