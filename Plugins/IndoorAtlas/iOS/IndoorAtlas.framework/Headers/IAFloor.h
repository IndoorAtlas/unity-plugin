// IndoorAtlas iOS SDK
// IAFloor.h

#import <Foundation/Foundation.h>
#define INDOORATLAS_API __attribute__((visibility("default")))

/**
 *  IACertainty
 *
 *  Discussion:
 *    Type used for representing the certainty that something is true.
 *    Has a value from 0.0 to 1.0, inclusive. A negative value indicates an invalid certainty.
 */
typedef double IACertainty;

/**
 * IAFloor specifies the floor of the building on which the user is located.
 * It is a replacement for CoreLocation's CLFloor as the interface for that is not open.
 */
INDOORATLAS_API
@interface IAFloor : NSObject

/**
 * Initializes and returns a floor object with the specified level information.
 * @param level initializes level value
 */
+ (nonnull IAFloor*)floorWithLevel:(NSInteger)level;

/**
 * Floor level values correspond to the floor numbers assigned by the user in the mapping phase.
 *
 * It is erroneous to use the user's level in a building as an estimate of altitude.
 */
@property (nonatomic, readonly) NSInteger level;

/**
 * Certainty that `IALocation` floor has the correct value.
 */
@property (nonatomic, readonly) IACertainty certainty;

@end

#undef INDOORATLAS_API
