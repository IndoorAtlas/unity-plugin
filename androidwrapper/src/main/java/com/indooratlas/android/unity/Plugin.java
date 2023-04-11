package com.indooratlas.android.unity;

import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.content.Context;

import com.unity3d.player.UnityPlayer;

import com.indooratlas.android.sdk.IAPOI;
import com.indooratlas.android.sdk.IARoute;
import com.indooratlas.android.sdk.IAGeofence;
import com.indooratlas.android.sdk.IARegion;
import com.indooratlas.android.sdk.IALocation;
import com.indooratlas.android.sdk.IALocationManager;
import com.indooratlas.android.sdk.IALocationRequest;
import com.indooratlas.android.sdk.IAOrientationListener;
import com.indooratlas.android.sdk.IAOrientationRequest;
import com.indooratlas.android.sdk.IALocationListener;
import com.indooratlas.android.sdk.IAWayfindingListener;
import com.indooratlas.android.sdk.IAARSession;

import com.indooratlas.android.sdk.resources.IAVenue;
import com.indooratlas.android.sdk.resources.IAFloorPlan;
import com.indooratlas.android.sdk.resources.IALatLngFloor;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.lang.String;
import java.util.concurrent.FutureTask;
import java.util.concurrent.ExecutionException;

public class Plugin implements IARegion.Listener, IALocationListener, IAWayfindingListener, IAOrientationListener {
    final static String TAG = "IndoorAtlasUnity";
    final static float[] mNilMatrix = new float[]{};
    private IALocationManager mLocationManager;
    private IAARSession mARSession;
    private double mDistanceFilter, mTimeFilter, mAttitudeFilter, mHeadingFilter;
    private String mGameObject, mTraceId, mVersion;
    private Handler mHandler;

    private void wait(FutureTask task) {
        try {
            task.get();
        } catch (InterruptedException | ExecutionException e) {
            Log.e(TAG, e.toString());
            throw new IllegalStateException(e.getMessage());
        }
    }

    public void close() {
        releaseArSession();
        FutureTask<Void> task = new FutureTask<>(new Runnable() {
            @Override
            public void run() {
                mLocationManager.destroy();
            }
        }, null);
        mHandler.post(task);
        wait(task);
        mLocationManager = null;
        mGameObject = null;
        mHandler = null;
    }

    public Plugin(final Object context, final String apiKey, final String apiSecret, final String apiEndpoint, final String gameObject) throws Exception {
        mGameObject = gameObject;
        final Bundle extras = new Bundle(2);
        extras.putString(IALocationManager.EXTRA_API_KEY, apiKey);
        extras.putString(IALocationManager.EXTRA_API_SECRET, apiSecret);
        extras.putString("com.indooratlas.android.sdk.intent.extras.wrapperName", "unity");
        // TODO: get version from saner place
        extras.putString("com.indooratlas.android.sdk.intent.extras.wrapperVersion", "0.0.1");
        if (apiEndpoint.length() > 0) extras.putString("com.indooratlas.android.sdk.intent.extras.restEndpoint", apiEndpoint);
        mHandler = new Handler(((Context)context).getMainLooper());
        FutureTask<Void> task = new FutureTask<>(new Runnable() {
            @Override
            public void run() {
                mLocationManager = IALocationManager.create((Context)context, extras);
                mVersion = mLocationManager.getExtraInfo().version;
            }
        }, null);
        mHandler.post(task);
        wait(task);
    }

    private JSONObject geofenceToJsonObject(IAGeofence iaGeofence) throws JSONException {
        if (iaGeofence == null) return new JSONObject();
        JSONObject geo = new JSONObject(), coordinate = new JSONObject(), position = new JSONObject();
        geo.put("id", iaGeofence.getId());
        geo.put("name", iaGeofence.getName());
        if (iaGeofence.hasPayload()) geo.put("payload", iaGeofence.getPayload().toString());
        // TODO: android doesn't expose center point
        // coordinate.put("latitude", ...);
        // coordinate.put("longitude", ...);
        position.put("coordinate", coordinate);
        if (iaGeofence.hasFloor()) position.put("floor", iaGeofence.getFloor().intValue());
        geo.put("position", position);
        JSONArray points = new JSONArray();
        for (int i = 0; i < iaGeofence.getEdges().size(); ++i) {
            JSONObject o = new JSONObject();
            o.put("latitude", iaGeofence.getEdges().get(i)[0]);
            o.put("longitude", iaGeofence.getEdges().get(i)[1]);
            points.put(o);
        }
        geo.put("points", points);
        return geo;
    }

    private JSONObject poiToJsonObject(IAPOI iaPoi) throws JSONException {
        if (iaPoi == null) return new JSONObject();
        JSONObject poi = new JSONObject(), coordinate = new JSONObject(), position = new JSONObject();
        poi.put("id", iaPoi.getId());
        poi.put("name", iaPoi.getName());
        if (iaPoi.hasPayload()) poi.put("payload", iaPoi.getPayload().toString());
        coordinate.put("latitude", iaPoi.getLatLngFloor().latitude);
        coordinate.put("longitude", iaPoi.getLatLngFloor().longitude);
        position.put("coordinate", coordinate);
        position.put("floor", iaPoi.getLatLngFloor().floor);
        poi.put("position", position);
        return poi;
    }

    private JSONObject floorplanToJsonObject(IAFloorPlan iaFloorPlan) throws JSONException {
        if (iaFloorPlan == null) return new JSONObject();
        JSONObject floorPlan = new JSONObject();
        floorPlan.put("id", iaFloorPlan.getId());
        floorPlan.put("name", iaFloorPlan.getName());
        floorPlan.put("imageUrl", iaFloorPlan.getUrl());
        floorPlan.put("width", iaFloorPlan.getBitmapWidth());
        floorPlan.put("height", iaFloorPlan.getBitmapHeight());
        floorPlan.put("pixelToMeterConversion", iaFloorPlan.getPixelsToMeters());
        floorPlan.put("meterToPixelConversion", iaFloorPlan.getMetersToPixels());
        floorPlan.put("widthMeters", iaFloorPlan.getWidthMeters());
        floorPlan.put("heightMeters", iaFloorPlan.getHeightMeters());
        floorPlan.put("floor", iaFloorPlan.getFloorLevel());
        return floorPlan;
    }

    private JSONObject venueToJsonObject(IAVenue iaVenue) throws JSONException {
        if (iaVenue == null) return new JSONObject();
        JSONArray floorplans = new JSONArray();
        for (int i = 0; i < iaVenue.getFloorPlans().size(); ++i) floorplans.put(floorplanToJsonObject(iaVenue.getFloorPlans().get(i)));
        JSONArray geofences = new JSONArray();
        for (int i = 0; i < iaVenue.getGeofences().size(); ++i) geofences.put(geofenceToJsonObject(iaVenue.getGeofences().get(i)));
        JSONArray pois = new JSONArray();
        for (int i = 0; i < iaVenue.getPOIs().size(); ++i) pois.put(poiToJsonObject(iaVenue.getPOIs().get(i)));
        JSONObject venue = new JSONObject();
        venue.put("id", iaVenue.getId());
        venue.put("name", iaVenue.getName());
        venue.put("floorplans", floorplans);
        venue.put("geofences", geofences);
        venue.put("pois", pois);
        return venue;
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

    private String regionToJson(IARegion iaRegion) {
        try {
            JSONObject region = new JSONObject();
            region.put("id", iaRegion.getId());
            region.put("name", iaRegion.getName());
            region.put("timestamp", iaRegion.getTimestamp());
            region.put("type", regionTypeToInt(iaRegion.getType()));
            region.put("venue", venueToJsonObject(iaRegion.getVenue()));
            region.put("floorplan", floorplanToJsonObject(iaRegion.getFloorPlan()));
            region.put("geofence", new JSONObject());
            return region.toString();
        } catch(JSONException e) {
            Log.e(TAG, e.toString());
            throw new IllegalStateException(e.getMessage());
        }
    }

    private String locationToJson(IALocation iaLocation) {
        if (iaLocation == null) return "";
        try {
            JSONObject location = new JSONObject(), position = new JSONObject(), coordinate = new JSONObject();
            location.put("accuracy", iaLocation.getAccuracy());
            location.put("altitude", iaLocation.getAltitude());
            location.put("bearing", iaLocation.getBearing());
            coordinate.put("latitude", iaLocation.getLatitude());
            coordinate.put("longitude", iaLocation.getLongitude());
            position.put("coordinate", coordinate);
            position.put("floor", iaLocation.getFloorLevel());
            location.put("position", position);
            location.put("timestamp", iaLocation.getTime());
            return location.toString();
        } catch (JSONException e) {
            Log.e(TAG, e.toString());
            throw new IllegalStateException(e.getMessage());
        }
    }

    @Override
    public void onEnterRegion(IARegion iaRegion) {
        UnityPlayer.UnitySendMessage(mGameObject, "NativeIndoorAtlasOnEnterRegion", regionToJson(iaRegion));
    }

    @Override
    public void onExitRegion(IARegion iaRegion) {
        UnityPlayer.UnitySendMessage(mGameObject, "NativeIndoorAtlasOnExitRegion", regionToJson(iaRegion));
    }

    @Override
    public void onHeadingChanged(long timestamp, double heading) {
        try {
            JSONObject headingObject = new JSONObject();
            headingObject.put("timestamp", timestamp);
            headingObject.put("heading", heading);
            UnityPlayer.UnitySendMessage(mGameObject, "NativeIndoorAtlasOnHeadingChanged", headingObject.toString());
        } catch(JSONException e) {
            Log.e(TAG, e.toString());
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
            UnityPlayer.UnitySendMessage(mGameObject, "NativeIndoorAtlasOnOrientationChanged", orientation.toString());
        } catch(JSONException e) {
            Log.e(TAG, e.toString());
            throw new IllegalStateException(e.getMessage());
        }
    }

    @Override
    public void onLocationChanged(IALocation iaLocation) {
        UnityPlayer.UnitySendMessage(mGameObject, "NativeIndoorAtlasOnLocationChanged", locationToJson(iaLocation));
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
            UnityPlayer.UnitySendMessage(mGameObject, "NativeIndoorAtlasOnStatusChanged", statusObject.toString());
        } catch(JSONException e) {
            Log.e(TAG, e.toString());
            throw new IllegalStateException(e.getMessage());
        }
    }

    private static JSONObject jsonObjectFromRoutingLeg(IARoute.Leg routingLeg) {
        JSONObject obj = new JSONObject();
        try {
            obj.put("begin", jsonObjectFromRoutingPoint(routingLeg.getBegin()));
            obj.put("end", jsonObjectFromRoutingPoint(routingLeg.getEnd()));
            obj.put("length", routingLeg.getLength());
            obj.put("direction", routingLeg.getDirection());
            obj.put("edgeIndex", routingLeg.getEdgeIndex());
        } catch(JSONException e) {
            Log.e(TAG, e.toString());
            throw new IllegalStateException(e.getMessage());
        }
        return obj;
    }

    private static JSONObject jsonObjectFromRoutingPoint(IARoute.Point routingPoint) {
        JSONObject obj = new JSONObject(), position = new JSONObject(), coordinate = new JSONObject();
        try {
            coordinate.put("latitude", routingPoint.getLatitude());
            coordinate.put("longitude", routingPoint.getLongitude());
            position.put("floor", routingPoint.getFloor());
            obj.put("position", position);
            obj.put("nodeIndex", routingPoint.getNodeIndex());
        } catch(JSONException e) {
            Log.e(TAG, e.toString());
            throw new IllegalStateException(e.getMessage());
        }
        return obj;
    }

    @Override
    public void onWayfindingUpdate(IARoute route) {
        try {
            JSONArray legs = new JSONArray();
            for (IARoute.Leg leg : route.getLegs()) legs.put(jsonObjectFromRoutingLeg(leg));
            JSONObject routeObject = new JSONObject();
            routeObject.put("legs", legs);
            routeObject.put("isSuccessful", route.isSuccessful());
            routeObject.put("error", route.getError());
            UnityPlayer.UnitySendMessage(mGameObject, "NativeIndoorAtlasOnRoute", routeObject.toString());
        } catch(JSONException e) {
            Log.e(TAG, e.toString());
            throw new IllegalStateException(e.getMessage());
        }
    }

    public String versionString() {
        return mVersion;
    }

    public void setDistanceFilter(double filter) {
        mDistanceFilter = filter;
    }

    public double getDistanceFilter() {
        return mDistanceFilter;
    }

    public void setTimeFilter(double filter) {
        mTimeFilter = filter;
    }

    public double getTimeFilter() {
        return mTimeFilter;
    }

    public void setHeadingFilter(double filter) {
        mHeadingFilter = filter;
    }

    public double getHeadingFilter() {
        return mHeadingFilter;
    }

    public void setAttitudeFilter(double filter) {
        mAttitudeFilter = filter;
    }

    public double getAttitudeFilter() {
        return mAttitudeFilter;
    }

    public void lockFloor(final int floor) {
        final Runnable r = new Runnable() {
            @Override
            public void run() {
                mLocationManager.lockFloor(floor);
            }
        };
        mHandler.post(r);
    }

    public void unlockFloor() {
        final Runnable r = new Runnable() {
            @Override
            public void run() {
                mLocationManager.unlockFloor();
            }
        };
        mHandler.post(r);
    }

    public void lockIndoors(final boolean lock) {
        final Runnable r = new Runnable() {
            @Override
            public void run() {
                mLocationManager.lockIndoors(lock);
            }
        };
        mHandler.post(r);
    }

    public void startUpdatingLocation() {
        final Runnable r = new Runnable() {
            @Override
            public void run() {
                mLocationManager.registerRegionListener(Plugin.this);
                // TODO: we should re-request location updates if mTimeFilter or mDistanceFilter changes
                IALocationRequest request = IALocationRequest.create();
                request.setFastestInterval((long)(mTimeFilter * 1000.0));
                request.setSmallestDisplacement((float)mDistanceFilter);
                mLocationManager.requestLocationUpdates(request, Plugin.this);
                // TODO: we should re-register orientation listener if mHeadingFilter or mAttitudeFilter changes
                mLocationManager.registerOrientationListener(new IAOrientationRequest(mHeadingFilter, mAttitudeFilter), Plugin.this);
            }
        };
        mHandler.post(r);
    }

    public void stopUpdatingLocation() {
        final Runnable r = new Runnable() {
            @Override
            public void run() {
                mLocationManager.unregisterRegionListener(Plugin.this);
                mLocationManager.removeLocationUpdates(Plugin.this);
                mLocationManager.unregisterOrientationListener(Plugin.this);
            }
        };
        mHandler.post(r);
    }

    public void startMonitoringForWayfinding(String to) {
        try {
            JSONObject target = new JSONObject(to);
            final double lat = target.getJSONObject("coordinate").getDouble("latitude");
            final double lon = target.getJSONObject("coordinate").getDouble("longitude");
            final int floor = target.getInt("floor");
            final IALatLngFloor ltf = new IALatLngFloor(lat, lon, floor);
            final Runnable r = new Runnable() {
                @Override
                public void run() {
                    if (mARSession != null) {
                        mARSession.startWayfinding(ltf);
                    } else {
                        mLocationManager.requestWayfindingUpdates(ltf, Plugin.this);
                    }
                }
            };
            mHandler.post(r);
        } catch(JSONException e) {
            Log.e(TAG, e.toString());
            throw new IllegalStateException(e.getMessage());
        }
    }

    public void stopMonitoringForWayfinding() {
        final Runnable r = new Runnable() {
            @Override
            public void run() {
                if (mARSession != null) {
                    mARSession.stopWayfinding();
                }
                mLocationManager.removeWayfindingUpdates();
            }
        };
        mHandler.post(r);
    }

    public String getTraceId() {
        // Potentially not thread safe, but very slow with thread dispatch :(
        return mLocationManager.getExtraInfo().traceId;
    }

    private IAARSession getArSession() {
        if (mARSession == null) {
            FutureTask<Void> task = new FutureTask<>(new Runnable() {
                @Override
                public void run() {
                    mARSession = mLocationManager.requestArUpdates();
                }
            }, null);
            mHandler.post(task);
            wait(task);
        }
        return mARSession;
    }

    public void releaseArSession() {
        if (mARSession != null) {
            mARSession.destroy();
            mARSession = null;
        }
    }

    public void setArPoseMatrix(float[] matrix) {
        getArSession().setPoseMatrix(matrix);
    }

    public void setArCameraToWorldMatrix(float[] matrix) {
        getArSession().setCameraToWorldMatrix(matrix);
    }

    public boolean getArIsConverged() {
        return getArSession().converged();
    }

    public float[] getArCompassMatrix() {
        float[] matrix = new float[16];
        if (getArSession().getWayfindingCompassArrow().updateModelMatrix(matrix)) return matrix;
        return mNilMatrix;
    }

    public float[] getArGoalMatrix() {
        float[] matrix = new float[16];
        if (getArSession().getWayfindingTarget().updateModelMatrix(matrix)) return matrix;
        return mNilMatrix;
    }

    public int getArTurnCount() {
        return getArSession().getWayfindingTurnArrows().size();
    }

    public float[] getArTurnMatrix(int index) {
        float[] matrix = new float[16];
        if (getArSession().getWayfindingTurnArrows().get(index).updateModelMatrix(matrix)) return matrix;
        return mNilMatrix;
    }

    public void addArPlane(float cx, float cy, float cz, float ex, float ez) {
        getArSession().addArPlane(new float[]{cx, cy, cz}, ex, ez);
    }

    public float[] geoToAr(double lat, double lon, int floor, float heading, float zOffset) {
        float[] matrix = new float[16];
        if (getArSession().geoToAr(lat, lon, floor, heading, zOffset, matrix)) return matrix;
        return mNilMatrix;
    }

    public String arToGeo(float x, float y, float z) {
        IALocation loc = getArSession().arToGeo(x, y, z);
        return locationToJson(loc);
    }
}
