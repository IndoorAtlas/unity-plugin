package com.indooratlas.android.unity;

import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.content.Context;

import com.unity3d.player.UnityPlayer;

import com.indooratlas.android.sdk.IALocation;
import com.indooratlas.android.sdk.IALocationListener;
import com.indooratlas.android.sdk.IALocationManager;
import com.indooratlas.android.sdk.IALocationRequest;
import com.indooratlas.android.sdk.IAOrientationListener;
import com.indooratlas.android.sdk.IAOrientationRequest;
import com.indooratlas.android.sdk.IARegion;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.lang.String;

public class IaUnityPlugin {
    private IALocationManager mManager;
    private final Handler mHandler;

    public IaUnityPlugin(Object context, final String apiKey, final String apiSecret, final String gameObjectName, double headingSensitivity, double orientationSensitivity) throws Exception {
        final Context ctx = (Context)context;
        mHandler = new Handler(ctx.getMainLooper());
        mHandler.post(new Runnable() {
            @Override
            public void run() {
                Bundle extras = new Bundle(2);
                extras.putString(IALocationManager.EXTRA_API_KEY, apiKey);
                extras.putString(IALocationManager.EXTRA_API_SECRET, apiSecret);
                mManager = IALocationManager.create(ctx, extras);

                if (!mManager.requestLocationUpdates(IALocationRequest.create(), new IALocationListener() {
                    @Override
                    public void onLocationChanged(IALocation iaLocation) {
                        JSONObject location = new JSONObject();
                        try {
                            location.put("accuracy", iaLocation.getAccuracy());
                            location.put("altitude", iaLocation.getAltitude());
                            location.put("bearing", iaLocation.getBearing());
                            location.put("floorLevel", iaLocation.getFloorLevel());
                            location.put("hasFloorlevel", iaLocation.hasFloorLevel());
                            location.put("latitude", iaLocation.getLatitude());
                            location.put("longitude", iaLocation.getLongitude());
                            location.put("timestamp", iaLocation.getTime());
                            UnityPlayer.UnitySendMessage(gameObjectName, "onLocationChanged",
                                location.toString());
                        } catch (JSONException e) {
                            Log.e("IAUNITY", e.toString());
                            throw new IllegalStateException(e.getMessage());
                        }
                    }

                    @Override
                    public void onStatusChanged(String provider, int status, Bundle bundle) {
                        int outputStatus = 0;
                        switch (status) {
                            case IALocationManager.STATUS_LIMITED:
                                outputStatus = 10;
                                break;
                            case IALocationManager.STATUS_OUT_OF_SERVICE:
                                outputStatus = 0;
                                break;
                            case IALocationManager.STATUS_TEMPORARILY_UNAVAILABLE:
                                outputStatus = 1;
                                break;
                            case IALocationManager.STATUS_AVAILABLE:
                                outputStatus = 2;
                                break;
                            default:
                                return;
                        }
                        try {
                            JSONObject statusObject = new JSONObject();
                            statusObject.put("status", outputStatus);
                            UnityPlayer.UnitySendMessage(gameObjectName, "onStatusChanged",
                                statusObject.toString());
                        } catch(JSONException e) {
                            Log.e("IAUNITY", e.toString());
                            throw new IllegalStateException(e.getMessage());
                        }
                    }
                })) {
                    Log.e("IAUNITY", "Requesting location updates failed");
                }

                if (!mManager.registerRegionListener(new IARegion.Listener() {
                    @Override
                    public void onEnterRegion(IARegion iaRegion) {
                        UnityPlayer.UnitySendMessage(gameObjectName, "onEnterRegion",
                            regionToJson(iaRegion));
                    }

                    @Override
                    public void onExitRegion(IARegion iaRegion) {
                        UnityPlayer.UnitySendMessage(gameObjectName, "onExitRegion",
                            regionToJson(iaRegion));
                    }

                    private String regionToJson(IARegion iaRegion) {
                        try {
                            JSONObject region = new JSONObject();
                            region.put("id", iaRegion.getId());
                            region.put("name", iaRegion.getName());
                            region.put("timestamp", iaRegion.getTimestamp());
                            region.put("type", regionTypeToInt(iaRegion.getType()));
                            return region.toString();
                        } catch(JSONException e) {
                            Log.e("IAUNITY", e.toString());
                            throw new IllegalStateException(e.getMessage());
                        }
                    }

                    private int regionTypeToInt(int type) {
                        switch(type) {
                            case IARegion.TYPE_FLOOR_PLAN:
                                return 1;
                            case IARegion.TYPE_VENUE:
                                return 2;
                        }
                        return 0;
                    }
                })) {
                    Log.e("IAUNITY", "Requesting region updates failed");
                }
            }
        });
        mHandler.post(new Runnable() {
            private double headingSensitivity;
            private double orientationSensitivity;

            @Override
            public void run() {
                IAOrientationRequest mOrientationRequest = new IAOrientationRequest(headingSensitivity, orientationSensitivity);

                if (!mManager.registerOrientationListener(mOrientationRequest, new IAOrientationListener() {
                    @Override
                    public void onHeadingChanged(long timestamp, double heading) {
                        try {
                            JSONObject headingObject = new JSONObject();
                            headingObject.put("timestamp", timestamp);
                            headingObject.put("heading", heading);
                            UnityPlayer.UnitySendMessage(gameObjectName, "onHeadingChanged",
                                headingObject.toString());
                        } catch(JSONException e) {
                            Log.e("IAUNITY", e.toString());
                            throw new IllegalStateException(e.getMessage());
                        }
                    }

                    @Override
                    public void onOrientationChange(long timestamp, double[] quaternion) {
                        try {
                            JSONObject orientation = new JSONObject();
                            orientation.put("x", quaternion[1]);
                            orientation.put("y", quaternion[2]);
                            orientation.put("z", quaternion[3]);
                            orientation.put("w", quaternion[0]);
                            orientation.put("timestamp", timestamp);
                            UnityPlayer.UnitySendMessage(gameObjectName, "onOrientationChange",
                                orientation.toString());
                        } catch(JSONException e) {
                            Log.e("IAUNITY", e.toString());
                            throw new IllegalStateException(e.getMessage());
                        }
                    }
                })) {
                    Log.e("IAUNITY", "Registering orientation listener failed.");
                }
            }

            private Runnable init(double hs, double os) {
                headingSensitivity = hs;
                orientationSensitivity = os;
                return this;
            }
        }.init(headingSensitivity, orientationSensitivity));
    }

    public void close() {
        mManager.destroy();
    }
}
