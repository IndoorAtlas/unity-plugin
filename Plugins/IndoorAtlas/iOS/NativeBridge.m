#import <Foundation/Foundation.h>
#import <IndoorAtlas/IALocationManager.h>

@interface IAUnityPlugin : NSObject <IALocationManagerDelegate>
@property (nonatomic, strong) IALocationManager *manager;
@property (nonatomic, copy) NSString *key, *secret;
@property (nonatomic, copy) NSString *gameObject;
@end

@interface IALocationManager ()
- (void)setObject:(id)object forKey:(NSString*)key;
@end

static NSString*
dict_to_json(NSDictionary *dictionary) {
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dictionary options:0 error:&error];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    if (error) {
        NSException *serializationError = [NSException
                                           exceptionWithName:@"unableToSerializeDictionaryException"
                                           reason:@"Unable to serialize NSDictionary"
                                           userInfo:nil];
        [serializationError raise];
    }
    return jsonString;
}

static NSDictionary*
geofence_to_dict(IAGeofence *geofence) {
    if (!geofence) return @{};
    if (![geofence isKindOfClass:[IAGeofence class]]) return @{};
    NSString *payload = (geofence.payload ? dict_to_json(geofence.payload) : @"{}");
    NSMutableArray *points = [NSMutableArray array];
    for (NSInteger i = 0; i < geofence.points.count; i += 2) {
        [points addObject:@{
              @"latitude": geofence.points[i],
             @"longitude": geofence.points[i + 1]
        }];
    }
    return @{
        @"id": geofence.identifier,
        @"name": geofence.name,
        @"payload": payload,
        @"position": @{
            @"coordinate": @{
              @"latitude": @(geofence.coordinate.latitude),
              @"longitude": @(geofence.coordinate.longitude)
            },
            @"floor": @(geofence.floor.level)
        },
        @"points": points
    };
}

static NSDictionary*
poi_to_dict(IAPOI *poi) {
    if (!poi) return @{};
    NSString *payload = (poi.payload ? dict_to_json(poi.payload) : @"{}");
    return @{
        @"id": poi.identifier,
        @"name": poi.name,
        @"payload": payload ? payload : @"{}",
        @"position": @{
            @"coordinate": @{
              @"latitude": @(poi.coordinate.latitude),
              @"longitude": @(poi.coordinate.longitude)
            },
            @"floor": @(poi.floor.level)
        }
    };
}

static NSDictionary*
floorplan_to_dict(IAFloorPlan *plan) {
    if (!plan) return @{};
    return @{
        @"id": plan.floorPlanId,
        @"name": plan.name,
        @"imageUrl": plan.imageUrl.absoluteString,
        @"width": @(plan.width),
        @"height": @(plan.height),
        @"pixelToMeterConversion": @(plan.pixelToMeterConversion),
        @"meterToPixelConversion": @(plan.meterToPixelConversion),
        @"widthMeters": @(plan.widthMeters),
        @"heightMeters": @(plan.heightMeters),
        @"floor": @(plan.floor.level),
    };
}

static NSDictionary*
venue_to_dict(IAVenue *venue) {
    if (!venue) return @{};
    NSMutableArray *floorplans = [NSMutableArray array];
    for (IAFloorPlan *plan in venue.floorplans) [floorplans addObject:floorplan_to_dict(plan)];
    NSMutableArray *geofences = [NSMutableArray array];
    for (IAGeofence *geo in venue.geofences) [geofences addObject:geofence_to_dict(geo)];
    NSMutableArray *pois = [NSMutableArray array];
    for (IAPOI *poi in venue.pois) [pois addObject:poi_to_dict(poi)];
    return @{
        @"id": venue.id,
        @"name": venue.name,
        @"floorplans": floorplans,
        @"geofences": geofences,
        @"pois": pois
    };
}

static NSString*
region_to_json(IARegion *region) {
    return dict_to_json(@{
                @"id": region.identifier,
                @"name": region.name,
                @"type": @(region.type),
                @"timestamp": region.timestamp ? @(region.timestamp.timeIntervalSince1970) : @(-1),
                @"venue": venue_to_dict(region.venue),
                @"floorplan": floorplan_to_dict(region.floorplan),
                @"geofence": geofence_to_dict((IAGeofence*)region)
            });
}

static NSDictionary*
route_point_to_dict(IARoutePoint *point) {
    return @{
        @"position": @{
            @"coordinate": @{
              @"latitude": @(point.coordinate.latitude),
              @"longitude": @(point.coordinate.longitude)
            },
            @"floor": @(point.floor),
        },
    @"nodeIndex": @(point.nodeIndex)
    };

}

static NSDictionary*
route_leg_to_dict(IARouteLeg *leg) {
    return @{
        @"begin": route_point_to_dict(leg.begin),
        @"end": route_point_to_dict(leg.end),
        @"length": @(leg.length),
        @"direction": @(leg.direction),
        @"edgeIndex": @(leg.edgeIndex),
    };
}

static NSString*
route_to_json(IARoute *route) {
    NSMutableArray *legs = [NSMutableArray array];
    NSString *error = @"NO_ERROR";
    switch (route.error) {
        case kIARouteErrorNoError: error = @"NO_ERROR"; break;
        case kIARouteErrorRoutingFailed: error = @"ROUTING_FAILED"; break;
        case kIARouteErrorGraphNotAvailable: error = @"GRAPH_NOT_AVAILABLE"; break;
    }
    for (IARouteLeg *leg in route.legs) [legs addObject:route_leg_to_dict(leg)];
    return dict_to_json(@{
            @"legs": legs,
     @"isSuccessful": @(route.isSuccessful),
           @"error": error
           });
}

static NSString*
location_to_json(IALocation *location) {
    if (!location) return @"";
    CLLocation *l = location.location;
    return dict_to_json(@{
          @"accuracy": @(l.horizontalAccuracy),
          @"altitude": @(l.altitude),
          @"bearing": @(l.course),
          @"position": @{
              @"coordinate": @{
                  @"latitude": @(l.coordinate.latitude),
                  @"longitude": @(l.coordinate.longitude)
              },
              @"floor": @(l.floor.level),
          },
          @"timestamp": @([l.timestamp timeIntervalSince1970])
      });
}

@implementation IAUnityPlugin
- (id)initWithObject:(NSString *)gameObjectName apiKey:(NSString *)apiKey apiSecret:(NSString *)apiSecret apiEndpoint:(NSString*)apiEndpoint {
    self = [super init];
    self.gameObject = gameObjectName;
    self.manager = [IALocationManager sharedInstance];
    self.manager.delegate = self;
    // TODO: get version from saner place
    [self.manager setObject:@{@"name": @"unity", @"version": @"0.0.1"} forKey:@"IAWrapper"];
    if (apiEndpoint.length) [self.manager setObject:apiEndpoint forKey:@"IACustomEndpoint"];
    [self.manager setApiKey:apiKey andSecret:apiSecret];
    return self;
}

- (void)close {
    [self.manager stopUpdatingLocation];
}

- (void)indoorLocationManager:(IALocationManager*)manager didUpdateLocations:(NSArray*)locations {
    (void)manager;
    NSString *json = location_to_json(locations.lastObject);
    UnitySendMessage(self.gameObject.UTF8String, "NativeIndoorAtlasOnLocationChanged", json.UTF8String);
}

- (void)indoorLocationManager:(IALocationManager *)manager didUpdateAttitude:(nonnull IAAttitude *)newAttitude {
    (void)manager;
    NSString *json = dict_to_json(@{
                @"x": @(newAttitude.quaternion.x),
                @"y": @(newAttitude.quaternion.y),
                @"z": @(newAttitude.quaternion.z),
                @"w": @(newAttitude.quaternion.w),
                @"timestamp": @((long)([newAttitude.timestamp timeIntervalSince1970] * 1000.0))
            });
    UnitySendMessage(self.gameObject.UTF8String, "NativeIndoorAtlasOnOrientationChanged", json.UTF8String);
}

- (void)indoorLocationManager:(nonnull IALocationManager *)manager didUpdateHeading:(nonnull IAHeading *)newHeading {
    (void)manager;
    NSString *json = dict_to_json(@{
                @"heading": @(newHeading.trueHeading),
                @"timestamp": @((long)(newHeading.timestamp.timeIntervalSince1970 * 1000.0))
            });
    UnitySendMessage(self.gameObject.UTF8String, "NativeIndoorAtlasOnHeadingChanged", json.UTF8String);
}

- (void)indoorLocationManager:(nonnull IALocationManager *)manager statusChanged:(nonnull IAStatus *)status {
    (void)manager;
    NSString *json = dict_to_json(@{@"status": @(status.type)});
    UnitySendMessage(self.gameObject.UTF8String, "NativeIndoorAtlasOnStatusChanged", json.UTF8String);
}

- (void)indoorLocationManager:(IALocationManager *)manager didEnterRegion:(IARegion *)region {
    (void)manager;
    NSString *json = region_to_json(region);
    UnitySendMessage(self.gameObject.UTF8String, "NativeIndoorAtlasOnEnterRegion", json.UTF8String);
}

- (void)indoorLocationManager:(IALocationManager *)manager didExitRegion:(IARegion *)region {
    (void)manager;
    NSString *json = region_to_json(region);
    UnitySendMessage(self.gameObject.UTF8String, "NativeIndoorAtlasOnExitRegion", json.UTF8String);
}

- (void)indoorLocationManager:(IALocationManager *)manager didUpdateRoute:(IARoute *)route {
    (void)manager;
    NSString *json = route_to_json(route);
    UnitySendMessage(self.gameObject.UTF8String, "NativeIndoorAtlasOnRoute", json.UTF8String);
}
@end

static IAUnityPlugin *_plugin;

static NSDictionary*
cstr_to_dict(const char *str) {
    NSData *data = [[NSData alloc] initWithBytesNoCopy:str length:strlen(str) freeWhenDone:false];
    return [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
}

static IAWayfindingRequest*
cstr_to_wayfinding_request(const char *str) {
    NSDictionary *dict = cstr_to_dict(str);
    IAWayfindingRequest *request = [IAWayfindingRequest new];
    request.coordinate = CLLocationCoordinate2DMake([dict[@"coordinate"][@"latitude"] doubleValue], [dict[@"coordinate"][@"longitude"] doubleValue]);
    request.floor = [dict[@"floor"] integerValue];
    return request;
}

static const char*
nsstring_to_unity_string(NSString *str) {
    NSData *data = [str dataUsingEncoding:NSUTF8StringEncoding];
    char *cpy = calloc(1, data.length + 1); // calloc initalizes with zero, thus zero terminates as well
    assert(cpy && "calloc failed");
    memcpy(cpy, data.bytes, data.length);
    return cpy;
}

static void
simd_float4x4_to_unity_matrix(const simd_float4x4 *in, float out[16]) {
    for (int i = 0; i < 4; ++i) {
        out[i * 4 + 0] = in->columns[i].x;
        out[i * 4 + 1] = in->columns[i].y;
        out[i * 4 + 2] = in->columns[i].z;
        out[i * 4 + 3] = in->columns[i].w;
    }
}

static void
unity_matrix_to_simd_float4x4(const float in[16], simd_float4x4 *out) {
    for (int i = 0; i < 4; ++i) {
        out->columns[i].x = in[i * 4 + 0];
        out->columns[i].y = in[i * 4 + 1];
        out->columns[i].z = in[i * 4 + 2];
        out->columns[i].w = in[i * 4 + 3];
    }
}

bool
indooratlas_close(void) {
    const bool ret = !!_plugin;
    [_plugin close];
    _plugin = nil;
    return ret;
}

bool
indooratlas_init(const char *apiKey, const char *apiSecret, const char *apiEndpoint, const char *gameObjectName) {
    if (_plugin) return false;
    NSString *key = [NSString stringWithUTF8String:apiKey];
    NSString *secret = [NSString stringWithUTF8String:apiSecret];
    NSString *endpoint = [NSString stringWithUTF8String:apiEndpoint];
    NSString *object = [NSString stringWithUTF8String:gameObjectName];
    _plugin = [[IAUnityPlugin alloc] initWithObject:object apiKey:key apiSecret:secret apiEndpoint:endpoint];
    return !!_plugin;
}

const char*
indooratlas_versionString(void) {
    return nsstring_to_unity_string([IALocationManager versionString]);
}

void
indooratlas_setDistanceFilter(double filter) {
    [_plugin.manager setDistanceFilter:filter];
}

double
indooratlas_getDistanceFilter(void) {
    return [_plugin.manager distanceFilter];
}

void
indooratlas_setTimeFilter(double filter) {
    [_plugin.manager setTimeFilter:filter];
}

double
indooratlas_getTimeFilter(void) {
    return [_plugin.manager timeFilter];
}

void
indooratlas_setHeadingFilter(double filter) {
    [_plugin.manager setHeadingFilter:filter];
}

double
indooratlas_getHeadingFilter(void) {
    return [_plugin.manager headingFilter];
}

void
indooratlas_setAttitudeFilter(double filter) {
    [_plugin.manager setAttitudeFilter:filter];
}

double
indooratlas_getAttitudeFilter(void) {
    return [_plugin.manager attitudeFilter];
}

void
indooratlas_lockFloor(int floor) {
    [_plugin.manager lockFloor:floor];
}

void
indooratlas_unlockFloor(void) {
    [_plugin.manager unlockFloor];
}

void
indooratlas_lockIndoors(bool lock) {
    [_plugin.manager lockIndoors:lock];
}

void
indooratlas_startUpdatingLocation(void) {
    [_plugin.manager startUpdatingLocation];
}

void
indooratlas_stopUpdatingLocation(void) {
    [_plugin.manager stopUpdatingLocation];
}

void
indooratlas_startMonitoringForWayfinding(const char *to) {
    [_plugin.manager startMonitoringForWayfinding:cstr_to_wayfinding_request(to)];
}

void
indooratlas_stopMonitoringForWayfinding(void) {
    [_plugin.manager stopMonitoringForWayfinding];
}

const char*
indooratlas_traceID(void) {
    return nsstring_to_unity_string(_plugin.manager.extraInfo[kIATraceId]);
}

void
indooratlas_releaseArSession(void) {
    [_plugin.manager releaseArSession];
}

void
indooratlas_setArPoseMatrix(const float matrix[16]) {
    simd_float4x4 simd;
    unity_matrix_to_simd_float4x4(matrix, &simd);
    [_plugin.manager.arSession setPoseMatrix:simd];
}

void
indooratlas_setArCameraToWorldMatrix(const float matrix[16]) {
    simd_float4x4 simd;
    unity_matrix_to_simd_float4x4(matrix, &simd);
    [_plugin.manager.arSession setCameraToWorldMatrix:simd];
}

bool
indooratlas_getArIsConverged(void) {
    return _plugin.manager.arSession.converged;
}

bool
indooratlas_getArCompassMatrix(float matrix[16]) {
    simd_float4x4 simd = matrix_identity_float4x4;
    const bool ret = [_plugin.manager.arSession.wayfindingCompassArrow updateModelMatrix:&simd];
    simd_float4x4_to_unity_matrix(&simd, matrix);
    return ret;
}

bool
indooratlas_getArGoalMatrix(float matrix[16]) {
    simd_float4x4 simd = matrix_identity_float4x4;
    const bool ret = [_plugin.manager.arSession.wayfindingTarget updateModelMatrix:&simd];
    simd_float4x4_to_unity_matrix(&simd, matrix);
    return ret;
}

int
indooratlas_getArTurnCount(void) {
    return (int)_plugin.manager.arSession.wayfindingTurnArrows.count;
}

bool
indooratlas_getArTurnMatrix(int index, float matrix[16]) {
    simd_float4x4 simd = matrix_identity_float4x4;
    const bool ret = [_plugin.manager.arSession.wayfindingTurnArrows[index] updateModelMatrix:&simd];
    simd_float4x4_to_unity_matrix(&simd, matrix);
    return ret;
}

void
indooratlas_addArPlane(float cx, float cy, float cz, float ex, float ez) {
    [_plugin.manager.arSession addPlaneWithCenterX:cx withCenterY:cy withCenterZ:cz withExtentX:ex withExtentZ:ez];
}

void
indooratlas_geoToAr(double lat, double lon, int floor, float heading, float zOffset, float matrix[16]) {
   CLLocationCoordinate2D coord = { lat, lon };
   simd_float4x4 simd = [_plugin.manager.arSession geoToAr:coord floorNumber:floor heading:heading zOffset:zOffset];
   simd_float4x4_to_unity_matrix(&simd, matrix);
}

const char*
indooratlas_arToGeo(double x, double y, double z) {
   IALocation *l = [_plugin.manager.arSession arToGeo:x Y:y Z:z];
   return nsstring_to_unity_string(location_to_json(l));
}
