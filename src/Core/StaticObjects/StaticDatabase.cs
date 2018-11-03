﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.Core
{
    public static class StaticDatabase
    {
        //Groups are stored by name within the body name

        private static Dictionary<string, Dictionary<string, GroupCenter>> groupsByPlanets = new Dictionary<string, Dictionary<string, GroupCenter>>();

        private static Dictionary<string, StaticModel> modelList = new Dictionary<string, StaticModel>();
        internal static List<StaticModel> allStaticModels = new List<StaticModel>();

        //make the list private, so nobody does easily add or remove from it. the array is updated in the Add and Remove functions
        // arrays are always optimized (also in foreach loops)
        private static List<StaticInstance> _allStaticInstances = new List<StaticInstance>();
        internal static StaticInstance[] allStaticInstances = new StaticInstance[0];


        internal static Dictionary<string, StaticInstance> instancedByUUID = new Dictionary<string, StaticInstance>();

        private static Dictionary<string, GroupCenter> allCenters = new Dictionary<string, GroupCenter>();

        internal static CelestialBody lastActiveBody = null;

        private static Vector3 vPlayerPos = Vector3.zero;


        internal static void Reset()
        {

            modelList = new Dictionary<string, StaticModel>();
            allStaticModels = new List<StaticModel>();
            _allStaticInstances = new List<StaticInstance>();
            allStaticInstances = new StaticInstance[0];
            groupsByPlanets = new Dictionary<string, Dictionary<string, GroupCenter>>();
        }

        /// <summary>
        /// Adds the Instance to the instances and Group lists. Also sets the PQSCity.name
        /// </summary>
        /// <param name="instance"></param>
        internal static void AddStatic(StaticInstance instance)
        {

            if (string.IsNullOrEmpty(instance.UUID))
            {
                instance.UUID = GetNewUUID();
            }

            _allStaticInstances.Add(instance);
            allStaticInstances = _allStaticInstances.ToArray();

            if (instancedByUUID.ContainsKey(instance.UUID))
            {
                instance.UUID = GetNewUUID();
            }
            instancedByUUID.Add(instance.UUID, instance);

            instance.groupCenter.AddInstance(instance);

            SetNewName(instance);
        }

        /// <summary>
        /// Generate a UUID that is not already in the database
        /// </summary>
        /// <returns></returns>
        internal static string GetNewUUID()
        {
            string newUUID = Guid.NewGuid().ToString();

            while (instancedByUUID.ContainsKey(newUUID))
            {
                newUUID = Guid.NewGuid().ToString();
                Log.UserWarning("Duplicate UUID generated. You should play lottery");
            }
            return newUUID;

        }


        /// <summary>
        /// Removes a Instance from the group and instance lists.
        /// </summary>
        /// <param name="instance"></param>
        internal static void DeleteStatic(StaticInstance instance)
        {
            if (instancedByUUID.ContainsKey(instance.UUID))
            {
                instancedByUUID.Remove(instance.UUID);
            }
            if (_allStaticInstances.Contains(instance))
            {
                _allStaticInstances.Remove(instance);
                allStaticInstances = _allStaticInstances.ToArray();
            }

            instance.groupCenter.RemoveInstance(instance);
            GameObject.Destroy(instance.gameObject);
        }

        /// <summary>
        /// Changes the group from a instance
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="newGroup"></param>
        internal static void ChangeGroup(StaticInstance instance, GroupCenter newGroup)
        {
            instance.groupCenter.RemoveInstance(instance);

            instance.groupCenter = newGroup;
            instance.Group = newGroup.Group;

            instance.gameObject.transform.parent = newGroup.gameObject.transform;

            SetNewName(instance);
            newGroup.AddInstance(instance);
            instance.Update();
        }


        /// <summary>
        /// Sets the PQSCity Name to Group_Modenlame_(index of the same models in group)
        /// </summary>
        /// <param name="instance"></param>
        private static void SetNewName(StaticInstance instance)
        {
            string modelName = instance.model.name;
            string groupName = instance.Group;

            int modelCount = (from obj in instance.groupCenter.childInstances where obj.model.name == instance.model.name select obj).Count();
            if (modelCount == 0)
            {
                Log.Warning("Shock and Horror! We cannot find at least ourself in our own group");
                return;
            }

            modelCount--;
            instance.indexInGroup = modelCount;
            instance.gameObject.name = groupName + "_" + instance.model.name + "_" + modelCount;
            //   Log.Normal("PQSCity.name: " + instance.pqsCity.name);
        }

        /// <summary>
        /// toggles the visiblility for all Instances at once
        /// </summary>
        /// <param name="active"></param>
        internal static void ToggleActiveAllStatics(bool activate)
        {
            Log.Debug("StaticDatabase.ToggleActiveAllStatics");

            foreach (GroupCenter center in allCenters.Values)
            {
                center.SetInstancesEnabled(activate);
            }
        }

        internal static void AddGroupCenter(GroupCenter center)
        {
            if (!HasGroupCenter(center.dbKey))
            {
                allCenters.Add(center.dbKey, center);
            }
            if (!groupsByPlanets.ContainsKey(center.CelestialBody.name))
            {
                groupsByPlanets.Add(center.CelestialBody.name, new Dictionary<string, GroupCenter>());
            }
            groupsByPlanets[center.CelestialBody.name].Add(center.dbKey, center);
        }

        internal static void RemoveGroupCenter(GroupCenter center)
        {
            if (groupsByPlanets.ContainsKey(center.CelestialBody.name))
            {
                groupsByPlanets[center.CelestialBody.name].Remove(center.dbKey);
            }
            if (HasGroupCenter(center.dbKey))
            {
                allCenters.Remove(center.dbKey);
            }
        }

        internal static bool HasGroupCenter(string centerKey)
        {
            return (allCenters.ContainsKey(centerKey));
        }


        internal static GroupCenter GetGroupCenter(string centerKey)
        {
            if (HasGroupCenter(centerKey))
            {
                return allCenters[centerKey];
            }
            else
            {
                return null;
            }
        }

        internal static GroupCenter[] allGroupCenters
        {
            get
            {
                return allCenters.Values.ToArray();
            }
        }


        internal static void DeactivateAllOnPlanet(CelestialBody body)
        {
            if (body == null || !groupsByPlanets.ContainsKey(body.name))
            {
                return;
            }
            foreach (GroupCenter center in groupsByPlanets[body.name].Values)
            {
                center.SetInstancesEnabled(false);
            }
        }



        /// <summary>
        /// Handles on what to do when a body changes
        /// </summary>
        /// <param name="newBody"></param>
        internal static void OnBodyChanged(CelestialBody newBody)
        {
            if (newBody != null)
            {
                if (newBody != lastActiveBody)
                {
                    DeactivateAllOnPlanet(lastActiveBody);
                    lastActiveBody = newBody;
                }
            }
            else
            {
                Log.Debug("StaticDatabase.onBodyChanged(): body is null. cacheAll(). Set activeBodyName empty " + lastActiveBody.name);
                DeactivateAllOnPlanet(lastActiveBody);
                lastActiveBody = null;
            }
        }



        internal static void UpdateCache(Vector3 playerPos)
        {

            float maxDistance = (float)(PhysicsGlobals.Instance.VesselRangesDefault.flying.load + (KerbalKonstructs.localGroupRange * 1.5));
            bool isInRange = false;

            //Log.Normal("StaticDatabase.updateCache(): activeBodyName is " + activeBodyName);
            if (playerPos == Vector3.zero)
            {

                vPlayerPos = Vector3.zero;

                if (FlightGlobals.ActiveVessel != null)
                {
                    vPlayerPos = FlightGlobals.ActiveVessel.GetWorldPos3D();
                    //Log.Normal("StaticDatabase.updateCache(): using active vessel " + FlightGlobals.ActiveVessel.vesselName);
                }
                if (vPlayerPos == Vector3.zero)
                {
                    vPlayerPos = FlightGlobals.camera_position;
                }
                if (vPlayerPos == Vector3.zero)
                {
                    Log.UserError("StaticDatabase.updateCache(): vPlayerPos is still v3.zero ");
                    return;
                }
            }
            else
            {
                vPlayerPos = playerPos;
            }

            if (groupsByPlanets.ContainsKey(lastActiveBody.name))
            {
                foreach (GroupCenter center in groupsByPlanets[lastActiveBody.name].Values)
                {
                    //Log.Normal("Checking Group: " + group.name  ); 
                    isInRange = (Vector3.Distance(center.gameObject.transform.position, vPlayerPos) < maxDistance);
                    // Log.Debug("StaticDatabase.updateCache(): group visrange is " + group.visibilityRange.ToString() + " for " + group.name);
                    center.SetInstancesEnabled(isInRange);
                }
            }
        }

        public static StaticInstance[] GetAllStatics()
        {
            return allStaticInstances;
        }

        internal static void RegisterModel(StaticModel model, string name)
        {
            allStaticModels.Add(model);
            if (modelList.ContainsKey(name))
            {
                Log.UserInfo("duplicate model name: " + name + " ,found in: " + model.configPath + " . This might be OK.");
                return;
            }
            else
            {
                modelList.Add(name, model);
            }
        }

        internal static StaticModel GetModelByName(string name)
        {
            if (!modelList.ContainsKey(name))
            {
                Log.UserError("No StaticModel found with name: " + name);
                return null;
            }
            else
            {
                return modelList[name];
            }
        }


        internal static List<StaticInstance> GetInstancesFromModel(StaticModel model)
        {
            return (from obj in allStaticInstances where obj.model == model select obj).ToList();
        }
    }
}
