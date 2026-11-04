using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;

public class Door : MonoBehaviour
{
    public bool IsOpen = false;
    [SerializeField]
    private bool IsRotatingDoor = true;
    [SerializeField]
    private float Speed = 2f;
    [Header("Rotation Configs")]
    [SerializeField]
    private float RotationAmount = 90f;
   //  [SerializeField]
   //  private float ForwardDirection = 0;

    private Vector3 StartRotation;
    // private Vector3 Forward;

    private Coroutine AnimationCoroutine;

    [SerializeField] private Transform hingePivot;

    private void Awake()
    {
        if (hingePivot == null)
        hingePivot = transform;

        StartRotation = hingePivot.localEulerAngles;
    }

    public void Open(Vector3 userPosition)
    {
        Debug.Log("Door Open() was called!");
        if (IsOpen)
        return;

        if (AnimationCoroutine != null)
        StopCoroutine(AnimationCoroutine);

        if (!IsRotatingDoor)
        {
            Debug.LogWarning("IsRotatingDoor is disabled!");
            return;
        }

            Vector3 directionToUser = userPosition - transform.position;
            directionToUser.y = 0f;
            directionToUser.Normalize();

            Vector3 doorForward = transform.forward;
            doorForward.y = 0f;
            doorForward.Normalize();

            float dot = Vector3.Dot(doorForward, directionToUser);
            
            Debug.Log($"Player position: {userPosition}");
            Debug.Log($"Door center: {transform.position}");
            Debug.Log($"Door forward: {doorForward}");
            Debug.Log($"Door opening dot: {dot:F3}");

            AnimationCoroutine = StartCoroutine(DoRotationOpen(dot));

        
            
    }

    private IEnumerator DoRotationOpen(float ForwardAmount)
    {
        Quaternion startRotation = hingePivot.localRotation;

        //opposite rotation formula depends which side player approaches
        float rotationDirection = ForwardAmount >= 0f ? 1f : -1f;

        Vector3 targetEuler = StartRotation + new Vector3(0f, RotationAmount * rotationDirection, 0f);

        Quaternion endRotation = Quaternion.Euler(targetEuler);

        IsOpen = true;

        float time = 0f;

        while (time < 1f)
        {
            hingePivot.localRotation = Quaternion.Slerp(startRotation, endRotation, time);

            time += Time.deltaTime * Speed;
            yield return null;
        }

        hingePivot.localRotation = endRotation;
        AnimationCoroutine = null;
    }

    public void Close()
    {
        if (IsOpen)
        {
            if (AnimationCoroutine != null)
            {
                StopCoroutine(AnimationCoroutine);
            }
           
            AnimationCoroutine = StartCoroutine(DoRotationClose());
            

        }

    }

    private IEnumerator DoRotationClose()
    {        
        Quaternion startRotation = hingePivot.localRotation;
        Quaternion endRotation = Quaternion.Euler(StartRotation);

        IsOpen = false;

        float time = 0f;

        while (time < 1f)
        {
            hingePivot.localRotation = Quaternion.Slerp(startRotation, endRotation, time);
            
            yield return null;
            time += Time.deltaTime * Speed;
        }

        hingePivot.localRotation = endRotation;
        AnimationCoroutine = null;

    }
}
