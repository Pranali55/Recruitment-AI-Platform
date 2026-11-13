using Microsoft.EntityFrameworkCore;
using Recruitment.Api.Data;
using Recruitment.Api.Models;
using Recruitment.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<RecruitmentDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Recruitment") ?? "Data Source=recruitment.db"));
builder.Services.AddHttpClient<ResumeMatcherClient>(c => { c.BaseAddress = new Uri(builder.Configuration["Matcher:BaseUrl"] ?? "http://127.0.0.1:8001"); c.Timeout = TimeSpan.FromSeconds(10); });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RecruitmentDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db);
}
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", service = "Recruitment.Api" }));
app.MapGet("/api/dashboard", async (RecruitmentDbContext db) => Results.Ok(new {
    activeJobs = await db.Jobs.CountAsync(j => j.IsActive),
    totalCandidates = await db.Candidates.CountAsync(),
    totalApplications = await db.Applications.CountAsync(),
    shortlisted = await db.Applications.CountAsync(a => a.Status == ApplicationStatus.Shortlisted)
}));
app.MapGet("/api/jobs", async (RecruitmentDbContext db) => Results.Ok(await db.Jobs.OrderByDescending(j => j.CreatedAt).ToListAsync()));
app.MapPost("/api/jobs", async (CreateJobRequest r, RecruitmentDbContext db) => {
    if (string.IsNullOrWhiteSpace(r.Title) || string.IsNullOrWhiteSpace(r.Department)) return Results.BadRequest(new { message = "Title and department are required." });
    var job = new Job { Title=r.Title.Trim(), Department=r.Department.Trim(), Location=r.Location?.Trim() ?? "Not specified", Description=r.Description?.Trim() ?? "", RequiredSkills=r.RequiredSkills ?? new(), IsActive=true };
    db.Jobs.Add(job); await db.SaveChangesAsync(); return Results.Created($"/api/jobs/{job.Id}", job);
});
app.MapGet("/api/candidates", async (RecruitmentDbContext db) => Results.Ok(await db.Candidates.OrderByDescending(c => c.CreatedAt).Select(c => new { c.Id,c.FullName,c.Email,c.Phone,c.CreatedAt }).ToListAsync()));
app.MapPost("/api/candidates", async (CreateCandidateRequest r, RecruitmentDbContext db) => {
    if (string.IsNullOrWhiteSpace(r.FullName) || string.IsNullOrWhiteSpace(r.Email)) return Results.BadRequest(new { message = "Candidate name and email are required." });
    if (await db.Candidates.AnyAsync(c => c.Email == r.Email)) return Results.Conflict(new { message = "A candidate with this email already exists." });
    var candidate = new Candidate { FullName=r.FullName.Trim(), Email=r.Email.Trim(), Phone=r.Phone?.Trim() ?? "", ResumeText=r.ResumeText?.Trim() ?? "" };
    db.Candidates.Add(candidate); await db.SaveChangesAsync();
    return Results.Created($"/api/candidates/{candidate.Id}", new { candidate.Id,candidate.FullName,candidate.Email,candidate.Phone });
});
app.MapPost("/api/candidates/{id:int}/resume", async (int id, IFormFile file, RecruitmentDbContext db) => {
    var candidate = await db.Candidates.FindAsync(id);
    if (candidate is null) return Results.NotFound(new { message="Candidate not found." });
    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (ext is not ".txt" and not ".md") return Results.BadRequest(new { message="Upload a .txt or .md resume file." });
    if (file.Length > 1_000_000) return Results.BadRequest(new { message="File must be 1 MB or smaller." });
    using var reader = new StreamReader(file.OpenReadStream());
    candidate.ResumeText = await reader.ReadToEndAsync(); await db.SaveChangesAsync();
    return Results.Ok(new { message="Resume text saved.", candidate.Id });
});
app.MapGet("/api/applications", async (RecruitmentDbContext db) => Results.Ok(await db.Applications.Include(a=>a.Job).Include(a=>a.Candidate).OrderByDescending(a=>a.CreatedAt).Select(a=>new { a.Id,a.Status,a.CreatedAt,Candidate=a.Candidate!.FullName,CandidateId=a.CandidateId,Job=a.Job!.Title,JobId=a.JobId }).ToListAsync()));
app.MapPost("/api/applications", async (CreateApplicationRequest r, RecruitmentDbContext db) => {
    if (!await db.Jobs.AnyAsync(j=>j.Id==r.JobId && j.IsActive)) return Results.BadRequest(new { message="Active job not found." });
    if (!await db.Candidates.AnyAsync(c=>c.Id==r.CandidateId)) return Results.BadRequest(new { message="Candidate not found." });
    if (await db.Applications.AnyAsync(a=>a.JobId==r.JobId && a.CandidateId==r.CandidateId)) return Results.Conflict(new { message="This candidate has already applied for this job." });
    var a = new JobApplication { JobId=r.JobId,CandidateId=r.CandidateId }; db.Applications.Add(a); await db.SaveChangesAsync(); return Results.Created($"/api/applications/{a.Id}",a);
});
app.MapPatch("/api/applications/{id:int}/status", async (int id, UpdateStatusRequest r, RecruitmentDbContext db) => {
    var a = await db.Applications.FindAsync(id); if (a is null) return Results.NotFound(new { message="Application not found." });
    if (!Enum.TryParse<ApplicationStatus>(r.Status,true,out var status)) return Results.BadRequest(new { message="Status must be Applied, Screening, Shortlisted, Interview, Offered, Hired or Rejected." });
    a.Status=status; await db.SaveChangesAsync(); return Results.Ok(new { a.Id,Status=a.Status.ToString() });
});
app.MapPost("/api/match/{jobId:int}/{candidateId:int}", async (int jobId,int candidateId,RecruitmentDbContext db,ResumeMatcherClient matcher) => {
    var job=await db.Jobs.FindAsync(jobId); var candidate=await db.Candidates.FindAsync(candidateId);
    if(job is null || candidate is null) return Results.NotFound(new { message="Job or candidate not found." });
    if(string.IsNullOrWhiteSpace(candidate.ResumeText)) return Results.BadRequest(new { message="Add resume text before matching." });
    try { return Results.Ok(await matcher.MatchAsync(candidate.ResumeText,job.Description,job.RequiredSkills)); }
    catch(HttpRequestException) { return Results.Problem("Python matcher unavailable. Start it on port 8001.",statusCode:503); }
});
app.Run();
public partial class Program { }
record CreateJobRequest(string Title,string Department,string? Location,string? Description,List<string>? RequiredSkills);
record CreateCandidateRequest(string FullName,string Email,string? Phone,string? ResumeText);
record CreateApplicationRequest(int JobId,int CandidateId);
record UpdateStatusRequest(string Status);
