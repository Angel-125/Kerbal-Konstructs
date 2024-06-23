using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using KSP.Localization;

namespace KerbalKonstructs.UI
{
    public enum ReferenceSystemModes
    {
        Local,
        Absolute
    }

    public enum PlacementModes
    {
        Place,
        Offset,
        Rotate
    }

    public class BaseEditor: KKWindow
    {
        #region Constants
        const float kMessageDuration = 5.0f;
        #endregion

        #region Housekeeping
        public static ReferenceSystemModes referenceSystem = ReferenceSystemModes.Absolute;
        public float increment = 1f;
        public PlacementModes placementMode = PlacementModes.Offset;
        public bool angleSnapEnabled = false;
        public GameObject localMoveObject;
        public GameObject absoluteMoveGameObject;
        #endregion

        #region API
        public virtual void HandleKeyboardInput()
        {
            if (!IsOpen())
                return;

            #region Translation Controls
            if (GameSettings.TRANSLATE_FWD.GetKey())
            {
                SetTransform(Vector3.forward);
            }
            if (GameSettings.TRANSLATE_BACK.GetKey())
            {
                SetTransform(Vector3.back);
            }
            if (GameSettings.TRANSLATE_LEFT.GetKey())
            {
                SetTransform(Vector3.left);
            }
            if (GameSettings.TRANSLATE_RIGHT.GetKey())
            {
                SetTransform(Vector3.right);
            }
            if (GameSettings.TRANSLATE_UP.GetKey())
            {
                SetTransform(Vector3.up);
            }
            if (GameSettings.TRANSLATE_DOWN.GetKey())
            {
                SetTransform(Vector3.down);
            }
            #endregion

            #region Rotation Controls
            if (Input.GetKey(KeyCode.E))
            {
                SetRotation(Vector3.up, -increment);
            }
            if (Input.GetKey(KeyCode.Q))
            {
                SetRotation(Vector3.up, increment);
            }
            #endregion

            #region Toggles
            // Local/Absolute (F)
            if (GameSettings.Editor_coordSystem.GetKeyUp())
            {
                if (referenceSystem == ReferenceSystemModes.Absolute)
                {
                    referenceSystem = ReferenceSystemModes.Local;
                    ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_6001222"), kMessageDuration, ScreenMessageStyle.UPPER_CENTER); // Offset: Local
                }
                else
                {
                    referenceSystem = ReferenceSystemModes.Absolute;
                    ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_6001221"), kMessageDuration, ScreenMessageStyle.UPPER_CENTER); // Offset: Absolute
                }
                UpdateMoveGizmo();
            }

            // Angle Snap (C)
            if (GameSettings.Editor_toggleAngleSnap.GetKeyUp())
            {
                angleSnapEnabled = !angleSnapEnabled;

                if (angleSnapEnabled)
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_KK_AngleSnapOn"), kMessageDuration, ScreenMessageStyle.UPPER_CENTER);
                }
                else
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_KK_AngleSnapOff"), kMessageDuration, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            #endregion

            #region Placement Controls
            // Mouse Click Drag Mode (1)
            if (GameSettings.Editor_modePlace.GetKeyUp())
            {
                placementMode = PlacementModes.Place;
                ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_148181"), kMessageDuration, ScreenMessageStyle.UPPER_CENTER);
            }

            // Move Gizmo: Offset (2)
            if (GameSettings.Editor_modeOffset.GetKeyUp())
            {
                placementMode = PlacementModes.Offset;
                ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_148182"), kMessageDuration, ScreenMessageStyle.UPPER_CENTER);
            }

            // Rotate Gizmo (3)
            if (GameSettings.Editor_modeRotate.GetKeyUp())
            {
                placementMode = PlacementModes.Rotate;
                ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_148183"), kMessageDuration, ScreenMessageStyle.UPPER_CENTER);
            }
            #endregion

            #region Misc Controls
            if (Input.GetKeyUp(KeyCode.Return))
            {
                ApplyInputStrings();
            }
            #endregion
        }

        /// <summary>
        /// closes the sub editor windows
        /// </summary>
        public static void CloseEditors()
        {
            FacilityEditor.instance.Close();
            LaunchSiteEditor.instance.Close();
        }
        #endregion

        #region Lifecycle Methods
        public override void Open()
        {
            base.Open();

            absoluteMoveGameObject = new GameObject();
            GameObject.DontDestroyOnLoad(absoluteMoveGameObject);
        }

        public override void Close()
        {
            if (KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.disable();
            }

            EditorGizmo.CloseGizmos();
            CloseEditors();
            base.Close();
        }

        #endregion

        #region Virtual Methods
        public virtual void SetTransform(Vector3 direction) 
        {
        }

        public virtual void ApplyInputStrings()
        {
        }

        public virtual void SetRotation(Vector3 axis, float increment)
        {
        }

        public virtual void UpdateMoveGizmo()
        {
            EditorGizmo.CloseGizmos();

            UpdateLocalMoveGameObject();
            UpdateAbsoluteGameObject();

            switch (referenceSystem)
            {
                case ReferenceSystemModes.Local:
                    EditorGizmo.SetupMoveGizmo(localMoveObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
                    break;
                case ReferenceSystemModes.Absolute:
                    EditorGizmo.SetupMoveGizmo(absoluteMoveGameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
                    break;
            }
        }

        public virtual void UpdateLocalMoveGameObject()
        {
        }

        public virtual void UpdateAbsoluteGameObject()
        {
        }

        public virtual void OnMoveCallBack(Vector3 vector)
        {
        }

        public virtual void WhenMovedCallBack(Vector3 vector)
        {
        }
        #endregion
    }
}
