using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ImportedComponent]
public class SpinTime_Roller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(13 * Time.deltaTime, 0f, 0f, Space.Self);
    }
}
