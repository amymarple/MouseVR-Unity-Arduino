using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[ExecuteInEditMode]
public class MakeConeProjection : MonoBehaviour
{

    // Variables for Compute Shader

    public ComputeShader computeShader;
    public RenderTexture CubeMap;
    public Material material;
    public RenderTexture output_normals;

    int x_resolution = 1980;
    int y_resolution = 1020;

    public Vector3 Z = new Vector3(0, 0, 0);

    //public GameObject[] WorldMarkers;
    public GameObject[] UVMarkers;
    public GameObject[] estimatedUVMarkers;
    public float[] importance;
    public Vector2 [] worldPointData;


    public float projector_distance = 4f;
    public float projector_theta_1 = 0f;
    public float projector_theta_2 = 0f;
    public float projector_phi = 0f;
    public float projector_tau = 0f;
    public float projector_scale = 0.0f;
    public float projector_height = 2f;

    public float cone_distance_to_top_row = 3.0f;
    public float cone_dinstance_to_second_row = 8.25f;
    public float cone_half_angle = 38.9f; // half angle of the cone in degrees

    public float cone_rescale = 0.0f;

    static private float s = 2 * Mathf.PI / (360.0f);
    public float learning_rate = 0.0f;

    public float loss = 0.0f;

    private int kernel;
    uint threadGroupSize_x;
    uint threadGroupSize_y;
    private Dictionary<string, int> varID;

    // Variables for cubeMap
   
    Camera targetCamera;

    // Start is called before the first frame update
    void Start()
    {
        targetCamera = gameObject.GetComponent<Camera>();

        initializeComputeShader();
    }

    void initializeComputeShader()
    {
        List<string> computeShaderVariableNames = new List<string>() {
            "x_resolution",
            "y_resolution",
            "beta",
            "tan_alpha",
            "X",
            "U",
            "V",
            "F",
            "Z",
            "Result",
            "InputImage",
            "cone_rescale"
        };

        varID = new Dictionary<string, int>();

        foreach (string s in computeShaderVariableNames)
        {
            varID.Add(s, Shader.PropertyToID(s));
        }

        output_normals = new RenderTexture(x_resolution, y_resolution, 0, RenderTextureFormat.ARGBFloat);
        output_normals.enableRandomWrite = true;
        output_normals.Create();


        material.SetTexture(Shader.PropertyToID("_MainTex"), output_normals);
        material.SetTexture(Shader.PropertyToID("_Cube"), CubeMap);

        gameObject.GetComponent<Camera>().pixelRect = new Rect(0, 0, 1920, 1080);

        //program we're executing
        kernel = computeShader.FindKernel("ConeProjection");
        computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSize_x, out threadGroupSize_y, out _);
    }

    // Update is called once per frame
    void Update()
    {
        if(varID == null)
        {
            initializeComputeShader();
        }

        SetGlobals();
        computeShader.SetTexture(kernel, varID["Result"], output_normals);
        computeShader.Dispatch(kernel, (int) x_resolution/ (int) threadGroupSize_x, (int)y_resolution/(int)threadGroupSize_y, 1);


        updateUVEstimates();
    }

    public void updateUVEstimates()
    {
        (Vector3[] worldPoints, Vector2[] uvPoints, float[] variables) = getVariables();

        (Vector2[] uvEstimates, float l) = ProjectionOptimizer.getUVLocations(worldPoints, uvPoints, variables);
        loss = l;
        for (int i = 0; i < uvEstimates.Length; i++)
        {
            Vector3 v = estimatedUVMarkers[i].transform.position;
            estimatedUVMarkers[i].transform.position = new Vector3(uvEstimates[i].x * 9.0f, v.y, uvEstimates[i].y * 9.0f);
        }
    }

    public (Vector3 [], Vector2[], float[]) getVariables()
    {
        Vector3[] worldPoints = new Vector3[worldPointData.Length];

        float r = Mathf.Tan(cone_half_angle*MakeConeProjection.s);
        for (int i = 0; i < worldPointData.Length; i++)
        {
            float h = -cone_distance_to_top_row - worldPointData[i].y * cone_dinstance_to_second_row;
            float th = worldPointData[i].x * MakeConeProjection.s;
            worldPoints[i] = new Vector3(Mathf.Cos(th) * Mathf.Abs(h) * r, Mathf.Sin(th) * Mathf.Abs(h) * r, h);
        }
        
        Vector2[] uvPoints = new Vector2[worldPoints.Length];
        for (int i = 0; i < UVMarkers.Length; i++)
        {
            uvPoints[i] = new Vector2(UVMarkers[i].transform.position.x / 9.0f, UVMarkers[i].transform.position.z / 9.0f);

        }
        float s = 2 * ((float)Math.PI) / (360.0f);
        float[] variables = new float[10];
        variables[0] = projector_theta_1 * s;
        variables[1] = projector_theta_2 * s;
        variables[2] = projector_phi * s;
        variables[3] = projector_tau * s;
        variables[4] = projector_distance;
        variables[5] = projector_height;
        variables[6] = (float)Math.Exp(-((float)(projector_scale / 10.0f)));
        variables[7] = cone_half_angle * s;
        variables[8] = cone_distance_to_top_row;
        variables[9] = cone_dinstance_to_second_row;

        return (worldPoints, uvPoints, variables);
    }

    public void setProjectionParametersFromVariables(float [] variables)
    {
        float s = 2 * ((float)Math.PI) / (360.0f);

        projector_theta_1 = variables[0] / s;
        projector_theta_2 = variables[1] / s;
        projector_phi = variables[2] / s;
        projector_tau = variables[3] / s;
        projector_distance = variables[4];
        projector_height = variables[5];
        projector_scale = -10.0f * ((float)Math.Log(variables[6]));
        cone_half_angle = variables[7] / s;
        cone_distance_to_top_row = variables[8];
        cone_dinstance_to_second_row = variables[9];
    } 

    public void optimizationStep()
    {
        (Vector3[] worldPoints, Vector2[] uvPoints, float[] variables) = getVariables();

        Vector2 [] uvEstimates = ProjectionOptimizer.gradientDescentStep(worldPointData, uvPoints, variables, learning_rate, importance);
        for(int i = 0; i < uvEstimates.Length; i++)
        {
            Vector3 v = estimatedUVMarkers[i].transform.position;
            estimatedUVMarkers[i].transform.position = new Vector3(uvEstimates[i].x * 9.0f, v.y, uvEstimates[i].y * 9.0f);
        }

        setProjectionParametersFromVariables(variables);
    }


    void SetFloat3(int ID, Vector3 v)
    {
        float[] vec = new float[3] { v.x, v.y, v.z };
        computeShader.SetFloats(ID, vec);
    }

    void SetGlobals()
    {

        float beta = (float)Math.Exp(-projector_scale / 10.0);
        float s = 2 * Mathf.PI / 360.0f;


        float Ph = projector_phi * s;
        float Th1 = projector_theta_1 * s;
        float Th2 = projector_theta_2 * s;
        float Tau = projector_tau * s;

        float sTh = (float)Math.Sin(Th1);
        float cTh = (float)Math.Cos(Th1);

        float sTh12 = (float)Math.Sin(Th1 + Th2);
        float cTh12 = (float)Math.Cos(Th1 + Th2);

        float sPh = (float)Math.Sin(Ph);
        float cPh = (float)Math.Cos(Ph);
        float sTau = (float)Math.Sin(Tau);
        float cTau = (float)Math.Cos(Tau);

        Vector3 U = new Vector3(-(cTh12 * sPh * sTau) - cTau * sTh12, cTau * cTh12 - sPh * sTau * sTh12, cPh * sTau);
        Vector3 V = new Vector3(-(cTau * cTh12 * sPh) + sTau * sTh12, -(cTh12 * sTau) - cTau * sPh * sTh12, cPh * cTau);
        Vector3 F = new Vector3(-(cPh * cTh12), -(cPh * sTh12), -sPh);

        Vector3 X = new Vector3(projector_distance * cTh, projector_distance * sTh, projector_height);


        computeShader.SetInt(varID["x_resolution"], x_resolution);
        computeShader.SetInt(varID["y_resolution"], y_resolution);

        computeShader.SetFloat(varID["beta"], beta);
        computeShader.SetFloat(varID["cone_rescale"], cone_rescale);

        computeShader.SetFloat(varID["tan_alpha"], Mathf.Tan(cone_half_angle / 360.0f * Mathf.PI * 2));

        SetFloat3(varID["X"], X);
        SetFloat3(varID["U"], U);
        SetFloat3(varID["V"], V);
        SetFloat3(varID["F"], F);
        SetFloat3(varID["Z"], Z);
        
    }
}
