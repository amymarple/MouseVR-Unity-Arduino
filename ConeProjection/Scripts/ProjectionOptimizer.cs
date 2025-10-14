using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static ProjectionOptimizer;


public static class ProjectionOptimizer
{

    static float[] gradient_weights = { 1f, 1f, 1f, 10.0f, 10.0f, 1f, 10f, 1f, 1f, 0f};

    public static Vector2 [] gradientDescentStep(Vector2[] worldPointData, Vector2 [] uvPoints, float[] variables, float learning_rate, float[] importance)
    {
        Vector2[] uvPointEstimates=null;
        float loss = 0;
        for (int step_number = 0; step_number < 1000000; step_number++)
        {
            (float[] gradient, Vector2 [] b, float c) = computeGradient(worldPointData, uvPoints, variables, importance);
            loss = c;
            uvPointEstimates = b; 
            for (int i = 0; i < gradient.Length; i++)
            {
                variables[i] += -learning_rate * gradient[i]*gradient_weights[i];
            }

            variables[0] = variables[0] % ((float) (2.0 * Math.PI));
            variables[1] = variables[1] % ((float) (2.0 * Math.PI));

            if(step_number % 10000 == 0)
            {
                Debug.Log(loss);
            }

        }

        Debug.Log(loss);

        return uvPointEstimates;
    }


    public static (float [], Vector2[], float) computeGradient(Vector2[] worldPointData, Vector2 [] uvPoints, float[] variables, float[] importance)
    {
        float Th1 = variables[0];
        float Th2 = variables[1];
        float Ph = variables[2];
        float Tau = variables[3];
        float D =  variables[4];
        float x3 = variables[5];
        float beta = variables[6];
        float cone_half_angle = variables[7];
        float distance_to_top = variables[8];
        float distance_to_bottom = variables[9];

        float tan_angle = Mathf.Tan(cone_half_angle);
        float sec_angle =1.0f / Mathf.Cos(cone_half_angle);
        float sec_angle_2 = sec_angle * sec_angle;

        float sTh = (float)Math.Sin(Th1);
        float cTh = (float)Math.Cos(Th1);

        float sTh12 = (float)Math.Sin(Th1 + Th2);
        float cTh12 = (float)Math.Cos(Th1 + Th2);

        float sPh = (float)Math.Sin(Ph);
        float cPh = (float)Math.Cos(Ph);
        float sTau = (float)Math.Sin(Tau);
        float cTau = (float)Math.Cos(Tau);

        Vector3 z = new Vector3(0, 0, 0);

        Vector3 U = new Vector3(-(cTh12 * sPh * sTau) - cTau * sTh12, cTau * cTh12 - sPh * sTau * sTh12, cPh * sTau);
        Vector3 V = new Vector3(-(cTau * cTh12 * sPh) + sTau * sTh12, -(cTh12 * sTau) - cTau * sPh * sTh12, cPh * cTau);
        Vector3 F = new Vector3(-(cPh * cTh12), -(cPh * sTh12), -sPh);
        Vector3 X = new Vector3(D * cTh, D * sTh, x3);

        // Gradients of the basis vectors:

        Vector3[] dU = {
            new Vector3(-(cTau * cTh12) + sPh * sTau * sTh12, -(cTh12 * sPh * sTau) - cTau * sTh12, 0),
            new Vector3(-(cTau * cTh12) + sPh * sTau * sTh12, -(cTh12 * sPh * sTau) - cTau * sTh12, 0),
            new Vector3(-(cPh * cTh12 * sTau), -(cPh * sTau * sTh12), -(sPh * sTau)),
            new Vector3(-(cTau * cTh12 * sPh) + sTau * sTh12, -(cTh12 * sTau) - cTau * sPh * sTh12, cPh * cTau),
            z, z, z, z, z, z
        };

        Vector3[] dV = {
            new Vector3(cTh12*sTau + cTau*sPh*sTh12, -(cTau*cTh12*sPh) + sTau*sTh12, 0),
            new Vector3(cTh12*sTau + cTau*sPh*sTh12, -(cTau*cTh12*sPh) + sTau*sTh12, 0),
            new Vector3(-(cPh*cTau*cTh12), -(cPh*cTau*sTh12), -(cTau*sPh)),
            new Vector3(cTh12*sPh*sTau + cTau*sTh12, -(cTau*cTh12) + sPh*sTau*sTh12, -(cPh*sTau)),
            z, z, z, z, z, z
        };

        Vector3[] dF = {
            new Vector3(cPh * sTh12, -(cPh * cTh12), 0),
            new Vector3(cPh * sTh12, -(cPh * cTh12), 0), 
            new Vector3(cTh12 * sPh, sPh * sTh12, -cPh), 
            new Vector3(0, 0, 0),
            z, z, z, z, z, z
        };

        Vector3[] dX = {
            new Vector3(-D * sTh, D * cTh, 0),
            z, z, z, z,
            new Vector3(0, 0, 1), z, z, z, z
        };

        float[] dbeta = { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0};

        int n = variables.Length;
        float[] gradient = new float[n];
        for(int i=0; i < n; i++)
        {
            gradient[i] = 0;
        }

        float[] dEq1 = new float[n];
        float[] dEq2 = new float[n];

        float loss = 0;
        float importance_total = 0f;
        for(int i = 0; i < importance.Length; i++)
        {
            importance_total += importance[i];
        }

        for (int i = 0; i < importance.Length; i++)
        {
            importance[i] = importance[i]/importance_total;
        }

        for (int i=0; i < worldPointData.Length; i++)
        {
            float world_angle = worldPointData[i].x*(2.0f*Mathf.PI/360.0f);
            float fraction = worldPointData[i].y;
            float h = -distance_to_top - fraction * distance_to_bottom;
            Vector3 Q = new Vector3(Mathf.Cos(world_angle) * (-h) * tan_angle, Mathf.Sin(world_angle) * (-h) * tan_angle, h);

            Vector3[] dQ =
            {
                z,z, z, z, z, z, z,
                new Vector3(Mathf.Cos(world_angle) * (-h) * sec_angle_2, Mathf.Sin(world_angle) * (-h) * sec_angle_2, h),
                new Vector3(Mathf.Cos(world_angle)  * tan_angle, Mathf.Sin(world_angle)  * tan_angle, -1),
                new Vector3(Mathf.Cos(world_angle) * (fraction) * tan_angle, Mathf.Sin(world_angle) * (fraction) * tan_angle, -fraction)
            };

            Vector2 uv = uvPoints[i];

            float u = uv.x;
            float v = uv.y;

            float r = 1.0f/(beta * Vector3.Dot(F, Q - X));

            float u_est = Vector3.Dot(U, Q - X) * r;
            float v_est = Vector3.Dot(V, Q - X) * r;


            loss += 0.5f * ((u_est - u) * (u_est - u) + (v_est - v) * (v_est - v));



            for (int j = 0; j < n; j++)
            {

                float denom_deriv = dbeta[j] * Vector3.Dot(F, Q - X) + beta * Vector3.Dot(dF[j], Q - X) + beta * Vector3.Dot(F, dQ[j] - dX[j]);

                dEq1[j] = (Vector3.Dot(dU[j], Q - X) + Vector3.Dot(U, dQ[j] - dX[j])) * r - Vector3.Dot(U, Q - X) * denom_deriv * r * r;
                dEq2[j] = (Vector3.Dot(dV[j], Q - X) + Vector3.Dot(V, dQ[j] - dX[j])) * r - Vector3.Dot(V, Q - X) * denom_deriv * r * r;


                dEq1[j] = Vector3.Dot(dU[j], Q - X) + Vector3.Dot(U, dQ[j] - dX[j]) - dbeta[j] * u * Vector3.Dot(F, Q - X) - beta * u * Vector3.Dot(dF[j], Q - X) - beta * u * Vector3.Dot(F, dQ[j] - dX[j]);
                dEq2[j] = Vector3.Dot(dV[j], Q - X) + Vector3.Dot(V, dQ[j] - dX[j]) - dbeta[j] * v * Vector3.Dot(F, Q - X) - beta * v * Vector3.Dot(dF[j], Q - X) - beta * v * Vector3.Dot(F, dQ[j] - dX[j]);

                gradient[j] += (dEq1[j] * (u_est - u) + dEq2[j] * (v_est - v))*importance[i];
            }

        }

        Vector2[] uvPointEstimates = new Vector2[uvPoints.Length];

        for(int i = 0; i < uvPointEstimates.Length; i++)
        {
            float world_angle = worldPointData[i].x * (2.0f * Mathf.PI / 360.0f);
            float fraction = worldPointData[i].y;
            float h = -distance_to_top - fraction * distance_to_bottom;
            Vector3 Q = new Vector3(Mathf.Cos(world_angle) * (-h) * tan_angle, Mathf.Sin(world_angle) * (-h) * tan_angle, h);

            float u = Vector3.Dot(U, Q - X)/(beta * Vector3.Dot(F, Q - X));
            float v = Vector3.Dot(V, Q - X)/(beta * Vector3.Dot(F, Q - X));
            uvPointEstimates[i] = new Vector2(u, v);
        }

        return (gradient, uvPointEstimates, loss);
    }

    public static (Vector2[], float) getUVLocations(Vector3[] worldPoints, Vector2[] uvPoints, float[] variables)
    {
        float Th1 = variables[0];
        float Th2 = variables[1];
        float Ph = variables[2];
        float Tau = variables[3];
        float D = variables[4];
        float x3 = variables[5];
        float beta = variables[6];

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
        Vector3 X = new Vector3(D * cTh, D * sTh, x3);

        float loss = 0;
        Vector2[] uvPointEstimates = new Vector2[uvPoints.Length];

        for (int i = 0; i < uvPointEstimates.Length; i++)
        {
            Vector3 Q = worldPoints[i];
            Vector2 uv = uvPoints[i];

            float u = uv.x;
            float v = uv.y;

            float Eq1 = Vector3.Dot(U, Q - X) - beta * u * Vector3.Dot(F, Q - X);
            float Eq2 = Vector3.Dot(V, Q - X) - beta * v * Vector3.Dot(F, Q - X);

            float u_est = Vector3.Dot(U, Q - X) / (beta * Vector3.Dot(F, Q - X));
            float v_est = Vector3.Dot(V, Q - X) / (beta * Vector3.Dot(F, Q - X));

            loss += 0.5f * ((u_est - u) * (u_est - u) + (v_est - v) * (v_est - v));

            uvPointEstimates[i] = new Vector2(u_est, v_est);
        }

        return (uvPointEstimates, loss);
    }
}
