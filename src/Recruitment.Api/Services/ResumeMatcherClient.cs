using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Recruitment.Api.Services;

public class ResumeMatcherClient(HttpClient client)
{
    public async Task<MatchResult?> MatchAsync(string resumeText, string description, List<string> skills)
    {
        var response = await client.PostAsJsonAsync("/match", new
        {
            resume_text = resumeText,
            job_description = description,
            required_skills = skills
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MatchResult>();
    }
}

public record MatchResult(
    [property: JsonPropertyName("match_score")] int MatchScore,
    [property: JsonPropertyName("matched_skills")] List<string> MatchedSkills,
    [property: JsonPropertyName("missing_skills")] List<string> MissingSkills,
    [property: JsonPropertyName("recommendation")] string Recommendation,
    [property: JsonPropertyName("explanation")] string Explanation);
