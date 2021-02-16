// IndoorAtlas iOS SDK
// IAFloorPlan.h

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>
#import <CoreGraphics/CoreGraphics.h>
#import <IndoorAtlas/IAFloor.h>
#define INDOORATLAS_API __attribute__((visibility("default")))

/**
 * IAFloorPlan represents floor plan data received from service.
 */
INDOORATLAS_API
@interface IAFloorPlan : NSObject

/**
 * @name Floor plan information
 */

/**
 * Identifier of the floor plan.
 */
@property (nonatomic, readonly, nullable) NSString *floorPlanId;

/**
 * Name of the floor plan.
 */
@property (nonatomic, readonly, nullable) NSString *name;

/**
 * Image URL of the floor plan.
 */
@property (nonatomic, readonly, nullable) NSURL *imageUrl;

/**
 * Width of the image bitmap in pixels.
 */
@property (nonatomic, readonly) NSUInteger width;

/**
 * Height of the image bitmap in pixels.
 */
@property (nonatomic, readonly) NSUInteger height;

/**
 * Conversion multiplier from pixels to meters.
 */
@property (nonatomic, readonly) float pixelToMeterConversion;

/**
 * Conversion multiplier from meters to pixels.
 */
@property (nonatomic, readonly) float meterToPixelConversion;

/**
 * Width of floor plan in meters.
 */
@property (nonatomic, readonly) float widthMeters;

/**
 * Height of floor plan in meters.
 */
@property (nonatomic, readonly) float heightMeters;

/**
 * Object containing the floor of floor plan.
 * If the object is nil, the floor is unspecified.
 */
@property (nonatomic, readonly, nullable) IAFloor *floor;

/**
 * The approximate bearing of left side of floor plan in
 * degrees east of true north.
 */
@property (nonatomic, readonly) double bearing;

/**
 * Corresponding WGS84 coordinate of center of floor plan bitmap placed
 * on the surface of Earth.
 */
@property (nonatomic, readonly) CLLocationCoordinate2D center;

/**
 * Corresponding WGS84 coordinate of top left of floor plan bitmap placed
 * on the surface of Earth.
 */
@property (nonatomic, readonly) CLLocationCoordinate2D topLeft;

/**
 * Corresponding WGS84 coordinate of top right of floor plan bitmap placed
 * on the surface of Earth.
 */
@property (nonatomic, readonly) CLLocationCoordinate2D topRight;

/**
 * Corresponding WGS84 coordinate of bottom left of floor plan bitmap placed
 * on the surface of Earth.
 */
@property (nonatomic, readonly) CLLocationCoordinate2D bottomLeft;

/**
 * Corresponding WGS84 coordinate of bottom right of floor plan bitmap placed
 * on the surface of Earth.
 */
@property (nonatomic, readonly) CLLocationCoordinate2D bottomRight;

/**
 * Converts coordinate to corresponding point.
 *
 * @param coord WGS84 coordinate
 * @return corresponding pixel point on floor plan bitmap
 */
- (CGPoint)coordinateToPoint:(CLLocationCoordinate2D)coord;

/**
 * Converts point to corresponding coordinate.
 *
 * @param point pixel point of floor plan bitmap
 * @return corresponding WGS84 coordinate
 */
- (CLLocationCoordinate2D)pointToCoordinate:(CGPoint)point;

@end

#undef INDOORATLAS_API
