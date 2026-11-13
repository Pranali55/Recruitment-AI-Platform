# TalentFlow — Recruitment Management & Resume Matching

Full-stack recruitment starter using **ASP.NET Core 8**, **Python FastAPI**, **EF Core**, **SQLite**, and a responsive JavaScript dashboard.

## Features
- Create and list job vacancies with required skills.
- Register candidates and store resume text.
- Upload `.txt` or `.md` resume files to an existing candidate.
- Create applications and track Applied, Screening, Shortlisted, Interview, Offered, Hired and Rejected stages.
- Dashboard counts for active jobs, candidates, applications and shortlisted applications.
- Python service returns a transparent skill-overlap score, matched skills and missing skills.
- SQLite database is created automatically; a demo job and candidate are seeded on first run.
- Swagger API documentation and Python unit tests.

## Architecture
```text
Browser dashboard
   | HTTP
   v
ASP.NET Core 8 API :5000 ---- EF Core ---- SQLite
   |
   +---- HTTP POST /match ----> Python FastAPI :8001
                                  |
                                  +-- Skills overlap / explanation
```

## Folder structure
```text
Recruitment-AI-Platform/
├── RecruitmentAI.sln
├── README.md
├── LICENSE
├── .gitignore
├── src/Recruitment.Api/
│   ├── Data/ (DbContext and seed data)
│   ├── Models/ (Job, Candidate, JobApplication)
│   ├── Services/ (Python matcher HTTP client)
│   ├── wwwroot/ (HTML, CSS, JavaScript dashboard)
│   ├── Program.cs
│   ├── appsettings.json
│   └── Recruitment.Api.csproj
└── python-matcher/
    ├── app/ (FastAPI app and matching logic)
    ├── tests/
    └── requirements.txt
```

## Prerequisites — Windows 10/11
- .NET 8 SDK (`dotnet --version`)
- Python 3.10+ (`py --version`)
- Git (`git --version`)
- VS Code or Visual Studio

## Run the project

### 1. Python matcher — first PowerShell window
From the repository root:
```powershell
cd python-matcher
py -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install --upgrade pip
pip install -r requirements.txt
uvicorn app.main:app --host 127.0.0.1 --port 8001 --reload
```
If activation is blocked:
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\.venv\Scripts\Activate.ps1
```
Keep this terminal running. Check `http://127.0.0.1:8001/health` and `http://127.0.0.1:8001/docs`.

### 2. .NET API and dashboard — second PowerShell window
From the repository root:
```powershell
cd src\Recruitment.Api
dotnet restore
dotnet run --urls http://localhost:5000
```
Open:
- Dashboard: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- API health: http://localhost:5000/api/health

The database `recruitment.db` is created in `src/Recruitment.Api/`. Sample data is seeded only when there are no jobs in the database.

### 3. Try the demo
1. Open the dashboard; the demo job, candidate and application should appear.
2. Select Aarav Sharma and Senior .NET + Python Developer under Resume match.
3. Click Calculate match.
4. Add your own vacancy and candidate using the forms.
5. Update an application's recruitment stage in the pipeline table.

## API reference
| Method | Route | Description |
|---|---|---|
| GET | `/api/health` | API health |
| GET | `/api/dashboard` | Dashboard metrics |
| GET / POST | `/api/jobs` | List / create jobs |
| GET / POST | `/api/candidates` | List / create candidates |
| POST | `/api/candidates/{id}/resume` | Upload `.txt` / `.md` resume |
| GET | `/api/applications` | List applications |
| POST | `/api/applications` | Create an application |
| PATCH | `/api/applications/{id}/status` | Change application status |
| POST | `/api/match/{jobId}/{candidateId}` | Run resume matching |

Example job request:
```json
{
  "title": "Python Developer",
  "department": "Engineering",
  "location": "Remote",
  "description": "Build APIs with Python, FastAPI, SQL and AWS.",
  "requiredSkills": ["Python", "FastAPI", "SQL", "AWS"]
}
```
Example candidate request:
```json
{
  "fullName": "Riya Patil",
  "email": "riya@example.com",
  "phone": "+91 9876543210",
  "resumeText": "Python developer with FastAPI, SQL and AWS experience."
}
```

## Testing

Run Python unit tests from repository root:
```powershell
cd python-matcher
.\.venv\Scripts\Activate.ps1
python -m unittest discover -s tests -v
```
For API integration checks, run both services and use Swagger at `http://localhost:5000/swagger`. Test job creation, candidate creation, application creation, status update and resume matching.

Direct Python service test:
```powershell
Invoke-RestMethod -Method Post -Uri http://127.0.0.1:8001/match -ContentType "application/json" -Body '{"resume_text":"Python, FastAPI, SQL","job_description":"API developer","required_skills":["Python","FastAPI","AWS"]}'
```

## GitHub upload
Create a **Public** repository named `Recruitment-AI-Platform`. Suggested description:

> Full-stack recruitment management platform built with ASP.NET Core 8, Python FastAPI and SQLite, featuring candidate tracking, job management and explainable resume-to-job skill matching.

Do not initialize the remote repository with a second README or `.gitignore`. In PowerShell at the project root:
```powershell
git init
git add .
git commit -m "Initial commit - Recruitment AI Platform"
git branch -M main
git remote add origin https://github.com/YOUR_GITHUB_USERNAME/Recruitment-AI-Platform.git
git push -u origin main
```
Replace `YOUR_GITHUB_USERNAME` with your GitHub username. If `origin` already exists:
```powershell
git remote set-url origin https://github.com/YOUR_GITHUB_USERNAME/Recruitment-AI-Platform.git
git push -u origin main
```
Suggested GitHub topics: `dotnet`, `aspnet-core`, `python`, `fastapi`, `recruitment`, `applicant-tracking-system`, `resume-matching`, `sqlite`, `full-stack`.

## Resume / portfolio description
**TalentFlow — Recruitment Management & Resume Matching**
- Built a full-stack recruitment application using ASP.NET Core 8 Minimal APIs, EF Core and SQLite to manage vacancies, candidate profiles and application pipelines.
- Integrated a Python FastAPI service to calculate explainable resume-to-job skill overlap and identify matched and missing skills.
- Developed a responsive JavaScript dashboard, REST endpoints, seed data and automated Python unit tests.

## Responsible-use notes and current limitations
- The matching service is a deterministic keyword-overlap baseline, **not** a trained ML model or LLM. It does not understand synonyms, skill depth, seniority, context or quality of experience.
- Never use a numeric score to automatically accept or reject a person. Use consistent criteria and human review for every hiring decision.
- Starter resume upload supports `.txt` and `.md`; PDF/DOCX parsing, authentication, authorization, audit logs, secure file storage and production deployment are future enhancements.
- This is a portfolio/demo project, not a production HR system. Before using real candidate data, add access controls, encryption, consent and retention/deletion workflows, audit logging and a security review.
