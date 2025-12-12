using UnityEngine;

public static class APIConfig
{
#if UNITY_EDITOR
    public const string API_BASE_URL = "http://localhost:3000";
#else
    public const string API_BASE_URL = "https://api.tilevania.com";
#endif

    // Bump when breaking changes require a forced client update.
    public const string CLIENT_VERSION = "1.0.0";

    public const int REQUEST_TIMEOUT_SECONDS = 15;

    // API Endpoints
    // Sessions & game profile
    public static string Sessions => "/api/sessions";
    public static string Session(string sessionId) => $"/api/sessions/{sessionId}";
    public static string EndSession(string sessionId) => $"/api/sessions/{sessionId}/end";
    public static string GameProfile(string userId) => $"/api/gameProfile/{userId}";
    public static string GameProfileScore(string userId) => $"/api/gameProfile/{userId}/score";
    public static string GameProfileCoins(string userId) => $"/api/gameProfile/{userId}/coins";
    public static string GameProfileDeath(string userId) => $"/api/gameProfile/{userId}/death";

    // Levels & level progress
    public static string Levels => "/api/levels";
    public static string LevelProgress(string userId) => $"/api/levelProgress/{userId}";
    public static string LevelProgressLevel(string userId, string levelId) => $"/api/levelProgress/{userId}/{levelId}";
    public static string CompleteLevel(string userId, string levelId) => $"/api/levelProgress/{userId}/{levelId}/complete";

    // Achievements
    public static string Achievements => "/api/achievements";
    public static string PlayerAchievements(string userId) => $"/api/achievements/{userId}/unlocked";
    public static string UnlockAchievement(string userId, string achievementId) => $"/api/achievements/{userId}/unlock/{achievementId}";
}

