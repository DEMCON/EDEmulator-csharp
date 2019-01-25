# EDEmulator
Embedded debug device emulator

This pc application emulates a embedded device.
The embedded devices communicates with the embedded debug protocol described in https://demcon.github.io/EDProtocol

This pc application makes it easy to test the Embedded Debugger pc application, because you don't need a connected embedded device.

The PC application is developed by a intern at DEMCON. The application is not very stable at the moment.
The emulator now emulates a unreleased embedded debugger protocol. This emulator needs rework to emulate the correct embedded debugger protocol.
More info can be found in the ApplicationInformation document https://github.com/DEMCON/EDApplication-csharp/blob/master/ApplicationInformation.pdf

TODO:
[] rework to released debug protocol.
[] Make the application more stable.
[] Make a doxygen output for the brances
[] Auto compile after commit with Appveyor