﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    
    /// <summary> Interface for canvas raycast target. </summary>
    public interface ICanvasRaycastTarget
    {
        /// <summary> Gets the canvas. </summary>
        /// <value> The canvas. </value>
        Canvas canvas { get; }
        /// <summary> Gets a value indicating whether this object is enabled. </summary>
        /// <value> True if enabled, false if not. </value>
        bool enabled { get; }
        /// <summary> Gets a value indicating whether the ignore reversed graphics. </summary>
        /// <value> True if ignore reversed graphics, false if not. </value>
        bool ignoreReversedGraphics { get; }

        void GraphicRaycast(bool ignoreReversedGraphics, Ray ray, float distance, Vector3 screenCenterPoint, NRPointerRaycaster raycaster, List<RaycastResult> raycastResults);
    }
    

    /// <summary>
    /// The class enables an UGUI Canvas and its children to be interactive with NRInput raycasters. </summary>
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public class CanvasRaycastTarget : UIBehaviour, ICanvasRaycastTarget
    {
        /// <summary> The canvas. </summary>
        private Canvas m_canvas;
        /// <summary> True to ignore reversed graphics. </summary>
        [SerializeField]
        private bool m_IgnoreReversedGraphics = true;

        /// <summary> Gets the canvas. </summary>
        /// <value> The canvas. </value>
        public virtual Canvas canvas { get { return m_canvas ?? (m_canvas = GetComponent<Canvas>()); } }
        /// <summary> Gets or sets a value indicating whether the ignore reversed graphics. </summary>
        /// <value> True if ignore reversed graphics, false if not. </value>
        public bool ignoreReversedGraphics { get { return m_IgnoreReversedGraphics; } set { m_IgnoreReversedGraphics = value; } }

        /// <summary> <para>See MonoBehaviour.OnEnable.</para> </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            CanvasTargetCollector.AddTarget(this);
        }

        /// <summary> <para>See MonoBehaviour.OnDisable.</para> </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            CanvasTargetCollector.RemoveTarget(this);
        }

        public virtual void GraphicRaycast(bool ignoreReversedGraphics, Ray ray, float distance, Vector3 screenCenterPoint, NRPointerRaycaster raycaster, List<RaycastResult> raycastResults)
        {
            var eventCamera = raycaster.eventCamera;
            var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            for (int i = 0; i < graphics.Count; ++i)
            {
                var graphic = graphics[i];
                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget) { continue; }

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, screenCenterPoint, eventCamera)) { continue; }

                if (ignoreReversedGraphics && Vector3.Dot(ray.direction, graphic.transform.forward) <= 0f) { continue; }

                if (!graphic.Raycast(screenCenterPoint, eventCamera)) { continue; }

                float dist;
                new Plane(graphic.transform.forward, graphic.transform.position).Raycast(ray, out dist);
                if (dist > distance) { continue; }
                var racastResult = new RaycastResult
                {
                    gameObject = graphic.gameObject,
                    module = raycaster,
                    distance = dist,
                    worldPosition = ray.GetPoint(dist),
                    worldNormal = -graphic.transform.forward,
                    screenPosition = screenCenterPoint,
                    index = raycastResults.Count,
                    depth = graphic.depth,
                    sortingLayer = canvas.sortingLayerID,
                    sortingOrder = canvas.sortingOrder
                };
                raycastResults.Add(racastResult);
            }
        }
    }
}