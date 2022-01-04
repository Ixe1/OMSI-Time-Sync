**Created by:** Paul Lewis

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/Ixe1/OMSI-Time-Sync)](https://github.com/Ixe1/OMSI-Time-Sync/releases)

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://paypal.me/ixe1)

[![GitHub issues](https://img.shields.io/github/issues/Ixe1/OMSI-Time-Sync)](https://github.com/Ixe1/OMSI-Time-Sync/issues) [![GitHub forks](https://img.shields.io/github/forks/Ixe1/OMSI-Time-Sync)](https://github.com/Ixe1/OMSI-Time-Sync/network) [![GitHub license](https://img.shields.io/github/license/Ixe1/OMSI-Time-Sync)](https://github.com/Ixe1/OMSI-Time-Sync)

# OMSI Time Sync
A simple tool which automatically keeps OMSI 2's in-game time in sync with either the system time or Bus Company Simulator virtual company's time. Optionally you can manually sync the in-game time by either pressing a hotkey or the button on the UI.

By using this tool it's no longer necessary to manually adjust the OMSI in-game time, typically after every tour or so, due to the game usually lagging periodically and therefore the in-game time drifts from either the system's actual time or virtual bus company's actual time.

# Screenshot
![OMSI Time Sync](https://github.com/Ixe1/OMSI-Time-Sync/blob/master/OMSI%20Time%20Sync/screenshot/app.png)

# Video Preview (v1.00)
https://user-images.githubusercontent.com/96985590/147923517-8446a49a-b5c6-478c-9d2f-65b2b7f9d84c.mp4

**Important:** This tool modifies parts of the memory of OMSI.exe. It's strongly recommended that you close any games which have anti-cheat detection prior to running this tool as this activity might be falsely flagged as a cheat/hack by one or more of the various anti-cheat solutions out there.

This program will need to be ran as administrator due to the memory editing it will perform on the 'Omsi.exe' process.

# Prerequisites
- OMSI 2 **version 2.3.004** - this tool will not work with other versions of OMSI 2
- .NET Framework 4.7.2 Runtime (https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)
- Memory.dll (https://github.com/erfg12/memory.dll/) - Included with the releases
- System.Security.Principal.Windows (https://www.nuget.org/packages/System.Security.Principal.Windows/) - Included with the releases

# Optional
- [OMSI Time Sync's Telemetry Plugin](https://github.com/Ixe1/OMSI-Time-Sync-Telemetry-Plugin)

# Installation Steps
1. Download the latest release from [here](https://github.com/Ixe1/OMSI-Time-Sync/releases)
2. Extract the ZIP file to somewhere convenient
3. Run 'OMSI Time Sync.exe'
4. Configure as appropriate via the UI
5. Run OMSI or Bus Company Simulator and continue as usual

**Note:** If you're in a virtual company in Bus Company Simulator then please make sure you set the correct 'OMSI Time Offset' for that virtual company's timezone so that the OMSI in-game time is correct when it's being synced.

# Important note about SmartScreen

You may also need to 'unblock' the 'OMSI Time Sync.exe' executable file if Windows Smartscreen prevents you from running it. This is because the executable file isn't digitally signed and is therefore considered untrusted. Sadly a code signing certificate costs money as well as ultimately revealing too many personal details about me, the author, which on the internet can result in unfortunate consequences when used in the wrong hands.

In order to do unblock the file, please right click on the 'OMSI Time Sync.exe' executable file and go to 'properties' from the pop-up menu. In the newly shown dialog window, please tick the box next to 'unblock' and press the 'OK' button. You should now be able to do step 3 successfully this time.

# Questions?
Contact me on Discord at Ixel#6107 or send something via Github.

# Donation
While this is a free and open source program, if you like this tool then a donation of some kind is highly appreciated. Doing so also encourages me to develop something else in the future that you and others might find useful, as well as to maintain this specific tool as and when it might be necessary or appropriate.

Thanks in advance to anyone who chooses to donate something.

**Donate at https://paypal.me/ixe1**

# Licence
This is licenced under the GNU General Public License v3 (GPL-3) licence. Please see https://www.gnu.org/licenses/gpl-3.0.en.html for more information.

# Credits
- [sjain](https://github.com/sjain882) - Helping beta test both the tool and the plugin
- Charlie S#6270 on Discord - Helping beta test both the tool and the plugin
