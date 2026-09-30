using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Grade
{
    /*
        هذا الكلاس يمثل طالب واحد داخل جدول درجات نصف العام

        في نصف العام ندخل:
        - المحصلة الأولى من 20
        - اختبار نصف العام من 30

        والمجموع يكون من 50
    */
    public class MidYearStudentGradeRowViewModel
    {
        public string StudentNumber { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        [Range(0, 20, ErrorMessage = "المحصلة الأولى يجب أن تكون من 0 إلى 20")]
        public decimal FirstAssessmentScore { get; set; }

        [Range(0, 30, ErrorMessage = "اختبار نصف العام يجب أن يكون من 0 إلى 30")]
        public decimal MidtermExamScore { get; set; }

        /*
            هذا المجموع للعرض فقط
            لا يتم إدخاله يدويًا
        */
        public decimal MidYearTotal => FirstAssessmentScore + MidtermExamScore;
    }
}