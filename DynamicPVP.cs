using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("DynamicPVP", "CatMeat", "2.2.2", ResourceId = 2728)]
    [Description("Create temporary PVP zones around SupplyDrops, LockedCrates, APC and/or Heli")]

    public class DynamicPVP : RustPlugin
    {
        #region References
        [PluginReference]
        Plugin ZoneManager, TruePVE, ZoneDomes, BotSpawn;
        #endregion

        #region Declarations
        bool starting = true;
        bool validcommand;
        bool configChanged;

        float compareRadius = 50;
        float zoneRadius;
        float zoneDuration;
        string botProfile;

        string msg;
        string pluginVersion = "";
        string debugfilename = "debug";

        List<BaseEntity> activeSupplySignals = new List<BaseEntity>();
        Dictionary<string, Vector3> ActiveDynamicZones = new Dictionary<string, Vector3>();
        ConsoleSystem.Arg arguments;

        // Start Configuration Defaults
        bool blockTeleport = true;
        bool botsEnabled = true;
        string configVersion = "";
        bool debugEnabled = true;
        string extraZoneFlags = "";
        string msgEnter = "Entering a PVP area!";
        string msgLeave = "Leaving a PVP area.";
        bool pluginEnabled = true;

        bool domesEnabled = true;
        int domesSphereDarkness = 1;

        bool apcEnabled = true;
        float apcRadius = 100.0f;
        float apcDuration = 600.0f;
        string apcBotProfile = "DynamicPVP";

        bool ch47Enabled = true;
        float ch47Radius = 100.0f;
        float ch47Duration = 600.0f;
        string ch47BotProfile = "DynamicPVP";

        bool heliEnabled = true;
        float heliRadius = 100.0f;
        float heliDuration = 600.0f;
        string heliBotProfile = "DynamicPVP";

        bool signalEnabled = false;
        float signalRadius = 100.0f;
        float signalDuration = 600.0f;
        string signalBotProfile = "";

        bool timedCrateEnabled = true;
        float timedCrateRadius = 100.0f;
        float timedCrateDuration = 1200.0f;
        string timedCrateBotProfile = "DynamicPVP";

        bool timedDropEnabled = true;
        float timedDropRadius = 100.0f;
        float timedDropDuration = 600.0f;
        string timedDropBotProfile = "DynamicPVP";
        // End Configuration Defaults
        #endregion

        #region Plugin Initialization

        private bool ZoneCreateAllowed()
        {
            Plugin ZoneManager = (Plugin)plugins.Find("ZoneManager");
            Plugin TruePVE = (Plugin)plugins.Find("TruePVE");

            if ((TruePVE != null) && (ZoneManager != null))
                if (pluginEnabled)
                    return true;
            return false;
        }
        private bool BotSpawnAllowed()
        {
            Plugin BotSpawn = (Plugin)plugins.Find("BotSpawn");

            if (BotSpawn != null && botsEnabled) return true;
            return false;
        }
        private bool DomeCreateAllowed()
        {
            Plugin ZoneDomes = (Plugin)plugins.Find("ZoneDomes");

            if (ZoneDomes != null && domesEnabled) return true;
            return false;
        }

        void SetConfig(string categoryName, string settingName, object newValue)
        {
            Dictionary<string, object> data = Config[categoryName] as Dictionary<string, object>;
            data[settingName] = newValue;
            SaveConfig();
        }

        void Init() => LoadSettings();

        void Unload()
        {
            //List<string> keys = new List<string>(ActiveDynamicZones.Keys);
            //if (keys.Count > 0) DebugPrint($"Deleting {keys.Count} ActiveZones", false);
            //foreach (string key in keys) DeleteDynZone(key);
        }

        protected override void LoadDefaultConfig()
        {
            Config.Clear();
            LoadSettings();
        }

        object GetConfig(string categoryName, string settingName, object defaultValue)
        {
            Dictionary<string, object> data = Config[categoryName] as Dictionary<string, object>;

            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[categoryName] = data;
                configChanged = true;
            }

            object value;

            if (!data.TryGetValue(settingName, out value))
            {
                value = defaultValue;
                data[settingName] = value;
                configChanged = true;
            }
            return value;
        }

        void LoadSettings()
        {
            pluginVersion = this.Version.ToString();

            Dictionary<string, object> checkConfig = (Config.Get("General") as Dictionary<string, object>);

            if (checkConfig != null)
            {
                configVersion = Convert.ToString(GetConfig("General", "configVersion", pluginVersion));
                if (configVersion != pluginVersion)
                {
                    //DebugPrint("Old version 2 configuration.", false);
                }
                else
                {
                    //DebugPrint("Current Version 2 configuration... continue loading", false);
                }
            }
            else
            {
                DebugPrint("Deprecated version 1 configuration, Create new config file.", false);
                Config.Clear();
            }

            blockTeleport = Convert.ToBoolean(GetConfig("General", "blockTeleport", blockTeleport));
            botsEnabled = Convert.ToBoolean(GetConfig("General", "botsEnabled", botsEnabled));
            configVersion = Convert.ToString(GetConfig("General", "configVersion", configVersion));
            debugEnabled = Convert.ToBoolean(GetConfig("General", "debugEnabled", debugEnabled));
            extraZoneFlags = Convert.ToString(GetConfig("General", "extraZoneFlags", extraZoneFlags));
            msgEnter = Convert.ToString(GetConfig("General", "msgEnter", msgEnter));
            msgLeave = Convert.ToString(GetConfig("General", "msgLeave", msgLeave));
            pluginEnabled = Convert.ToBoolean(GetConfig("General", "pluginEnabled", pluginEnabled));

            domesEnabled = Convert.ToBoolean(GetConfig("ZoneDomes", "domesEnabled", domesEnabled));
            domesSphereDarkness = Convert.ToInt32(GetConfig("ZoneDomes", "domesSphereDarkness", domesSphereDarkness));

            apcEnabled = Convert.ToBoolean(GetConfig("BradleyAPC", "apcEnabled", apcEnabled));
            apcRadius = Convert.ToSingle(GetConfig("BradleyAPC", "apcRadius", apcRadius));
            apcDuration = Convert.ToSingle(GetConfig("BradleyAPC", "apcDuration", apcDuration));
            apcBotProfile = Convert.ToString(GetConfig("BradleyAPC", "apcBotProfile", apcBotProfile));

            ch47Enabled = Convert.ToBoolean(GetConfig("CH47Chinook", "ch47Enabled", ch47Enabled));
            ch47Radius = Convert.ToSingle(GetConfig("CH47Chinook", "ch47Radius", ch47Radius));
            ch47Duration = Convert.ToSingle(GetConfig("CH47Chinook", "ch47Duration", ch47Duration));
            ch47BotProfile = Convert.ToString(GetConfig("CH47Chinook", "ch47BotProfile", ch47BotProfile));

            heliEnabled = Convert.ToBoolean(GetConfig("PatrolHelicopter", "heliEnabled", heliEnabled));
            heliRadius = Convert.ToSingle(GetConfig("PatrolHelicopter", "heliRadius", heliRadius));
            heliDuration = Convert.ToSingle(GetConfig("PatrolHelicopter", "heliDuration", heliDuration));
            heliBotProfile = Convert.ToString(GetConfig("PatrolHelicopter", "heliBotProfile", heliBotProfile));

            signalEnabled = Convert.ToBoolean(GetConfig("SupplySignal", "signalEnabled", signalEnabled));
            signalRadius = Convert.ToSingle(GetConfig("SupplySignal", "signalRadius", signalRadius));
            signalDuration = Convert.ToSingle(GetConfig("SupplySignal", "signalDuration", signalDuration));
            signalBotProfile = Convert.ToString(GetConfig("SupplySignal", "signalBotProfile", signalBotProfile));

            timedCrateEnabled = Convert.ToBoolean(GetConfig("TimedCrate", "timedCrateEnabled", timedCrateEnabled));
            timedCrateRadius = Convert.ToSingle(GetConfig("TimedCrate", "timedCrateRadius", timedCrateRadius));
            timedCrateDuration = Convert.ToSingle(GetConfig("TimedCrate", "timedCrateDuration", timedCrateDuration));
            timedCrateBotProfile = Convert.ToString(GetConfig("TimedCrate", "timedCrateBotProfile", timedCrateBotProfile));

            timedDropEnabled = Convert.ToBoolean(GetConfig("TimedDrop", "timedDropEnabled", timedDropEnabled));
            timedDropRadius = Convert.ToSingle(GetConfig("TimedDrop", "timedDropRadius", timedDropRadius));
            timedDropDuration = Convert.ToSingle(GetConfig("TimedDrop", "timedDropDuration", timedDropDuration));
            timedDropBotProfile = Convert.ToString(GetConfig("TimedDrop", "timedDropBotProfile", timedDropBotProfile));

            if (!configChanged) return;
            SaveConfig();
            configChanged = false;
        }
        #endregion

        #region Commands
        [ChatCommand("dynpvp")]
        private void CmdChatCommand(BasePlayer player, string command, string[] args)
        {
            if (player?.net?.connection != null && player.net.connection.authLevel > 0)
            {
                if (args.Count() != 2)
                {
                    ProcessCommand(player, "list", "");
                    return;
                }
                ProcessCommand(player, args[0], args[1]);
            }
        }

        [ConsoleCommand("dynpvp")]
        private void CmdConsoleCommand(ConsoleSystem.Arg arg)
        {
            arguments = arg; //save for responding later
            if (arg.Args == null || arg.Args.Length != 2)
            {
                ProcessCommand(null, "list", "");
                return;
            }
            ProcessCommand(null, arg.Args[0], arg.Args[1]);
        }

        private void ProcessCommand(BasePlayer player, string command, string value)
        {
            var commandToLower = command.Trim().ToLower();
            var valueToLower = value.Trim().ToLower();
            float numberValue;
            var number = Single.TryParse(value, out numberValue);

            validcommand = true;

            switch (commandToLower)
            {
                case "botsenabled":
                    switch (valueToLower)
                    {
                        case "true":
                            botsEnabled = true;
                            break;
                        case "false":
                            botsEnabled = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("General", "botsEnabled", botsEnabled);
                    }
                    break;

                case "extrazoneflags":
                    extraZoneFlags = value;
                    SetConfig("General", "extraZoneFlags", extraZoneFlags);
                    break;
                case "pluginenabled":
                    switch (valueToLower)
                    {
                        case "true":
                            pluginEnabled = true;
                            break;
                        case "false":
                            pluginEnabled = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("General", "pluginEnabled", pluginEnabled);
                    }
                    break;
                case "msgenter":
                    msgEnter = value;
                    SetConfig("General", "msgEnter", msgEnter);
                    break;
                case "msgleave":
                    msgLeave = value;
                    SetConfig("General", "msgLeave", msgLeave);
                    break;
                case "timedcrateenabled":
                    switch (valueToLower)
                    {
                        case "true":
                            timedCrateEnabled = true;
                            break;
                        case "false":
                            timedCrateEnabled = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("TimedCrate", "timedCrateEnabled", timedCrateEnabled);
                    }
                    break;
                case "timedcrateradius":
                    //rtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        timedDropRadius = numberValue;
                        SetConfig("TimedCrate", "timedCrateRadius", timedCrateRadius);
                    }
                    else validcommand = false;
                    break;
                case "timedcrateduration":
                    //dtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        timedDropDuration = numberValue;
                        SetConfig("TimedCrate", "timedCrateDuration", timedCrateDuration);
                    }
                    else validcommand = false;
                    break;
                case "timeddropenabled":
                    switch (valueToLower)
                    {
                        case "true":
                            timedDropEnabled = true;
                            break;
                        case "false":
                            timedDropEnabled = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("TimedDrop", "timedDropEnabled", timedDropEnabled);
                    }
                    break;
                case "timeddropradius":
                    //rtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        timedDropRadius = numberValue;
                        SetConfig("TimedDrop", "timedDropRadius", timedDropRadius);
                    }
                    else validcommand = false;
                    break;
                case "timeddropduration":
                    //dtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        timedDropDuration = numberValue;
                        SetConfig("TimedDrop", "timedDropDuration", timedDropDuration);
                    }
                    else validcommand = false;
                    break;
                case "signalenabled":
                    switch (valueToLower)
                    {
                        case "true":
                            signalEnabled = true;
                            break;
                        case "false":
                            signalEnabled = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("SupplySignal", "signalEnabled", signalEnabled);
                    }
                    break;
                case "signalradius":
                    //rtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        signalRadius = numberValue;
                        SetConfig("SupplySignal", "signalRadius", signalRadius);
                    }
                    else validcommand = false;
                    break;
                case "signalduration":
                    //dtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        signalDuration = numberValue;
                        SetConfig("SupplySignal", "signalDuration", signalDuration);
                    }
                    else validcommand = false;
                    break;
                case "apcenabled":
                    switch (valueToLower)
                    {
                        case "true":
                            apcEnabled = true;
                            break;
                        case "false":
                            apcEnabled = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("BradleyAPC", "apcEnabled", apcEnabled);
                    }
                    break;
                case "apcradius":
                    //rtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        apcRadius = numberValue;
                        SetConfig("BradleyAPC", "apcRadius", apcRadius);
                    }
                    else validcommand = false;
                    break;
                case "apcduration":
                    //dtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        apcDuration = numberValue;
                        SetConfig("BradleyAPC", "apcDuration", apcDuration);
                    }
                    else validcommand = false;
                    break;

                //case "ch47enabled":
                //    switch (valueToLower)
                //    {
                //        case "true":
                //            ch47Enabled = true;
                //            break;
                //        case "false":
                //            ch47Enabled = false;
                //            break;
                //        default:
                //            validcommand = false;
                //            break;
                //    }
                //    if (validcommand)
                //    {
                //        SetConfig("CH47Chinook", "ch47enabled", ch47Enabled);
                //    }
                //    break;
                //case "ch47radius":
                //    //rtemp = Convert.ToSingle(value);
                //    if (number && numberValue > 0)
                //    {
                //        ch47Radius = numberValue;
                //        SetConfig("CH47Chinook", "ch47radius", ch47Radius);
                //    }
                //    else validcommand = false;
                //    break;
                //case "ch47duration":
                //    //dtemp = Convert.ToSingle(value);
                //    if (number && numberValue > 0)
                //    {
                //        ch47Duration = numberValue;
                //        SetConfig("CH47Chinook", "ch47duration", ch47Duration);
                //    }
                //    else validcommand = false;
                //    break;

                case "helienabled":
                    switch (valueToLower)
                    {
                        case "true":
                            heliEnabled = true;
                            break;
                        case "false":
                            heliEnabled = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("PatrolHelicopter", "heliEnabled", heliEnabled);
                    }
                    break;
                case "heliradius":
                    //rtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        heliRadius = numberValue;
                        SetConfig("PatrolHelicopter", "heliRadius", heliRadius);
                    }
                    else validcommand = false;
                    break;
                case "heliduration":
                    //dtemp = Convert.ToSingle(value);
                    if (number && numberValue > 0)
                    {
                        heliDuration = numberValue;
                        SetConfig("PatrolHelicopter", "heliDuration", heliDuration);
                    }
                    else validcommand = false;
                    break;

                case "debugenabled":
                    switch (valueToLower)
                    {
                        case "true":
                            debugEnabled = true;
                            break;
                        case "false":
                            debugEnabled = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("General", "debugEnabled", debugEnabled);
                    }
                    break;
                case "domesenabled":
                    switch (valueToLower)
                    {
                        case "true":
                            domesEnabled = true;
                            break;
                        case "false":
                            domesEnabled = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("ZoneDomes", "domesEnabled", domesEnabled);
                    }
                    break;
                case "domesspheredarkness":
                    if (number && numberValue > 0)
                    {
                        domesSphereDarkness = Convert.ToInt32(numberValue);
                        SetConfig("ZoneDomes", "domesSphereDarkness", domesSphereDarkness);
                    }
                    else validcommand = false;
                    break;
                case "blockteleport":
                    switch (valueToLower)
                    {
                        case "true":
                            blockTeleport = true;
                            break;
                        case "false":
                            blockTeleport = false;
                            break;
                        default:
                            validcommand = false;
                            break;
                    }
                    if (validcommand)
                    {
                        SetConfig("General", "blockTeleport", blockTeleport);
                    }
                    break;
                case "list":
                    // valid command but process later in method
                    break;
                default:
                    validcommand = false;
                    break;
            }
            if (validcommand)
            {
                if (command != "list")
                    RespondWith(player, "DynamicPVP: " + command + " set to: " + value);
                else
                {
                    msg = "DynamicPVP current settings =====================";

                    msg = msg + "\n       pluginEnabled: " + pluginEnabled.ToString();
                    msg = msg + "\n        debugEnabled: " + debugEnabled.ToString();
                    msg = msg + "\n       blockTeleport: " + blockTeleport.ToString();
                    msg = msg + "\n         botsEnabled: " + botsEnabled.ToString();
                    msg = msg + "\n      extraZoneFlags: " + extraZoneFlags;
                    msg = msg + "\n            msgEnter: " + msgEnter;
                    msg = msg + "\n            msgLeave: " + msgLeave;

                    msg = msg + "\n\n        domesEnabled: " + domesEnabled.ToString();
                    msg = msg + "\ndomesSphereDarkness : " + domesSphereDarkness.ToString();

                    msg = msg + "\n\n          apcEnabled: " + apcEnabled.ToString();
                    msg = msg + "\n          apcRadius : " + apcRadius.ToString();
                    msg = msg + "\n        apcDuration : " + apcDuration.ToString();
                    msg = msg + "\n      apcBotProfile : " + apcBotProfile;

                    msg = msg + "\n\n         ch47Enabled: " + ch47Enabled.ToString();
                    msg = msg + "\n         ch47Radius : " + ch47Radius.ToString();
                    msg = msg + "\n       ch47Duration : " + ch47Duration.ToString();
                    msg = msg + "\n     ch47BotProfile : " + ch47BotProfile;

                    msg = msg + "\n\n          heliEnabled: " + heliEnabled.ToString();
                    msg = msg + "\n          heliRadius : " + heliRadius.ToString();
                    msg = msg + "\n        heliDuration : " + heliDuration.ToString();
                    msg = msg + "\n      heliBotProfile : " + heliBotProfile;

                    msg = msg + "\n\n        signalEnabled: " + signalEnabled.ToString();
                    msg = msg + "\n        signalRadius : " + signalRadius.ToString();
                    msg = msg + "\n      signalDuration : " + signalDuration.ToString();
                    msg = msg + "\n    signalBotProfile : " + signalBotProfile;

                    msg = msg + "\n\n    timedCrateEnabled: " + timedCrateEnabled.ToString();
                    msg = msg + "\n    timedCrateRadius : " + timedCrateRadius.ToString();
                    msg = msg + "\n  timedCrateDuration : " + timedCrateDuration.ToString();
                    msg = msg + "\ntimedCrateBotProfile : " + timedCrateBotProfile;

                    msg = msg + "\n\n     timedDropEnabled: " + timedDropEnabled.ToString();
                    msg = msg + "\n     timedDropRadius : " + timedDropRadius.ToString();
                    msg = msg + "\n   timedDropDuration : " + timedDropDuration.ToString();
                    msg = msg + "\n timedDropBotProfile : " + timedDropBotProfile;

                    msg = msg + "\n==============================================\n";
                    RespondWith(player, msg);
                }
            }
            else
                RespondWith(player, $"Syntax error! ({command}:{value})");
        }
        #endregion

        #region OxideHooks
        void OnServerInitialized()
        {
            starting = false;
            DeleteOldZones();
        }
        void OnEntitySpawned(BaseNetworkable entity)
        {
            if (!pluginEnabled || starting || entity == null || entity.IsDestroyed) return;
            switch (entity.ShortPrefabName)
            {
                case "supply_drop":
                    if (IsProbablySupplySignal(entity.transform.position))
                    {
                        if (!signalEnabled) return;
                        zoneRadius = signalRadius;
                        zoneDuration = signalDuration;
                        botProfile = signalBotProfile;
                        break;
                    }
                    else
                    {
                        if (!timedDropEnabled) return;
                        zoneRadius = timedDropRadius;
                        zoneDuration = timedDropDuration;
                        botProfile = timedDropBotProfile;
                        break;
                    }
                case "codelockedhackablecrate":
                    if (!timedCrateEnabled) return;
                    zoneRadius = timedCrateRadius;
                    zoneDuration = timedCrateDuration;
                    botProfile = timedCrateBotProfile;
                    break;
                default:
                    return;
            }
            Vector3 DynPosition = entity.transform.position;
            DynPosition.y = TerrainMeta.HeightMap.GetHeight(DynPosition);
            CreateDynZone(DynPosition, zoneRadius, zoneDuration, botProfile);
        }
        void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (!pluginEnabled || starting || entity == null || entity.IsDestroyed) return;
            switch (entity.ShortPrefabName)
            {
                case "patrolhelicopter":
                    if (!heliEnabled) return;
                    zoneRadius = heliRadius;
                    zoneDuration = heliDuration;
                    botProfile = heliBotProfile;
                    break;
                case "bradleyapc":
                    if (!apcEnabled) return;
                    zoneRadius = apcRadius;
                    zoneDuration = apcDuration;
                    botProfile = apcBotProfile;
                    break;
                default:
                    return;
            }

            Vector3 DynPosition = entity.transform.position;
            DynPosition.y = TerrainMeta.HeightMap.GetHeight(DynPosition);
            CreateDynZone(DynPosition, zoneRadius, zoneDuration, botProfile);
        }
        #endregion

        #region ZoneHandling
        void CreateDynZone(Vector3 DynPosition, float _radius, float _duration, string _profile)
        {
            if (ZoneCreateAllowed())
            {
                string DynZoneID = DateTime.Now.ToString("HHmmssff");
                string _ZoneFlags =
                    $"name,DynamicPVP,radius,{_radius.ToString()},enter_message,\"{msgEnter}\",leave_message,\"{msgLeave}\",undestr,true,notp,{blockTeleport.ToString()}";
                if (!String.IsNullOrEmpty(extraZoneFlags))
                {
                    string _extraZoneFlags = extraZoneFlags.Replace(' ', ',');
                    _ZoneFlags = _ZoneFlags + "," + _extraZoneFlags;
                }
                List<string> DynArgs = _ZoneFlags.Split(',').ToList();
                string[] DynZoneArgs = DynArgs.ToArray();

                bool ZoneAdded = AddZone(DynZoneID, DynZoneArgs, DynPosition);
                if (ZoneAdded)
                {
                    DebugPrint($"Created DynamicPVP Zone: {DynZoneID} ({DynPosition}, {_radius.ToString()}, {_duration.ToString()})", false);
                    ActiveDynamicZones.Add(DynZoneID, DynPosition);
                    bool MappingAdded = AddMapping(DynZoneID);
                    if (!MappingAdded) DebugPrint("ERROR: PVP Mapping failed.", true);
                    if (DomeCreateAllowed())
                    {
                        bool DomeAdded = AddDome(DynZoneID);
                        if (!DomeAdded) DebugPrint("ERROR: Dome NOT added for Zone: " + DynZoneID, true);
                    }
                    if (BotSpawnAllowed())
                    {
                        DebugPrint($"SpawnBots: {DynPosition}, {_profile})", false);
                        string[] result = SpawnBots(DynPosition, _profile, DynZoneID);
                        if (result[0] == "false") DebugPrint($"ERROR: Bot spawn failed: {result[1]}", true);
                    }
                    timer.Once(_duration, () => { DeleteDynZone(DynZoneID); });
                }
                else DebugPrint("ERROR: Zone creation failed.", true);
            }
        }
        void DeleteDynZone(string DynZoneID)
        {
            if (ZoneCreateAllowed())
            {
                if (BotSpawnAllowed())
                {
                    DebugPrint($"Removing Bots: {DynZoneID}", false);
                    string[] result = RemoveBots(DynZoneID);
                    if (result[0] == "false") DebugPrint($"ERROR: Bot spawn failed: {result[1]}", true);
                }
                if (DomeCreateAllowed())
                {
                    bool DomeRemoved = RemoveDome(DynZoneID);
                    if (!DomeRemoved) DebugPrint("ERROR: Dome NOT removed for Zone: " + DynZoneID, true);
                }
                bool MappingRemoved = RemoveMapping(DynZoneID);
                if (!MappingRemoved) DebugPrint("ERROR: PVP NOT disabled for Zone: " + DynZoneID, true);

                bool ZoneRemoved = RemoveZone(DynZoneID);
                if (!ZoneRemoved) DebugPrint("ERROR: Zone removal failed.", true);
                DebugPrint($"Deleted DynamicPVP Zone: {DynZoneID}", false);
                ActiveDynamicZones.Remove(DynZoneID);
            }
        }
        void DeleteOldZones()
        {
            int _count = 0;
            string[] ZoneIDs = (string[])ZoneManager?.Call("GetZoneIDs");
            if (ZoneIDs != null)
            {
                for (int i = 0; i < ZoneIDs.Length; i++)
                {
                    string zoneName = (string)ZoneManager?.Call($"GetZoneName({ZoneIDs[i]}");
                    if (zoneName == "DynamicPVP")
                    {
                        DeleteDynZone(ZoneIDs[i]);
                        _count++;
                        DebugPrint($"Zone [{i + 1}/{ZoneIDs.Length}]: {ZoneIDs[i]} ({zoneName})... deleted.", false);
                    }
                    else DebugPrint($"Zone [{i + 1}/{ZoneIDs.Length}]: {ZoneIDs[i]} ({zoneName})... skipped.", false);
                }
                if (_count > 0) DebugPrint($"Deleted {_count} existing DynamicPVP zones", false);
                return;
            }
            DebugPrint($"No existing DynamicPVP zones", false);
            return;
        }
        #endregion

        #region ExternalHooks
        // TruePVE API
        bool AddMapping(string zoneID) => (bool)TruePVE?.Call("AddOrUpdateMapping", zoneID, "exclude");
        bool RemoveMapping(string zoneID) => (bool)TruePVE?.Call("RemoveMapping", zoneID);

        // ZoneDomes API
        bool AddDome(string zoneID) => (bool)ZoneDomes?.Call("AddNewDome", null, zoneID);
        bool RemoveDome(string zoneID) => (bool)ZoneDomes?.Call("RemoveExistingDome", null, zoneID);

        // ZoneManager API
        bool AddZone(string zoneID, string[] zoneArgs, Vector3 zoneLocation) => (bool)ZoneManager?.Call("CreateOrUpdateZone", zoneID, zoneArgs, zoneLocation);
        bool RemoveZone(string zoneID) => (bool)ZoneManager?.Call("EraseZone", zoneID);

        // BotSpawn API
        string[] SpawnBots(Vector3 zoneLocation, string zoneProfile, string zoneGroupID) => (String[])BotSpawn?.CallHook("AddGroupSpawn", zoneLocation, zoneProfile, zoneGroupID);
        string[] RemoveBots(string zoneGroupID) => (String[]) BotSpawn?.CallHook("RemoveGroupSpawn", zoneGroupID);
        #endregion

        #region Messaging
        void DebugPrint(string msg, bool warning)
        {
            if (debugEnabled)
            {
                switch (warning)
                {
                    case true:
                        PrintWarning(msg);
                        break;
                    case false:
                        Puts(msg);
                        break;
                }
            }

            LogToFile(debugfilename, "[" + DateTime.Now.ToString() + "] | " + msg, this, true);
        }
        void RespondWith(BasePlayer player, string msg)
        {
            if (player == null)
                arguments.ReplyWith(msg);
            else
                SendReply(player, msg);
            return;
        }
        #endregion

        #region SupplySignals
        bool IsProbablySupplySignal(Vector3 landingposition)
        {
            bool probable = false;

            // potential issues with signals thrown near each other (<40m)
            // definite issues with modifications that create more than one supply drop per cargo plane.
            // potential issues with player moving while throwing signal.

            //DebugPrint($"Checking {activeSupplySignals.Count()} active supply signals", false);
            if (activeSupplySignals.Count() > 0)
            {
                foreach (BaseEntity supplysignal in activeSupplySignals.ToList())
                {
                    if (supplysignal == null)
                    {
                        activeSupplySignals.Remove(supplysignal);
                        continue;
                    }

                    Vector3 thrownposition = supplysignal.transform.position;
                    float xdiff = Math.Abs(thrownposition.x - landingposition.x);
                    float zdiff = Math.Abs(thrownposition.z - landingposition.z);

                    //DebugPrint($"Known SupplySignal at {thrownposition} differing by {xdiff}, {zdiff}", false);

                    if (xdiff < compareRadius && zdiff < compareRadius)
                    {
                        probable = true;
                        activeSupplySignals.Remove(supplysignal);
                        DebugPrint("Found matching SupplySignal.", false);
                        DebugPrint($"Active supply signals remaining: {activeSupplySignals.Count()}", false);

                        break;
                    }
                }
                if (!probable)
                    //DebugPrint($"No matches found, probably from a timed event cargo_plane", false);
                    return probable;
            }
            //DebugPrint($"No active signals, must be from a timed event cargo_plane", false);
            return false;
        }
        void OnExplosiveThrown(BasePlayer player, BaseEntity entity)
        {
            if (entity == null || !(entity is SupplySignal))
                return;
            if (entity.net == null)
                entity.net = Network.Net.sv.CreateNetworkable();

            Vector3 position = entity.transform.position;

            if (activeSupplySignals.Contains(entity))
                return;
            SupplyThrown(player, entity, position);
            return;
        }
        void OnExplosiveDropped(BasePlayer player, BaseEntity entity)
        {
            if (entity == null || !(entity is SupplySignal)) return;
            if (activeSupplySignals.Contains(entity)) return;

            Vector3 position = entity.transform.position;
            SupplyThrown(player, entity, position);
            return;
        }
        void SupplyThrown(BasePlayer player, BaseEntity entity, Vector3 position)
        {
            Vector3 thrownposition = player.transform.position;

            timer.Once(2.0f, () =>
            {
                if (entity == null)
                {
                    activeSupplySignals.Remove(entity);
                    return;
                }
            });

            timer.Once(2.3f, () =>
            {
                if (entity == null) return;
                activeSupplySignals.Add(entity);
                DebugPrint($"SupplySignal position of {position}", false);
            });
        }
        #endregion

        #region Classes
        private class ActiveZone
        {
            public string DynZoneID { get; set; }

        }
        #endregion
    }
}