using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Grade
{
    /*
        هذا الكلاس يمثل طالب واحد داخل جدول درجات نهاية العام

        في نهاية العام ندخل:
        - المحصلة الثانية من 20
        - اختبار نهاية العام من 30

        والمجموع يكون من 50
    */
    public class FinalYearStudentGradeRowViewModel
    {
        public string StudentNumber { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        [Range(0, 20, ErrorMessage = "المحصلة الثانية يجب أن تكون من 0 إلى 20")]
        public decimal SecondAssessmentScore { get; set; }

        [Range(0, 30, ErrorMessage = "اختبار نهاية العام يجب أن يكون من 0 إلى 30")]
        public decimal FinalExamScore { get; set; }

        /*
            هذا المجموع للعرض فقط
            لا يتم إدخاله يدويًا
        */
        public decimal FinalYearTotal => SecondAssessmentScore + FinalExamScore;
    }
}