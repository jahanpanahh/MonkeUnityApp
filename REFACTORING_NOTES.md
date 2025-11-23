# Monke! Project - Technical Debt & Refactoring Notes

This document tracks known issues, workarounds, and future refactoring tasks.

---

## 🔴 HIGH PRIORITY - Scene Transition Workaround

### Issue
Scene transitions were getting stuck when using the standard coroutine approach with `SceneTransitionManager`. The coroutines would start but never complete, even though:
- Time.timeScale was normal (1.0)
- WaitForSecondsRealtime was being used
- No errors in console

### Current Workaround (Temporary)
All scene controllers now use `Update()` timers instead of coroutines:
- `SplashScreenController.cs` - Line 18-30
- `LogoScreenController.cs` - Line 15-27
- `LoadingScreenController.cs` - Line 27-72

### What Should Be Done (When Time Permits)

#### 1. Investigate Root Cause
Check for:
- [ ] Any script destroying SceneTransitionManager
- [ ] Any script pausing/manipulating Time.timeScale
- [ ] Unity 6 version-specific coroutine bugs
- [ ] AudioManager or AIManager conflicts with DontDestroyOnLoad
- [ ] Scene loading order issues in Build Settings

#### 2. Restore Proper Pattern
Once root cause is found, restore the correct approach:

```csharp
// Proper SceneTransitionManager with coroutines
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName, float delay = 0f)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, delay));
    }

    IEnumerator LoadSceneCoroutine(string sceneName, float delay)
    {
        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);

        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}
```

Then update all controllers to use it:
```csharp
void Start()
{
    SceneTransitionManager.Instance.LoadScene("NextScene", displayDuration);
}
```

#### 3. Testing Checklist
After refactoring:
- [ ] SplashScreen → LogoScreen transition works
- [ ] LogoScreen → LoadingScreen transition works
- [ ] LoadingScreen → MonkeScene transition works
- [ ] AudioManager persists across all scenes
- [ ] AIManager initializes correctly in MonkeScene
- [ ] No duplicate managers spawned
- [ ] Build and test on iOS device

### Why It Matters
- **Code Quality**: Coroutines are the standard Unity pattern
- **Maintainability**: Update() timers are harder to understand/modify
- **Performance**: Minor - Update() runs every frame vs coroutine triggers
- **Best Practices**: Following Unity conventions makes onboarding easier

### When to Refactor
- ✅ **After AI integration is fully tested** (current priority)
- ✅ **Before production release** (polish phase)
- ✅ **When onboarding new developers** (code quality matters)

---

## 🟡 MEDIUM PRIORITY - Audio System

### Current State
AudioManager is manually placed in SplashScreen and uses DontDestroyOnLoad.

### Potential Improvements
1. **Auto-initialization**: AudioManager could auto-create itself if missing
2. **Resource-based audio**: Load audio clips from Resources folder dynamically
3. **Audio mixer**: Add volume controls and audio categories
4. **Fade transitions**: Already implemented, but could be more flexible

### Files
- `Assets/Scripts/Managers/AudioManager.cs`
- `Assets/Scripts/LoadingScreenAudio.cs`
- `Assets/Scripts/LogoScreenAudio.cs`
- `Assets/Scripts/MonkeSceneAudio.cs`

---

## 🟢 LOW PRIORITY - AI System

### Future Enhancements (Not Urgent)

#### Android Support
- [ ] Implement `SpeechToText_Android.cs` using Android SpeechRecognizer
- [ ] Implement `TextToSpeech_Android.cs` using Android TextToSpeech
- [ ] Create Android native plugins (Java/Kotlin)
- [ ] Add Android build post-processor

Files to create:
- `Assets/Scripts/AI/Speech/SpeechToText_Android.cs`
- `Assets/Scripts/AI/Speech/TextToSpeech_Android.cs`
- `Assets/Plugins/Android/SpeechRecognizer.java`
- `Assets/Plugins/Android/TextToSpeech.java`

#### Additional AI Services
- [ ] Implement `ClaudeService.cs` (Anthropic Claude API)
- [ ] Implement `LocalLLMService.cs` (llama.cpp server)
- [ ] Add fallback/retry logic between services

Files to update:
- `Assets/Scripts/AI/Services/ClaudeService.cs` (currently stub)
- `Assets/Scripts/AI/Services/LocalLLMService.cs` (currently stub)

#### Advanced Features
- [ ] Conversation persistence (save/load to disk)
- [ ] Multiple conversation topics/modes
- [ ] Voice activity detection (VAD)
- [ ] Interrupt/cancel mid-response
- [ ] Background mode support
- [ ] Analytics integration

---

## 📝 Code Quality Improvements

### Naming Conventions
All code follows C# naming conventions:
- PascalCase for public members ✅
- camelCase for private fields ✅
- SCREAMING_SNAKE_CASE for constants ✅

### Documentation
- [ ] Add XML documentation comments to public APIs
- [ ] Create architecture diagram
- [ ] Document conversation flow state machine

### Testing
- [ ] Add unit tests for AIManager state machine
- [ ] Add integration tests for scene transitions
- [ ] Add mock services for testing without API keys

---

## 🔧 Build & Deployment

### iOS
- ✅ Build post-processor works correctly
- ✅ Permissions auto-configured
- ✅ Frameworks auto-linked
- [ ] Test on multiple iOS versions (13+)
- [ ] Test on iPhone and iPad

### Android (Future)
- [ ] Create Android build post-processor
- [ ] Configure permissions (RECORD_AUDIO, INTERNET)
- [ ] Test on multiple Android versions (6.0+)

---

## 📚 Documentation Files

### User-Facing
- `AI_SETUP_GUIDE.md` - Step-by-step setup instructions ✅
- `IMPLEMENTATION_SUMMARY.md` - Technical overview ✅

### Developer-Facing
- `REFACTORING_NOTES.md` - This file ✅
- [ ] ARCHITECTURE.md - System design and data flow
- [ ] CONTRIBUTING.md - Development guidelines
- [ ] CHANGELOG.md - Version history

---

## 🎯 Priority Order

**Phase 1: Core Functionality (Current)**
1. ✅ AI integration working
2. ✅ Scene transitions working (with workaround)
3. ✅ Audio working across scenes
4. ⏳ Test on iOS device
5. ⏳ Verify full flow end-to-end

**Phase 2: Refactoring (Before Production)**
1. ⏳ Investigate and fix scene transition root cause
2. ⏳ Restore coroutine-based scene management
3. ⏳ Add XML documentation
4. ⏳ Clean up debug logs

**Phase 3: Polish (Production Ready)**
1. ⏳ Remove debug UI completely
2. ⏳ Optimize API calls (caching, rate limiting)
3. ⏳ Add error analytics
4. ⏳ Performance profiling

**Phase 4: Expansion (Post-Launch)**
1. ⏳ Android support
2. ⏳ Additional AI services
3. ⏳ Advanced features
4. ⏳ Localization

---

## 💡 Quick Reference

### Where to Find Things

**Scene Controllers:**
- SplashScreenController: `Assets/Scripts/SplashScreenController.cs`
- LogoScreenController: `Assets/Scripts/LogoScreenController.cs`
- LoadingScreenController: `Assets/Scripts/LoadingScreenController.cs`

**Managers:**
- AudioManager: `Assets/Scripts/Managers/AudioManager.cs`
- SceneTransitionManager: `Assets/Scripts/Managers/SceneTransitionManager.cs` (currently unused)
- AIManager: `Assets/Scripts/AI/Core/AIManager.cs`

**AI System:**
- Configuration: `Assets/Resources/AI/MonkeAIConfig.asset`
- OpenAI Service: `Assets/Scripts/AI/Services/OpenAIService.cs`
- iOS Speech: `Assets/Scripts/AI/Speech/SpeechToText_iOS.cs`, `TextToSpeech_iOS.cs`
- Native Plugins: `Assets/Plugins/iOS/SpeechRecognizer.mm`, `TextToSpeech.mm`

**Build:**
- iOS Post-Processor: `Assets/Editor/iOSBuildPostProcessor.cs`

---

## 📞 Notes from Development Session

### What Worked Well
- ✅ AI integration from MonkeUnityAppPOC was successful
- ✅ Platform abstraction (interfaces) makes Android support easier
- ✅ ScriptableObject configuration is flexible
- ✅ Event-driven architecture is clean

### What Needs Improvement
- ⚠️ Scene transitions broke mysteriously - root cause unknown
- ⚠️ Manager persistence pattern could be more robust
- ⚠️ Debug logging is excessive (good for development, remove for production)

### Lessons Learned
- Unity 6 may have different behavior than older versions
- DontDestroyOnLoad can have subtle timing issues
- Always have fallback patterns for critical systems
- Pragmatic > Perfect when shipping features

---

**Last Updated**: [Auto-generated by Claude Code during implementation]

**Status**: Scene transitions use workaround, AI fully functional, ready for iOS testing
