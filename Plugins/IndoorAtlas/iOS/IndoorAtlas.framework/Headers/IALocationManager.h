// IndoorAtlas iOS SDK
// IALocationManager.h

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>
#import <CoreMotion/CoreMotion.h>
#import <IndoorAtlas/IAFloor.h>
#import <IndoorAtlas/IAFloorPlan.h>
#import <simd/simd.h>

#define INDOORATLAS_API __attribute__((visibility("default")))

/**
 * Indicates that a feature that is still subject to change in a future release.
 */
@protocol IABeta
@end
/**
 * Indicates a feature that is not available by default but can be enabled by contacting IndoorAtlas sales.
 */
@protocol IARestricted
@end

/**
 * Use this key to obtain the session trace id from the `[IALocationManager extraInfo]` dictionary.
 */
INDOORATLAS_API extern NSString * _Nonnull const kIATraceId;

@class IALocationManager;
@class IAGeofence;
@class IAPOI;
@class IALatLngFloor;

/**
 * Defines the type of region.
 */
typedef NS_ENUM(NSInteger, ia_region_type) {
    /**
     * Region type is not known.
     * This may be the result of running outdated SDK.
     */
    kIARegionTypeUnknown,

    /**
     * Region type is floor plan.
     */
    kIARegionTypeFloorPlan,

    /**
     * Region type is venue.
     */
    kIARegionTypeVenue,

    /**
     * Region type is geofence.
     */
    kIARegionTypeGeofence,
};

/**
 * Defines the location service status.
 */
typedef NS_ENUM(NSInteger, ia_status_type) {
    /**
     * Location service is not available and the condition is not expected to resolve itself soon.
     */
    kIAStatusServiceOutOfService = 0,

    /**
     * Location service temporarily unavailable. This could be due to no network connectivity.
     * This mostly happens in the beginning of a positioning session when the SDK need to
     * authenticate itself in the IndoorAtlas cloud and download map data, if it has not been
     * cached yet.
     */
    kIAStatusServiceUnavailable = 1,

    /**
     * Location service running normally.
     */
    kIAStatusServiceAvailable = 2,

    /**
     * Location service is running but with limited accuracy and functionality.
     * This typically means that location permissions were not given to the application.
     */
    kIAStatusServiceLimited = 10,
};

/**
 * Defines the accuracy of location.
 */
typedef NS_ENUM(NSInteger, ia_location_accuracy) {
    /**
     * Best accuracy.
     */
    kIALocationAccuracyBest,

    /**
     * Low accuracy.
     * Locations with this accuracy are typically obtained with lowest amount of processing to reduce device power drain.
     */
    kIALocationAccuracyLow,

    /**
     * Best accuracy for cart use case.
     * Use when device is mounted to a shopping cart or similar platform with wheels.
     */
    kIALocationAccuracyBestForCart
};

/**
 * Defines the error status of a wayfinding request
 */
typedef NS_ENUM(NSInteger, ia_route_error) {
    /** Routing was successful */
    kIARouteErrorNoError = 0,
    /** Route could not be computed */
    kIARouteErrorRoutingFailed = 1,
    /** Wayfinding graph not available */
    kIARouteErrorGraphNotAvailable = 2
};

@protocol IALatLngFloorCompatible
@property (nonatomic,readonly) IALatLngFloor * _Nonnull latLngFloor;
@end

/**
 * Represents generic location with latitude, longitude and floor number
 */
INDOORATLAS_API
@interface IALatLngFloor : NSObject <IALatLngFloorCompatible>
@property (nonatomic,readonly) CLLocationDegrees latitude;
@property (nonatomic,readonly) CLLocationDegrees longitude;
@property (nonatomic,readonly) NSInteger floor;
+ (nonnull IALatLngFloor *)latLngFloorWithLatitude:(CLLocationDegrees)latitude andLongitude:(CLLocationDegrees)longitude andFloor:(NSInteger)floor;
+ (nonnull IALatLngFloor *)latLngFloorWithCoordinate:(CLLocationCoordinate2D)coordinate andFloor:(NSInteger)floor;
@end

/**
 * Represents a venue in IndoorAtlas system
 */
INDOORATLAS_API
@interface IAVenue : NSObject
/**
 * Name of the venue
 */
@property (nonatomic, readonly, nonnull) NSString *name;
/**
 * Mapped floors that the venue has
 */
@property (nonatomic, readonly, nonnull) NSArray<IAFloorPlan*> *floorplans;
/**
 * ID of the venue in IndoorAtlas developer console
 */
@property (nonatomic, readonly, nonnull) NSString *id;
/**
 * Geofences for this venue
 */
@property (nonatomic, readonly, nullable) NSArray<IAGeofence*> *geofences;
/**
 * Point of interests for this venue
 */
@property (nonatomic, readonly, nullable) NSArray<IAPOI*> *pois;

@end

/**
 * An IARegion object represents a region on Earth.
 */
INDOORATLAS_API
@interface IARegion : NSObject
/**
 * Region identifier. For objects of type kIARegionTypeFloorPlan this is same as floor plan id.
 */
@property (nonatomic, strong, nonnull) NSString *identifier;
/**
 * Human readable name
 */
@property (nonatomic, strong, nullable) NSString *name;
/**
 * Region type
 * See possible values at `ia_region_type`
 */
@property (nonatomic, assign) enum ia_region_type type;
/**
 * If there is an event related to region this is the timestamp of that event.
 */
@property (nonatomic, strong, nullable) NSDate *timestamp;
/**
 * If this is a venue region then this will point to the venue object.
 */
@property (nonatomic, strong, nullable) IAVenue *venue;
/**
 * If this is a floorplan region then this will point to the floorplan object.
 */
@property (nonatomic, strong, nullable) IAFloorPlan *floorplan;
@end

/**
 * An IAGeofence object provides a way to represent custom regions.
 */
INDOORATLAS_API
@interface IAGeofence : IARegion
/**
 * The floor the geofence is located on.
 */
@property (nonatomic, strong, nullable) IAFloor *floor;
/**
 * The JSON payload for this geofence.
 */
@property (nonatomic, strong, nullable) NSDictionary *payload;
/**
 * Center coordinate of the geofence.
 */
@property (nonatomic, readonly) CLLocationCoordinate2D coordinate;
/**
 * Is this geofence cloud defined (static) or runtime defined (dynamic)
 */
@property (nonatomic, readonly) BOOL isCloudGeofence;
/**
 * Unique [lat, lon] points of the geofence, if any.
 */
@property (nonatomic, readonly, strong) NSArray<NSNumber*> * _Nonnull points;

/**
 * Does the geofence contain the coordinate?
 */
- (BOOL)containsCoordinate:(CLLocationCoordinate2D)coordinate;
@end

/**
 * IAPolygonGeofence object represents a polygonal region on Earth.
 */
INDOORATLAS_API
@interface IAPolygonGeofence : IAGeofence
/**
 * Creates a new polygonal region from unique edges.
 * @param identifier Identifier for the geofence.
 * @param floor `IAFloor` object with level information. Nil `IAFloor` means that the floor is unknown.
 * @param edges Coordinates specifying the polygon.
 *
 * The edges must be supplied in clockwise order for the polygon to be valid.
 */
+ (nonnull IAPolygonGeofence*)polygonGeofenceWithIdentifier:(nonnull NSString*)identifier andFloor:(nullable IAFloor*)floor edges:(nonnull NSArray<NSNumber*>*)edges;
@end

/**
 * Represents a point of interest.
 */
INDOORATLAS_API
@interface IAPOI : IAGeofence <IALatLngFloorCompatible>
/**
 * The floor the POI is located on.
 */
@property (nonatomic, strong, nonnull) IAFloor *floor;
@end

/**
 * Provides wayfinding destination for the SDK.
 */
INDOORATLAS_API
@interface IAWayfindingRequest : NSObject <IALatLngFloorCompatible>
/**
 * Wayfinding request's destination coordinate.
 */
@property (nonatomic, assign) CLLocationCoordinate2D coordinate;

/**
 * Wayfinding request's destination floor level.
 */
@property (nonatomic, assign) NSInteger floor;
@end

/**
 * Represents a point in a route.
 */
INDOORATLAS_API
@interface IARoutePoint : NSObject <IALatLngFloorCompatible>
/**
 * Coordinate of this point.
 */
@property (nonatomic, readonly) CLLocationCoordinate2D coordinate;

/**
 * Floor number of this point.
 */
@property (nonatomic, readonly) NSInteger floor;

/**
 * Get the index of the point. This is the zero-based index of the node
 * in the original JSON graph this point corresponds to. If this is a
 * virtual wayfinding node, e.g., a starting point of the route outside
 * the original graph, nodeIndex will be -1.
 */
@property (nonatomic, readonly) NSInteger nodeIndex;
@end

/**
 * Object representing the line segment between two `IARoutePoint` objects.
 * Includes the distance and direction of the segment as well as the start and end points.
 */
INDOORATLAS_API
@interface IARouteLeg : NSObject
/**
 * The `IARoutePoint` representing the beginning of this leg.
 */
@property (nonatomic, readonly, nonnull) IARoutePoint *begin;

/**
 * The `IARoutePoint` representing the end of this leg.
 */
@property (nonatomic, readonly, nonnull) IARoutePoint *end;

/**
 * Length of the line segment in meters.
 */
@property (nonatomic, readonly) double length;

/**
 * Direction of the line segment in ENU coordinates in degrees.
 * 0 is North and 90 is East.
 */
@property (nonatomic, readonly) double direction;

/**
 * Zero-based index of the edge corresponding to this leg in the
 * original JSON graph. If this is a virtual leg, for example, a
 * segment connecting an off-graph starting point to the graph,
 * edgeIndex will be -1.
 */
@property (nonatomic, readonly) NSInteger edgeIndex;
@end

/**
 * Structure representing a route from user's location to destination.
 */
INDOORATLAS_API
@interface IARoute : NSObject
/**
 * An array of `IARouteLeg` objects connecting user's location to destination.
 */
@property (nonatomic, readonly, nonnull) NSArray<IARouteLeg*> *legs;
/** Whether route is available */
@property (nonatomic, readonly) bool isSuccessful;
/** Error status for routing */
@property (nonatomic, readonly) enum ia_route_error error;
@end

/**
 * IAStatus specifies the current status of the locationing service.
 */
INDOORATLAS_API
@interface IAStatus : NSObject
/**
 * Type of the status message.
 */
@property (nonatomic, assign) enum ia_status_type type;
@end

/**
 * An IALocation object represents the location data generated by an `IALocationManager` object. This object incorporates the geographical coordinates along with values indicating the accuracy of the measurements and when those measurements were made.
 * This class also reports information about the the course, the direction in which the device is traveling.
 *
 * Typically, you use an `IALocationManager` object to create instances of this class based on the last known location of the user's device.
 * You can create instances yourself, however, if you want to cache custom location data or get the distance between two points.
 *
 * This class is designed to be used as is and should not be subclassed.
 */
INDOORATLAS_API
@interface IALocation : NSObject <IALatLngFloorCompatible>

/**
 * @name Initializing a Location Object
 *
 * Indicate current location to positioning service. Can be used to set the explicit location (latitude, longitude, accuracy, floor level)
 */

/**
* Initializes and returns a location object with specified CoreLocation information.
* @param location CLLocation object. Might be initialized in code or from CLLocationManager.
*
* An explicit location is used as a hint in the system. This means that the inputted location is used only to determine the initial position and setting the location does not lock the floor or venue context.
*/
+ (nonnull IALocation*)locationWithCLLocation:(nonnull CLLocation*)location;

/**
 * Initializes and returns a location object with specified CoreLocation information.
 * @param location CLLocation object. Might be initialized in code or from CLLocationManager.
 * @param floor `IAFloor` object with level information. Nil `IAFloor` means that the floor is unknown.
 *
 * An explicit location is used as a hint in the system. This means that the inputted location is used only to determine the initial position and setting the location does not lock the floor or venue context.
 */
+ (nonnull IALocation*)locationWithCLLocation:(nonnull CLLocation*)location andFloor:(nullable IAFloor*)floor;

/**
 * @name Location Attributes
 */

/**
 * CoreLocation compatible [location information](https://developer.apple.com/library/ios/documentation/CoreLocation/Reference/CLLocation_Class/). (read-only)
 *
 * When running in the simulator, `IALocationManager` provides fake values.
 * You must run your application on an actual iOS device to get the actual location of the device.
 */
@property (nonatomic, readonly, nullable) CLLocation *location;

/**
 * @name Optional attributes
 */

/**
 * The logical floor of the building.
 *
 * This property is included as CLLocation's CLFloor is private interface.
 * Thus it may be deprecated in future.
 */
@property (nonatomic, strong, nullable) IAFloor *floor;

/**
 * Region this location was obtained from.
 */
@property (nonatomic, strong, nullable) IARegion *region;

/**
 * Experimental feature for soft location updates.
 * The API may be changed in future.
 * Set to true before giving as a custom location to `IALocationManager` to make the location behave more like radio source rather than a location hint.
 */
@property (nonatomic, assign) bool soft;

@end

/**
 * Object containing heading data. Generated by `IALocationManager`
 */
INDOORATLAS_API
@interface IAHeading : NSObject
/**
 * The heading in degrees. Relative to true north.
 */
@property (nonatomic, readonly) CLLocationDirection trueHeading;
/**
 * Time when heading was obtained
 */
@property(readonly, nonatomic, copy, nullable) NSDate *timestamp;
@end

/**
 * Object containing orientation data. Generated by `IALocationManager`
 */
INDOORATLAS_API
@interface IAAttitude : NSObject
/**
 * The orientation
 */
@property(readonly, nonatomic) CMQuaternion quaternion;
/**
 * Time when orientation was obtained
 */
@property(readonly, nonatomic, copy, nullable) NSDate *timestamp;
@end

/**
 * Object describing an object in the AR space.
 * The methods of an instance of this class can be called from any thread.
 *
 * NOTE! To enable AR features, please contact IndoorAtlas sales.
 */
INDOORATLAS_API
@interface IAARObject : NSObject <IABeta,IARestricted>
/**
 * Get the current model matrix for this object.
 *
 * @param outModelMatrix Output: a 4x4 homogeneous model-to-world matrix. .
 * @return true if the outtModelMatrix was set and the object can be displayed,
           false if the model matrix is not available or object should not be displayed for
 *         some other reason.
 */
- (bool)updateModelMatrix:(nonnull simd_float4x4*)outModelMatrix;
@end

/**
 * IndoorAtlas AR fusion API
 *
 * NOTE! To enable AR features, please contact IndoorAtlas sales.
 *
 * The AR API provides you with convenient means of converting between two important
 * coordinate systems:
 *     * The global coordinates encoded in IALocation objects and their components.
 *       In practice, this means latitude, longitude, floor number and heading/orientation
 *       information.
 *     * An augmented reality (AR) coordinate system, which is assumed to be a right-handed
 *       3D metric coordinate system, where the Y axis points up (towards the sky).
 *
 * The local tracking of the device in the AR coordinate system is assumed to be handled by an
 * external AR solution like ARCore, whose certain outputs are given to this class. The IndoorAtlas
 * platform fuses this information with the IndoorAtlas position estimates and provides the
 * relevant coordinate transforms in a stable and visually consistent manner, which allows you
 * to easily place geographically referenced content in the AR world.
 *
 * The methods of an instance of this class can be called from any thread.
 */
INDOORATLAS_API
@interface IAARSession : NSObject <IABeta,IARestricted>
/**
 * Wayfinding arrow AR object.
 */
@property (nonatomic, readonly, nonnull) IAARObject *wayfindingCompassArrow;

/**
 * Wayfinding target (goal) AR object.
 */
@property (nonatomic, readonly, nonnull) IAARObject *wayfindingTarget;

/**
 * Array of waypoint AR objects constructed from the wayfinding route.
 */
@property (nonatomic, readonly, nullable) NSArray<IAARObject*> *wayfindingTurnArrows;

/**
 * Check if the positioning session has approximately converged. If false, it
 * is recommended to advise the user to walk for a couple of meters to any direction so
 * that they coordinate systems can be oriented correctly. This is optional, but the
 * `IAARObject` instances may first appear in clearly incorrect directions on positions
 * otherwise.
 */
@property (nonatomic, readonly) bool converged;


/**
 * Create an AR Point-of-Interest in the given geographical coordinates. The coordinates of
 * the object in the AR world update in a visually pleasing manner.
 *
 * @param coords latitude in degrees
 * @param floorNumber IndoorAtlas integer floor number
 * @return IAARObject
 */
- (nonnull IAARObject*)createPoi:(CLLocationCoordinate2D)coords floorNumber:(int)floorNumber;

/**
 * Create an AR Point-of-Interest in the given geographical coordinates. The coordinates of
 * the object in the AR world update in a visually pleasing manner.
 *
 * @param coords latitude in degrees
 * @param floorNumber IndoorAtlas integer floor number
 * @param heading Heading in degrees 0=North, 90=East, 180=South, 270=West
 * @param zOffset Vertical offset from the floor plane in meters
 * @return IAARObject
 */
- (nonnull IAARObject*)createPoi:(CLLocationCoordinate2D)coords floorNumber:(int)floorNumber heading:(double)heading zOffset:(double)zOffset;

/**
 * Create an AR Point-of-Interest in the given geographical coordinates. The coordinates of
 * the object in the AR world update in a visually pleasing manner.
 *
 * @param location location of the POI
 * @return IAARObject
 */
- (nonnull IAARObject*)createPoi:(nonnull IALocation*)location;

/**
 * Convert from geographical to AR coordinates.
 *
 * @param coords Geographical coordinates
 * @param floorNumber IndoorAtlas integer floor number
 * @param heading heading in degrees 0=North, 90=East, 180=South, 270=West
 * @param zOffset Vertical offset from the floor plane in meters
 * @return a 4x4 homogeneous model-to-world matrix. The matrix will be a identity
 *         matrix in case of conversion failure. The conversion fails unless both
 *         IndoorAtlas positioning and the AR input have been obtained (i.e.,
 *         before the first fix or before setArCameraMatrix has been called for the
 *         first time).
 */
- (simd_float4x4)geoToAr:(const CLLocationCoordinate2D)coords floorNumber:(int)floorNumber heading:(double)heading zOffset:(double)zOffset;

/**
 * Convert from AR to geographic coordinates.
 *
 * @param matrix 4x4 homogeneous model-to-world matrix
 * @return geographical coordinates. Nil if not available
 */
- (nonnull IALocation*)arToGeo:(const simd_float4x4)matrix;

/**
 * Convert from AR to geographic coordinates.
 *
 * @param x AR coordinate system X-coordinate (horizontal)
 * @param y AR coordinate system Y-coordinate (vertical)
 * @param z AR coordinate system Z-coordinate (horizontal)
 *
 * @return geographical coordinates. Nil if not available
 */
- (nonnull IALocation*)arToGeo:(double)x Y:(double)y Z:(double)z;

/**
 * Input current pose from the external AR tracking. This method should be called on each
 * successfully tracked AR camera frame.
 *
 * @param poseMatrix Current "sensor pose" from AR tracking.
                     a 4x4 homogeneous local-to-world matrix, equivalent to ARKit's ARCamera.transform[1]
                     This matrix does not change with UI orientation.
                     1: https://developer.apple.com/documentation/arkit/arcamera/2866108-transform.
 */
- (void)setPoseMatrix:(const simd_float4x4)poseMatrix;

/**
 * Set the current camera-to-world matrix. Should be called regularly as long as one wishes
 * to render objects. Calling on each AR frame is recommended.
 *
 * @param cameraToWorldMatrix a 4x4 homogeneous camera-to-world matrix, where the the negative
 *                            Z axis is points "into the screen" in camera coordinates. Unlike
 *                            the poseMatrix method, this matrix may change with UI orientation.
 *                            This matrix is equivalent to inverse of ARKit's ARCamera.viewMatrix[1]
 *                            1: https://developer.apple.com/documentation/arkit/arcamera/2921672-viewmatrix
 */
- (void)setCameraToWorldMatrix:(const simd_float4x4)cameraToWorldMatrix;

/**
 * Input AR plane tracking information. This input is optional, but allows more accurate
 * vertical tracking, e.g., placing geo-referenced AR objects so that they appear to be on
 * the floor. If used, should be called on each AR frame for each tracked horizontal
 * upward-facing planes as input.
 *
 * The planes will be applied on the next `setPoseMatrix:` call.
 *
 * @param centerX Center X coordinate of the plane
 * @param centerY Center Y coordinate of the plane
 * @param centerZ Center Z coordinate of the plane
 * @param extentX Extent X of the plane
 * @param extentZ Extent Z of the plane
 */
- (void)addPlaneWithCenterX:(float)centerX withCenterY:(float)centerY withCenterZ:(float)centerZ withExtentX:(float)extentX withExtentZ:(float)extentZ;
@end

/**
 * The IALocationManagerDelegate protocol defines the methods used to receive location updates from an `IALocationManager` object.
 *
 * Upon receiving a successful location update, you can use the result to update your user interface or perform other actions.
 *
 * The methods of your delegate object are called from the main thread.
 */
INDOORATLAS_API
@protocol IALocationManagerDelegate <NSObject>
@optional

/**
 * @name Responding to Location Events
 */

/**
 * Tells the delegate that new location data is available.
 *
 * Implementation of this method is optional but recommended.
 *
 * @param manager The location manager object that generated the update event.
 * @param locations An array of `IALocation` objects containing the location data. This array always contains at least one object representing the current location.
 * If updates were deferred or if multiple locations arrived before they could be delivered, the array may contain additional entries.
 * The objects in the array are organized in the order in which they occured. Threfore, the most recent location update is at the end of the array.
 */
- (void)indoorLocationManager:(nonnull IALocationManager*)manager didUpdateLocations:(nonnull NSArray<IALocation*>*)locations;
- (void)indoorLocationManager:(nonnull IALocationManager*)manager didUpdateLocationsDeprecated:(nonnull NSArray*)locations NS_SWIFT_NAME(indoorLocationManager(_:didUpdateLocations:));

/**
 * Tells the delegate that the user entered the specified region.
 * @param manager The location manager object that generated the event.
 * @param region The region related to event.
 */
- (void)indoorLocationManager:(nonnull IALocationManager*)manager didEnterRegion:(nonnull IARegion*)region;

/**
 * Tells the delegate that the user left the specified region.
 * @param manager The location manager object that generated the event.
 * @param region The region related to event.
 */
- (void)indoorLocationManager:(nonnull IALocationManager*)manager didExitRegion:(nonnull IARegion*)region;

/**
 * Tells the delegate that the wayfinding route was updated.
 * @param manager The location manager object that generated the event.
 * @param route The new route.
 */
- (void)indoorLocationManager:(nonnull IALocationManager*)manager didUpdateRoute:(nonnull IARoute*)route;

/**
 * Tells that `IALocationManager` status changed. This is used to signal network connection issues.
 * @param manager The location manager object that generated the event.
 * @param status The status at the time of the event.
 */
- (void)indoorLocationManager:(nonnull IALocationManager*)manager statusChanged:(nonnull IAStatus*)status;

/**
 * Tells that extra information dictionary was received. This dictionary contains
 * identifier for debugging positioning.
 * @param manager The location manager object that generated the event.
 * @param extraInfo NSDictionary containing extra information about positioning.
 */
- (void)indoorLocationManager:(nonnull IALocationManager*)manager didReceiveExtraInfo:(nonnull NSDictionary*)extraInfo;

/**
 * Tells the delegate that updated heading information is available.
 * @param manager The location manager object that generated the event.
 * @param newHeading New heading data.
 */
- (void)indoorLocationManager:(nonnull IALocationManager*)manager didUpdateHeading:(nonnull IAHeading*)newHeading;

/**
 * Tells the delegate that updated attitude (orientation) information is available.
 * @param manager The location manager object that generated the event.
 * @param newAttitude New attitude data.
 */
- (void)indoorLocationManager:(nonnull IALocationManager*)manager didUpdateAttitude:(nonnull IAAttitude*)newAttitude;
@end

/**
 * The IALocationManager class is central point for configuring the delivery of indoor location related events to your app.
 * You use and instance of this class to establish the parameters that determine when location events should be delivered and to start and stop the actual delivery of those events.
 * You can also use a location manager object to retrieve the most recent location data.
 *
 * The shared IALocationManager instance is thread safe. (since SDK version 3.3+)
 */
INDOORATLAS_API
@interface IALocationManager : NSObject

/**
 * The latest location update.
 *
 * This property can be set for a custom location.
 */
@property (nonatomic, readwrite, nullable) IALocation *location;

/**
 * The latest sample of device attitude.
 *
 */
@property (nonatomic, readwrite, nullable) IAAttitude *attitude;

/**
 * The latest sample of device heading.
 *
 */
@property (nonatomic, readwrite, nullable) IAHeading *heading;

/**
 * The minimum distance measured in meters that the device must move horizontally before an update event is generated.
 * Setting this to 0 disables distance based updates.
 * Maximum update frequency is determined from values of distanceFilter and timeFilter. Update is generated when either of conditions specified by these filters are met.
 * Default value is 0.7 meters.
 * Uses CoreLocation [CLLocationDistance](https://developer.apple.com/documentation/corelocation/cllocationdistance?language=objc).
 */
@property (assign, nonatomic) CLLocationDistance distanceFilter;

/**
 * The minimum amount of time measured in seconds that must be elapsed before an update event is generated.
 * Setting this to 0 disables time based updates.
 * Maximum update frequency is determined from values of distanceFilter and timeFilter. Update is generated when either of conditions specified by these filters are met.
 * Default value is 2.
 */
@property (assign, nonatomic) NSTimeInterval timeFilter;

/**
 * The minimum angular change in degrees required to generate new didUpdateHeading event.
 * Default value is 1 degree.
 */
@property(assign, nonatomic) CLLocationDegrees headingFilter;

/**
 * The minimum angular change in degrees required to generate new didUpdateAttitude event.
 * Default value is 1 degree.
 */
@property(assign, nonatomic) CLLocationDegrees attitudeFilter;

/**
 * The accuracy of the location data.
 *
 * The receiver does its best to achieve the requested accuracy; however, the actual accuracy is not guaranteed.
 * You should assign a value to this property that is appropriate for your usage scenario.
 * Determining a location with greater accuracy requires more time and more power.
 *
 * Default value is kIALocationAccuracyBest.
 *
 * See possible values at `ia_location_accuracy`
 */
@property(assign, nonatomic) enum ia_location_accuracy desiredAccuracy;

/**
 * Explicitly enable background location updates. (iOS 9.0+ only)
 *
 * Location updates must be active when app goes to background for this flag to have effect.
 * If you have enabled background execution in some other way, you may still receive location updates in background even if this flag is not set.
 *
 * NOTE! you must also enable the Location updates background mode and include NSLocationAlwaysAndWhenInUseUsageDescription
 * key in your app's Info.plist file, and the user must authorize the always on background location permission in order for this flag to have any effect.
 *
 * For more info, see:
 *
 * https://developer.apple.com/documentation/corelocation/getting_the_user_s_location/handling_location_events_in_the_background
 *
 * https://developer.apple.com/documentation/corelocation/cllocationmanager/1620568-allowsbackgroundlocationupdates
 *
 * https://developer.apple.com/documentation/corelocation/choosing_the_authorization_level_for_location_services/requesting_always_authorization
 *
 * Default value is false, i.e. background location updates are not explicitly enabled
 */
@property(assign, nonatomic) BOOL allowsBackgroundLocationUpdates;

/**
 * The set of (dynamic) geofences monitored by the location manager. Note that automatically monitored cloud geofences are not included.
 *
 * You cannot add geofences to this property directly. Instead use the `[IALocationManager startMonitoringGeofence:]` method.
 */
@property(nonatomic, readonly, strong, nullable) NSArray<IAGeofence*> *monitoredGeofences;

/**
 * The latest extra information dictionary.
 *
 * Used for debugging positioning.
 */
@property (nonatomic, readonly, nullable) NSDictionary *extraInfo;

/**
 * @name Accessing the Delegate
 */

/**
 * The delegate object to receive update events.
 */
@property (nullable, nonatomic, readwrite, weak) id<IALocationManagerDelegate> delegate;

/**
 * @name SDK version
 */

/**
 * Returns SDK version string.
 *
 * The version string returned is in format "major.minor.patch". (see: [Semantic Versioning](http://semver.org/))
 */
+ (nonnull NSString*)versionString;

/**
 * Locks positioning to specified floor level
 * @param floorNumber Floor level where the positioning is restricted
 */
- (void)lockFloor:(int)floorNumber;

/**
 * Unlocks positioning from locked floor. If lockFloor has not been called before, this is
 * no-op.
 */
- (void)unlockFloor;

/**
 * Lock or unlock positioning indoors. Disables indoor-outdoor detection when locked.
 * Indoor lock is enabled by default.
 *
 * @param lockIndoor boolean value indicating whether to lock or unlock indoor positioning
 */
- (void)lockIndoors:(bool)lockIndoor;

/**
 * @name Authenticate your session
 */

/**
 * Set IndoorAtlas API key and secret for authentication.
 *
 * This method must be called once before starting location updates.
 * Changing API key at runtime will stop location updates and reset state.
 *
 * @param key API key used for authentication.
 * @param secret API secret used for authentication.
 */
- (void)setApiKey:(nonnull NSString*)key andSecret:(nonnull NSString*)secret;

/**
 * Starts the generation of updates that report the user's current location.
 *
 * This method returns immediately. Calling this method causes the location manager to obtain an initial location fix (which may take several seconds)
 * and notify your delegate by calling its `[IALocationManagerDelegate indoorLocationManager:didUpdateLocations:]` method. After that, the receiver `
 * generates update events whenever there is new estimate.
 *
 * Calling this method several times in succession does not automatically result in new events being generated.
 * Calling `[IALocationManager stopUpdatingLocation]` in-between, however, does cause a new initial event to be sent the next time you call this method.
 *
 * If you start this service and your app is suspended, the system stops the delivery of events until your app starts running again (only in foreground).
 * If your app is terminated, the delivery of new location events stops altogether.
 */
- (void)startUpdatingLocation;

/**
 * Stops the generation of location updates.
 *
 * Call this method whenever your code no longer needs to receive location-related events. Disabling event delivery gives the receiver the option of disabling the
 * appropriate hardware (and thereby saving power) when no clients need location data. You can always restart the generation of location updates by calling the
 * <startUpdatingLocation> method again.
 */
- (void)stopUpdatingLocation;

/**
 * Starts monitoring the specified geofence. Note that cloud geofences are monitored automatically.
 *
 * You must call this method once for each geofence you want to monitor. If an existing geofence with the same identifier is already being monitored by the app, the old geofence is replaced by the new one.
 * Geofence events are delivered as regions to the <indoorLocationManager:didEnterRegion:> and <indoorLocationManager:didExitRegion:> methods of your delegate.
 *
 * @param geofence The geofence object that defines the boundary to monitor.
 */
- (void)startMonitoringForGeofence:(nonnull IAGeofence*)geofence;

/**
 * Stops monitoring the specified geofence.
 *
 * If the specified geofence object is not currently being monitored, this method has no effect.
 *
 * @param geofence The geofence object currently being monitored.
 */
- (void)stopMonitoringForGeofence:(nonnull IAGeofence*)geofence;

/**
 * Start monitoring for wayfinding updates.
 * Calling this method causes the location manager to obtain a route from user's current location to destination defined in parameter "to" (this may take several seconds).
 * Calling this method notify your delegate by calling its <indoorLocationManager:didUpdateRoute:> method. After that, the receiver generates update events whenever the route changes.
 *
 * Calling this method several times in succession overwrites the previously done requests.
 * Calling <stopUpdatingLocation> in-between, however, does cause a new initial event to be sent the next time you call this method.
 *
 * @param to An <IALatLngFloorCompatible> object specifying the wayfinding destination
 */
- (void)startMonitoringForWayfinding:(nonnull id<IALatLngFloorCompatible>)to;

/**
 * Stops monitoring for wayfinding updates.
 *
 * Call this method whenever your code no longer needs to receive wayfinding route update events.
 * You can always restart the generation of wayfinding route updates by calling the <startMonitoringForWayfinding> method again.
 */
- (void)stopMonitoringForWayfinding;

/**
 * Request a single-shot wayfinding route. Callback with route result is called from the application main thread.
 *
 * @param from An <IALatLngFloorCompatible> object specifying the wayfinding starting location
 * @param to An <IALatLngFloorCompatible> object specifying the wayfinding destination
 * @param callback callback to call with route result
 */
- (void)requestWayfindingRouteFrom:(nonnull id<IALatLngFloorCompatible>)from to:(nonnull id<IALatLngFloorCompatible>)to callback:(void(^_Nonnull)(IARoute *_Nonnull))callback;

/**
 * Lazily creates AR session.
 *
 * To release memory and stop all AR related processing you must call `releaseArSession`.
 *
 * NOTE! To enable AR features, please contact IndoorAtlas sales.
 */
@property (nonatomic, readonly, nullable) IAARSession *arSession;

/**
 * Stops all AR related activity and releases the memory allocated for it.
 */
- (void)releaseArSession;

/**
 * Returns the shared <IALocationManager> instance.
 */
+ (nonnull IALocationManager *)sharedInstance;
@end

#undef INDOORATLAS_API
