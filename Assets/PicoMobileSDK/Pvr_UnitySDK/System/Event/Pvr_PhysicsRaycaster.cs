// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Pvr_PhysicsRaycaster : PhysicsRaycaster
{
    
    protected Vector2 lastKnownPosition;
    protected const float UI_CONTROL_OFFSET = 0.00001f;

    public override Camera eventCamera
    {
        get
        {
            if (m_EventCamera == null)
                m_EventCamera = GetComponent<Camera>();
            return m_EventCamera ?? Camera.main;
        }
    }

    [NonSerialized]
    // Use a static to prevent list reallocation. We only need one of these globally (single main thread), and only to hold temporary data
    private static List<RaycastResult> s_RaycastResults = new List<RaycastResult>();

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (eventCamera == null)
            return;

        var ray = new Ray(eventData.pointerCurrentRaycast.worldPosition, eventData.pointerCurrentRaycast.worldNormal);
        Raycast(eventCamera, ray, ref s_RaycastResults);
        SetNearestRaycast(ref eventData, ref resultAppendList, ref s_RaycastResults);
        s_RaycastResults.Clear();
    }

    protected virtual void SetNearestRaycast(ref PointerEventData eventData, ref List<RaycastResult> resultAppendList, ref List<RaycastResult> raycastResults)
    {
        RaycastResult? nearestRaycast = null;
        for (var index = 0; index < raycastResults.Count; index++)
        {
            RaycastResult castResult = raycastResults[index];
            castResult.index = resultAppendList.Count;
            if (!nearestRaycast.HasValue || castResult.distance < nearestRaycast.Value.distance)
            {
                nearestRaycast = castResult;
            }
            resultAppendList.Add(castResult);
        }

        if (nearestRaycast.HasValue)
        {
            eventData.position = nearestRaycast.Value.screenPosition;
            eventData.delta = eventData.position - lastKnownPosition;
            lastKnownPosition = eventData.position;
            eventData.pointerCurrentRaycast = nearestRaycast.Value;
        }
    }



    protected virtual void Raycast(Camera eventCamera, Ray ray, ref List<RaycastResult> results)
    {
        var hitDistance = eventCamera.farClipPlane - eventCamera.nearClipPlane;
        var hits = Physics.RaycastAll(ray, hitDistance, finalEventMask);

        if (hits.Length > 1)
            System.Array.Sort(hits, (r1, r2) => r1.distance.CompareTo(r2.distance));



        Vector3 position = ray.GetPoint(hitDistance);
        Vector2 pointerPosition = eventCamera.WorldToScreenPoint(position);



        if (hits.Length != 0)
        {

            for (int b = 0, bmax = hits.Length; b < bmax; ++b)
            {
                var result = new RaycastResult
                {
                    gameObject = hits[b].collider.gameObject,
                    module = this,
                    distance = hitDistance,
                    index = results.Count,
                    sortingLayer = 0,
                    sortingOrder = 0
                };
                results.Add(result);
            }

        }
    }

    
   }

   


