using Microsoft.EntityFrameworkCore;
using Recruitment.Api.Models;
namespace Recruitment.Api.Data;
public static class SeedData {
 public static async Task InitializeAsync(RecruitmentDbContext db) {
  if(await db.Jobs.AnyAsync()) return;
  var job=new Job { Title="Senior .NET + Python Developer",Department="Engineering",Location="Pune / Hybrid",Description="Build REST APIs using C#, ASP.NET Core, Python and FastAPI. Work with SQL Server, AWS, Docker, microservices and CI/CD.",RequiredSkills=new(){"C#",".NET","ASP.NET Core","Python","FastAPI","SQL","AWS","Docker","REST API"} };
  var candidate=new Candidate { FullName="Aarav Sharma",Email="aarav.demo@example.com",Phone="+91 90000 12345",ResumeText="Senior software developer with 6 years experience in C#, .NET, ASP.NET Core, Python, FastAPI, SQL Server, AWS, Docker and REST API development. Built microservices and CI/CD pipelines." };
  db.Jobs.Add(job); db.Candidates.Add(candidate); await db.SaveChangesAsync();
  db.Applications.Add(new JobApplication{JobId=job.Id,CandidateId=candidate.Id}); await db.SaveChangesAsync();
 }
}
