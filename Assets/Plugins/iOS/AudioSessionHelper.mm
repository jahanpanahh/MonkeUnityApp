#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import "AudioSessionHelper.h"

static NSString *currentCategory = nil;
static AVAudioSessionCategoryOptions currentOptions = 0;
static AVAudioSessionMode currentMode = nil;
static BOOL sessionActive = NO;

static void SetSession(NSString *category, AVAudioSessionMode mode, AVAudioSessionCategoryOptions options)
{
    AVAudioSession *session = [AVAudioSession sharedInstance];
    NSError *error = nil;

    BOOL needsCategoryChange = !currentCategory || ![currentCategory isEqualToString:category] || currentOptions != options || (currentMode && ![currentMode isEqualToString:mode]);

    if (needsCategoryChange)
    {
        [session setCategory:category mode:mode options:options error:&error];
        if (error)
        {
            NSLog(@"MonkeAudioSession setCategory warning: %@", error);
            error = nil;
        }

        currentCategory = category;
        currentMode = mode;
        currentOptions = options;
    }

    if (!sessionActive)
    {
        [session setActive:YES error:&error];
        if (error)
        {
            NSLog(@"MonkeAudioSession setActive warning: %@", error);
        }
        else
        {
            sessionActive = YES;
        }
    }
}

void Monke_SetAudioSessionPlayback(void)
{
    // Use a single, stable category (PlayAndRecord) to avoid route churn when switching
    // between playback, STT, and TTS. Keeping the same category prevents stutters/ducks.
    SetSession(AVAudioSessionCategoryPlayAndRecord,
               AVAudioSessionModeDefault,
               AVAudioSessionCategoryOptionDefaultToSpeaker | AVAudioSessionCategoryOptionMixWithOthers | AVAudioSessionCategoryOptionAllowBluetooth);
}

void Monke_SetAudioSessionRecord(void)
{
    // Same category/options as playback to avoid any category switches during STT
    SetSession(AVAudioSessionCategoryPlayAndRecord,
               AVAudioSessionModeDefault,
               AVAudioSessionCategoryOptionDefaultToSpeaker | AVAudioSessionCategoryOptionMixWithOthers | AVAudioSessionCategoryOptionAllowBluetooth);
}
