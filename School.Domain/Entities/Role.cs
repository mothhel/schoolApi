namespace school.Domain.Entities
{
    /*
        هذا الكلاس يمثل جدول Roles في قاعدة البيانات

        وظيفة هذا الجدول:
        تخزين أنواع المستخدمين داخل النظام

        أمثلة:
        Admin   = المدير
        Teacher = المدرس
        Student = الطالب

        كل مستخدم في جدول Users يجب أن يكون له Role واحد
    */
    public class Role
    {
        public int Id { get; set; }

        // اسم الدور مثل: Admin أو Teacher أو Student
        public string Name { get; set; } = string.Empty;

        /*
            علاقة One-to-Many
            الدور الواحد يمكن أن يكون له أكثر من مستخدم
            مثال:
            Role = Student
            يمكن أن يرتبط به عدد كبير من الطلاب
        */
        public ICollection<User>? Users { get; set; }
    }
}
