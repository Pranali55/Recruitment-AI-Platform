using Microsoft.EntityFrameworkCore;
using Recruitment.Api.Models;
namespace Recruitment.Api.Data;
public class RecruitmentDbContext(DbContextOptions<RecruitmentDbContext> options) : DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<JobApplication> Applications => Set<JobApplication>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Candidate>().HasIndex(x=>x.Email).IsUnique();
        b.Entity<JobApplication>().HasIndex(x=>new{x.JobId,x.CandidateId}).IsUnique();
        b.Entity<JobApplication>().HasOne(x=>x.Job).WithMany(x=>x.Applications).HasForeignKey(x=>x.JobId);
        b.Entity<JobApplication>().HasOne(x=>x.Candidate).WithMany(x=>x.Applications).HasForeignKey(x=>x.CandidateId);
        b.Entity<Job>().Property(x=>x.RequiredSkills).HasConversion(v=>string.Join("|",v),v=>v.Split("|",StringSplitOptions.RemoveEmptyEntries).ToList());
    }
}
