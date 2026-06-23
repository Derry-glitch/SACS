using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SACS.Application.Common.Interfaces;

namespace SACS.Infrastructure.Services;

public class PythonAiServiceClient : IAiServiceClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PythonAiServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<DeadlineExtractionResponseDto> ExtractDeadlinesAsync(string text, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/ai/extract-deadline", new { text }, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DeadlineExtractionResponseDto>(JsonOptions, cancellationToken);
        return result ?? new DeadlineExtractionResponseDto();
    }

    public async Task<SummaryResponseDto> SummarizeLectureNotesAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        
        content.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync("/ai/summarize", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SummaryResponseDto>(JsonOptions, cancellationToken);
        return result ?? new SummaryResponseDto { Summary = string.Empty };
    }

    public async Task<QuizGenerationResponseDto> GenerateQuizAsync(string content, string difficulty, CancellationToken cancellationToken = default)
    {
        var payload = new { content, difficulty };
        var response = await _httpClient.PostAsJsonAsync("/ai/generate-quiz", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<QuizGenerationResponseDto>(JsonOptions, cancellationToken);
        return result ?? new QuizGenerationResponseDto();
    }

    public async Task<StudyPlanResponseDto> GenerateStudyPlanAsync(StudyPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/ai/generate-study-plan", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<StudyPlanResponseDto>(JsonOptions, cancellationToken);
        return result ?? new StudyPlanResponseDto();
    }
}
