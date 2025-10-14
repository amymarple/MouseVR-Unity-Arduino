using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]

public class MakeCubeMap : MonoBehaviour
{

    public RenderTexture CubeMap;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Capture();
    }

    void Capture()
    {
        gameObject.GetComponent<Camera>().RenderToCubemap(CubeMap, 63, Camera.MonoOrStereoscopicEye.Left);
    }
}
