﻿using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;
using System.Collections.Generic;
using KerbalKonstructs.UI;


namespace KerbalKonstructs.Core
{
    public class LaunchSiteManager
    {
        private static List<KKLaunchSite> launchSites = new List<KKLaunchSite>();
        private static string currentLaunchSite
        {
            get
            {
                return KerbalKonstructs.instance.lastLaunchSiteUsed;
            }
            set
            {
                KerbalKonstructs.instance.lastLaunchSiteUsed = value;
            }
        }
        private static Texture defaultLaunchSiteLogo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);
        public static float rangeNearestOpenBase = 0f;
        public static string nearestOpenBase = "";
        public static float rangeNearestBase = 0f;
        public static string nearestBase = "";

        internal static KKLaunchSite runway = new KKLaunchSite();
        internal static KKLaunchSite launchpad = new KKLaunchSite();

        internal static KKLaunchSite ksc2 = new KKLaunchSite();



        // Handy get of all launchSites
        public static KKLaunchSite[] allLaunchSites = null;


        // API for Kerbal Construction Time not for internal use
        public static List<KKLaunchSite> AllLaunchSites
        {
            get
            {
                return allLaunchSites.ToList();
            }
        }


        private static float getKSCLon
        {
            get
            {
                CelestialBody body = ConfigUtil.GetCelestialBody("HomeWorld");
                var mods = body.pqsController.transform.GetComponentsInChildren(typeof(PQSCity), true);
                double retval = 0d;

                foreach (var m in mods)
                {
                    PQSCity mod = m as PQSCity;
                    if (mod.name == "KSC")
                    {
                        retval = (KKMath.GetLongitudeInDeg(mod.repositionRadial));
                        break;
                    }
                }
                return (float)retval;
            }
        }

        private static float getKSCLat
        {
            get
            {
                CelestialBody body = ConfigUtil.GetCelestialBody("HomeWorld");
                var mods = body.pqsController.transform.GetComponentsInChildren(typeof(PQSCity), true);
                double retval = 0d;

                foreach (var m in mods)
                {
                    PQSCity mod = m as PQSCity;
                    if (mod.name == "KSC")
                    {
                        retval = (KKMath.GetLatitudeInDeg(mod.repositionRadial));
                        break;
                    }
                }
                return (float)retval;
            }
        }

        /// <summary>
        /// prefills LaunchSiteManager with the runway and the KSC
        /// </summary>
        private static void AddKSC()
        {

            runway.LaunchSiteName = "Runway";
            runway.LaunchSiteAuthor = "Squad";
            runway.LaunchSiteType = SiteType.SPH;
            runway.sitecategory = LaunchSiteCategory.Runway;
            runway.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCRunway", false);
            runway.LaunchSiteDescription = "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.";
            runway.body = ConfigUtil.GetCelestialBody("HomeWorld");
            runway.refLat = getKSCLat;
            runway.refLon = getKSCLon;
            runway.refAlt = 69f;
            runway.LaunchSiteLength = 2500f;
            runway.LaunchSiteWidth = 75f;
            runway.InitialCameraRotation = -60f;
            runway.lsGameObject = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>().Where(x => x.name == "ResearchAndDevelopment").First().gameObject;
            runway.SetOpen();

            launchpad.LaunchSiteName = "LaunchPad";
            launchpad.LaunchSiteAuthor = "Squad";
            launchpad.LaunchSiteType = SiteType.VAB;
            launchpad.sitecategory = LaunchSiteCategory.RocketPad;
            launchpad.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/KSCLaunchpad", false);
            launchpad.LaunchSiteDescription = "The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went...";
            launchpad.body = ConfigUtil.GetCelestialBody("HomeWorld");
            launchpad.refLat = getKSCLat;
            launchpad.refLon = getKSCLon;
            launchpad.refAlt = 72;
            launchpad.LaunchSiteLength = 20f;
            launchpad.LaunchSiteWidth = 20f;
            launchpad.InitialCameraRotation = -60f;
            launchpad.lsGameObject = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>().Where(x => x.name == "ResearchAndDevelopment").First().gameObject;
            launchpad.SetOpen();


            AddLaunchSite(runway);
            AddLaunchSite(launchpad);
        }


        public static void AddKSC2()
        {
            CelestialBody body = ConfigUtil.GetCelestialBody("HomeWorld");
            var mods = body.pqsController.transform.GetComponentsInChildren<PQSCity>(true);
            PQSCity ksc2PQS = null;



            foreach (var m in mods)
            {
                if (m.name == "KSC2")
                {
                    ksc2PQS = m;
                    break;
                }
            }

            if (ksc2PQS == null)
            {
                return; 
            }

            StaticInstance ksc2Instance = new StaticInstance();

            ksc2Instance.gameObject = ksc2PQS.gameObject;
            ksc2Instance.hasLauchSites = true;
            ksc2Instance.RadialPosition = ksc2PQS.repositionRadial;
            ksc2Instance.RefLatitude = KKMath.GetLatitudeInDeg(ksc2PQS.repositionRadial);
            ksc2Instance.RefLongitude = KKMath.GetLongitudeInDeg(ksc2PQS.repositionRadial);
            ksc2Instance.CelestialBody = body;


            ksc2.staticInstance = ksc2Instance;
            ksc2.LaunchSiteName = "KSC2";
            ksc2.LaunchPadTransform = "launchpad/PlatformPlane";
            ksc2.LaunchSiteAuthor = "Squad";
            ksc2.logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);
            ksc2.LaunchSiteType = SiteType.VAB;
            ksc2.sitecategory = LaunchSiteCategory.RocketPad;
            ksc2.LaunchSiteDescription = "The hidden KSC2";
            ksc2.body = ConfigUtil.GetCelestialBody("HomeWorld");
            ksc2.refLat = (float)ksc2Instance.RefLatitude;
            ksc2.refLon = (float)ksc2Instance.RefLongitude;
            ksc2.refAlt = (float)ksc2Instance.surfaceHeight;
            ksc2.LaunchSiteLength = 15f;
            ksc2.LaunchSiteWidth = 15f;
            ksc2.InitialCameraRotation = 135f;
            ksc2.lsGameObject = ksc2PQS.gameObject.GetComponentsInChildren<Transform>(true).Where(x => x.name.Equals("launchpad", StringComparison.CurrentCultureIgnoreCase) ).FirstOrDefault().gameObject;
            ksc2.OpenCost = 1f;
            ksc2.SetClosed();
            ksc2.LaunchSiteIsHidden = true;

            ksc2Instance.launchSite = ksc2;
            RegisterLaunchSite(ksc2);

        }


        /// <summary>
        /// contructor
        /// </summary>
        static LaunchSiteManager()
        {
            AddKSC();
            AddKSC2();
        }

        internal static void AttachLaunchSite(StaticInstance instance, ConfigNode instanceNode)
        {
            if (instanceNode.HasValue("LaunchPadTransform") && !string.IsNullOrEmpty(instanceNode.GetValue("LaunchPadTransform")) && instanceNode.HasValue("LaunchSiteName") && !string.IsNullOrEmpty(instanceNode.GetValue("LaunchSiteName")))
            {
                // legacy Launchsite within instanceNode
                CreateLaunchSite(instance, instanceNode);
            }
            else
            {
                // check for new LaunchSite ConfigNode
                if (instanceNode.HasNode("LaunchSite"))
                {
                    ConfigNode lsNode = instanceNode.GetNode("LaunchSite");
                    if (lsNode.HasValue("LaunchPadTransform") && !string.IsNullOrEmpty(lsNode.GetValue("LaunchPadTransform")) && lsNode.HasValue("LaunchSiteName") && !string.IsNullOrEmpty(lsNode.GetValue("LaunchSiteName")))
                    {
                        // legacy Launchsite within instanceNode
                        CreateLaunchSite(instance, lsNode);
                    }
                }
            }
        }

        /// <summary>
        /// Function that is called when a launchSite is opened
        /// </summary>
        internal static void OpenLaunchSite(KKLaunchSite site)
        {
    //        Log.Normal("LSM: OpenLaunchSite");
        }

        /// <summary>
        /// Called when a Launchsite is closed
        /// </summary>
        internal static void CloseLaunchSite(KKLaunchSite site)
        {
     //       Log.Normal("LSM: CloseLaunchSite");
        }

        /// <summary>
        /// (Re)Imports all open LaunchSites to Stock LaunchSite handler
        /// </summary>
        internal static void KKSitesToKSP()
        {
            List<KKLaunchSite> myLaunchSites = new List<KKLaunchSite>();
            Log.Normal("LSM: KKSitesToKSP");

            ClearKSPLaunchSites();

            foreach (KKLaunchSite site in allLaunchSites)
            {
                if (CheckLaunchSiteIsValid(site))
                {
               //     Log.Normal("Added Site to List: " + site.LaunchSiteName);
                    myLaunchSites.Add(site);
                }
            }
        }

        internal static void ClearKSPLaunchSites()
        {
            //stockSite = PSystemSetup.Instance.launchSites;
            foreach (KKLaunchSite site in allLaunchSites)
            {
                //if (stockLaunchSite.contains(site.stockSite))
                //{
                //    stocklaunchSite.Remove(site.Stocksite);
                //}
            }
        }


        /// <summary>
        /// Creates a new LaunchSite out of a cfg-node and Registers it with RegisterLaunchSite
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="cfgNode"></param>
        internal static void CreateLaunchSite(StaticInstance instance, ConfigNode cfgNode)
        {
            KKLaunchSite mySite = new KKLaunchSite();
            mySite.ParseLSConfig(instance,cfgNode);
            instance.hasLauchSites = true;
            instance.launchSite = mySite;
            RegisterLaunchSite(mySite);
        }

        /// <summary>
        /// Registers the a created LaunchSite to the PSystemSetup and LaunchSiteManager
        /// </summary>
        /// <param name="site"></param>
        internal static void RegisterLaunchSite(KKLaunchSite site)
        {
            if (! string.IsNullOrEmpty(site.LaunchSiteName) && site.staticInstance.gameObject.transform.Find(site.LaunchPadTransform) != null)
            {
                site.staticInstance.gameObject.transform.name = site.LaunchSiteName;
                site.staticInstance.gameObject.name = site.LaunchSiteName;

				List<PSystemSetup.SpaceCenterFacility> facilities = PSystemSetup.Instance.SpaceCenterFacilities.ToList();

                if (facilities.Where(fac => fac.facilityName == site.LaunchSiteName).FirstOrDefault() == null )
                {
                    Log.Normal("Registering LaunchSite: " + site.LaunchSiteName);

                    PSystemSetup.SpaceCenterFacility newSpaceCenterFacility = new PSystemSetup.SpaceCenterFacility();


                    newSpaceCenterFacility.name = site.LaunchSiteName;
                    newSpaceCenterFacility.facilityName = site.LaunchSiteName;
                    newSpaceCenterFacility.facilityPQS = site.staticInstance.CelestialBody.pqsController;
                    if (site.staticInstance.groupCenter == null)
                    {
                        newSpaceCenterFacility.facilityTransformName = site.staticInstance.gameObject.name;
                    }
                    else
                    {
                        newSpaceCenterFacility.facilityTransformName = site.staticInstance.groupCenter.gameObject.name + "/" + site.staticInstance.gameObject.name;
                    }

                    //     newFacility.facilityTransformName = instance.gameObject.transform.name;
                    newSpaceCenterFacility.pqsName = site.body.pqsController.name;


                    PSystemSetup.SpaceCenterFacility.SpawnPoint spawnPoint = new PSystemSetup.SpaceCenterFacility.SpawnPoint();
                    spawnPoint.name = site.LaunchSiteName;
                    spawnPoint.spawnTransformURL = site.LaunchPadTransform;

                    newSpaceCenterFacility.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[1];
                    newSpaceCenterFacility.spawnPoints[0] = spawnPoint;


                    newSpaceCenterFacility.Setup(new PQS[] { site.staticInstance.CelestialBody.pqsController });
                    //SetupKSPFacilities();

                    facilities.Add(newSpaceCenterFacility);

					PSystemSetup.Instance.SpaceCenterFacilities = facilities.ToArray();

                    site.facility = newSpaceCenterFacility;
                    //SetupKSPFacilities();
                    AddLaunchSite(site);
                }
                else
                {
                    Log.Error("Launch site " + site.LaunchSiteName + " already exists.");
                }



                if (PSystemSetup.Instance.SpaceCenterFacilities.ToList().Where(fac => fac.facilityName == site.LaunchSiteName).FirstOrDefault() != null)
                {
                    Log.Normal("LaunchSite registered: " + site.LaunchSiteName);
                }
                else
                {
                    Log.Normal("LaunchSite registration failed: " + site.LaunchSiteName);
                }


                    if (site.staticInstance.gameObject != null)
                {                    
                    CustomSpaceCenter.CreateFromLaunchsite(site);
                }

            }
            else
            {
                Log.UserWarning("Launch pad transform \"" + site.LaunchPadTransform + "\" missing for " + site.LaunchSiteName);
            }
        }


        /// <summary>
        /// Removes the launchSite from the facilities
        /// </summary>
        /// <param name="site"></param>
        internal static void UnregisterLaunchSite(KKLaunchSite site)
        {
            if (site.isOpen)
            {
                CloseLaunchSite(site);
            }

            List<PSystemSetup.SpaceCenterFacility> spaceCenters = PSystemSetup.Instance.SpaceCenterFacilities.ToList();
            PSystemSetup.SpaceCenterFacility spaceTodel = spaceCenters.Where(x => x.facilityName == site.LaunchSiteName).FirstOrDefault();

            if (spaceTodel != null)
            {
                spaceCenters.Remove(spaceTodel);
                PSystemSetup.Instance.SpaceCenterFacilities = spaceCenters.ToArray();
                Log.Normal("Launchsite: " + site.LaunchSiteName + " sucessfully unregistered");
            }
        }


        /// <summary>
        /// Deletes a LaunchSite from the internal Database
        /// </summary>
        /// <param name="site2delete"></param>
        internal static void DeleteLaunchSite (KKLaunchSite site2delete)
        {
            if (launchSites.Contains(site2delete))
            {
                launchSites.Remove(site2delete);

                CustomSpaceCenter csc = SpaceCenterManager.GetCSC(site2delete.LaunchSiteName);
                if (csc != null)
                {
                    SpaceCenterManager.spaceCenters.Remove(csc);
                }

                allLaunchSites = launchSites.ToArray();
                UnregisterLaunchSite(site2delete);
            } 
        }

        /// <summary>
        /// Adds a LaunchSite to the internal Database
        /// </summary>
        /// <param name="site2add"></param>
        internal static void AddLaunchSite(KKLaunchSite site2add)
        {

            launchSites.Add(site2add);
            List<KKLaunchSite> tmpList = launchSites.ToList();
            tmpList.Sort(delegate (KKLaunchSite a, KKLaunchSite b)
            {
                return (a.LaunchSiteName).CompareTo(b.LaunchSiteName);
            });
            allLaunchSites = tmpList.ToArray();
        }

        internal static KKLaunchSite GetCurrentLaunchSite()
        {
            Log.Normal("retuning CurrentSite: " + currentLaunchSite);
            return GetLaunchSiteByName(currentLaunchSite);
        }


        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="sState"></param>
        public static void setSiteOpenCloseState(string siteName, string state)
        {
            if (checkLaunchSiteExists(siteName))
            {
                if (state == "Open")
                {
                    GetLaunchSiteByName(siteName).SetOpen();
                }
                if (state == "Closed")
                {
                    GetLaunchSiteByName(siteName).SetClosed();
                }


            }
        }


        internal static void SetupKSPFacilities()
        {

            // Log.Normal("SetupKSPFacilities Called");

            MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupFacilities", BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateSitesMI == null)
            {
                Log.UserError("You are screwed. Failed to find SetupFacilities().");
            }
            else
            {
                updateSitesMI.Invoke(PSystemSetup.Instance, null);
            }
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        public static void setSiteLocked(string siteName)
        {
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        public static void setSiteUnlocked(string siteName)
        {
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="sOpenCloseState"></param>
        /// <param name="fOpenCost"></param>
        public static void getSiteOpenCloseState(string siteName, out string sOpenCloseState, out float fOpenCost)
        {
            if (checkLaunchSiteExists(siteName))
            {
                KKLaunchSite site = GetLaunchSiteByName(siteName);
                sOpenCloseState = site.OpenCloseState;
                fOpenCost = site.OpenCost;
            }
            else
            {
                sOpenCloseState = "Open";
                fOpenCost = 0;
            }
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool getIsSiteLocked(string siteName)
        {
            return false;
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool getIsSiteOpen(string siteName)
        {
            if (checkLaunchSiteExists(siteName))
            {
                return GetLaunchSiteByName(siteName).isOpen;             
            } else
            {
                return false;
            }
        }

        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool getIsSiteClosed(string siteName)
        {
            if (checkLaunchSiteExists(siteName))
            {
                return (!GetLaunchSiteByName(siteName).isOpen);

            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Contract configurator API call
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool checkLaunchSiteExists(string siteName)
        {
            return (launchSites.Where(x => x.LaunchSiteName.Equals(siteName,StringComparison.InvariantCultureIgnoreCase)).Count() > 0);
        }


        // Returns a specific Launchsite, keyed by site.name
        public static KKLaunchSite GetLaunchSiteByName(string siteName)
        {
            KKLaunchSite mySite = null;
            if (checkLaunchSiteExists(siteName))
            {
                
                foreach (KKLaunchSite site in allLaunchSites)
                {
                    if (site.LaunchSiteName.Equals(siteName))
                    {
                        mySite = site;
                    }
                }
                Log.Normal("Returning LS:" + mySite.LaunchSiteName);
                return mySite;
            }
            else
            {
                Log.UserError("Could not find Launchsite in list: " + siteName);
                return null;
            }
        }


        // Returns the distance in m from a position to a specified Launchsite
        public static float getDistanceToBase(Vector3 position, KKLaunchSite site)
        {
            return Vector3.Distance(position, site.lsGameObject.transform.position);
        }

        // Returns the nearest open Launchsite to a position and range to the Launchsite in m
        // The basic ATC feature is in here for now
        public static void GetNearestOpenBase(Vector3 position, out string sBase, out float flRange, out KKLaunchSite lNearest)
        {
            SpaceCenter KSC = SpaceCenter.Instance;
            var smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            string sNearestBase = "";
            KKLaunchSite lNearestBase = null;
            KKLaunchSite lKSC = null;

            foreach (KKLaunchSite site in allLaunchSites)
            {

                if (site.isOpen)
                {
                    var radialposition = site.lsGameObject.transform.position;
                    var dist = Vector3.Distance(position, radialposition);

                    if (site.LaunchSiteName == "Runway")
                    {
                        if (lNearestBase == null)
                        {
                            lNearestBase = site;
                        }

                        lKSC = site;
                    }
                    else
                        if (site.LaunchSiteName != "LaunchPad")
                    {
                        if ((float)dist < (float)smallestDist)
                        {
                            {
                                sNearestBase = site.LaunchSiteName;
                                lNearestBase = site;
                                smallestDist = dist;
                            }
                        }
                    }
                    else
                    {
                        lKSC = site;
                    }
                }
            }

            if (sNearestBase == "")
            {
                sNearestBase = "KSC";
                lNearestBase = lKSC;
            }

            rangeNearestOpenBase = (float)smallestDist;

            // Air traffic control messaging
            if (LandingGuideUI.instance.IsOpen())
            {
                if (sNearestBase != nearestOpenBase)
                {
                    if (rangeNearestOpenBase < 25000)
                    {
                        nearestOpenBase = sNearestBase;
                        MessageSystemButton.MessageButtonColor color = MessageSystemButton.MessageButtonColor.BLUE;
                        MessageSystem.Message m = new MessageSystem.Message("KK ATC", "You have entered the airspace of " + sNearestBase + " ATC. Please keep this channel open and obey all signal lights. Thank you. " + sNearestBase + " Air Traffic Control out.", color, MessageSystemButton.ButtonIcons.MESSAGE);
                        MessageSystem.Instance.AddMessage(m);
                    }
                    else
                        if (nearestOpenBase != "")
                    {
                        // you have left ...
                        MessageSystemButton.MessageButtonColor color = MessageSystemButton.MessageButtonColor.GREEN;
                        MessageSystem.Message m = new MessageSystem.Message("KK ATC", "You are now leaving the airspace of " + sNearestBase + ". Safe journey. " + sNearestBase + " Air Traffic Control out.", color, MessageSystemButton.ButtonIcons.MESSAGE);
                        MessageSystem.Instance.AddMessage(m);
                        nearestOpenBase = "";
                    }
                }
            }

            sBase = sNearestBase;
            flRange = rangeNearestOpenBase;
            lNearest = lNearestBase;
        }

        // Returns the nearest Launchsite to a position and range in m to the Launchsite, regardless of whether it is open or closed
        public static void getNearestBase(Vector3 position, out string sBase, out string sBase2, out float flRange, out KKLaunchSite lSite, out KKLaunchSite lSite2)
        {
            SpaceCenter KSC = SpaceCenter.Instance;
            var smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            var lastSmallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
            string sNearestBase = "";
            KKLaunchSite lTargetSite = null;
            KKLaunchSite lLastSite = null;
            KKLaunchSite lKSC = null;
            string sLastNearest = "";


            foreach (KKLaunchSite site in allLaunchSites)
            {
                if (site.lsGameObject == null) continue;

                var radialposition = site.lsGameObject.transform.position;
                var dist = Vector3.Distance(position, radialposition);

                if (radialposition == position) continue;

                if (site.LaunchSiteName == "Runway" || site.LaunchSiteName == "LaunchPad")
                {
                    lKSC = site;
                }
                else
                {
                    if ((float)dist < (float)smallestDist)
                    {
                        sLastNearest = sNearestBase;
                        lLastSite = lTargetSite;
                        lastSmallestDist = smallestDist;
                        sNearestBase = site.LaunchSiteName;
                        smallestDist = dist;
                        lTargetSite = site;
                    }
                    else if (dist < lastSmallestDist)
                    {
                        sLastNearest = site.LaunchSiteName;
                        lastSmallestDist = dist;
                        lLastSite = site;
                    }
                }
            }

            if (sNearestBase == "")
            {
                sNearestBase = "KSC";
                lTargetSite = lKSC;
            }
            if (sLastNearest == "")
            {
                sLastNearest = "KSC";
                lLastSite = lKSC;
            }

            rangeNearestBase = (float)smallestDist;

            sBase = sNearestBase;
            sBase2 = sLastNearest;
            flRange = rangeNearestBase;
            lSite = lTargetSite;
            lSite2 = lLastSite;
        }

        // Pokes KSP to change the launchsite to use. There's near hackery here again that may get broken by Squad
        // This only works because they use multiple variables to store the same value, basically its black magic
        // Original author: medsouz
        public static void setLaunchSite(KKLaunchSite site)
        {
            if (site.facility != null)
            {
                if (EditorDriver.editorFacility == EditorFacility.SPH)
                {
                    site.facility.name = "Runway";
                }
                else
                {
                    site.facility.name = "LaunchPad";
                }
            }
            Log.Normal("Setting LaunchSite to " + site.LaunchSiteName);
            currentLaunchSite = site.LaunchSiteName;
            EditorLogic.fetch.launchSiteName = site.LaunchSiteName;

            KerbalKonstructs.instance.lastLaunchSiteUsed = site.LaunchSiteName;
        }



        // Returns the internal launchSite that KSP has been told is the launchsite
        public static string getCurrentLaunchSite()
        {
            return currentLaunchSite;

        }

        internal static bool CheckLaunchSiteIsValid(KKLaunchSite site)
        {
            if (!KerbalKonstructs.instance.launchFromAnySite && (EditorDriver.editorFacility == EditorFacility.VAB) && (site.LaunchSiteType == SiteType.SPH))
            {
                return false;
            }
            if (!KerbalKonstructs.instance.launchFromAnySite && (EditorDriver.editorFacility == EditorFacility.SPH) && (site.LaunchSiteType == SiteType.VAB))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the currently available default LaunchSite in a editor
        /// </summary>
        /// <returns></returns>
        internal static KKLaunchSite GetDefaultSite()
        {
            KKLaunchSite defaultSite = null;
            if (EditorDriver.editorFacility == EditorFacility.VAB)
            {
                try
                {

                    defaultSite = GetLaunchSiteByName(KerbalKonstructs.instance.defaultVABlaunchsite);
                    if (defaultSite != null && defaultSite.isOpen)
                    {
                        return defaultSite;
                    } else
                    {
                        Log.UserError("DefaultSite is null");
                        defaultSite = GetLaunchSiteByName("LaunchPad");
                        KerbalKonstructs.instance.defaultSPHlaunchsite = "LaunchPad";
                        return defaultSite;
                    }

                }
                catch
                {

                    Log.Error("LaunchSite is broken");
                    defaultSite = GetLaunchSiteByName("LaunchPad");
                    KerbalKonstructs.instance.defaultSPHlaunchsite = "LaunchPad";
                    return defaultSite;
                }
            }

            else 
            {
                try
                {
                    defaultSite = GetLaunchSiteByName(KerbalKonstructs.instance.defaultSPHlaunchsite);
                    if (defaultSite.isOpen)
                    {
                        Log.Normal("defaultSite retuned: " + defaultSite.LaunchSiteName);
                        return defaultSite;
                    }
                    else
                    {
                        Log.Error("LaunchSite is invalid");
                        defaultSite = GetLaunchSiteByName("Runway");
                        KerbalKonstructs.instance.defaultSPHlaunchsite = "Runway";
                        return defaultSite;
                    }
                } catch
                {
                    Log.Error("LaunchSite is broken");
                    defaultSite = GetLaunchSiteByName("Runway");
                    KerbalKonstructs.instance.defaultSPHlaunchsite = "Runway";
                    return defaultSite;
                }

            }
        }

    }
}