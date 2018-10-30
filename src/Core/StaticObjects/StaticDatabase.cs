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
		private static Dictionary<string, Dictionary<string, StaticGroup>> groupList = new Dictionary<string,Dictionary<string,StaticGroup>>();

        private static Dictionary<string, StaticModel> modelList = new Dictionary<string, StaticModel>();

        internal static List<StaticModel> allStaticModels = new List<StaticModel>();

        //make the list private, so nobody does easily add or remove from it. the array is updated in the Add and Remove functions
        // arrays are always optimized (also in foreach loops)
        private static List<StaticInstance> _allStaticInstances = new List<StaticInstance>();
        internal static StaticInstance [] allStaticInstances  = new StaticInstance [0] ;

        internal static Dictionary<string, StaticInstance> instancedByUUID = new Dictionary<string, StaticInstance>();

        internal static Dictionary<string, GroupCenter> allCenters = new Dictionary<string, GroupCenter>();


        internal static string activeBodyName = "";

        private static Vector3 vPlayerPos = Vector3.zero;


        internal static void Reset()
        {
            groupList = new Dictionary<string, Dictionary<string, StaticGroup>>();
            modelList = new Dictionary<string, StaticModel>();
            allStaticModels = new List<StaticModel>();
            _allStaticInstances = new List<StaticInstance>();
            allStaticInstances = new StaticInstance[0];
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

            String bodyName = instance.CelestialBody.bodyName;
			String groupName = instance.Group;

			if (!groupList.ContainsKey(bodyName))
            {
                groupList.Add(bodyName, new Dictionary<string, StaticGroup>());
            }

            if (!groupList[bodyName].ContainsKey(groupName))
			{
				StaticGroup group = new StaticGroup(groupName, bodyName);			
				groupList[bodyName].Add(groupName, group);
			}
			groupList[bodyName][groupName].AddStatic(instance);

            SetNewName(instance);

            instance.groupCenter.AddInstance(instance);

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
            if (_allStaticInstances.Contains(instance))
            {
                _allStaticInstances.Remove(instance);
                allStaticInstances = _allStaticInstances.ToArray();
            }

            if (instancedByUUID.ContainsKey(instance.UUID))
            {
                instancedByUUID.Remove(instance.UUID);
            }

            instance.groupCenter.RemoveInstance(instance);

            String bodyName = instance.CelestialBody.bodyName;
            String groupName = instance.Group;


            if (groupList.ContainsKey(bodyName))
            {
                if (groupList[bodyName].ContainsKey(groupName))
                {
                    Log.Normal("StaticDatabase deleteObject");
                    groupList[bodyName][groupName].DeleteObject(instance);
                }
            }

        }

        /// <summary>
        /// Changes the group from a instance
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="newGroup"></param>
        internal static void ChangeGroup(StaticInstance instance, GroupCenter newGroup)
        {
            String bodyName = instance.CelestialBody.bodyName;
            String groupName = instance.Group;


            instance.groupCenter.RemoveInstance(instance);

            if (groupList.ContainsKey(bodyName))
            {
                if (groupList[bodyName].ContainsKey(groupName))
                {
                    Log.Normal("StaticDatabase deleteObject");
                    groupList[bodyName][groupName].RemoveStatic(instance);
                }
            }



            instance.Group = newGroup.Group;
            instance.groupCenter = newGroup;


            bodyName = instance.CelestialBody.bodyName;
            groupName = instance.Group;

            if (!groupList.ContainsKey(bodyName))
            {
                groupList.Add(bodyName, new Dictionary<string, StaticGroup>());
            }

            if (!groupList[bodyName].ContainsKey(groupName))
            {
                StaticGroup group = new StaticGroup(groupName, bodyName);
                groupList[bodyName].Add(groupName, group);
            }
            groupList[bodyName][groupName].AddStatic(instance);

            SetNewName(instance);

            instance.groupCenter.AddInstance(instance);

            instance.gameObject.transform.parent = instance.groupCenter.gameObject.transform;

        }

        /// <summary>
        /// Sets the PQSCity Name to Group_Modenlame_(index of the same models in group)
        /// </summary>
        /// <param name="instance"></param>
        private static void SetNewName(StaticInstance instance)
        {
            string modelName = instance.model.name;
            string groupName = instance.Group; 

            int modelCount = (from obj in groupList[instance.CelestialBody.name][groupName].GetStatics() where obj.model.name == instance.model.name select obj).Count();
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

            foreach (var groupList in groupList.Values)
            {
                foreach (StaticGroup group in groupList.Values)
                {
                    if (activate)
                    {
                        group.ActivateGroupMembers();
                    }
                    else
                    {
                        group.DeactivateGroupMembers();
                    }
                }
            }
		}

        /// <summary>
        /// toggles the visiblility of all statics on a planet
        /// </summary>
        /// <param name="cBody"></param>
        /// <param name="bActive"></param>
        /// <param name="bOpposite"></param>
        internal static void ToggleActiveStaticsOnPlanet(CelestialBody cBody, bool bActive = true, bool bOpposite = false)
		{
            Log.Debug("StaticDatabase.ToggleActiveStaticsOnPlanet " + cBody.bodyName);

			foreach (StaticInstance instance in allStaticInstances)
			{
                if (instance.CelestialBody == cBody)
                {
                    InstanceUtil.SetActiveRecursively(instance, bActive);
                }
                else
                {
                    if (bOpposite)
                    {
                        InstanceUtil.SetActiveRecursively(instance, !bActive);
                    }
                }
            }
		}

        /// <summary>
        /// toggles the visiblility of all statics in a group
        /// </summary>
        /// <param name="sGroup"></param>
        /// <param name="bActive"></param>
        /// <param name="bOpposite"></param>
        internal static void ToggleActiveStaticsInGroup(string sGroup, bool bActive = true, bool bOpposite = false)
		{
            Log.Debug("StaticDatabase.ToggleActiveStaticsInGroup");

			foreach (StaticInstance instance in allStaticInstances)
			{
				if (instance.Group == sGroup)
					InstanceUtil.SetActiveRecursively(instance, bActive);
				else
					if (bOpposite)
						InstanceUtil.SetActiveRecursively(instance, !bActive);
			}
		}

        /// <summary>
        /// Disables all Statics on the current active planet
        /// </summary>
        internal static void DeactivateAllOnPlanet()
		{
			if (activeBodyName == "")
			{
                Log.UserWarning("StaticDatabase.cacheAll() skipped. No activeBodyName.");
				
				return;
			}

			if (groupList.ContainsKey(activeBodyName))
			{
				foreach (StaticGroup group in groupList[activeBodyName].Values)
				{
                    Log.Debug("StaticDatabase.cacheAll(): cacheAll() " + group.name);

                    if (group.isActive)
                    {
                        group.Deactivate();
                    }
				}
			}
		}



        /// <summary>
        /// Handles on what to do when a body changes
        /// </summary>
        /// <param name="body"></param>
        internal static void OnBodyChanged(CelestialBody body)
		{
            DeactivateAllOnPlanet();
            if (body != null)
			{
                if (body.bodyName != activeBodyName)
				{
					activeBodyName = body.name;
				}
			}
			else
			{
                Log.Debug("StaticDatabase.onBodyChanged(): body is null. cacheAll(). Set activeBodyName empty " + activeBodyName);
				activeBodyName = "";
			}
		}

        

        internal static void UpdateCache(Vector3 playerPos)
		{
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
			
			if (groupList.ContainsKey(activeBodyName))
			{
				foreach (StaticGroup group in groupList[activeBodyName].Values)
				{
                        //Log.Normal("Checking Group: " + group.name  ); 
                        var dist = Vector3.Distance(group.groupCenter.gameObject.transform.position, vPlayerPos);
                        bool isClose = (dist < group.visibilityRange);
                       // Log.Debug("StaticDatabase.updateCache(): group visrange is " + group.visibilityRange.ToString() + " for " + group.name);

                        if (group.isActive == false && isClose == true)
                        {
                            group.ActivateGroupMembers();
                        }

                        if (group.isActive == true && isClose == false)
                        {
                            group.DeactivateGroupMembers();
                        }

                    
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
                Log.UserInfo("duplicate model name: " + name + " ,found in: "  + model.configPath + " . This might be OK.");
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
