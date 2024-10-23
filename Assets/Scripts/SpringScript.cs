using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringScript : MonoBehaviour
{
    public Transform object1, object2;

    // Start is called before the first frame update
    void Start()
    {
        this.setPosition();
    }

    // Update is called once per frame
    void Update()
    {
        this.setPosition();
    }

    void setPosition()
    {
        transform.position = (object1.position + object2.position) / 2;

        var scale = transform.localScale;
        scale.z = Vector3.Distance(object1.position, object2.position) / 2;
        transform.localScale = scale;

        transform.LookAt(object1);
    }
}
