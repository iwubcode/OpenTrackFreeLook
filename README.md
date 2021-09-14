# OpenTrackFreeLook

Receives OpenTrack data and sends it to Dolphin FreeLook

## Configuration

A configuration json "appsettings.json" is shipped with the application.  This file contains application settings that can be modified.

**FreeLookClientIp**: This is the ip that the emulator will listen on.
**FreeLookClientPort**: This is the port that the emulator will listen on.

**OpenTrackIp**: This is the ip that OpenTrack will send data to.
**OpenTrackPort**: This is the port that OpenTrack will send data to.

**DebugOpenTrack**: Sometimes helpful if you are debugging the software.  This doesn't send the data to an emulator but instead prints what OpenTrack sent to the console.

## OpenTrack

Please go to their [github](https://github.com/opentrack/opentrack) to get more information on how to configure and use OpenTrack.

OpenTrack should be running with its output set to "UDP over network".  To match the default settings of this project, you will need to click the wrench icon and change the the ip to "127.0.0.1".  Alternatively, you can modify this application's json configuration.

## FreeLook

This application listens for OpenTrack data and will send packets to Dolphin emulator's FreeLook feature over UDP.
