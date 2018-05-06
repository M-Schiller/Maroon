﻿using UnityEngine;
using VRTK;

public class SnapSlipPlate : MonoBehaviour
{
    [SerializeField]
    private Rigidbody plateHandleBodyRight;

    [SerializeField]
    private Rigidbody plateHandleBodyLeft;

    [SerializeField]
    private Vector3 plateScale = Vector3.one;

    private VRTK_InteractableObject snappedPlate = null;

    private FixedJoint handleJointRight;

    private FixedJoint handleJointLeft;

    private Vector3 previousPlateScale;
            
    private RigidbodyConstraints previousPlateBodyConstraints;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("SlitPlate") || snappedPlate)
            return;

        VRTK_InteractableObject plate = other.GetComponent<VRTK_InteractableObject>();
        if (!plate || plate.IsGrabbed())
            return;

        snappedPlate = plate;
        StorePreviousState();

        snappedPlate.transform.position = Vector3.Lerp(plateHandleBodyRight.transform.position, plateHandleBodyLeft.transform.position, 0.5f);
        snappedPlate.transform.rotation = Quaternion.Euler(90, 0, 0);
        snappedPlate.transform.localScale = plateScale;

        // Create Handle Joints
        handleJointRight = snappedPlate.gameObject.AddComponent<FixedJoint>();
        handleJointRight.connectedBody = plateHandleBodyRight;

        handleJointLeft = snappedPlate.gameObject.AddComponent<FixedJoint>();
        handleJointLeft.connectedBody = plateHandleBodyLeft;

        snappedPlate.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        snappedPlate.InteractableObjectGrabbed += OnSnappedPlateGrabbed;
    }

    private void OnSnappedPlateGrabbed(object sender, InteractableObjectEventArgs e)
    {
        // Destroy all current joints
        foreach (Joint joint in snappedPlate.GetComponents<Joint>())
            Destroy(joint);

        LoadPreviousState();

        snappedPlate.InteractableObjectGrabbed -= OnSnappedPlateGrabbed;
        snappedPlate = null;
    }

    private void StorePreviousState()
    {
        previousPlateScale = snappedPlate.transform.localScale;
        previousPlateBodyConstraints = snappedPlate.GetComponent<Rigidbody>().constraints;
    }

    private void LoadPreviousState()
    {
        snappedPlate.transform.localScale = previousPlateScale;
        snappedPlate.GetComponent<Rigidbody>().constraints = previousPlateBodyConstraints;
    }
}
