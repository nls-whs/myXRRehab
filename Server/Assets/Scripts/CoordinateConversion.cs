using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Astra;

public  class CoordinateConversion : MonoBehaviour
{
	
	private Vector3 TO, TH, NO, NH = Vector3.zero;
	private Vector3 P1O_new, P2O_new, P3O_new, P1H_new, P2H_new, P3H_new = Vector3.zero;
	private Vector3 V1O, V2O,YO,ZO, V1H, V2H, YH,ZH = Vector3.zero;
	bool _isCalculated = false;
	private const float OB_SCALE = 0.001f;
	private const float HL_SCALE = 1f;

	Matrix4x4 RO, RH, RO_H;
	Matrix4x4 m_NO, m_NH, NH_Inv, final_matrix;
	
	

	void Update()
	{
		if (Calibration.instance._isFinishClicked == true)
		{
			if (Calibration.instance.P1O != Vector3.zero)
			{
				//calculate T(O) = (-P1.x, -P1.y, -P1.z)
				TO = CalculateTranslation(Calibration.instance.P1O);
				FindNormalVector("OB", TO);
				FindNormalizedMatrix("OB", NO);
				FindNewPoints("OB", Calibration.instance.P1O, Calibration.instance.P2O, Calibration.instance.P3O, TO);
				FormVectors("OB");
				FindRotationVector("OB", V1O, YO, ZO);				
			}
			if (Calibration.instance.P1H != Vector3.zero)
			{
				//calculate T(H) = (-P1.x, -P1.y, -P1.z)
				TH = CalculateTranslation(Calibration.instance.P1H);
				FindNormalVector("HL", TH);
				FindNormalizedMatrix("HL", NH);
				FindNewPoints("HL", Calibration.instance.P1H, Calibration.instance.P2H, Calibration.instance.P3H, TH);
				FormVectors("HL");
				FindRotationVector("HL",V1H, YH, ZH);				
			}

			Rotation_OrbbecToHololens(RO, RH);
			FormFinalMatrix(NH_Inv, RO_H, m_NO);

			// some checking with final matrix
			Debug.Log("Multiply P1O with final matrix gives: " + Multiply(Calibration.instance.P1O).ToString("f6"));
			Debug.Log("Multiply P2O with final matrix gives: " + Multiply(Calibration.instance.P2O).ToString("f6"));
			Debug.Log("Multiply P3O with final matrix gives: " + Multiply(Calibration.instance.P3O).ToString("f6"));

			Calibration.instance._isCalibrationCompleted = true;
			Calibration.instance._isFinishClicked = false;
			Debug.Log("finish value: " + Calibration.instance._isFinishClicked);
		
		}	
	}

    private void FormFinalMatrix(Matrix4x4 nH_Inv, Matrix4x4 rO_H, Matrix4x4 nO)
    {
		// This is the matrix that is to be multiplied with every point coming from Orbbec sensor to get to converted coordinate in HoloLens coordinate system
		// M(H<--O)  = NH^-1 . R(H<--O) . NO
		final_matrix = new Matrix4x4();
		Matrix4x4 res = nH_Inv * rO_H ;
		Debug.Log("res matrix : " + res.ToString("f6"));

		final_matrix = res * nO;

		Debug.Log("final matrix : " + final_matrix.ToString("f6"));
		
	}

    private void FindNormalizedMatrix(string scale, Vector3 Normal)
    {
		switch (scale)
		{
			case "OB":
				m_NO = new Matrix4x4();
				m_NO.SetRow(0, new Vector4(OB_SCALE, 0f, 0f, NO.x));
				m_NO.SetRow(1, new Vector4(0f, OB_SCALE, 0f, NO.y));
				m_NO.SetRow(2, new Vector4(0f, 0f, OB_SCALE, NO.z));
				m_NO.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
				
				break;

			case "HL":
				m_NH = new Matrix4x4();
				m_NH.SetRow(0, new Vector4(HL_SCALE, 0f, 0f, NH.x));
				m_NH.SetRow(1, new Vector4(0f, HL_SCALE, 0f, NH.y));
				m_NH.SetRow(2, new Vector4(0f, 0f, HL_SCALE, NH.z));
				m_NH.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

				// find NH^-1 or NH exp -1

				NH_Inv = new Matrix4x4();
				NH_Inv = m_NH.inverse;

				Debug.Log("NH inverse: " + NH_Inv.ToString("f6"));

				break;
			default:
				break;
		}
    }

    private void Rotation_OrbbecToHololens(Matrix4x4 RO, Matrix4x4 RH)
    {
		// Rotation vector R(H<--O)
		RO_H = new Matrix4x4();

		// This is found by multiplication of matrices: RH and RO
		RO_H = RH * RO;

		Debug.Log("RO_H matrix: " + RO_H.ToString("f6"));
    }

    private void FindRotationVector(string ind, Vector3 vO, Vector3 yO, Vector3 zO)
    {
		switch (ind)
		{
			case "OB":
				// RO: rotation of O into the origin
				RO = new Matrix4x4();
				RO.SetRow(0, new Vector4(vO.x, vO.y, vO.z, 0f));
				RO.SetRow(1, new Vector4(yO.x, yO.y, yO.z, 0f));
				RO.SetRow(2, new Vector4(zO.x, zO.y, zO.z, 0f));
				RO.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
				Debug.Log("RO matrix : " + RO.ToString("f6"));
				break;
			case "HL":
				// RH: rotation from the origin in H
				RH = new Matrix4x4();
				RH.SetColumn(0, new Vector4(vO.x, vO.y, vO.z, 0f));
				RH.SetColumn(1, new Vector4(yO.x, yO.y, yO.z, 0f));
				RH.SetColumn(2, new Vector4(zO.x, zO.y, zO.z, 0f));
				RH.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
				Debug.Log("RH matrix : " + RH.ToString("f6"));
				break;
			default:
				break;
		}
		

		
    }

    private void FormVectors(string ind)
    {
		switch (ind)
		{
			case "OB":
				// Forming basis vector for Orbbec

				V1O = P2O_new - P1O_new; // X-axis;
				V2O = P3O_new - P1O_new;

				// Cross product of V1 and V2 gives V3 = Z-axis
				ZO = Vector3.Cross(V1O, V2O);

				// Cross product of V3 and V1 gives V4 = Y-axis
				YO = Vector3.Cross(ZO, V1O);

				//Normalizing all vectors
				V1O = V1O.normalized;
				YO = YO.normalized;
				ZO = ZO.normalized;

				Debug.Log("XO: " + V1O.ToString("f6") + " , YO: " + YO.ToString("f6") + " , ZO: " + ZO.ToString("f6"));
				break;

			case "HL":
				//Forming basis vector for HoloLens

				V1H = P2H_new - P1H_new; // X-axis;
				V2H = P3H_new - P1H_new;

				// Cross product of V1 and V2 gives V3 = Z-axis
				ZH = Vector3.Cross(V1H, V2H);

				// Cross product of V3 and V1 gives V4 = Y-axis
				YH = Vector3.Cross(ZH, V1H);

				//Normalizing all vectors
				V1H = V1H.normalized;
				YH = YH.normalized;
				ZH = ZH.normalized;
				Debug.Log("XH: " + V1H.ToString("f6") + " , YH: " + YH.ToString("f6") + " , ZH: " + ZH.ToString("f6"));
				break;
			default:
				break;
		}
		
	}

	private void FindNewPoints(string scale, Vector3 P1, Vector3 P2, Vector3 P3, Vector3 TR)
    {
		switch (scale)
		{
			case "OB":
				P1O_new = OB_SCALE * (TR + P1);
				
				P2O_new = OB_SCALE * (TR + P2);
				Debug.Log("P2O_new: " + P2O_new.ToString("f6"));
				P3O_new = OB_SCALE * (TR + P3);
				break;

			case "HL":
				P1H_new = HL_SCALE * (TR + P1);
				
				P2H_new = HL_SCALE * (TR + P2);
				Debug.Log("p2H_new: " + P2H_new.ToString("f6"));
				P3H_new = HL_SCALE * (TR + P3);
				break;

			default:
				break;
		}
    }

    private void FindNormalVector(string scale, Vector3 Translated)
    {
		switch (scale)
		{
			case "OB":
				//scale factor for Orbbec sensor = 1/1000 or 0.001 because values are in mm
				NO = new Vector3(OB_SCALE * Translated.x, OB_SCALE * Translated.y, OB_SCALE * Translated.z);
				Debug.Log("NO vector: " + NO.ToString("f6"));
				break;
			case "HL":
				//scale factor for HoloLens = 1.0 because values are in m
				NH = new Vector3(HL_SCALE * Translated.x, HL_SCALE * Translated.y, HL_SCALE * Translated.z);
				Debug.Log("NH vector: " + NH.ToString("f6"));
				break;
			default:
				break;
		}
    }

    public Vector3 CalculateTranslation(Vector3 vec)
    {
		float x = -vec.x;
		float y = -vec.y;
		float z = -vec.z;

		return new Vector3(x, y, z);
    }

	public Vector3 Multiply(Vector3 point)
	{
		return final_matrix.MultiplyPoint(point);		
	}

	public Matrix4x4 Multiply(Matrix4x4 m)
	{
		return m * RO_H;
	}

	public Vector3 GetOrigintoHead()
	{
		return final_matrix.GetColumn(3);
	}


	internal Matrix4x4 ConverttoMatrix4x4(Matrix3x3 m)
	{
		Matrix4x4 newMatrix = new Matrix4x4();
		newMatrix.SetColumn(0, new Vector4 (m.AxisX.X, m.AxisX.Y, m.AxisX.Z, 0f));
		newMatrix.SetColumn(1, new Vector4 (m.AxisY.X, m.AxisY.Y, m.AxisY.Z, 0f));
		newMatrix.SetColumn(2, new Vector4 (m.AxisZ.X, m.AxisZ.Y, m.AxisZ.Z, 0f));
		newMatrix.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));

		return newMatrix;
	}

    
}
