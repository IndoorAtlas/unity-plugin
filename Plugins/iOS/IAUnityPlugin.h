#import <Foundation/Foundation.h>

@interface IAUnityPlugin : NSObject
- (void)init:(NSString *)gameObjectName apiKey:(NSString *)apiKey apiSecret:(NSString *)apiSecret headingSensitivity:(double)headingSensitivity orientationSensitivity:(double)orientationSensitivity;
- (void)close;
@end
