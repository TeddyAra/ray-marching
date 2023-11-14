using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestComputeShaderScript : MonoBehaviour {
    // The compute shader and texture
    [SerializeField] private ComputeShader computeShader;
    private RenderTexture renderTexture;
    private int kernel;
    ComputeBuffer sphereBuffer;

    // The spheres in the scene
    public struct Sphere {
        public Vector3 position;
        public float radius;
        public Vector3 colour;
        public float pad;

        public Sphere(float x, float y, float z, float radius, float r, float g, float b) : this() {
            position = new Vector3(x, y, z);
            this.radius = radius;
            colour = new Vector3(r, g, b);
        }
    }

    void Awake() {
        // Finds kernel
        kernel = computeShader.FindKernel("CSMain");

        // Creates render texture and assigns it to the compute shader
        renderTexture = new RenderTexture(1920, 1080, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        computeShader.SetTexture(kernel, "Result", renderTexture);

        // Gets all spheres in a scene
        GameObject[] unitySpheres = GameObject.FindGameObjectsWithTag("Sphere");
        Sphere[] _spheres = new Sphere[unitySpheres.Length];

        if (unitySpheres.Length > 0) {
            for (int i = 0; i < _spheres.Length; i++) {
                _spheres[i].position = unitySpheres[i].transform.position;
                _spheres[i].radius = unitySpheres[i].transform.localScale.x / 2;

                Color colour = unitySpheres[i].GetComponent<MeshRenderer>().material.color;
                _spheres[i].colour = new Vector3(colour.r, colour.g, colour.b);
            }

            // Sends data over to the buffer
            sphereBuffer = new ComputeBuffer(_spheres.Length, sizeof(float) * 8);
            sphereBuffer.SetData(_spheres);
            computeShader.SetBuffer(kernel, "_spheres", sphereBuffer);
        }

        // Sends over variables
        computeShader.SetFloat("numSpheres", _spheres.Count());

        // Dispatches the shader
        computeShader.Dispatch(kernel, renderTexture.width / 8, renderTexture.height / 8, 1);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        computeShader.SetTexture(kernel, "Result", renderTexture);
        computeShader.Dispatch(kernel, renderTexture.width / 8, renderTexture.height / 8, 1);
        
        Graphics.Blit(renderTexture, dest);
    }

    private void OnApplicationQuit() {
        // Releases the buffer from memory
        sphereBuffer.Release();
    }
}