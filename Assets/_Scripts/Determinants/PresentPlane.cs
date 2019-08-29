﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Determinants
{
    public class PresentPlane : MonoBehaviour
    {

        public Transform point1, point2, point3;
        public Transform centerPt;
        public TextMesh pt1Label, pt2Label, pt3Label;

        public Transform plane;
        public Transform forwardPlane;
        public Transform backwardPlane;
        public Transform lookAtTarget;
        public List<GameObject> walls;

        public PtManager ptManager;
        public PtManager2D ptManager2D; 
        private PtCoord rawPt1, rawPt2, rawPt3;

        public Vector3 center = new Vector3(0,0,0);
        //public Vector3 vector12, vector13, vector23; //TODO: remove these
        public Vector3 vector1, vector2, vector3;
        public Vector3 scaledPt1, scaledPt2, scaledPt3;
        public Vector3 scaledVector12, scaledVector13;
        //public Vector3 normalVector; // TAG
        public Vector3 scaledNormal;

        public List<Vector3> vertices;
        //public string rawEquation; //TAG

        AK.ExpressionSolver solver;
        AK.Expression expr;

        public AxisLabelManager xLabelManager;
        public AxisLabelManager yLabelManager;
        public AxisLabelManager zLabelManager;

        public float steps = 1.5f;
        public float dummySteps = 10;
        public float stepSize;
        public float defaultStepSize = 5;

        bool ptSetExist = false;

        public float d;

        void Awake()
        {
            solver = new AK.ExpressionSolver();
            expr = new AK.Expression();
            vertices = new List<Vector3>();
            stepSize = defaultStepSize;
            if (ptManager != null && ptManager2D == null){
                if (ptManager.ptSet != null)
                {
                    ptSetExist = true;
                    rawPt1 = ptManager.ptSet.ptCoords["pt1"];
                    rawPt2 = ptManager.ptSet.ptCoords["pt2"];
                    rawPt3 = ptManager.ptSet.ptCoords["pt3"];
                }
            } 
            if (ptManager == null && ptManager2D != null) {
                if (ptManager2D != null && ptManager2D.ptSet != null)
                {
                    ptSetExist = true;
                    rawPt1 = ptManager2D.ptSet.ptCoords["pt1"];
                    rawPt2 = ptManager2D.ptSet.ptCoords["pt2"];
                }
            }
        }

        void Update()
        {   
            if (ptManager!=null){
                if (!ptSetExist && ptManager.ptSet != null)
                {
                    ptSetExist = true;
                    rawPt1 = ptManager.ptSet.ptCoords["pt1"];
                    rawPt2 = ptManager.ptSet.ptCoords["pt2"];
                    rawPt3 = ptManager.ptSet.ptCoords["pt3"];
                }
            } else {
                if (!ptSetExist && ptManager2D != null && ptManager2D.ptSet != null)
                {
                    ptSetExist = true;
                    rawPt1 = ptManager2D.ptSet.ptCoords["pt1"];
                    rawPt2 = ptManager2D.ptSet.ptCoords["pt2"];
                }
            }
            plane.LookAt(lookAtTarget);

            if (pt3Label != null){
                pt1Label.text = "(" + rawPt1.X.Value + "," + rawPt1.Y.Value + "," + rawPt1.Z.Value + ")";
                pt2Label.text = "(" + rawPt2.X.Value + "," + rawPt2.Y.Value + "," + rawPt2.Z.Value + ")";
                pt3Label.text = "(" + rawPt3.X.Value + "," + rawPt3.Y.Value + "," + rawPt3.Z.Value + ")";
            }
            else{
                pt1Label.text = "(" + rawPt1.X.Value + "," + rawPt1.Y.Value + ")";
                pt2Label.text = "(" + rawPt2.X.Value + "," + rawPt2.Y.Value + ")";
            }
            //pt2Label.text = string.Format("({0:F3},{1:F3},{2:F3})", rawPt2.X.Value, rawPt2.Y.Value, rawPt2.Z.Value); //TAG

            /* //TAG
            var sharedMaterial = forwardPlane.GetComponent<MeshRenderer>().sharedMaterial; 
            sharedMaterial.SetInt("_planeClippingEnabled", 1);

            for (int i = 0; i < 6; i++)
            {
                sharedMaterial.SetVector("_planePos" + i, walls[i].transform.position);
                //plane normal vector is the rotated 'up' vector.
                sharedMaterial.SetVector("_planeNorm" + i, walls[i].transform.rotation * Vector3.up);
            }

            sharedMaterial = backwardPlane.GetComponent<MeshRenderer>().sharedMaterial;
            sharedMaterial.SetInt("_planeClippingEnabled", 1);

            for (int i = 0; i < 6; i++)
            {
                sharedMaterial.SetVector("_planePos" + i, walls[i].transform.position);
                //plane normal vector is the rotated 'up' vector.
                sharedMaterial.SetVector("_planeNorm" + i, walls[i].transform.rotation * Vector3.up);
            }
            */
        }

        public void ApplyGraphAdjustment()
        {
            //center = new Vector3(0, 0, 0); defined globally above
            
            //TODO: modify graph adjustments - need to incorporate flipped rows vs columns
            float v1 = PtCoordToVector(rawPt1).magnitude;
            float v2 = PtCoordToVector(rawPt2).magnitude;
            if (rawPt3 != null){
                float v3 = PtCoordToVector(rawPt3).magnitude;
                stepSize = Mathf.Max(v1, v2, v3);
            } else{
                stepSize = Mathf.Max(v1, v2);
            }
            
            //PtCoord centerPt = new PtCoord(new AxisCoord(centerX), new AxisCoord(centerY), new AxisCoord(centerZ));
            //Get the range of the box
            if (stepSize == 0)
            {
                stepSize = defaultStepSize;
            }

            xLabelManager.Min = center.x - stepSize * steps;
            yLabelManager.Min = center.y - stepSize * steps;
            if (zLabelManager != null){
                zLabelManager.Min = center.z - stepSize * steps; // Z 
            }
            xLabelManager.Max = center.x + stepSize * steps;
            yLabelManager.Max = center.y + stepSize * steps;
            if (zLabelManager != null){
                zLabelManager.Max = center.z + stepSize * steps; //Z
            }
            //Get the interaction points between the box edges and the plane
            //expr = solver.SymbolicateExpression(rawEquation);
        }

        public void ApplyUnroundCenter(string ptName, Vector3 newLoc) //TAG - changes origin location, so this should be removed
        {
            /* 
            if (ptName.Equals("pt1")) center = (newLoc + PtCoordToVector(rawPt2) + PtCoordToVector(rawPt3)) / 3;
            else if (ptName.Equals("pt2")) center = (PtCoordToVector(rawPt1) + newLoc + PtCoordToVector(rawPt3)) / 3;
            else if (ptName.Equals("pt3")) center = (PtCoordToVector(rawPt1) + PtCoordToVector(rawPt2) + newLoc) / 3;
            */
        }

        public Vector3 GenerateVector(PtCoord pt1, PtCoord pt2)
        {
            Vector3 result = Vector3.zero;
            result.x = pt2.X.Value - pt1.X.Value;
            result.y = pt2.Y.Value - pt1.Y.Value;
            result.z = pt2.Z.Value - pt1.Z.Value;
            return result;
        }

        public float DeterminantTwoD(float a, float b, float c, float d){
            return a*d-b*c;
        }


        // Return the determinant calculation
        public bool CalculatePlane()  //previously returned bool
        {
            if (ptManager!=null){
                rawPt1 = ptManager.ptSet.ptCoords["pt1"];
                rawPt2 = ptManager.ptSet.ptCoords["pt2"];
                rawPt3 = ptManager.ptSet.ptCoords["pt3"];
                
                // Calculating the determinant
                float determinant = rawPt1.X.Value*DeterminantTwoD(rawPt2.Y.Value,rawPt3.Y.Value,rawPt2.Z.Value,rawPt3.Z.Value)
                                    - rawPt2.X.Value*DeterminantTwoD(rawPt1.Y.Value,rawPt3.Y.Value,rawPt1.Z.Value,rawPt3.Z.Value)
                                    + rawPt3.X.Value*DeterminantTwoD(rawPt1.Y.Value,rawPt2.Y.Value,rawPt1.Z.Value,rawPt2.Z.Value);
                
                ptManager.updateEqn(determinant, 0f, 0f, 0f);
                
            } else {
                rawPt1 = ptManager2D.ptSet.ptCoords["pt1"];
                rawPt2 = ptManager2D.ptSet.ptCoords["pt2"];
                float determinant = DeterminantTwoD(rawPt1.X.Value, rawPt2.X.Value, rawPt1.Y.Value, rawPt2.Y.Value);
                ptManager2D.updateEqn(determinant, 0f, 0f, 0f);
            }
            
            //TAG
            //ptManager.updateEqn(-3.33f, -2.22f, -1.11f, rawPt1.X.Value);
            //rawPt1.X.Value * normalVector.x + rawPt1.Y.Value * normalVector.y + rawPt1.Z.Value * normalVector.z;

            //vector1 = GenerateVector(center, rawPt1);
            //vector2 = GenerateVector(center, rawPt2);
            //vector3 = GenerateVector(center, rawPt3);
            //Debug.Log("Vector 12 is: " + vector12 +". Vector13 is: " + vector13);
            /* normalVector = Vector3.Cross(vector12, vector13);
            if (PlaneValid())
            {
                //forwardPlane.GetComponent<MeshRenderer>().enabled = true;  TM edit
                //backwardPlane.GetComponent<MeshRenderer>().enabled = true;  TM edit

                // Basic formula of the equation
                d = rawPt1.X.Value * normalVector.x + rawPt1.Y.Value * normalVector.y + rawPt1.Z.Value * normalVector.z;
                // string[] formattedValue = roundString(new float[] {normalVector.x, normalVector.y, normalVector.z});
                // // Formatting equation
                // if (formattedValue[1][0] != '-') formattedValue[1] = '+' + formattedValue[1];
                // if (formattedValue[2][0] != '-') formattedValue[2] = '+' + formattedValue[2];
                // rawEquation = formattedValue[0] + "x" + formattedValue[1] + "y" + formattedValue[2] + "z=" + d;
                ptManager.updateEqn(normalVector.x, normalVector.y, normalVector.z, d);
                return true;
            }
            else
            {
                forwardPlane.GetComponent<MeshRenderer>().enabled = false;
                backwardPlane.GetComponent<MeshRenderer>().enabled = false;
                // rawEquation = "Invalid Plane";
                ptManager.updateEqn();
                return false;
            } */

            return true;

            //Debug.Log("Normal vector is: " + normalVector);
        }

        /* //TAG 
        public string[] roundString(float[] input)
        {
            string[] result = new string[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                string a = input[i].ToString();
                string b = string.Format("{0:0.000}", input[i]);
                print("comparing: " + a + "  " + b);
                if (a.Length <= b.Length)
                {
                    result[i] = a;
                }
                else
                {
                    result[i] = b;
                }
            }
            return result;
        }
        */

        public void GetLocalPoint()
        {
            scaledPt1 = ScaledPoint(PtCoordToVector(rawPt1));
            point1.localPosition = scaledPt1;
            scaledPt2 = ScaledPoint(PtCoordToVector(rawPt2));
            point2.localPosition = scaledPt2;
            if (rawPt3 != null && point3 != null){
                scaledPt3 = ScaledPoint(PtCoordToVector(rawPt3));
                point3.localPosition = scaledPt3;
            }
            //centerPt.localPosition = ScaledPoint(center);
        }

        public void GetPlaneDirection()
        {
            if (point3 != null) 
            {
                scaledVector12 = point2.localPosition - point1.localPosition;
                scaledVector13 = point3.localPosition - point1.localPosition;
                scaledNormal = Vector3.Cross(scaledVector12, scaledVector13);
                float scale = dummySteps * stepSize / scaledNormal.magnitude;
                Vector3 dummyPos = scaledNormal * scale;
                //Debug.Log("The Normal vector after scale is: " + dummyPos);
                lookAtTarget.localPosition = dummyPos + ScaledPoint(center);
                centerPt.localPosition = ScaledPoint(center);
                plane.localPosition = ScaledPoint(center);
            }
        }

        /*  //TAG
        public bool PlaneValid()
        {
            // no points are the same
            if (PtCoordToVector(rawPt1) == PtCoordToVector(rawPt2) || PtCoordToVector(rawPt1) == PtCoordToVector(rawPt3) || PtCoordToVector(rawPt2) == PtCoordToVector(rawPt3))
            {
                return false;
            }
            vector12 = GenerateVector(rawPt1, rawPt2);
            vector13 = GenerateVector(rawPt1, rawPt3);
            // points are not in same line
            float scale;
            if (vector13.x != 0)
            {
                scale = vector12.x / vector13.x;
            }
            else if (vector13.y != 0)
            {
                scale = vector12.y / vector13.y;
            }
            else
            {
                scale = vector12.z / vector13.z;
            }
            Vector3 temp = vector13 * scale;
            if (vector12.Equals(temp))
            {
                return false;
            }

            return true;
        }
        */

        public Vector3 PtCoordToVector(PtCoord pt)
        {
            return (new Vector3(pt.Y.Value, pt.X.Value, pt.Z.Value));
        }

        public Vector3 ScaledPoint(Vector3 pt)
        {
            Vector3 result = Vector3.zero;
            //print("raw pt1 position: " + pt);
            result.z = (pt.x - xLabelManager.Min) / (xLabelManager.Max - xLabelManager.Min) * 20 - 10;
            result.x = (pt.y - yLabelManager.Min) / (yLabelManager.Max - yLabelManager.Min) * 20 - 10;
            if (zLabelManager != null){
                result.y = (pt.z - zLabelManager.Min) / (zLabelManager.Max - zLabelManager.Min) * 20 - 10;
            }
            return result;
        }

        public Vector3 UnscaledPoint(Vector3 pt)
        {
            Vector3 result = Vector3.zero;
            print("raw pt1 position: " + pt);
            result.x = (pt.z + 10) / 20 * (xLabelManager.Max - xLabelManager.Min) + xLabelManager.Min;
            result.y = (pt.x + 10) / 20 * (yLabelManager.Max - yLabelManager.Min) + yLabelManager.Min;
            if (zLabelManager != null){
                result.z = (pt.y + 10) / 20 * (zLabelManager.Max - zLabelManager.Min) + zLabelManager.Min;
            }
            return result;
        }
    }

}
