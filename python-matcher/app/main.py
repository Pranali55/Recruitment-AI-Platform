from fastapi import FastAPI
from pydantic import BaseModel, Field
from app.matcher import match_resume

app = FastAPI(title="TalentFlow Resume Matcher", description="Local, explainable skills-based resume matching.", version="1.0.0")
class MatchRequest(BaseModel):
    resume_text: str = Field(min_length=1)
    job_description: str = ""
    required_skills: list[str] = []
@app.get("/health")
def health():
    return {"status":"healthy","service":"python-matcher"}
@app.post("/match")
def match(request: MatchRequest):
    return match_resume(request.resume_text, request.job_description, request.required_skills)
