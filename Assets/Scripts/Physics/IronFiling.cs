﻿//-----------------------------------------------------------------------------
// IronFiling.cs
//
// Class to create a field image with iron filing
//
//
// Authors: Michael Stefan Holly
//          Michael Schiller
//          Christopher Schinnerl
//-----------------------------------------------------------------------------
//

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to create a field image with iron filing
/// </summary>
public class IronFiling : MonoBehaviour, IResetObject
{
    /// <summary>
    /// The iron material
    /// </summary>
    public Material ironMaterial;

    /// <summary>
    /// Tha number of iteration
    /// </summary>
    public int iterations = 1000;

    /// <summary>
    /// The field which is represented
    /// </summary>
    private IField field;

    /// <summary>
    /// The line renderers to draw the field
    /// </summary>
    private LineRenderer[] linerenderers;

    /// <summary>
    /// The filling height
    /// </summary>
    private float height;

    /// <summary>
    /// The filling width
    /// </summary>
    private float width;

    /// <summary>
    /// The maximum of vertices
    /// </summary>
    public int maxvertexCount = 100;

    [SerializeField]
    private float lineSegmentLength = 0.1f;

    [SerializeField]
    private float lineStartWidth = 0.4f;

    [SerializeField]
    private float lineEndWidth = 0.004f;

    private SimulationController simController;

    /// <summary>
    /// Initialization
    /// </summary>
    void Start()
    {
        GameObject simControllerObject = GameObject.Find("SimulationController");
        if (simControllerObject)
            simController = simControllerObject.GetComponent<SimulationController>();

        height = gameObject.GetComponent<MeshFilter>().mesh.bounds.size.z;
        width = gameObject.GetComponent<MeshFilter>().mesh.bounds.size.x;

        field = GameObject.FindGameObjectWithTag("Field").GetComponent<IField>();
        linerenderers = new LineRenderer[2 * iterations];

        for (int i = 0; i < iterations * 2; i++)
        {
            GameObject line = new GameObject("line");
            line.transform.parent = this.transform;
            //line.transform.localRotation = Quaternion.identity;

            LineRenderer linerenderer = line.AddComponent<LineRenderer>();
            //linerenderer.useWorldSpace = false;
            linerenderer.shadowCastingMode = ShadowCastingMode.On;
            linerenderer.receiveShadows = true;
            linerenderer.material = ironMaterial;
            linerenderer.lightProbeUsage = LightProbeUsage.Off;
            linerenderer.startWidth = lineStartWidth;
            linerenderer.endWidth = lineEndWidth;
            linerenderers[i] = linerenderer;
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Update is called every frame and disables the object during a running simulation.
    /// </summary>
    void Update()
    {
        if (simController.SimulationRunning)
            gameObject.SetActive(false);
    }

    /// <summary>
    /// Stops the simulations and generate the field image by drawing random irons.
    /// </summary>
    public void generateFieldImage()
    {
        if (field == null)
            return;

        simController.StopSimulation();
        simController.AddNewResetObject(this);

        Debug.Log("Start IronFiling");

        for (int i = 0; i < iterations * 2; i++)
        {
            Vector3 origin = new Vector3(Random.Range(-1f, 1f) * width/2, 0, Random.Range(-1f, 1f) * height/2);
            drawIron(origin, linerenderers[i]);
        }
        Debug.Log("End IronFiling");

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Draws a iron line starting at the given origin.
    /// </summary>
    /// <param name="origin">The origin to start</param>
    /// <param name="linerender">The line renderer to draw line</param>
    private void drawIron(Vector3 origin, LineRenderer linerender)
    {
        // line renderer points in world space!
        List<Vector3> linePoints = new List<Vector3> { transform.TransformPoint(origin) };

        Vector3 linePoint = origin;
        int numberOfPoints = 1;
        while (linePoint.x <= width / 2.0f && linePoint.x >= -width / 2.0f &&
               linePoint.z <= height / 2.0f && linePoint.z >= -height / 2.0f && numberOfPoints < maxvertexCount)
        {
            Vector3 globalLinePoint = transform.TransformPoint(linePoint);
            globalLinePoint += Vector3.Normalize(field.get(globalLinePoint)) * lineSegmentLength;

            linePoints.Add(globalLinePoint);

            linePoint = transform.InverseTransformPoint(globalLinePoint);

            numberOfPoints++;
        }

        linerender.positionCount = numberOfPoints;
        for (int i = 0; i < numberOfPoints; i++)
        {
            linerender.SetPosition(i, linePoints[i]);
        }
    }

    /// <summary>
    /// Resets the object
    /// </summary>
    public void resetObject()
    {
        gameObject.SetActive(false);
    }
}
