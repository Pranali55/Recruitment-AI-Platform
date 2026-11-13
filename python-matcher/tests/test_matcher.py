import unittest
from app.matcher import match_resume
class MatcherTests(unittest.TestCase):
    def test_full_match(self):
        r=match_resume("C# .NET Python SQL","",["C#",".NET","Python","SQL"])
        self.assertEqual(r["match_score"],100);self.assertEqual(r["missing_skills"],[])
    def test_partial_match(self):
        r=match_resume("Python and SQL developer","",["Python","SQL","AWS","Docker"])
        self.assertEqual(r["match_score"],50);self.assertEqual(r["missing_skills"],["AWS","Docker"])
    def test_no_skills(self):
        r=match_resume("Experienced developer","General role",[])
        self.assertEqual(r["match_score"],0)
    def test_token_boundary(self):
        r=match_resume("I use C# daily","",["C","C#"])
        self.assertEqual(r["matched_skills"],["C#"])
if __name__=="__main__": unittest.main()
