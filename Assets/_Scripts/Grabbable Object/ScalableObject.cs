﻿using UnityEngine;
using System.Collections;
//using OvrTouch.Hands;
using NanoVRController;

using VoxelBusters.RuntimeSerialization;

[RuntimeSerializable(typeof(MonoBehaviour), false)]
public class ScalableObject : GrabbableObject
{
    float distance;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (activeGrabbers.Count > 0)
            UpdatePivot();
    }

    protected override void OnRegisterController(Grabber g)
    {
        KillMomentum();

        AttachToNewPivot(false);
    }

    protected override void OnReleaseController(Grabber g)
    {
        if (activeGrabbers.Count > 0)
        {
            AttachToNewPivot(false);
        }
        else
        {
            DetachFromPivot();
            DeletePivot();
            if (momentumOn)
            {
                TransferMomentum(g.controller);
            }
        }
    }


    protected override void CreatePivot()
    {
        pivot = RSUtility.Instantiate(Resources.Load(EmptyUIDPrefab, typeof(GameObject)) as GameObject);
        pivot.name = "pivot";

        if (activeGrabbers.Count == 1)
        {
            pivot.transform.position = activeGrabbers.First.Value.transform.position;
            pivot.transform.rotation = activeGrabbers.First.Value.transform.rotation;
        }
        else
        {
            pivot = new GameObject();
            Vector3 dir = activeGrabbers.First.Value.transform.position - activeGrabbers.Last.Value.transform.position;
            Vector3 mid = (activeGrabbers.First.Value.transform.position + activeGrabbers.Last.Value.transform.position) * 0.5f;
            Vector3 forward = activeGrabbers.First.Value.transform.forward + activeGrabbers.Last.Value.transform.forward;
            distance = dir.magnitude;
            pivot.transform.position = mid;
            pivot.transform.rotation = Quaternion.LookRotation(dir, Vector3.Cross(dir, forward));
        }
    }

    protected override void UpdatePivot()
    {
        if (activeGrabbers.Count == 1)
        {
            pivot.transform.position = activeGrabbers.First.Value.transform.position;
            pivot.transform.rotation = activeGrabbers.First.Value.transform.rotation;
        }
        else
        {
            Vector3 dir = activeGrabbers.First.Value.transform.position - activeGrabbers.Last.Value.transform.position;
            Vector3 mid = (activeGrabbers.First.Value.transform.position + activeGrabbers.Last.Value.transform.position) * 0.5f;
            Vector3 forward = activeGrabbers.First.Value.transform.forward + activeGrabbers.Last.Value.transform.forward;
            float newDistance = dir.magnitude;
            pivot.transform.position = mid;
            pivot.transform.rotation = Quaternion.LookRotation(dir, Vector3.Cross(dir, forward));
            pivot.transform.localScale *= newDistance / distance;
            distance = newDistance;
        }
    }
}