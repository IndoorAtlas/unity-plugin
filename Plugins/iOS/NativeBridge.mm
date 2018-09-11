#import "IAUnityPlugin.h"
#include <string.h>

IAUnityPlugin *_plugin = nil;

extern "C"
{
    bool IAclose()
    {
        if (_plugin != nil) {
            [_plugin close];
            _plugin = nil;
            return true;
        }
        return false;
    }

    bool IAinit(const char* apiKey, const char *apiSecret, const char *gameObjectName, double headingSensitivity, double orientationSensitivity)
    {
        if (_plugin != nil) {
            return false;
        }

        _plugin = [IAUnityPlugin new];
        NSString *key = [NSString stringWithUTF8String:apiKey];
        NSString *secret = [NSString stringWithUTF8String:apiSecret];
        NSString *object = [NSString stringWithUTF8String:gameObjectName];
        [_plugin init:object apiKey:key apiSecret:secret headingSensitivity:headingSensitivity orientationSensitivity:orientationSensitivity];
        return true;
    }

    const char* traceID()
    {
        if (_plugin == nil) {
            return 0;
        }
        const char* traceId = [_plugin traceId];
        char* output = (char*)calloc(strlen(traceId) + 1, 1);
        return strcpy(output, traceId);
    }
}
