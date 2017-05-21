using UnityEngine;
using System.Collections;

public class onHit : MonoBehaviour
{

    float startRotate;
    bool isHit = false;
    public float downTime, swingSpeed;

    void Awake()
    {
        startRotate = transform.localEulerAngles.x;
    }

    public void Hit()
    {

        StartCoroutine(Spin());
    }

    IEnumerator Spin()
    {
        if (!isHit)
        {
            isHit = true;

            for (int i = 0; i < swingSpeed; i++)
            {
                transform.Rotate(new Vector3(90 / swingSpeed, 0, 0));
                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForSeconds(downTime);

            for (int i = 0; i < swingSpeed; i++)
            {
                transform.Rotate(new Vector3(-90 / swingSpeed, 0, 0));
                yield return new WaitForFixedUpdate();
            }


            isHit = false;
            yield break;
        }
        else
            yield break;
    }
}
