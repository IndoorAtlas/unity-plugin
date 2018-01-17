# IndoorAtlas Unity Plugin

[IndoorAtlas](https://www.indooratlas.com/) provides a unique Platform-as-a-Service (PaaS) solution that runs a disruptive geomagnetic positioning in its full-stack hybrid technology for accurately pinpointing a location inside a building. The IndoorAtlas SDK enables app developers to use high-accuracy indoor positioning in venues that have been fingerprinted.

Getting started requires you to set up a free developer account and fingerprint your indoor venue using the [IndoorAtlas MapCreator 2](https://play.google.com/store/apps/details?id=com.indooratlas.android.apps.jaywalker).

## Getting Started

* Set up your [free developer account](https://app.indooratlas.com) in the IndoorAtlas developer portal. Help with getting started is available in the [Quick Start Guide](http://docs.indooratlas.com/quick-start-guide.html).
* To enable IndoorAtlas indoor positioning in a venue, the venue needs to be fingerprinted with the [IndoorAtlas MapCreator 2](https://play.google.com/store/apps/details?id=com.indooratlas.android.apps.jaywalker) tool.
* To start developing your own app, create an [API key](https://app.indooratlas.com/apps).
* An example Unity project is included in the example folder.

## How to Use
### Receiving location events

At first, copy Plugins folder to your Unity project's Asset folder.

To start receiving location events, you have to add a `IaBehavior.cs` script from Plugins folder to a component which implements the following functions:

* `void onLocationChanged(string locationstr)`
    - `string locationstr` is a JSON serialized `IndoorAtlas.Location` object.
* `void onStatusChanged(string statusstr)`
    - `string statusstr` is a JSON serialized `IndoorAtlas.Status` object.
* `void onHeadingChanged(string headingstr)`
    - `string headingstr` is a JSON serialized `IndoorAtlas.Heading` object.
* `void onOrientationChange(string orientationstr)`
    - `string locationstr` is a JSON serialized `IndoorAtlas.Location` object.
* `void onEnterRegion (string regionstr)`
* `void onExitRegion (string regionstr)`
    - `string regionstr` is a JSON serialized `IndoorAtlas.Region` object.

Callback inputs can be parsed to IndoorAtlas data types using `JsonUtility.FromJson`.

`IaBehavior.cs` script has the following options which has to be filled before the positioning can start:

* IndoorAtlas API key and secret. You can generate credentials from [our website](https://app.indooratlas.com/apps).
* Orientation and heading sensitivity parameters (in the units of degree). These parameters control how often `onHeadingChanged` and `onOrientationChanged` callbacks are called. The smaller the sensitivity (degree) is, the smaller rotation triggers the callbacks.

IndoorAtlas Unity Plugin starts calling component's callbacks immediatelly from the start of the app.

### Coodinate systems

This repository contains `WGSConversion` class (in `WGSConversion.cs` file) which can be used to convert IndoorAtlas SDK's (latitude, longitude) coordinates to metric (east, north) coordinates.


#### A numerical example

Set first a fixed point ("origin") to your 3D scene with `setOrigin` method, for example:

```C#
IndoorAtlas.WGSConversion temp = new IndoorAtlas.WGSConversion ();
temp.setOrigin (63.357219, 27.403592);
```

Relative (east, north) transitions can be computed with `WGStoEN` method after the origin has been set, for example:
```C#
Vector2 eastNorth = temp.WGStoEN (63.357860, 27.402245);
Debug.Log ("East-North transition: " + eastNorth.x + ", " + eastNorth.y);
```

This gives a transition of (-67.42091, 71.45055) _from origin_, that is, a transition of ~67 meters to west and ~71 meters to north _from origin_.


## Platform Specific
### iOS

* You have to install [CocoaPods](https://cocoapods.org) dependency manager. For details see: [CocoaPods getting started](https://guides.cocoapods.org/using/getting-started.html)
* The project has to target iOS 8.0 or newer.
* The iOS plugin contains "XcodeFixes.cs" script which automates the following:
    - Adds IndoorAtlas SDK dependency to the project using [CocoaPods](https://cocoapods.org)
    - Adds NSLocationAlwaysUsageDescription and NSLocationWhenInUseUsageDescription plist entries.
    - Disables bitcode.

## Example

There's an example Unity project in example project which controls main camera's orientation using IndoorAtlas SDK's
orientation estimates.
To build and run it on a real device, you have to fill a bundle identifier field in Player Settings and fill
your IndoorAtlas credentials to `Ia Behavior` component in Main Camera component.

## Known Issues

* iOS builds have visually non-smooth orientation updates.
* IaBehavior should be assigned to at most one component.

## License

Copyright 2017 IndoorAtlas Ltd. The Unity Plugin is released under the Apache License. See the LICENSE file for details.


