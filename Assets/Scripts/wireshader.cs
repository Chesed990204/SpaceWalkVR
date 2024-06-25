using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wireshader : MonoBehaviour
{
    void Start()
    {
        MeshRenderer m = this.GetComponent<MeshRenderer>();
        m.material.shader = Shader.Find("Assets/Wireframe/Shaders/Wireframe.shader");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
