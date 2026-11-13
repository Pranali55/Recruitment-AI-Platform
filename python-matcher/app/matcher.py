import re
DEFAULT_SKILLS=["C#",".NET","ASP.NET Core","Python","FastAPI","Django","Flask","SQL","SQL Server","PostgreSQL","AWS","Azure","Docker","Kubernetes","REST API","React","Angular","JavaScript","Machine Learning","Generative AI","Git","Entity Framework","Microservices","Power BI","PySpark"]
def _normalise(text):
    return re.sub(r"\s+"," ",text.lower()).strip()
def _contains_skill(text,skill):
    pattern=r"(?<![a-z0-9+#])"+re.escape(skill.lower())+r"(?![a-z0-9+#])"
    return re.search(pattern,text) is not None
def match_resume(resume_text,job_description,required_skills):
    resume=_normalise(resume_text);description=_normalise(job_description)
    skills=list(dict.fromkeys(s.strip() for s in required_skills if s and s.strip()))
    if not skills: skills=[s for s in DEFAULT_SKILLS if _contains_skill(description,s)]
    matched=[s for s in skills if _contains_skill(resume,s)]
    missing=[s for s in skills if s not in matched]
    score=round(len(matched)/len(skills)*100) if skills else 0
    recommendation="Strong skills match" if score>=80 else "Potential match — review experience" if score>=50 else "Low skills match — manual review recommended"
    explanation=(f"Matched {len(matched)} of {len(skills)} listed skills. This score measures keyword overlap only; it does not assess seniority, experience quality, communication, qualifications or suitability." if skills else "No required skills were supplied or detected. Add required skills to calculate a useful score.")
    return {"match_score":score,"matched_skills":matched,"missing_skills":missing,"recommendation":recommendation,"explanation":explanation}
