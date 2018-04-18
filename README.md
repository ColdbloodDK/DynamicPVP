**DynamicPVP** brings a little PVP action to your PVE server! Creates temporary PVP zones around SupplyDrops,  LockedCrates, APC and/or Heli (requires TruePVE and ZoneManager). Great for introducing PVP action to new players.

As soon as any:

        supply drop has spawned, or
        locked crate has spawned, or
        tank has been destroyed, or
        heli has crashed

a PVP zone is created with a default radius of 100 meters. If ZoneDomes is installed an optional dome is created above the zone, with the same radius. Entry and exit notifications are automatically provided and teleportation (if installed) is blocked inside the PVP zone.

The PVP zone has a 10 minute (600 seconds) duration by default and will be deleted at the end of that duration. (Locked crate default is 20 minutes)
The PVP flag, teleport block and ZoneDome (if used) will also be removed.

##If upgrading from Version 1, DELETE your JSON file!

##Console Commands =======================================
(Restricted to Owners/Moderators)

dynpvp pluginEnabled <true|false> - Enables or disables Dynamic PVP. (Default: true)
dynpvp debugEnabled <true|false> - Enables or disables debugging. (Default: false)
dynpvp blockTeleport <true|false> - Enables or disables teleport blocking. (Default: true)
dynpvp extraZoneFlags <string> - Extra flags for zone creation (ie: "nogather true")
dynpvp msgEnter <string> - message for entering zone.(Default: "Entering a PVP area!")
dynpvp msgLeave <string> - message for leaving zone.(Default: Leaving a PVP area.")

dynpvp domesEnabled <true|false> - Enables or disables ZoneDome integration. (Default: false)

dynpvp apcEnabled <true|false> - enable PVP for Tank. (Default: true)
dynpvp apcRadius <meters> - changes the radius of the zone. (Default: 100)
dynpvp apcDuration <seconds> - changes the duration. (Default: 600)

dynpvp heliEnabled <true|false> - enable PVP for Heli. (Default: true)
dynpvp heliRadius <meters> - changes the radius of the zone. (Default: 100)
dynpvp heliDuration <seconds> - changes the duration. (Default: 600)

dynpvp signalEnabled <true|false> - enable PVP for SupplySignal. (Default: false)
dynpvp signalRadius <meters> - changes the radius of the zone. (Default: 100)
dynpvp signalDuration <seconds> - changes the duration. (Default: 1200)

dynpvp timedCrateEnabled <true|false> - enable PVP for LockedCrate. (Default: true)
dynpvp timedCrateRadius <meters> - changes the radius of the zone. (Default: 100)
dynpvp timedCrateDuration <seconds> - changes the duration. (Default: 600)

dynpvp timedDropEnabled <true|false> - enable PVP for SupplyDrop. (Default: true)
dynpvp timedDropRadius <meters> - changes the radius of the zone. (Default: 100)
dynpvp timedDropDuration <seconds> - changes the duration. (Default: 600)

dynpvp list - show current settings

(For Chat commands, just add the slash "/")

##Warnings =======================================
 - 1. Use ZoneManager version 2.4.61 or higher.
 - 2. Use TruePVE version 0.8.8 or higher.
 - 3. Reloading or unloading DynamicPVP while a zone is active will not delete the current PVP zones. They will be left in limbo and will require manual deletion.
 - 4. If using ZoneDomes, I highly recommend setting SphereDarkness to 1. *
 - 5. Command changes only affect future initiating events and should NOT be made while a PVP zone is active.
 - 6. Ignoring supply signals works 95% of the time. Also note:

 -  potential issues with signals thrown near each other (<40m)
 - definite issues with modifications that create more than one supply drop per cargo plane.
 - potential issues with player moving while throwing signal.

 - ZoneDomes increases the visibility of a zone by stacking spheres, based on SphereDarkeness. However, it currently only deletes one (1) of the spheres created.
 
```json
{
  "BradleyAPC": {
    "apcDuration": 600.0,
    "apcEnabled": true,
    "apcRadius": 100.0
  },
  "CH47Chinook": {
    "ch47Duration": 600.0,
    "ch47Enabled": true,
    "ch47Radius": 100.0
  },
  "General": {
    "blockTeleport": true,
    "configVersion": "2.0.0",
    "debugEnabled": true,
    "msgEnter": "Entering a PVP area!",
    "msgLeave": "Leaving a PVP area.",
    "pluginEnabled": true
  },
  "PatrolHelicopter": {
    "heliDuration": 600.0,
    "heliEnabled": true,
    "heliRadius": 100.0
  },
  "SupplySignal": {
    "signalDuration": 600.0,
    "signalEnabled": false,
    "signalRadius": 100.0
  },
  "TimedCrate": {
    "timedCrateDuration": 1200.0,
    "timedCrateEnabled": true,
    "timedCrateRadius": 100.0
  },
  "TimedDrop": {
    "timedDropDuration": 600.0,
    "timedDropEnabled": true,
    "timedDropRadius": 100.0
  },
  "ZoneDomes": {
    "domesEnabled": true,
    "domesSphereDarkness": 1
  }
}
```

##DONATIONS
If you LIKE this plugin, please consider donating: http://paypal.me/CatMeat