using Blazored.LocalStorage;
using PrismAI.Core.Models.PrismAIModels;

namespace PrismAI.Core.Services;

public class LocalBrowserStorageService(ILocalStorageService localStorage)
{
    private const string ExperienceStoragePrefix = "experience_";
    public async ValueTask SaveLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location cannot be null or empty.", nameof(location));
        
        await localStorage.SetItemAsync("locationPoint", location);
    }
    public async ValueTask<string?> GetLocationAsync()
    {
        return await localStorage.GetItemAsync<string>("locationPoint");
    }
    public async ValueTask SaveCurrentExperienceAsync(Experience experience, UserPreferences preferences)
    {
        if (experience == null)
            throw new ArgumentNullException(nameof(experience), "Experience cannot be null.");
        var date = DateTime.UtcNow.ToString("MMdd");
        var key = $"{ExperienceStoragePrefix}{experience.Title.Replace(' ','_')}_{date}";
        var experienceReqResp = new ExperienceRequestResponse
        {
            Key = key,
            ResponseExperience = experience,
            RequestPreferences = preferences,
            LastUpdate = DateTime.UtcNow
        };
        await localStorage.SetItemAsync(key, experienceReqResp);
    }

    public async ValueTask UpdateExperience(Experience experience)
    {
        var keys = (await localStorage.KeysAsync()).ToList();
        if (!keys.Any(k => k.Contains(ExperienceStoragePrefix + experience.Title.Replace(' ','_'))))
            return;
        var key = keys.First(k => k.Contains(ExperienceStoragePrefix + experience.Title.Replace(' ','_')));
        var existingExperience = await localStorage.GetItemAsync<ExperienceRequestResponse>(key);
        existingExperience.ResponseExperience = experience;
        existingExperience.LastUpdate = DateTime.UtcNow;
        await localStorage.SetItemAsync(key, existingExperience);
    }
    public async ValueTask<List<ExperienceRequestResponse>> LoadExperiencesAsync()
    {
        var keys = await localStorage.KeysAsync();
        var experienceKeys = keys.Where(k => k.StartsWith(ExperienceStoragePrefix)).ToList();
        var experiences = new List<ExperienceRequestResponse>();
        foreach (var key in experienceKeys)
        {
            var experience = await localStorage.GetItemAsync<ExperienceRequestResponse>(key);
            if (experience != null)
            {
                experiences.Add(experience);
            }
        }
        return experiences.OrderByDescending(x => x.LastUpdate).ToList();
    }

    public async ValueTask SaveUserProfileAsync(UserProfile profile)
    {
        await localStorage.SetItemAsync("user_profile", profile);
    }
    public async ValueTask<UserProfile?> GetUserProfileAsync()
    {
        var keys = await localStorage.KeysAsync();
        if (!keys.Contains("user_profile"))
            return null;
        return await localStorage.GetItemAsync<UserProfile>("user_profile");
    }
}