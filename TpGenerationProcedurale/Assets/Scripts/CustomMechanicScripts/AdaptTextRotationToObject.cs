using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptTextRotationToObject : MonoBehaviour
{

    public Transform OtherObject;
    private RectTransform UITransform;

    private void Start()
    {
        UITransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (-OtherObject.rotation.z == UITransform.rotation.z) return;

        Vector3 newRotation = UITransform.eulerAngles;
        newRotation.z = -OtherObject.rotation.z;
        UITransform.eulerAngles = newRotation;
    }
}
