using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using schoolApi.Data;
using schoolApi.DTOs;
using schoolApi.Models;

namespace schoolApi.Controllers
{
    /*
        هذا الكنترولر خاص بعمليات المدير داخل الـ API

        المدير يستطيع من خلال هذا الكنترولر:
        - إضافة مدير جديد
        - إضافة مدرس جديد
        - إضافة طالب جديد
        - إضافة صف جديد
        - إضافة شعبة جديدة

        كما يستطيع من خلاله عرض البيانات:
        - عرض الصفوف
        - عرض الشعب
        - عرض الطلاب
        - عرض المدرسين

        كما يستطيع التعديل والحذف:
        - تعديل وحذف الصف
        - تعديل وحذف الشعبة
        - تعديل وحذف المدرس
        - تعديل وحذف الطالب
    */

    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public AdminController(SchoolDbContext context)
        {
            _context = context;
        }


        /*
    =====================================================
    توليد رقم طالب حسب السنة الدراسية
    =====================================================

    مثال:
    سنة 2026:
    2026001
    2026002

    سنة 2027:
    2027001
    2027002

    الفكرة:
    - نأخذ السنة الحالية
    - نبحث عن أرقام الطلاب التي تبدأ بنفس السنة
    - نأخذ أكبر رقم
    - نزيده واحد
*/
        private async Task<string> GenerateStudentNumber()
        {
            int currentYear = DateTime.Now.Year;

            string yearPrefix = currentYear.ToString();

            int startNumber = int.Parse(yearPrefix + "000");

            var studentNumbers = await _context.Students
                .Where(s =>
                    !string.IsNullOrWhiteSpace(s.StudentNumber) &&
                    s.StudentNumber.StartsWith(yearPrefix))
                .Select(s => s.StudentNumber)
                .ToListAsync();

            int maxNumber = startNumber;

            foreach (var studentNumber in studentNumbers)
            {
                var cleanNumber = studentNumber.Trim();

                if (int.TryParse(cleanNumber, out int number))
                {
                    if (number > maxNumber)
                    {
                        maxNumber = number;
                    }
                }
            }

            return (maxNumber + 1).ToString();
        }



        /*
    =====================================================
    دالة توليد رقم مدرس تلقائي بدون تكرار
    =====================================================

    هذه الدالة:
    - تجلب كل أرقام المدرسين
    - تبحث عن أكبر رقم
    - ثم تولد الرقم التالي

    مثال:
    إذا الموجود:
    2026101
    2026102

    الجديد سيكون:
    2026103
*/
        private async Task<string> GenerateTeacherNumber()
        {
            int startNumber = 2026100;

            var teacherNumbers = await _context.Teachers
                .Where(t => !string.IsNullOrWhiteSpace(t.TeacherNumber))
                .Select(t => t.TeacherNumber)
                .ToListAsync();

            int maxNumber = startNumber;

            foreach (var teacherNumber in teacherNumbers)
            {
                if (int.TryParse(teacherNumber.Trim(), out int number))
                {
                    if (number > maxNumber)
                    {
                        maxNumber = number;
                    }
                }
            }

            return (maxNumber + 1).ToString();
        }



        /*
    =====================================================
    API تعديل اسم المستخدم
    =====================================================

    يستخدمه مدير النظام الرئيسي من schoolWeb
    لتعديل Username لأي مستخدم.
*/
        [HttpPut("update-username/{userId}")]
        public async Task<IActionResult> UpdateUsername(int userId, UpdateUsernameDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest("اسم المستخدم مطلوب.");
            }

            var newUsername = dto.Username.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            var usernameExists = await _context.Users
                .AnyAsync(u =>
                    u.Id != userId &&
                    u.Username.Trim().ToLower() == newUsername.ToLower());

            if (usernameExists)
            {
                return BadRequest("اسم المستخدم مستخدم بالفعل.");
            }

            user.Username = newUsername;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل اسم المستخدم بنجاح."
            });
        }



        /*
            =====================================================
            1) API لإضافة مدير جديد
            =====================================================
        */
        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin(CreateAdminDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (existingUser != null)
            {
                return BadRequest("اسم المستخدم موجود بالفعل.");
            }

            var adminUser = new User
            {
                FullName = dto.FullName,
                Username = dto.Username,
                Password = dto.Password,
                RoleId = 1,
                IsActive = true
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة المدير بنجاح"
            });
        }






        /*
    =====================================================
    2) API لإضافة مدرس جديد
    =====================================================

    ملاحظة:
    رقم المدرس لا يتم إدخاله يدويًا
    بل يتم توليده تلقائيًا من النظام
*/
        [HttpPost("create-teacher")]
        public async Task<IActionResult> CreateTeacher(CreateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (existingUser != null)
            {
                return BadRequest("اسم المستخدم موجود بالفعل.");
            }

            // توليد رقم المدرس تلقائيًا
            var generatedTeacherNumber = await GenerateTeacherNumber();

            var user = new User
            {
                FullName = dto.FullName,
                Username = dto.Username,
                Password = dto.Password,
                RoleId = 2,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var teacher = new Teacher
            {
                UserId = user.Id,

                // رقم المدرس يتم توليده تلقائيًا هنا
                TeacherNumber = generatedTeacherNumber,

                Specialty = dto.Specialty,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                HireDate = dto.HireDate
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة المدرس بنجاح",
                teacherNumber = teacher.TeacherNumber
            });
        }

        /*
    =====================================================
    3) API لإضافة طالب جديد
    =====================================================

    هنا الإدخال يتم باسم الصف والشعبة

    ملاحظة مهمة:
    رقم الطالب لا يتم إدخاله من المستخدم
    بل يتم توليده تلقائيًا من النظام
*/
        [HttpPost("create-student")]
        public async Task<IActionResult> CreateStudent(CreateStudentDto dto)
        {
            // التحقق من صحة البيانات القادمة من المستخدم
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // التأكد أن اسم المستخدم غير موجود مسبقًا
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (existingUser != null)
            {
                return BadRequest("اسم المستخدم موجود بالفعل.");
            }

            // البحث عن الصف بالاسم
            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c =>
                    c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود. تأكد من كتابة اسم الصف بشكل صحيح.");
            }

            // البحث عن الشعبة داخل الصف المحدد
            var section = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() &&
                    s.ClassRoomId == classRoom.Id);

            if (section == null)
            {
                return BadRequest("الشعبة غير موجودة داخل الصف المحدد.");
            }

            // توليد رقم الطالب تلقائيًا
            var generatedStudentNumber = await GenerateStudentNumber();

            // إنشاء حساب المستخدم الخاص بالطالب
            var user = new User
            {
                FullName = dto.FullName,
                Username = dto.Username,
                Password = dto.Password,
                RoleId = 3,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // إنشاء بيانات الطالب وربطها بحساب المستخدم
            var student = new Student
            {
                UserId = user.Id,

                // هنا يتم حفظ رقم الطالب التلقائي
                StudentNumber = generatedStudentNumber,

                Gender = dto.Gender,
                BirthDate = dto.BirthDate,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                ParentName = dto.ParentName,
                ParentPhone = dto.ParentPhone,
                ClassRoomId = classRoom.Id,
                SectionId = section.Id
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الطالب بنجاح",
                studentNumber = student.StudentNumber,
                classRoom = classRoom.Name,
                section = section.Name
            });
        }


        /*
    =====================================================
    API لعرض المستخدمين مع إمكانية الفلترة حسب نوع الحساب
    =====================================================

    فكرة العمل:
    - إذا لم يتم إرسال roleName أو كانت All → يعرض جميع المستخدمين
    - إذا كانت Admin → يعرض المدراء فقط
    - إذا كانت Teacher → يعرض المدرسين فقط
    - إذا كانت Student → يعرض الطلاب فقط

    هذا يفيدنا في الواجهة لعمل Dropdown احترافي
    واختيار نوع المستخدم قبل العرض.
*/
        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers(string? roleName = "All")
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .AsQueryable();

            /*
                إذا اختار المدير نوع حساب معين
                نقوم بفلترة النتائج حسب اسم الدور
            */
            if (!string.IsNullOrWhiteSpace(roleName) && roleName != "All")
            {
                query = query.Where(u => u.Role != null && u.Role.Name == roleName);
            }

            var users = await query
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : "",
                    u.IsActive,

                    /*
                        إذا كان المستخدم طالبًا
                        نعرض رقم الطالب
                    */
                    StudentNumber = u.Student != null ? u.Student.StudentNumber : null,

                    /*
                        إذا كان المستخدم مدرسًا
                        نعرض رقم المدرس
                    */
                    TeacherNumber = u.Teacher != null ? u.Teacher.TeacherNumber : null
                })
                .OrderBy(u => u.RoleName)
                .ThenBy(u => u.FullName)
                .ToListAsync();

            return Ok(users);
        }



        /*
    =====================================================
    API لتفعيل أو تعطيل حساب مستخدم
    =====================================================

    هذا الـ endpoint يسمح للمدير بتغيير حالة الحساب:
    - تفعيل
    - تعطيل

    ملاحظة:
    لا نسمح بتعطيل آخر مدير مفعل في النظام
*/
        [HttpPut("update-user-status/{id}")]
        public async Task<IActionResult> UpdateUserStatus(int id, UpdateUserStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            /*
                حماية مهمة:
                إذا كان المستخدم مديرًا، لا نسمح بتعطيل آخر مدير مفعل
            */
            if (user.RoleId == 1 && dto.IsActive == false)
            {
                var activeAdminsCount = await _context.Users
                    .CountAsync(u => u.RoleId == 1 && u.IsActive && u.Id != id);

                if (activeAdminsCount == 0)
                {
                    return BadRequest("لا يمكن تعطيل آخر مدير مفعل في النظام.");
                }
            }

            user.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = dto.IsActive ? "تم تفعيل الحساب بنجاح." : "تم تعطيل الحساب بنجاح."
            });
        }



        /*
            =====================================================
            4) API لإضافة صف جديد
            =====================================================
        */
        [HttpPost("create-classroom")]
        public async Task<IActionResult> CreateClassRoom(CreateClassRoomDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingClassRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.Name.Trim().ToLower());

            if (existingClassRoom != null)
            {
                return BadRequest("الصف موجود بالفعل.");
            }

            var classRoom = new ClassRoom
            {
                Name = dto.Name.Trim()
            };

            _context.ClassRooms.Add(classRoom);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الصف بنجاح",
                classRoomId = classRoom.Id,
                classRoomName = classRoom.Name
            });
        }

        /*
            =====================================================
            5) API لإضافة شعبة جديدة
            =====================================================
        */
        [HttpPost("create-section")]
        public async Task<IActionResult> CreateSection(CreateSectionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود. أضف الصف أولاً قبل إضافة الشعبة.");
            }

            var existingSection = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name.Trim().ToLower() == dto.Name.Trim().ToLower() &&
                    s.ClassRoomId == classRoom.Id);

            if (existingSection != null)
            {
                return BadRequest("هذه الشعبة موجودة بالفعل داخل هذا الصف.");
            }

            var section = new Section
            {
                Name = dto.Name.Trim(),
                ClassRoomId = classRoom.Id
            };

            _context.Sections.Add(section);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الشعبة بنجاح",
                sectionId = section.Id,
                sectionName = section.Name,
                classRoomName = classRoom.Name
            });
        }

        /*
            =====================================================
            6) API لعرض جميع الصفوف
            =====================================================
        */
        [HttpGet("get-classrooms")]
        public async Task<IActionResult> GetClassRooms()
        {
            var classRooms = await _context.ClassRooms
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(classRooms);
        }

        /*
            =====================================================
            7) API لعرض جميع الشعب
            =====================================================
        */
        [HttpGet("get-sections")]
        public async Task<IActionResult> GetSections()
        {
            var sections = await _context.Sections
                .Include(s => s.ClassRoom)
                .Select(s => new
                {
                    s.Id,
                    SectionName = s.Name,
                    ClassRoomId = s.ClassRoomId,
                    ClassRoomName = s.ClassRoom != null ? s.ClassRoom.Name : null
                })
                .OrderBy(s => s.ClassRoomName)
                .ThenBy(s => s.SectionName)
                .ToListAsync();

            return Ok(sections);
        }

        /*
            =====================================================
            8) API لعرض الشعب حسب اسم الصف
            =====================================================
        */
        [HttpGet("get-sections-by-classroom/{classRoomName}")]
        public async Task<IActionResult> GetSectionsByClassRoom(string classRoomName)
        {
            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == classRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود.");
            }

            var sections = await _context.Sections
                .Where(s => s.ClassRoomId == classRoom.Id)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Ok(new
            {
                ClassRoomId = classRoom.Id,
                ClassRoomName = classRoom.Name,
                Sections = sections
            });
        }

        /*
            =====================================================
            9) API لعرض جميع الطلاب
            =====================================================
        */
        [HttpGet("get-students")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.ClassRoom)
                .Include(s => s.Section)
                .Select(s => new
                {
                    s.Id,
                    s.StudentNumber,
                    FullName = s.User != null ? s.User.FullName : null,
                    Username = s.User != null ? s.User.Username : null,
                    s.Gender,
                    s.BirthDate,
                    s.Address,
                    s.PhoneNumber,
                    s.ParentName,
                    s.ParentPhone,
                    ClassRoomName = s.ClassRoom != null ? s.ClassRoom.Name : null,
                    SectionName = s.Section != null ? s.Section.Name : null
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return Ok(students);
        }

        /*
            =====================================================
            10) API لعرض طالب واحد بواسطة Id
            =====================================================
        */
        [HttpGet("get-student/{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.ClassRoom)
                .Include(s => s.Section)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.StudentNumber,
                    FullName = s.User != null ? s.User.FullName : null,
                    Username = s.User != null ? s.User.Username : null,
                    s.Gender,
                    s.BirthDate,
                    s.Address,
                    s.PhoneNumber,
                    s.ParentName,
                    s.ParentPhone,
                    ClassRoomId = s.ClassRoomId,
                    ClassRoomName = s.ClassRoom != null ? s.ClassRoom.Name : null,
                    SectionId = s.SectionId,
                    SectionName = s.Section != null ? s.Section.Name : null
                })
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            return Ok(student);
        }

        /*
            =====================================================
            11) API لعرض جميع المدرسين
            =====================================================
        */
        [HttpGet("get-teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Select(t => new
                {
                    t.Id,
                    t.TeacherNumber,
                    FullName = t.User != null ? t.User.FullName : null,
                    Username = t.User != null ? t.User.Username : null,
                    t.Specialty,
                    t.PhoneNumber,
                    t.Address,
                    t.HireDate
                })
                .OrderBy(t => t.FullName)
                .ToListAsync();

            return Ok(teachers);
        }

        /*
            =====================================================
            12) API لعرض مدرس واحد بواسطة Id
            =====================================================
        */
        [HttpGet("get-teacher/{id}")]
        public async Task<IActionResult> GetTeacherById(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.TeacherNumber,
                    FullName = t.User != null ? t.User.FullName : null,
                    Username = t.User != null ? t.User.Username : null,
                    t.Specialty,
                    t.PhoneNumber,
                    t.Address,
                    t.HireDate
                })
                .FirstOrDefaultAsync();

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            return Ok(teacher);
        }

        /*
            =====================================================
            13) API لتعديل صف
            =====================================================
        */
        [HttpPut("update-classroom/{id}")]
        public async Task<IActionResult> UpdateClassRoom(int id, UpdateClassRoomDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var classRoom = await _context.ClassRooms.FindAsync(id);

            if (classRoom == null)
            {
                return NotFound("الصف غير موجود.");
            }

            var existingClassRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c =>
                    c.Id != id &&
                    c.Name.Trim().ToLower() == dto.Name.Trim().ToLower());

            if (existingClassRoom != null)
            {
                return BadRequest("يوجد صف آخر بنفس الاسم.");
            }

            classRoom.Name = dto.Name.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الصف بنجاح",
                classRoomId = classRoom.Id,
                classRoomName = classRoom.Name
            });
        }

        /*
            =====================================================
            14) API لحذف صف
            =====================================================

            لا نحذف الصف إذا كان مرتبطًا بشعب أو طلاب
        */
        [HttpDelete("delete-classroom/{id}")]
        public async Task<IActionResult> DeleteClassRoom(int id)
        {
            var classRoom = await _context.ClassRooms
                .Include(c => c.Sections)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classRoom == null)
            {
                return NotFound("الصف غير موجود.");
            }

            if (classRoom.Sections != null && classRoom.Sections.Any())
            {
                return BadRequest("لا يمكن حذف الصف لأنه يحتوي على شعب.");
            }

            if (classRoom.Students != null && classRoom.Students.Any())
            {
                return BadRequest("لا يمكن حذف الصف لأنه مرتبط بطلاب.");
            }

            _context.ClassRooms.Remove(classRoom);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الصف بنجاح"
            });
        }

        /*
            =====================================================
            15) API لتعديل شعبة
            =====================================================
        */
        [HttpPut("update-section/{id}")]
        public async Task<IActionResult> UpdateSection(int id, UpdateSectionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var section = await _context.Sections.FindAsync(id);

            if (section == null)
            {
                return NotFound("الشعبة غير موجودة.");
            }

            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود.");
            }

            var existingSection = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Id != id &&
                    s.Name.Trim().ToLower() == dto.Name.Trim().ToLower() &&
                    s.ClassRoomId == classRoom.Id);

            if (existingSection != null)
            {
                return BadRequest("توجد شعبة أخرى بنفس الاسم داخل هذا الصف.");
            }

            section.Name = dto.Name.Trim();
            section.ClassRoomId = classRoom.Id;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الشعبة بنجاح",
                sectionId = section.Id,
                sectionName = section.Name,
                classRoomName = classRoom.Name
            });
        }

        /*
            =====================================================
            16) API لحذف شعبة
            =====================================================

            لا نحذف الشعبة إذا كانت مرتبطة بطلاب
        */
        [HttpDelete("delete-section/{id}")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _context.Sections
                .Include(s => s.Students)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (section == null)
            {
                return NotFound("الشعبة غير موجودة.");
            }

            if (section.Students != null && section.Students.Any())
            {
                return BadRequest("لا يمكن حذف الشعبة لأنها مرتبطة بطلاب.");
            }

            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الشعبة بنجاح"
            });
        }

        /*
            =====================================================
            17) API لتعديل مدرس
            =====================================================
        */
        [HttpPut("update-teacher/{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, UpdateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var existingUsername = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id != teacher.UserId &&
                    u.Username.Trim().ToLower() == dto.Username.Trim().ToLower());

            if (existingUsername != null)
            {
                return BadRequest("اسم المستخدم مستخدم من قبل.");
            }

            var existingTeacherNumber = await _context.Teachers
                .FirstOrDefaultAsync(t =>
                    t.Id != id &&
                    t.TeacherNumber.Trim().ToLower() == dto.TeacherNumber.Trim().ToLower());

            if (existingTeacherNumber != null)
            {
                return BadRequest("رقم المدرس مستخدم من قبل.");
            }

            teacher.TeacherNumber = dto.TeacherNumber.Trim();
            teacher.Specialty = dto.Specialty;
            teacher.PhoneNumber = dto.PhoneNumber;
            teacher.Address = dto.Address;
            teacher.HireDate = dto.HireDate;

            if (teacher.User != null)
            {
                teacher.User.FullName = dto.FullName;
                teacher.User.Username = dto.Username.Trim();
                teacher.User.IsActive = dto.IsActive;

                /*
                    إذا أدخل المدير كلمة مرور جديدة
                    نغير كلمة مرور المدرس
                    وإذا تركها فارغة تبقى القديمة كما هي
                */
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    teacher.User.Password = dto.Password.Trim();
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل المدرس بنجاح"
            });
        }

        /*
            =====================================================
            18) API لحذف مدرس
            =====================================================

            نحذف سجل المدرس ثم نحذف حساب المستخدم المرتبط به
        */
        [HttpDelete("delete-teacher/{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var user = teacher.User;

            _context.Teachers.Remove(teacher);

            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف المدرس بنجاح"
            });
        }

        /*
            =====================================================
            19) API لتعديل طالب
            =====================================================
        */
        [HttpPut("update-student/{id}")]
        public async Task<IActionResult> UpdateStudent(int id, UpdateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var existingUsername = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id != student.UserId &&
                    u.Username.Trim().ToLower() == dto.Username.Trim().ToLower());

            if (existingUsername != null)
            {
                return BadRequest("اسم المستخدم مستخدم من قبل.");
            }

            var existingStudentNumber = await _context.Students
                .FirstOrDefaultAsync(s =>
                    s.Id != id &&
                    s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (existingStudentNumber != null)
            {
                return BadRequest("رقم الطالب مستخدم من قبل.");
            }

            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود.");
            }

            var section = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() &&
                    s.ClassRoomId == classRoom.Id);

            if (section == null)
            {
                return BadRequest("الشعبة غير موجودة داخل الصف المحدد.");
            }

            student.StudentNumber = dto.StudentNumber.Trim();
            student.Gender = dto.Gender;
            student.BirthDate = dto.BirthDate;
            student.Address = dto.Address;
            student.PhoneNumber = dto.PhoneNumber;
            student.ParentName = dto.ParentName;
            student.ParentPhone = dto.ParentPhone;
            student.ClassRoomId = classRoom.Id;
            student.SectionId = section.Id;

            if (student.User != null)
            {
                student.User.FullName = dto.FullName;
                student.User.Username = dto.Username.Trim();
                student.User.IsActive = dto.IsActive;

                /*
        إذا أدخل المدير كلمة مرور جديدة
        يتم تغيير كلمة مرور الطالب
        أما إذا تركها فارغة تبقى كلمة المرور القديمة كما هي
    */
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    student.User.Password = dto.Password.Trim();
                }

            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الطالب بنجاح"
            });
        }

        /*
    =====================================================
    20) API لحذف طالب
    =====================================================

    هذه الميثود تقوم بحذف:
    - درجات الطالب
    - حضور وغياب الطالب
    - ملاحظات الطالب
    - المهام اليومية الخاصة به
    - سجل الطالب
    - حساب المستخدم المرتبط به

    وذلك حتى لا يحدث خطأ بسبب العلاقات بين الجداول
*/
        [HttpDelete("delete-student/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            // حذف درجات الطالب
            var grades = await _context.Grades
                .Where(g => g.StudentId == student.Id)
                .ToListAsync();

            _context.Grades.RemoveRange(grades);

            // حذف حضور وغياب الطالب
            var attendances = await _context.Attendances
                .Where(a => a.StudentId == student.Id)
                .ToListAsync();

            _context.Attendances.RemoveRange(attendances);

            // حذف ملاحظات الطالب
            var notes = await _context.StudentNotes
                .Where(n => n.StudentId == student.Id)
                .ToListAsync();

            _context.StudentNotes.RemoveRange(notes);

            // حذف المهام اليومية الخاصة بالطالب
            var dailyTasks = await _context.DailyTasks
                .Where(t => t.StudentId == student.Id)
                .ToListAsync();

            _context.DailyTasks.RemoveRange(dailyTasks);

            var user = student.User;

            // حذف سجل الطالب
            _context.Students.Remove(student);

            // حذف حساب المستخدم المرتبط بالطالب
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الطالب وجميع البيانات المرتبطة به بنجاح"
            });
        }

        /*
    =====================================================
    21) API لإضافة مادة جديدة
    =====================================================

    هذا الـ endpoint يسمح للمدير بإضافة مادة جديدة
    مثل:
    - رياضيات
    - علوم
    - عربي
*/
        [HttpPost("create-subject")]
        public async Task<IActionResult> CreateSubject(CreateSubjectDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // نتأكد أن المادة غير موجودة مسبقاً
            var existingSubject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.Name.Trim().ToLower());

            if (existingSubject != null)
            {
                return BadRequest("المادة موجودة بالفعل.");
            }

            var subject = new Subject
            {
                Name = dto.Name.Trim()
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة المادة بنجاح",
                subjectId = subject.Id,
                subjectName = subject.Name
            });
        }

        /*
    =====================================================
    22) API لعرض جميع المواد
    =====================================================
*/
        [HttpGet("get-subjects")]
        public async Task<IActionResult> GetSubjects()
        {
            var subjects = await _context.Subjects
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Ok(subjects);
        }

        /*
    =====================================================
    23) API لعرض مادة واحدة بواسطة Id
    =====================================================
*/
        [HttpGet("get-subject/{id}")]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            var subject = await _context.Subjects
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .FirstOrDefaultAsync();

            if (subject == null)
            {
                return NotFound("المادة غير موجودة.");
            }

            return Ok(subject);
        }

        /*
    =====================================================
    24) API لتعديل مادة
    =====================================================
*/
        [HttpPut("update-subject/{id}")]
        public async Task<IActionResult> UpdateSubject(int id, UpdateSubjectDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                return NotFound("المادة غير موجودة.");
            }

            // نتأكد أنه لا توجد مادة أخرى بنفس الاسم
            var existingSubject = await _context.Subjects
                .FirstOrDefaultAsync(s =>
                    s.Id != id &&
                    s.Name.Trim().ToLower() == dto.Name.Trim().ToLower());

            if (existingSubject != null)
            {
                return BadRequest("توجد مادة أخرى بنفس الاسم.");
            }

            subject.Name = dto.Name.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل المادة بنجاح",
                subjectId = subject.Id,
                subjectName = subject.Name
            });
        }

        /*
    =====================================================
    25) API لحذف مادة
    =====================================================

    لا نحذف المادة إذا كانت مرتبطة بتكليفات تدريس
*/
        [HttpDelete("delete-subject/{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _context.Subjects
                .Include(s => s.TeacherAssignments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
            {
                return NotFound("المادة غير موجودة.");
            }

            if (subject.TeacherAssignments != null && subject.TeacherAssignments.Any())
            {
                return BadRequest("لا يمكن حذف المادة لأنها مرتبطة بتكليفات تدريس.");
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف المادة بنجاح"
            });
        }

        /*
    =====================================================
    26) API لإضافة تكليف تدريسي جديد
    =====================================================

    هذا الـ endpoint يربط:
    - مدرس
    - مادة
    - صف
    - شعبة
*/
        [HttpPost("create-teacher-assignment")]
        public async Task<IActionResult> CreateTeacherAssignment(CreateTeacherAssignmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن المدرس برقم المدرس
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherNumber.Trim().ToLower() == dto.TeacherNumber.Trim().ToLower());

            if (teacher == null)
            {
                return BadRequest("المدرس غير موجود.");
            }

            // البحث عن المادة بالاسم
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SubjectName.Trim().ToLower());

            if (subject == null)
            {
                return BadRequest("المادة غير موجودة.");
            }

            // البحث عن الصف بالاسم
            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود.");
            }

            // البحث عن الشعبة داخل نفس الصف
            var section = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() &&
                    s.ClassRoomId == classRoom.Id);

            if (section == null)
            {
                return BadRequest("الشعبة غير موجودة داخل الصف المحدد.");
            }

            // التأكد من أن نفس التكليف غير موجود مسبقاً
            var existingAssignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.TeacherId == teacher.Id &&
                    a.SubjectId == subject.Id &&
                    a.ClassRoomId == classRoom.Id &&
                    a.SectionId == section.Id);

            if (existingAssignment != null)
            {
                return BadRequest("هذا التكليف التدريسي موجود بالفعل.");
            }

            var assignment = new TeacherAssignment
            {
                TeacherId = teacher.Id,
                SubjectId = subject.Id,
                ClassRoomId = classRoom.Id,
                SectionId = section.Id
            };

            _context.TeacherAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة التكليف التدريسي بنجاح",
                teacherAssignmentId = assignment.Id,
                teacherName = teacher.User != null ? teacher.User.FullName : null,
                teacherNumber = teacher.TeacherNumber,
                subjectName = subject.Name,
                classRoomName = classRoom.Name,
                sectionName = section.Name
            });
        }

        /*
    =====================================================
    27) API لعرض جميع التكليفات التدريسية
    =====================================================
*/
        [HttpGet("get-teacher-assignments")]
        public async Task<IActionResult> GetTeacherAssignments()
        {
            var assignments = await _context.TeacherAssignments
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .Select(a => new
                {
                    a.Id,
                    TeacherId = a.TeacherId,
                    TeacherName = a.Teacher != null && a.Teacher.User != null ? a.Teacher.User.FullName : null,
                    TeacherNumber = a.Teacher != null ? a.Teacher.TeacherNumber : null,
                    SubjectId = a.SubjectId,
                    SubjectName = a.Subject != null ? a.Subject.Name : null,
                    ClassRoomId = a.ClassRoomId,
                    ClassRoomName = a.ClassRoom != null ? a.ClassRoom.Name : null,
                    SectionId = a.SectionId,
                    SectionName = a.Section != null ? a.Section.Name : null
                })
                .OrderBy(a => a.TeacherName)
                .ThenBy(a => a.SubjectName)
                .ToListAsync();

            return Ok(assignments);
        }

        /*
    =====================================================
    28) API لعرض تكليف تدريسي واحد بواسطة Id
    =====================================================
*/
        [HttpGet("get-teacher-assignment/{id}")]
        public async Task<IActionResult> GetTeacherAssignmentById(int id)
        {
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    TeacherId = a.TeacherId,
                    TeacherName = a.Teacher != null && a.Teacher.User != null ? a.Teacher.User.FullName : null,
                    TeacherNumber = a.Teacher != null ? a.Teacher.TeacherNumber : null,
                    SubjectId = a.SubjectId,
                    SubjectName = a.Subject != null ? a.Subject.Name : null,
                    ClassRoomId = a.ClassRoomId,
                    ClassRoomName = a.ClassRoom != null ? a.ClassRoom.Name : null,
                    SectionId = a.SectionId,
                    SectionName = a.Section != null ? a.Section.Name : null
                })
                .FirstOrDefaultAsync();

            if (assignment == null)
            {
                return NotFound("التكليف التدريسي غير موجود.");
            }

            return Ok(assignment);
        }

        /*
    =====================================================
    29) API لعرض جميع تكليفات مدرس معين
    =====================================================
*/
        [HttpGet("get-teacher-assignments-by-teacher/{teacherId}")]
        public async Task<IActionResult> GetTeacherAssignmentsByTeacher(int teacherId)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var assignments = await _context.TeacherAssignments
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .Where(a => a.TeacherId == teacherId)
                .Select(a => new
                {
                    a.Id,
                    SubjectId = a.SubjectId,
                    SubjectName = a.Subject != null ? a.Subject.Name : null,
                    ClassRoomId = a.ClassRoomId,
                    ClassRoomName = a.ClassRoom != null ? a.ClassRoom.Name : null,
                    SectionId = a.SectionId,
                    SectionName = a.Section != null ? a.Section.Name : null
                })
                .OrderBy(a => a.SubjectName)
                .ToListAsync();

            return Ok(new
            {
                TeacherId = teacher.Id,
                TeacherName = teacher.User != null ? teacher.User.FullName : null,
                TeacherNumber = teacher.TeacherNumber,
                Assignments = assignments
            });
        }

        /*
    =====================================================
    30) API لتعديل تكليف تدريسي
    =====================================================
*/
        [HttpPut("update-teacher-assignment/{id}")]
        public async Task<IActionResult> UpdateTeacherAssignment(int id, UpdateTeacherAssignmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignment = await _context.TeacherAssignments.FindAsync(id);

            if (assignment == null)
            {
                return NotFound("التكليف التدريسي غير موجود.");
            }

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherNumber.Trim().ToLower() == dto.TeacherNumber.Trim().ToLower());

            if (teacher == null)
            {
                return BadRequest("المدرس غير موجود.");
            }

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SubjectName.Trim().ToLower());

            if (subject == null)
            {
                return BadRequest("المادة غير موجودة.");
            }

            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود.");
            }

            var section = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() &&
                    s.ClassRoomId == classRoom.Id);

            if (section == null)
            {
                return BadRequest("الشعبة غير موجودة داخل الصف المحدد.");
            }

            var existingAssignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.Id != id &&
                    a.TeacherId == teacher.Id &&
                    a.SubjectId == subject.Id &&
                    a.ClassRoomId == classRoom.Id &&
                    a.SectionId == section.Id);

            if (existingAssignment != null)
            {
                return BadRequest("يوجد تكليف تدريسي آخر بنفس البيانات.");
            }

            assignment.TeacherId = teacher.Id;
            assignment.SubjectId = subject.Id;
            assignment.ClassRoomId = classRoom.Id;
            assignment.SectionId = section.Id;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل التكليف التدريسي بنجاح"
            });
        }

        /*
    =====================================================
    31) API لحذف تكليف تدريسي
    =====================================================
*/
        [HttpDelete("delete-teacher-assignment/{id}")]
        public async Task<IActionResult> DeleteTeacherAssignment(int id)
        {
            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound("التكليف التدريسي غير موجود.");
            }

            _context.TeacherAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف التكليف التدريسي بنجاح"
            });
        }

        /*
    =====================================================
    32) API لإضافة درس جديد
    =====================================================

    هذا الـ endpoint يقوم بإضافة درس مرتبط بتكليف تدريسي
*/
        [HttpPost("create-lesson")]
        public async Task<IActionResult> CreateLesson(CreateLessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // التأكد من وجود التكليف التدريسي
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId);

            if (assignment == null)
            {
                return BadRequest("التكليف التدريسي غير موجود.");
            }

            var lesson = new Lesson
            {
                TeacherAssignmentId = dto.TeacherAssignmentId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                LessonDate = dto.LessonDate
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الدرس بنجاح",
                lessonId = lesson.Id,
                lessonTitle = lesson.Title,
                teacherName = assignment.Teacher != null && assignment.Teacher.User != null ? assignment.Teacher.User.FullName : null,
                subjectName = assignment.Subject != null ? assignment.Subject.Name : null,
                classRoomName = assignment.ClassRoom != null ? assignment.ClassRoom.Name : null,
                sectionName = assignment.Section != null ? assignment.Section.Name : null
            });
        }

        /*
      =====================================================
      API جلب كل الدروس
      =====================================================

      هذا API يستخدم في تطبيق الطالب لعرض الدروس.

      أضفنا:
      - وقت الإضافة CreatedAt
      - يوم الإضافة CreatedDayName
      - وقت الإضافة CreatedTime
      - وقت آخر تعديل UpdatedAt
      - يوم آخر تعديل UpdatedDayName
      - وقت آخر تعديل UpdatedTime

      مهم:
      لا نستدعي GetArabicDayName داخل Select قبل ToListAsync
      حتى لا يحدث خطأ مع Entity Framework.
  */
        [HttpGet("get-lessons")]
        public async Task<IActionResult> GetLessons()
        {
            var lessonsFromDb = await _context.Lessons
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();

            var lessons = lessonsFromDb.Select(l => new
            {
                l.Id,
                l.Title,
                l.Description,
                l.LessonDate,

                l.TeacherAssignmentId,

                CreatedAt = l.CreatedAt,
                CreatedDayName = GetArabicDayName(l.CreatedAt),
                CreatedTime = l.CreatedAt.ToString("hh:mm tt"),

                UpdatedAt = l.UpdatedAt,
                UpdatedDayName = l.UpdatedAt.HasValue
                    ? GetArabicDayName(l.UpdatedAt.Value)
                    : "",
                UpdatedTime = l.UpdatedAt.HasValue
                    ? l.UpdatedAt.Value.ToString("hh:mm tt")
                    : "",

                TeacherName = l.TeacherAssignment != null &&
                              l.TeacherAssignment.Teacher != null &&
                              l.TeacherAssignment.Teacher.User != null
                    ? l.TeacherAssignment.Teacher.User.FullName
                    : "",

                SubjectName = l.TeacherAssignment != null &&
                              l.TeacherAssignment.Subject != null
                    ? l.TeacherAssignment.Subject.Name
                    : "",

                ClassRoomName = l.TeacherAssignment != null &&
                                l.TeacherAssignment.ClassRoom != null
                    ? l.TeacherAssignment.ClassRoom.Name
                    : "",

                SectionName = l.TeacherAssignment != null &&
                              l.TeacherAssignment.Section != null
                    ? l.TeacherAssignment.Section.Name
                    : ""
            }).ToList();

            return Ok(lessons);
        }



        /*
    =====================================================
    تحويل اليوم إلى اللغة العربية
    =====================================================
*/
        private string GetArabicDayName(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => "السبت",
                DayOfWeek.Sunday => "الأحد",
                DayOfWeek.Monday => "الاثنين",
                DayOfWeek.Tuesday => "الثلاثاء",
                DayOfWeek.Wednesday => "الأربعاء",
                DayOfWeek.Thursday => "الخميس",
                DayOfWeek.Friday => "الجمعة",
                _ => ""
            };
        }





        /*
    =====================================================
    34) API لعرض درس واحد بواسطة Id
    =====================================================
*/
        [HttpGet("get-lesson/{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Where(l => l.Id == id)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.Description,
                    l.LessonDate,
                    TeacherAssignmentId = l.TeacherAssignmentId,
                    TeacherName = l.TeacherAssignment != null &&
                                  l.TeacherAssignment.Teacher != null &&
                                  l.TeacherAssignment.Teacher.User != null
                                  ? l.TeacherAssignment.Teacher.User.FullName
                                  : null,
                    SubjectName = l.TeacherAssignment != null && l.TeacherAssignment.Subject != null
                                  ? l.TeacherAssignment.Subject.Name
                                  : null,
                    ClassRoomName = l.TeacherAssignment != null && l.TeacherAssignment.ClassRoom != null
                                  ? l.TeacherAssignment.ClassRoom.Name
                                  : null,
                    SectionName = l.TeacherAssignment != null && l.TeacherAssignment.Section != null
                                  ? l.TeacherAssignment.Section.Name
                                  : null
                })
                .FirstOrDefaultAsync();

            if (lesson == null)
            {
                return NotFound("الدرس غير موجود.");
            }

            return Ok(lesson);
        }

        /*
    =====================================================
    35) API لعرض الدروس حسب التكليف التدريسي
    =====================================================
*/
        [HttpGet("get-lessons-by-assignment/{teacherAssignmentId}")]
        public async Task<IActionResult> GetLessonsByAssignment(int teacherAssignmentId)
        {
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .FirstOrDefaultAsync(a => a.Id == teacherAssignmentId);

            if (assignment == null)
            {
                return NotFound("التكليف التدريسي غير موجود.");
            }

            var lessons = await _context.Lessons
                .Where(l => l.TeacherAssignmentId == teacherAssignmentId)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.Description,
                    l.LessonDate
                })
                .OrderByDescending(l => l.LessonDate)
                .ToListAsync();

            return Ok(new
            {
                TeacherAssignmentId = assignment.Id,
                TeacherName = assignment.Teacher != null && assignment.Teacher.User != null ? assignment.Teacher.User.FullName : null,
                SubjectName = assignment.Subject != null ? assignment.Subject.Name : null,
                ClassRoomName = assignment.ClassRoom != null ? assignment.ClassRoom.Name : null,
                SectionName = assignment.Section != null ? assignment.Section.Name : null,
                Lessons = lessons
            });
        }

        /*
    =====================================================
    36) API لتعديل درس
    =====================================================
*/
        [HttpPut("update-lesson/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, UpdateLessonDto dto)
        {
            var lesson = await _context.Lessons.FindAsync(id);

            if (lesson == null)
            {
                return NotFound("الدرس غير موجود.");
            }

            lesson.TeacherAssignmentId = dto.TeacherAssignmentId;
            lesson.Title = dto.Title.Trim();
            lesson.Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();
            lesson.LessonDate = dto.LessonDate;

            /*
                نحفظ وقت آخر تعديل.
                هذا مهم حتى يظهر للطالب أن الدرس تم تعديله.
            */
            lesson.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الدرس بنجاح",
                lessonId = lesson.Id,
                updatedAt = lesson.UpdatedAt
            });
        }

        /*
    =====================================================
    37) API لحذف درس
    =====================================================
*/
        [HttpDelete("delete-lesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound("الدرس غير موجود.");
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الدرس بنجاح"
            });
        }


        /*
     =====================================================
     38) API لإضافة واجب جديد
     =====================================================

     هذا الـ endpoint يقوم بإضافة واجب جديد
     مرتبط بتكليف تدريسي معين.

     عند الإضافة نحفظ:
     - CreatedAt = وقت إضافة الواجب
     - UpdatedAt = null لأن الواجب جديد ولم يتم تعديله بعد

     من خلال TeacherAssignmentId نستطيع معرفة:
     - المدرس
     - المادة
     - الصف
     - الشعبة
 */
        [HttpPost("create-homework")]
        public async Task<IActionResult> CreateHomework(CreateHomeworkDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // التأكد من وجود التكليف التدريسي
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId);

            if (assignment == null)
            {
                return BadRequest("التكليف التدريسي غير موجود.");
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("عنوان الواجب مطلوب.");
            }

            // إنشاء الواجب الجديد
            var homework = new Homework
            {
                TeacherAssignmentId = dto.TeacherAssignmentId,
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),
                DueDate = dto.DueDate,

                /*
                    وقت الإضافة الحقيقي.
                    هذا سيظهر للأستاذ والطالب في Flutter.
                */
                CreatedAt = DateTime.Now,

                /*
                    الواجب جديد، لذلك لا يوجد وقت تعديل بعد.
                */
                UpdatedAt = null
            };

            _context.Homeworks.Add(homework);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الواجب بنجاح",
                homeworkId = homework.Id,
                homeworkTitle = homework.Title,
                description = homework.Description,
                dueDate = homework.DueDate,
                teacherAssignmentId = homework.TeacherAssignmentId,

                createdAt = homework.CreatedAt,
                createdDayName = GetArabicDayName(homework.CreatedAt),
                createdTime = homework.CreatedAt.ToString("hh:mm tt"),

                updatedAt = homework.UpdatedAt,
                updatedDayName = "",
                updatedTime = "",

                teacherName = assignment.Teacher != null && assignment.Teacher.User != null
                    ? assignment.Teacher.User.FullName
                    : "",

                subjectName = assignment.Subject != null
                    ? assignment.Subject.Name
                    : "",

                classRoomName = assignment.ClassRoom != null
                    ? assignment.ClassRoom.Name
                    : "",

                sectionName = assignment.Section != null
                    ? assignment.Section.Name
                    : ""
            });
        }

        /*
    =====================================================
    39) API لعرض جميع الواجبات
    =====================================================

    هذا الـ endpoint يعرض جميع الواجبات
    مع معلومات:
    - المادة
    - الصف
    - الشعبة
    - المدرس
    - وقت الإضافة
    - وقت آخر تعديل

    مهم:
    نستخدم ToListAsync أولًا ثم Select
    حتى لا يحدث خطأ مع GetArabicDayName داخل استعلام قاعدة البيانات.
*/
        [HttpGet("get-homeworks")]
        public async Task<IActionResult> GetHomeworks()
        {
            var homeworksFromDb = await _context.Homeworks
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            var homeworks = homeworksFromDb.Select(h => new
            {
                h.Id,
                h.Title,
                h.Description,
                h.DueDate,

                TeacherAssignmentId = h.TeacherAssignmentId,

                h.CreatedAt,
                CreatedDayName = GetArabicDayName(h.CreatedAt),
                CreatedTime = h.CreatedAt.ToString("hh:mm tt"),

                h.UpdatedAt,
                UpdatedDayName = h.UpdatedAt.HasValue
                    ? GetArabicDayName(h.UpdatedAt.Value)
                    : "",
                UpdatedTime = h.UpdatedAt.HasValue
                    ? h.UpdatedAt.Value.ToString("hh:mm tt")
                    : "",

                TeacherName = h.TeacherAssignment != null &&
                              h.TeacherAssignment.Teacher != null &&
                              h.TeacherAssignment.Teacher.User != null
                    ? h.TeacherAssignment.Teacher.User.FullName
                    : "",

                SubjectName = h.TeacherAssignment != null &&
                              h.TeacherAssignment.Subject != null
                    ? h.TeacherAssignment.Subject.Name
                    : "",

                ClassRoomName = h.TeacherAssignment != null &&
                                h.TeacherAssignment.ClassRoom != null
                    ? h.TeacherAssignment.ClassRoom.Name
                    : "",

                SectionName = h.TeacherAssignment != null &&
                              h.TeacherAssignment.Section != null
                    ? h.TeacherAssignment.Section.Name
                    : ""
            }).ToList();

            return Ok(homeworks);
        }

        /*
    =====================================================
    40) API لعرض واجب واحد بواسطة Id
    =====================================================

    هذا الـ endpoint يعرض واجبًا واحدًا بالتفصيل
    مع وقت الإضافة ووقت آخر تعديل.
*/
        [HttpGet("get-homework/{id}")]
        public async Task<IActionResult> GetHomeworkById(int id)
        {
            var homeworkFromDb = await _context.Homeworks
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (homeworkFromDb == null)
            {
                return NotFound("الواجب غير موجود.");
            }

            var homework = new
            {
                homeworkFromDb.Id,
                homeworkFromDb.Title,
                homeworkFromDb.Description,
                homeworkFromDb.DueDate,

                TeacherAssignmentId = homeworkFromDb.TeacherAssignmentId,

                homeworkFromDb.CreatedAt,
                CreatedDayName = GetArabicDayName(homeworkFromDb.CreatedAt),
                CreatedTime = homeworkFromDb.CreatedAt.ToString("hh:mm tt"),

                homeworkFromDb.UpdatedAt,
                UpdatedDayName = homeworkFromDb.UpdatedAt.HasValue
                    ? GetArabicDayName(homeworkFromDb.UpdatedAt.Value)
                    : "",
                UpdatedTime = homeworkFromDb.UpdatedAt.HasValue
                    ? homeworkFromDb.UpdatedAt.Value.ToString("hh:mm tt")
                    : "",

                TeacherName = homeworkFromDb.TeacherAssignment != null &&
                              homeworkFromDb.TeacherAssignment.Teacher != null &&
                              homeworkFromDb.TeacherAssignment.Teacher.User != null
                    ? homeworkFromDb.TeacherAssignment.Teacher.User.FullName
                    : "",

                SubjectName = homeworkFromDb.TeacherAssignment != null &&
                              homeworkFromDb.TeacherAssignment.Subject != null
                    ? homeworkFromDb.TeacherAssignment.Subject.Name
                    : "",

                ClassRoomName = homeworkFromDb.TeacherAssignment != null &&
                                homeworkFromDb.TeacherAssignment.ClassRoom != null
                    ? homeworkFromDb.TeacherAssignment.ClassRoom.Name
                    : "",

                SectionName = homeworkFromDb.TeacherAssignment != null &&
                              homeworkFromDb.TeacherAssignment.Section != null
                    ? homeworkFromDb.TeacherAssignment.Section.Name
                    : ""
            };

            return Ok(homework);
        }

        /*
     =====================================================
     41) API لعرض الواجبات حسب التكليف التدريسي
     =====================================================

     هذا الـ endpoint يعرض جميع الواجبات
     المرتبطة بتكليف تدريسي معين
     مع وقت الإضافة ووقت آخر تعديل.
 */
        [HttpGet("get-homeworks-by-assignment/{teacherAssignmentId}")]
        public async Task<IActionResult> GetHomeworksByAssignment(int teacherAssignmentId)
        {
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .FirstOrDefaultAsync(a => a.Id == teacherAssignmentId);

            if (assignment == null)
            {
                return NotFound("التكليف التدريسي غير موجود.");
            }

            var homeworksFromDb = await _context.Homeworks
                .Where(h => h.TeacherAssignmentId == teacherAssignmentId)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            var homeworks = homeworksFromDb.Select(h => new
            {
                h.Id,
                h.Title,
                h.Description,
                h.DueDate,

                h.CreatedAt,
                CreatedDayName = GetArabicDayName(h.CreatedAt),
                CreatedTime = h.CreatedAt.ToString("hh:mm tt"),

                h.UpdatedAt,
                UpdatedDayName = h.UpdatedAt.HasValue
                    ? GetArabicDayName(h.UpdatedAt.Value)
                    : "",
                UpdatedTime = h.UpdatedAt.HasValue
                    ? h.UpdatedAt.Value.ToString("hh:mm tt")
                    : ""
            }).ToList();

            return Ok(new
            {
                TeacherAssignmentId = assignment.Id,

                TeacherName = assignment.Teacher != null && assignment.Teacher.User != null
                    ? assignment.Teacher.User.FullName
                    : "",

                SubjectName = assignment.Subject != null
                    ? assignment.Subject.Name
                    : "",

                ClassRoomName = assignment.ClassRoom != null
                    ? assignment.ClassRoom.Name
                    : "",

                SectionName = assignment.Section != null
                    ? assignment.Section.Name
                    : "",

                Homeworks = homeworks
            });
        }

        /*
     =====================================================
     42) API لتعديل واجب
     =====================================================

     هذا الـ endpoint يسمح بتعديل:
     - التكليف التدريسي
     - عنوان الواجب
     - الوصف
     - تاريخ التسليم

     وعند التعديل نحفظ:
     - UpdatedAt = وقت آخر تعديل

     هذا مهم حتى يظهر للطالب والأستاذ في Flutter.
 */
        [HttpPut("update-homework/{id}")]
        public async Task<IActionResult> UpdateHomework(int id, UpdateHomeworkDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن الواجب
            var homework = await _context.Homeworks.FindAsync(id);

            if (homework == null)
            {
                return NotFound("الواجب غير موجود.");
            }

            // التأكد من وجود التكليف التدريسي الجديد
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId);

            if (assignment == null)
            {
                return BadRequest("التكليف التدريسي غير موجود.");
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("عنوان الواجب مطلوب.");
            }

            // تعديل بيانات الواجب
            homework.TeacherAssignmentId = dto.TeacherAssignmentId;
            homework.Title = dto.Title.Trim();
            homework.Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();
            homework.DueDate = dto.DueDate;

            /*
                نحفظ وقت آخر تعديل.
            */
            homework.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الواجب بنجاح",
                homeworkId = homework.Id,
                homeworkTitle = homework.Title,
                description = homework.Description,
                dueDate = homework.DueDate,

                teacherAssignmentId = homework.TeacherAssignmentId,

                updatedAt = homework.UpdatedAt,
                updatedDayName = homework.UpdatedAt.HasValue
                    ? GetArabicDayName(homework.UpdatedAt.Value)
                    : "",
                updatedTime = homework.UpdatedAt.HasValue
                    ? homework.UpdatedAt.Value.ToString("hh:mm tt")
                    : "",

                teacherName = assignment.Teacher != null && assignment.Teacher.User != null
                    ? assignment.Teacher.User.FullName
                    : "",

                subjectName = assignment.Subject != null
                    ? assignment.Subject.Name
                    : "",

                classRoomName = assignment.ClassRoom != null
                    ? assignment.ClassRoom.Name
                    : "",

                sectionName = assignment.Section != null
                    ? assignment.Section.Name
                    : ""
            });
        }

        /*
    =====================================================
    43) API لحذف واجب
    =====================================================

    هذا الـ endpoint يقوم بحذف واجب بواسطة Id.

    لا نحتاج UpdatedAt هنا لأن الواجب سيتم حذفه نهائيًا،
    لكن نرجع معلومات بسيطة حتى يستفيد منها Flutter أو Web.
*/
        [HttpDelete("delete-homework/{id}")]
        public async Task<IActionResult> DeleteHomework(int id)
        {
            var homework = await _context.Homeworks
                .FirstOrDefaultAsync(h => h.Id == id);

            if (homework == null)
            {
                return NotFound("الواجب غير موجود.");
            }

            var deletedHomeworkId = homework.Id;
            var deletedHomeworkTitle = homework.Title;

            _context.Homeworks.Remove(homework);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الواجب بنجاح",
                homeworkId = deletedHomeworkId,
                homeworkTitle = deletedHomeworkTitle
            });
        }


        /*
=====================================================
44) API لإضافة سجل حضور أو غياب جديد
=====================================================

هذا الـ endpoint يقوم بإضافة حضور أو غياب لطالب معين

ويتم تحديد:
- الطالب برقم الطالب
- المدرس برقم المدرس
- المادة
- الصف
- الشعبة

ثم يتم الوصول إلى التكليف التدريسي المناسب
وربط الحضور به

ملاحظة مهمة:
لا يمكن تسجيل الحضور إذا كان الطالب لا ينتمي
إلى الصف أو الشعبة المحددة
*/
        [HttpPost("create-attendance")]
        public async Task<IActionResult> CreateAttendance(CreateAttendanceDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن الطالب برقم الطالب
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            // البحث عن المدرس برقم المدرس
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherNumber.Trim().ToLower() == dto.TeacherNumber.Trim().ToLower());

            if (teacher == null)
            {
                return BadRequest("المدرس غير موجود.");
            }

            // البحث عن المادة
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SubjectName.Trim().ToLower());

            if (subject == null)
            {
                return BadRequest("المادة غير موجودة.");
            }

            // البحث عن الصف
            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود.");
            }

            // البحث عن الشعبة داخل الصف المحدد
            var section = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() &&
                    s.ClassRoomId == classRoom.Id);

            if (section == null)
            {
                return BadRequest("الشعبة غير موجودة داخل الصف المحدد.");
            }

            // التحقق أن الطالب ينتمي فعلاً لنفس الصف والشعبة
            if (student.ClassRoomId != classRoom.Id || student.SectionId != section.Id)
            {
                return BadRequest("هذا الطالب لا ينتمي إلى الصف أو الشعبة المحددة.");
            }

            // البحث عن التكليف التدريسي
            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.TeacherId == teacher.Id &&
                    a.SubjectId == subject.Id &&
                    a.ClassRoomId == classRoom.Id &&
                    a.SectionId == section.Id);

            if (assignment == null)
            {
                return BadRequest("لا يوجد تكليف تدريسي بهذه البيانات.");
            }

            // التحقق من عدم وجود سجل حضور سابق لنفس الطالب ونفس المادة ونفس التاريخ
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.StudentId == student.Id &&
                    a.TeacherAssignmentId == assignment.Id &&
                    a.AttendanceDate.Date == dto.AttendanceDate.Date);

            if (existingAttendance != null)
            {
                return BadRequest("يوجد سجل حضور أو غياب لهذا الطالب في نفس التاريخ ونفس المادة.");
            }

            // إنشاء سجل الحضور
            var attendance = new Attendance
            {
                StudentId = student.Id,
                TeacherAssignmentId = assignment.Id,
                AttendanceDate = dto.AttendanceDate,
                Status = dto.Status.Trim(),
                Notes = dto.Notes
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة سجل الحضور/الغياب بنجاح",
                attendanceId = attendance.Id,
                studentName = student.User != null ? student.User.FullName : null,
                studentNumber = student.StudentNumber,
                teacherName = teacher.User != null ? teacher.User.FullName : null,
                teacherNumber = teacher.TeacherNumber,
                subjectName = subject.Name,
                classRoomName = classRoom.Name,
                sectionName = section.Name,
                attendanceDate = attendance.AttendanceDate,
                status = attendance.Status
            });
        }

        /*
            =====================================================
            45) API لعرض جميع سجلات الحضور والغياب
            =====================================================

            هذا الـ endpoint يعرض جميع سجلات الحضور والغياب
            مع الطالب والمدرس والمادة والصف والشعبة
        */
        [HttpGet("get-attendances")]
        public async Task<IActionResult> GetAttendances()
        {
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Teacher)
                        .ThenInclude(t => t.User)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Subject)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.ClassRoom)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Section)
                .Select(a => new
                {
                    a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student != null && a.Student.User != null ? a.Student.User.FullName : null,
                    StudentNumber = a.Student != null ? a.Student.StudentNumber : null,
                    TeacherAssignmentId = a.TeacherAssignmentId,
                    TeacherName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Teacher != null &&
                                  a.TeacherAssignment.Teacher.User != null
                                  ? a.TeacherAssignment.Teacher.User.FullName
                                  : null,
                    TeacherNumber = a.TeacherAssignment != null &&
                                    a.TeacherAssignment.Teacher != null
                                    ? a.TeacherAssignment.Teacher.TeacherNumber
                                    : null,
                    SubjectName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Subject != null
                                  ? a.TeacherAssignment.Subject.Name
                                  : null,
                    ClassRoomName = a.TeacherAssignment != null &&
                                    a.TeacherAssignment.ClassRoom != null
                                    ? a.TeacherAssignment.ClassRoom.Name
                                    : null,
                    SectionName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Section != null
                                  ? a.TeacherAssignment.Section.Name
                                  : null,
                    a.AttendanceDate,
                    a.Status,
                    a.Notes
                })
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            return Ok(attendances);
        }

        /*
            =====================================================
            46) API لعرض سجل حضور أو غياب واحد بواسطة Id
            =====================================================

            هذا الـ endpoint يعرض سجلًا واحدًا بالتفصيل
        */
        [HttpGet("get-attendance/{id}")]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Teacher)
                        .ThenInclude(t => t.User)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Subject)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.ClassRoom)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Section)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student != null && a.Student.User != null ? a.Student.User.FullName : null,
                    StudentNumber = a.Student != null ? a.Student.StudentNumber : null,
                    TeacherAssignmentId = a.TeacherAssignmentId,
                    TeacherName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Teacher != null &&
                                  a.TeacherAssignment.Teacher.User != null
                                  ? a.TeacherAssignment.Teacher.User.FullName
                                  : null,
                    TeacherNumber = a.TeacherAssignment != null &&
                                    a.TeacherAssignment.Teacher != null
                                    ? a.TeacherAssignment.Teacher.TeacherNumber
                                    : null,
                    SubjectName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Subject != null
                                  ? a.TeacherAssignment.Subject.Name
                                  : null,
                    ClassRoomName = a.TeacherAssignment != null &&
                                    a.TeacherAssignment.ClassRoom != null
                                    ? a.TeacherAssignment.ClassRoom.Name
                                    : null,
                    SectionName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Section != null
                                  ? a.TeacherAssignment.Section.Name
                                  : null,
                    a.AttendanceDate,
                    a.Status,
                    a.Notes
                })
                .FirstOrDefaultAsync();

            if (attendance == null)
            {
                return NotFound("سجل الحضور أو الغياب غير موجود.");
            }

            return Ok(attendance);
        }

        /*
            =====================================================
            47) API لعرض سجلات الحضور والغياب لطالب معين
            =====================================================

            هذا الـ endpoint يعرض جميع سجلات الحضور
            الخاصة بطالب محدد
        */
        [HttpGet("get-attendances-by-student/{studentId}")]
        public async Task<IActionResult> GetAttendancesByStudent(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var attendances = await _context.Attendances
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Teacher)
                        .ThenInclude(t => t.User)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Subject)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.ClassRoom)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Section)
                .Where(a => a.StudentId == studentId)
                .Select(a => new
                {
                    a.Id,
                    TeacherName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Teacher != null &&
                                  a.TeacherAssignment.Teacher.User != null
                                  ? a.TeacherAssignment.Teacher.User.FullName
                                  : null,
                    SubjectName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Subject != null
                                  ? a.TeacherAssignment.Subject.Name
                                  : null,
                    ClassRoomName = a.TeacherAssignment != null &&
                                    a.TeacherAssignment.ClassRoom != null
                                    ? a.TeacherAssignment.ClassRoom.Name
                                    : null,
                    SectionName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Section != null
                                  ? a.TeacherAssignment.Section.Name
                                  : null,
                    a.AttendanceDate,
                    a.Status,
                    a.Notes
                })
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            return Ok(new
            {
                StudentId = student.Id,
                StudentName = student.User != null ? student.User.FullName : null,
                StudentNumber = student.StudentNumber,
                Attendances = attendances
            });
        }

        /*
            =====================================================
            48) API لتعديل سجل حضور أو غياب
            =====================================================

            هذا الـ endpoint يسمح بتعديل:
            - الطالب
            - المدرس
            - المادة
            - الصف
            - الشعبة
            - التاريخ
            - الحالة
            - الملاحظات

            ملاحظة مهمة:
            لا يمكن تعديل السجل إذا كان الطالب لا ينتمي
            إلى الصف أو الشعبة المحددة
        */
        [HttpPut("update-attendance/{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, UpdateAttendanceDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن سجل الحضور
            var attendance = await _context.Attendances.FindAsync(id);

            if (attendance == null)
            {
                return NotFound("سجل الحضور أو الغياب غير موجود.");
            }

            // البحث عن الطالب
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            // البحث عن المدرس
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherNumber.Trim().ToLower() == dto.TeacherNumber.Trim().ToLower());

            if (teacher == null)
            {
                return BadRequest("المدرس غير موجود.");
            }

            // البحث عن المادة
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SubjectName.Trim().ToLower());

            if (subject == null)
            {
                return BadRequest("المادة غير موجودة.");
            }

            // البحث عن الصف
            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود.");
            }

            // البحث عن الشعبة
            var section = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() &&
                    s.ClassRoomId == classRoom.Id);

            if (section == null)
            {
                return BadRequest("الشعبة غير موجودة داخل الصف المحدد.");
            }

            // التحقق أن الطالب ينتمي فعلاً لنفس الصف والشعبة
            if (student.ClassRoomId != classRoom.Id || student.SectionId != section.Id)
            {
                return BadRequest("هذا الطالب لا ينتمي إلى الصف أو الشعبة المحددة.");
            }

            // البحث عن التكليف التدريسي
            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.TeacherId == teacher.Id &&
                    a.SubjectId == subject.Id &&
                    a.ClassRoomId == classRoom.Id &&
                    a.SectionId == section.Id);

            if (assignment == null)
            {
                return BadRequest("لا يوجد تكليف تدريسي بهذه البيانات.");
            }

            // التحقق من عدم وجود سجل آخر مكرر
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.Id != id &&
                    a.StudentId == student.Id &&
                    a.TeacherAssignmentId == assignment.Id &&
                    a.AttendanceDate.Date == dto.AttendanceDate.Date);

            if (existingAttendance != null)
            {
                return BadRequest("يوجد سجل حضور أو غياب آخر بنفس البيانات.");
            }

            // تعديل السجل
            attendance.StudentId = student.Id;
            attendance.TeacherAssignmentId = assignment.Id;
            attendance.AttendanceDate = dto.AttendanceDate;
            attendance.Status = dto.Status.Trim();
            attendance.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل سجل الحضور/الغياب بنجاح"
            });
        }

        /*
            =====================================================
            49) API لحذف سجل حضور أو غياب
            =====================================================

            هذا الـ endpoint يقوم بحذف سجل حضور أو غياب
            بواسطة Id
        */
        [HttpDelete("delete-attendance/{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound("سجل الحضور أو الغياب غير موجود.");
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف سجل الحضور/الغياب بنجاح"
            });
        }


        /*
 =====================================================
 50) API لإضافة درجة نصف العام
 =====================================================
 */
        [HttpPost("create-midyear-grade")]
        public async Task<IActionResult> CreateMidYearGrade(CreateMidYearGradeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.FirstAssessmentScore < 0 || dto.FirstAssessmentScore > 20)
                return BadRequest("المحصلة الأولى من 20");

            if (dto.MidtermExamScore < 0 || dto.MidtermExamScore > 30)
                return BadRequest("اختبار نصف العام من 30");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber == dto.StudentNumber);

            if (student == null)
                return BadRequest("الطالب غير موجود");

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name == dto.SubjectName);

            if (subject == null)
                return BadRequest("المادة غير موجودة");

            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name == dto.ClassRoomName);

            var section = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name == dto.SectionName &&
                    s.ClassRoomId == classRoom.Id);

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.SubjectId == subject.Id &&
                    a.ClassRoomId == classRoom.Id &&
                    a.SectionId == section.Id);

            if (assignment == null)
                return BadRequest("لا يوجد تكليف تدريسي");

            // حذف القديم إن وجد (تحديث تلقائي)
            var existingMidYear = await _context.Grades.AnyAsync(g =>
    g.StudentId == student.Id &&
    g.TeacherAssignmentId == assignment.Id &&
    g.TermName == "نصف العام");

            if (existingMidYear)
            {
                return BadRequest("تم إدخال درجات نصف العام لهذا الطالب في هذه المادة مسبقًا. استخدم التعديل بدل الإضافة.");
            }

            // إضافة محصلة
            _context.Grades.Add(new Grade
            {
                StudentId = student.Id,
                TeacherAssignmentId = assignment.Id,
                TermName = "نصف العام",
                GradeType = "المحصلة الأولى",
                Score = dto.FirstAssessmentScore,
                MaxScore = 20
            });

            // إضافة اختبار
            _context.Grades.Add(new Grade
            {
                StudentId = student.Id,
                TeacherAssignmentId = assignment.Id,
                TermName = "نصف العام",
                GradeType = "اختبار نصف العام",
                Score = dto.MidtermExamScore,
                MaxScore = 30
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حفظ درجات نصف العام",
                total = dto.FirstAssessmentScore + dto.MidtermExamScore
            });
        }

        /*
        =====================================================
        51) API لإضافة درجة نهاية العام
        =====================================================
        */
        [HttpPost("create-finalyear-grade")]
        public async Task<IActionResult> CreateFinalYearGrade(CreateFinalYearGradeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.SecondAssessmentScore < 0 || dto.SecondAssessmentScore > 20)
                return BadRequest("المحصلة الثانية من 20");

            if (dto.FinalExamScore < 0 || dto.FinalExamScore > 30)
                return BadRequest("اختبار نهاية العام من 30");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber == dto.StudentNumber);

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name == dto.SubjectName);

            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Name == dto.ClassRoomName);

            var section = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name == dto.SectionName &&
                    s.ClassRoomId == classRoom.Id);

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.SubjectId == subject.Id &&
                    a.ClassRoomId == classRoom.Id &&
                    a.SectionId == section.Id);

            if (assignment == null)
                return BadRequest("لا يوجد تكليف تدريسي");

            var existingFinalYear = await _context.Grades.AnyAsync(g =>
                g.StudentId == student.Id &&
                g.TeacherAssignmentId == assignment.Id &&
                g.TermName == "نهاية العام");

            if (existingFinalYear)
            {
                return BadRequest("تم إدخال درجات نهاية العام لهذا الطالب في هذه المادة مسبقًا. استخدم التعديل بدل الإضافة.");
            }

            var oldGrades = await _context.Grades
                .Where(g =>
                    g.StudentId == student.Id &&
                    g.TeacherAssignmentId == assignment.Id &&
                    g.TermName == "نهاية العام")
                .ToListAsync();

            _context.Grades.RemoveRange(oldGrades);

            _context.Grades.Add(new Grade
            {
                StudentId = student.Id,
                TeacherAssignmentId = assignment.Id,
                TermName = "نهاية العام",
                GradeType = "المحصلة الثانية",
                Score = dto.SecondAssessmentScore,
                MaxScore = 20
            });

            _context.Grades.Add(new Grade
            {
                StudentId = student.Id,
                TeacherAssignmentId = assignment.Id,
                TermName = "نهاية العام",
                GradeType = "اختبار نهاية العام",
                Score = dto.FinalExamScore,
                MaxScore = 30
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حفظ درجات نهاية العام",
                total = dto.SecondAssessmentScore + dto.FinalExamScore
            });
        }


        /*
      =====================================================
      API عرض درجات طالب محدد
      =====================================================

      يرجع درجات الطالب مع:
      - المادة
      - المدرس
      - الصف
      - الشعبة
      - نوع الدرجة
  */
        [HttpGet("get-student-grades/{studentId}")]
        public async Task<IActionResult> GetStudentGrades(int studentId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var grades = await _context.Grades
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Where(g => g.StudentId == studentId)
                .Select(g => new
                {
                    g.Id,

                    SubjectName = g.TeacherAssignment.Subject.Name,

                    TeacherName = g.TeacherAssignment.Teacher.User != null
                        ? g.TeacherAssignment.Teacher.User.FullName
                        : "",

                    ClassRoomName = g.TeacherAssignment.ClassRoom.Name,
                    SectionName = g.TeacherAssignment.Section.Name,

                    g.TermName,
                    g.GradeType,
                    g.Score,
                    g.MaxScore
                })
                .OrderBy(g => g.ClassRoomName)
                .ThenBy(g => g.SubjectName)
                .ThenBy(g => g.GradeType)
                .ToListAsync();

            return Ok(grades);
        }





        /*
    =====================================================
    API عرض حضور وغياب طالب محدد
    =====================================================

    يستخدم في بوابة الطالب
    بحيث الطالب يرى سجله فقط.
*/
        [HttpGet("get-student-attendance/{studentId}")]
        public async Task<IActionResult> GetStudentAttendance(int studentId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var attendances = await _context.Attendances
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Subject)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.ClassRoom)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Section)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Teacher)
                        .ThenInclude(t => t.User)
                .Where(a => a.StudentId == studentId)
                .Select(a => new
                {
                    a.Id,
                    AttendanceDate = a.AttendanceDate,
                    a.Status,
                    SubjectName = a.TeacherAssignment.Subject.Name,
                    ClassRoomName = a.TeacherAssignment.ClassRoom.Name,
                    SectionName = a.TeacherAssignment.Section.Name,
                    TeacherName = a.TeacherAssignment.Teacher.User.FullName
                })
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            return Ok(attendances);
        }





        //عرض نتيجة طالب اما نصف العام او العام كامل  


        [HttpGet("get-student-full-result/{studentNumber}")]
        public async Task<IActionResult> GetStudentFullResult(string studentNumber)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.ClassRoom)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.StudentNumber.Trim() == studentNumber.Trim());

            if (student == null)
                return NotFound("الطالب غير موجود.");

            var grades = await _context.Grades
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Where(g => g.StudentId == student.Id)
                .ToListAsync();

            if (!grades.Any())
                return BadRequest("لا توجد درجات مدخلة لهذا الطالب.");

            bool hasFinalYear = grades.Any(g => g.TermName == "نهاية العام");

            var result = grades
                .GroupBy(g => g.TeacherAssignment.Subject.Name)
                .Select(group =>
                {
                    var firstAssessment = group.FirstOrDefault(g =>
                        g.TermName == "نصف العام" &&
                        g.GradeType == "المحصلة الأولى")?.Score ?? 0;

                    var midtermExam = group.FirstOrDefault(g =>
                        g.TermName == "نصف العام" &&
                        (g.GradeType == "اختبار نصف العام" || g.GradeType == "اختبار"))?.Score ?? 0;

                    var secondAssessment = group.FirstOrDefault(g =>
                        g.TermName == "نهاية العام" &&
                        g.GradeType == "المحصلة الثانية")?.Score ?? 0;

                    var finalExam = group.FirstOrDefault(g =>
                        g.TermName == "نهاية العام" &&
                        (g.GradeType == "اختبار نهاية العام" || g.GradeType == "اختبار"))?.Score ?? 0;

                    var midYearTotal = firstAssessment + midtermExam;
                    var finalYearTotal = secondAssessment + finalExam;
                    var total = midYearTotal + finalYearTotal;

                    var subjectFailed = hasFinalYear
                        ? total < 50
                        : midYearTotal < 25;

                    return new
                    {
                        SubjectName = group.Key,
                        FirstAssessmentScore = firstAssessment,
                        MidtermExamScore = midtermExam,
                        MidYearTotal = midYearTotal,
                        SecondAssessmentScore = secondAssessment,
                        FinalExamScore = finalExam,
                        FinalYearTotal = finalYearTotal,
                        Total = total,
                        IsFailed = subjectFailed
                    };
                })
                .OrderBy(r => r.SubjectName)
                .ToList();

            decimal grandTotal = result.Sum(r => hasFinalYear ? r.Total : r.MidYearTotal);
            decimal maxTotal = result.Count * (hasFinalYear ? 100 : 50);
            decimal percentage = maxTotal > 0 ? Math.Round((grandTotal / maxTotal) * 100, 2) : 0;

            int failedSubjectsCount = result.Count(r => r.IsFailed);

            string finalStatus = failedSubjectsCount > 0 ? "راسب" : "ناجح";

            string estimate =
                percentage >= 90 ? "ممتاز" :
                percentage >= 80 ? "جيد جدًا" :
                percentage >= 70 ? "جيد" :
                percentage >= 60 ? "مقبول" :
                "ضعيف";

            return Ok(new
            {
                StudentName = student.User != null ? student.User.FullName : "",
                StudentNumber = student.StudentNumber,
                ClassRoomName = student.ClassRoom != null ? student.ClassRoom.Name : "",
                SectionName = student.Section != null ? student.Section.Name : "",
                HasFinalYear = hasFinalYear,
                Results = result,
                GrandTotal = grandTotal,
                MaxTotal = maxTotal,
                Percentage = percentage,
                Estimate = estimate,
                FinalStatus = finalStatus,
                FailedSubjectsCount = failedSubjectsCount
            });
        }




        /*
        =====================================================
        53) API تعديل درجة
        =====================================================
        */
        /*
     =====================================================
     API لتعديل درجات نصف العام أو نهاية العام
     =====================================================

     إذا TermName = نصف العام:
     - يعدل المحصلة الأولى
     - يعدل اختبار نصف العام

     إذا TermName = نهاية العام:
     - يعدل المحصلة الثانية
     - يعدل اختبار نهاية العام
 */
        [HttpPut("update-grade/{id}")]
        public async Task<IActionResult> UpdateGrade(int id, UpdateGradeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s =>
                    s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s =>
                    s.Name.Trim().ToLower() == dto.SubjectName.Trim().ToLower());

            if (subject == null)
            {
                return BadRequest("المادة غير موجودة.");
            }

            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c =>
                    c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());

            if (classRoom == null)
            {
                return BadRequest("الصف غير موجود.");
            }

            var section = await _context.Sections
                .FirstOrDefaultAsync(s =>
                    s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() &&
                    s.ClassRoomId == classRoom.Id);

            if (section == null)
            {
                return BadRequest("الشعبة غير موجودة داخل الصف المحدد.");
            }

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.SubjectId == subject.Id &&
                    a.ClassRoomId == classRoom.Id &&
                    a.SectionId == section.Id);

            if (assignment == null)
            {
                return BadRequest("لا يوجد تكليف تدريسي لهذه المادة.");
            }

            if (dto.TermName == "نصف العام")
            {
                if (dto.FirstAssessmentScore < 0 || dto.FirstAssessmentScore > 20)
                    return BadRequest("المحصلة الأولى يجب أن تكون من 0 إلى 20.");

                if (dto.MidtermExamScore < 0 || dto.MidtermExamScore > 30)
                    return BadRequest("اختبار نصف العام يجب أن يكون من 0 إلى 30.");

                var firstGrade = await _context.Grades.FirstOrDefaultAsync(g =>
                    g.StudentId == student.Id &&
                    g.TeacherAssignmentId == assignment.Id &&
                    g.TermName == "نصف العام" &&
                    g.GradeType == "المحصلة الأولى");

                var examGrade = await _context.Grades.FirstOrDefaultAsync(g =>
    g.StudentId == student.Id &&
    g.TeacherAssignmentId == assignment.Id &&
    g.TermName == "نصف العام" &&
    (
        g.GradeType == "اختبار نصف العام" ||
        g.GradeType == "اختبار"
    ));

                if (firstGrade == null || examGrade == null)
                {
                    return NotFound("درجات نصف العام غير موجودة لهذا الطالب في هذه المادة.");
                }

                firstGrade.Score = dto.FirstAssessmentScore ?? firstGrade.Score;
                firstGrade.MaxScore = 20;

                examGrade.Score = dto.MidtermExamScore ?? examGrade.Score;
                examGrade.MaxScore = 30;
            }
            else if (dto.TermName == "نهاية العام")
            {
                if (dto.SecondAssessmentScore < 0 || dto.SecondAssessmentScore > 20)
                    return BadRequest("المحصلة الثانية يجب أن تكون من 0 إلى 20.");

                if (dto.FinalExamScore < 0 || dto.FinalExamScore > 30)
                    return BadRequest("اختبار نهاية العام يجب أن يكون من 0 إلى 30.");

                var secondGrade = await _context.Grades.FirstOrDefaultAsync(g =>
                    g.StudentId == student.Id &&
                    g.TeacherAssignmentId == assignment.Id &&
                    g.TermName == "نهاية العام" &&
                    g.GradeType == "المحصلة الثانية");

                var finalGrade = await _context.Grades.FirstOrDefaultAsync(g =>
    g.StudentId == student.Id &&
    g.TeacherAssignmentId == assignment.Id &&
    g.TermName == "نهاية العام" &&
    (
        g.GradeType == "اختبار نهاية العام" ||
        g.GradeType == "اختبار"
    ));

                if (secondGrade == null || finalGrade == null)
                {
                    return NotFound("درجات نهاية العام غير موجودة لهذا الطالب في هذه المادة.");
                }

                secondGrade.Score = dto.SecondAssessmentScore ?? secondGrade.Score;
                secondGrade.MaxScore = 20;

                finalGrade.Score = dto.FinalExamScore ?? finalGrade.Score;
                finalGrade.MaxScore = 30;
            }
            else
            {
                return BadRequest("الفترة يجب أن تكون: نصف العام أو نهاية العام.");
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الدرجة بنجاح"
            });
        }


        /*
=====================================================
API عرض جميع الدرجات
=====================================================
*/
        [HttpGet("get-grades")]
        public async Task<IActionResult> GetGrades()
        {
            var grades = await _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Select(g => new
                {
                    g.Id,

                    StudentName = g.Student.User.FullName,
                    StudentNumber = g.Student.StudentNumber,

                    SubjectName = g.TeacherAssignment.Subject.Name,
                    ClassRoomName = g.TeacherAssignment.ClassRoom.Name,
                    SectionName = g.TeacherAssignment.Section.Name,

                    g.TermName,
                    g.GradeType,
                    g.Score,
                    g.MaxScore
                })
                .OrderBy(g => g.StudentName)
                .ToListAsync();

            return Ok(grades);
        }



        /*
=====================================================
API عرض درجة واحدة بواسطة Id
=====================================================
*/
        [HttpGet("get-grade/{id}")]
        public async Task<IActionResult> GetGradeById(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Where(g => g.Id == id)
                .Select(g => new
                {
                    g.Id,
                    StudentName = g.Student.User.FullName,
                    StudentNumber = g.Student.StudentNumber,
                    SubjectName = g.TeacherAssignment.Subject.Name,
                    ClassRoomName = g.TeacherAssignment.ClassRoom.Name,
                    SectionName = g.TeacherAssignment.Section.Name,
                    g.TermName,
                    g.GradeType,
                    g.Score,
                    g.MaxScore
                })
                .FirstOrDefaultAsync();

            if (grade == null)
                return NotFound("الدرجة غير موجودة.");

            return Ok(grade);
        }




        /*
        =====================================================
        54) API حذف درجة
        =====================================================
        */
        [HttpDelete("delete-grade/{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var grade = await _context.Grades.FindAsync(id);

            if (grade == null)
                return NotFound();

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();

            return Ok("تم الحذف");
        } 


        /*
    =====================================================
    57) API لإضافة ملاحظة جديدة لطالب
    =====================================================

    هذا الـ endpoint يقوم بإضافة ملاحظة لطالب معين
    من خلال رقم الطالب
*/
        [HttpPost("create-student-note")]
        public async Task<IActionResult> CreateStudentNote(CreateStudentNoteDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن الطالب برقم الطالب
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            // إنشاء الملاحظة
            var studentNote = new StudentNote
            {
                StudentId = student.Id,
                NoteText = dto.NoteText.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.StudentNotes.Add(studentNote);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الملاحظة بنجاح",
                noteId = studentNote.Id,
                studentName = student.User != null ? student.User.FullName : null,
                studentNumber = student.StudentNumber,
                noteText = studentNote.NoteText,
                createdAt = studentNote.CreatedAt
            });
        }

        /*
            =====================================================
            58) API لعرض جميع ملاحظات الطلاب
            =====================================================

            هذا الـ endpoint يعرض جميع الملاحظات
            الخاصة بجميع الطلاب
        */
        [HttpGet("get-student-notes")]
        public async Task<IActionResult> GetStudentNotes()
        {
            var notes = await _context.StudentNotes
                .Include(n => n.Student)
                    .ThenInclude(s => s.User)
                .Select(n => new
                {
                    n.Id,
                    StudentId = n.StudentId,
                    StudentName = n.Student != null && n.Student.User != null ? n.Student.User.FullName : null,
                    StudentNumber = n.Student != null ? n.Student.StudentNumber : null,
                    n.NoteText,
                    n.CreatedAt
                })
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notes);
        }

        /*
            =====================================================
            59) API لعرض ملاحظة واحدة بواسطة Id
            =====================================================

            هذا الـ endpoint يعرض ملاحظة واحدة بالتفصيل
        */
        [HttpGet("get-student-note/{id}")]
        public async Task<IActionResult> GetStudentNoteById(int id)
        {
            var note = await _context.StudentNotes
                .Include(n => n.Student)
                    .ThenInclude(s => s.User)
                .Where(n => n.Id == id)
                .Select(n => new
                {
                    n.Id,
                    StudentId = n.StudentId,
                    StudentName = n.Student != null && n.Student.User != null ? n.Student.User.FullName : null,
                    StudentNumber = n.Student != null ? n.Student.StudentNumber : null,
                    n.NoteText,
                    n.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (note == null)
            {
                return NotFound("الملاحظة غير موجودة.");
            }

            return Ok(note);
        }

        /*
     =====================================================
     API عرض ملاحظات طالب محدد
     =====================================================

     يستخدم في بوابة الطالب
     حتى يرى الطالب ملاحظاته فقط.

     مهم:
     نرجع List مباشرة حتى يعمل العداد في لوحة الطالب.
 */
        [HttpGet("get-student-notes-by-student/{studentId}")]
        public async Task<IActionResult> GetStudentNotesByStudent(int studentId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var notes = await _context.StudentNotes
                .Where(n => n.StudentId == studentId)
                .Select(n => new
                {
                    n.Id,
                    n.StudentId,
                    NoteText = n.NoteText,
                    CreatedAt = n.CreatedAt
                })
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notes);
        }

        /*
            =====================================================
            61) API لتعديل ملاحظة طالب
            =====================================================

            هذا الـ endpoint يسمح بتعديل:
            - الطالب
            - نص الملاحظة
        */
        [HttpPut("update-student-note/{id}")]
        public async Task<IActionResult> UpdateStudentNote(int id, UpdateStudentNoteDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن الملاحظة
            var note = await _context.StudentNotes.FindAsync(id);

            if (note == null)
            {
                return NotFound("الملاحظة غير موجودة.");
            }

            // البحث عن الطالب
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            // تعديل الملاحظة
            note.StudentId = student.Id;
            note.NoteText = dto.NoteText.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الملاحظة بنجاح"
            });
        }

        /*
            =====================================================
            62) API لحذف ملاحظة طالب
            =====================================================

            هذا الـ endpoint يقوم بحذف ملاحظة بواسطة Id
        */
        [HttpDelete("delete-student-note/{id}")]
        public async Task<IActionResult> DeleteStudentNote(int id)
        {
            var note = await _context.StudentNotes
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null)
            {
                return NotFound("الملاحظة غير موجودة.");
            }

            _context.StudentNotes.Remove(note);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الملاحظة بنجاح"
            });
        }


        /*
    =====================================================
    63) API لإضافة مهمة يومية جديدة لطالب
    =====================================================

    هذا الـ endpoint يقوم بإضافة مهمة يومية لطالب معين
    من خلال رقم الطالب
*/
        [HttpPost("create-daily-task")]
        public async Task<IActionResult> CreateDailyTask(CreateDailyTaskDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن الطالب برقم الطالب
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            // إنشاء المهمة اليومية
            var dailyTask = new DailyTask
            {
                StudentId = student.Id,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                TaskDate = dto.TaskDate,
                IsCompleted = dto.IsCompleted
            };

            _context.DailyTasks.Add(dailyTask);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة المهمة اليومية بنجاح",
                dailyTaskId = dailyTask.Id,
                studentName = student.User != null ? student.User.FullName : null,
                studentNumber = student.StudentNumber,
                title = dailyTask.Title,
                description = dailyTask.Description,
                taskDate = dailyTask.TaskDate,
                isCompleted = dailyTask.IsCompleted
            });
        }

        /*
            =====================================================
            64) API لعرض جميع المهام اليومية
            =====================================================

            هذا الـ endpoint يعرض جميع المهام اليومية
            الخاصة بجميع الطلاب
        */
        [HttpGet("get-daily-tasks")]
        public async Task<IActionResult> GetDailyTasks()
        {
            var tasks = await _context.DailyTasks
                .Include(t => t.Student)
                    .ThenInclude(s => s.User)
                .Select(t => new
                {
                    t.Id,
                    StudentId = t.StudentId,
                    StudentName = t.Student != null && t.Student.User != null ? t.Student.User.FullName : null,
                    StudentNumber = t.Student != null ? t.Student.StudentNumber : null,
                    t.Title,
                    t.Description,
                    t.TaskDate,
                    t.IsCompleted
                })
                .OrderByDescending(t => t.TaskDate)
                .ToListAsync();

            return Ok(tasks);
        }

        /*
            =====================================================
            65) API لعرض مهمة يومية واحدة بواسطة Id
            =====================================================

            هذا الـ endpoint يعرض مهمة يومية واحدة بالتفصيل
        */
        [HttpGet("get-daily-task/{id}")]
        public async Task<IActionResult> GetDailyTaskById(int id)
        {
            var task = await _context.DailyTasks
                .Include(t => t.Student)
                    .ThenInclude(s => s.User)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    StudentId = t.StudentId,
                    StudentName = t.Student != null && t.Student.User != null ? t.Student.User.FullName : null,
                    StudentNumber = t.Student != null ? t.Student.StudentNumber : null,
                    t.Title,
                    t.Description,
                    t.TaskDate,
                    t.IsCompleted
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound("المهمة اليومية غير موجودة.");
            }

            return Ok(task);
        }

        /*
            =====================================================
            66) API لعرض المهام اليومية لطالب معين
            =====================================================

            هذا الـ endpoint يعرض جميع المهام اليومية
            الخاصة بطالب محدد
        */
        [HttpGet("get-daily-tasks-by-student/{studentId}")]
        public async Task<IActionResult> GetDailyTasksByStudent(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var tasks = await _context.DailyTasks
                .Where(t => t.StudentId == studentId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.TaskDate,
                    t.IsCompleted
                })
                .OrderByDescending(t => t.TaskDate)
                .ToListAsync();

            return Ok(new
            {
                StudentId = student.Id,
                StudentName = student.User != null ? student.User.FullName : null,
                StudentNumber = student.StudentNumber,
                DailyTasks = tasks
            });
        }

        /*
            =====================================================
            67) API لتعديل مهمة يومية
            =====================================================

            هذا الـ endpoint يسمح بتعديل:
            - الطالب
            - عنوان المهمة
            - الوصف
            - التاريخ
            - حالة الإنجاز
        */
        [HttpPut("update-daily-task/{id}")]
        public async Task<IActionResult> UpdateDailyTask(int id, UpdateDailyTaskDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن المهمة
            var task = await _context.DailyTasks.FindAsync(id);

            if (task == null)
            {
                return NotFound("المهمة اليومية غير موجودة.");
            }

            // البحث عن الطالب
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            // تعديل المهمة
            task.StudentId = student.Id;
            task.Title = dto.Title.Trim();
            task.Description = dto.Description;
            task.TaskDate = dto.TaskDate;
            task.IsCompleted = dto.IsCompleted;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل المهمة اليومية بنجاح"
            });
        }

        /*
            =====================================================
            68) API لحذف مهمة يومية
            =====================================================

            هذا الـ endpoint يقوم بحذف مهمة يومية
            بواسطة Id
        */
        [HttpDelete("delete-daily-task/{id}")]
        public async Task<IActionResult> DeleteDailyTask(int id)
        {
            var task = await _context.DailyTasks
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound("المهمة اليومية غير موجودة.");
            }

            _context.DailyTasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف المهمة اليومية بنجاح"
            });
        }


        /*
     =====================================================
     API لحذف مستخدم
     =====================================================

     هذه الميثود تقوم بحذف المستخدم حسب نوعه:

     إذا كان طالبًا:
     - تحذف درجاته
     - حضوره وغيابه
     - ملاحظاته
     - مهامه اليومية
     - سجل الطالب
     - حساب المستخدم

     إذا كان مدرسًا:
     - لا يتم حذفه إذا كان مرتبطًا بتكليفات تدريسية
     حتى لا يحدث خطأ في العلاقات

     إذا كان مديرًا:
     - لا يتم حذف آخر مدير في النظام
 */
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            /*
                حماية:
                لا نحذف آخر مدير في النظام
            */
            if (user.RoleId == 1)
            {
                var adminsCount = await _context.Users
                    .CountAsync(u => u.RoleId == 1 && u.Id != id);

                if (adminsCount == 0)
                {
                    return BadRequest("لا يمكن حذف آخر مدير في النظام.");
                }
            }

            /*
                إذا كان المستخدم طالبًا
                نحذف البيانات المرتبطة به أولًا
            */
            if (user.Student != null)
            {
                var studentId = user.Student.Id;

                var grades = await _context.Grades
                    .Where(g => g.StudentId == studentId)
                    .ToListAsync();

                _context.Grades.RemoveRange(grades);

                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == studentId)
                    .ToListAsync();

                _context.Attendances.RemoveRange(attendances);

                var notes = await _context.StudentNotes
                    .Where(n => n.StudentId == studentId)
                    .ToListAsync();

                _context.StudentNotes.RemoveRange(notes);

                var dailyTasks = await _context.DailyTasks
                    .Where(t => t.StudentId == studentId)
                    .ToListAsync();

                _context.DailyTasks.RemoveRange(dailyTasks);

                _context.Students.Remove(user.Student);
            }

            /*
                إذا كان المستخدم مدرسًا
                لا نحذفه إذا كان مرتبطًا بتكليفات
            */
            if (user.Teacher != null)
            {
                var hasAssignments = await _context.TeacherAssignments
                    .AnyAsync(a => a.TeacherId == user.Teacher.Id);

                if (hasAssignments)
                {
                    return BadRequest("لا يمكن حذف المدرس لأنه مرتبط بتكليفات تدريسية. احذف التكليفات أولًا.");
                }

                _context.Teachers.Remove(user.Teacher);
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف المستخدم بنجاح."
            });
        }


        /*
    =====================================================
    API إحصائيات لوحة المدير
    =====================================================
*/
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var studentsCount = await _context.Students.CountAsync();
            var teachersCount = await _context.Teachers.CountAsync();
            var subjectsCount = await _context.Subjects.CountAsync();
            var classRoomsCount = await _context.ClassRooms.CountAsync();
            var sectionsCount = await _context.Sections.CountAsync();
            var assignmentsCount = await _context.TeacherAssignments.CountAsync();
            var lessonsCount = await _context.Lessons.CountAsync();
            var homeworksCount = await _context.Homeworks.CountAsync();
            var attendancesCount = await _context.Attendances.CountAsync();
            var gradesCount = await _context.Grades.CountAsync();

            return Ok(new
            {
                Students = studentsCount,
                Teachers = teachersCount,
                Subjects = subjectsCount,
                ClassRooms = classRoomsCount,
                Sections = sectionsCount,
                Assignments = assignmentsCount,
                Lessons = lessonsCount,
                Homeworks = homeworksCount,
                Attendances = attendancesCount,
                Grades = gradesCount
            });
        }



        /*
     =====================================================
     API عرض كل الجدول الدراسي
     =====================================================

     يعرض كل الحصص مع:
     - اليوم
     - رقم الحصة
     - المادة
     - المدرس
     - الصف
     - الشعبة

     مهم:
     نرجع ClassRoomId و SectionId أيضًا
     حتى نستفيد منهم في التعديل والفلترة لاحقًا.
 */
        [HttpGet("get-school-schedules")]
        public async Task<IActionResult> GetSchoolSchedules()
        {
            var schedules = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Select(s => new
                {
                    s.Id,
                    s.TeacherAssignmentId,

                    /*
                        أرقام الصف والشعبة
                        مهمة لصفحات التعديل والفلترة.
                    */
                    ClassRoomId = s.TeacherAssignment.ClassRoomId,
                    SectionId = s.TeacherAssignment.SectionId,

                    s.DayName,
                    s.PeriodNumber,
                    s.StartTime,
                    s.EndTime,
                    s.RoomName,
                    s.Notes,

                    SubjectName = s.TeacherAssignment.Subject.Name,
                    ClassRoomName = s.TeacherAssignment.ClassRoom.Name,
                    SectionName = s.TeacherAssignment.Section.Name,
                    TeacherName = s.TeacherAssignment.Teacher.User != null
                        ? s.TeacherAssignment.Teacher.User.FullName
                        : ""
                })
                .OrderBy(s => s.DayName)
                .ThenBy(s => s.PeriodNumber)
                .ToListAsync();

            return Ok(schedules);
        }

        /*
            =====================================================
            API عرض جدول صف وشعبة
            =====================================================

            يستخدم لاحقًا في بوابة الطالب
            حتى يرى الطالب جدول صفه وشعبته فقط.
        */
        [HttpGet("get-school-schedules-by-class-section")]
        public async Task<IActionResult> GetSchoolSchedulesByClassSection(int classRoomId, int sectionId)
        {
            var schedules = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Where(s =>
                    s.TeacherAssignment.ClassRoomId == classRoomId &&
                    s.TeacherAssignment.SectionId == sectionId)
                .Select(s => new
                {
                    s.Id,
                    s.TeacherAssignmentId,
                    s.DayName,
                    s.PeriodNumber,
                    s.StartTime,
                    s.EndTime,
                    s.RoomName,
                    s.Notes,

                    SubjectName = s.TeacherAssignment.Subject.Name,
                    ClassRoomName = s.TeacherAssignment.ClassRoom.Name,
                    SectionName = s.TeacherAssignment.Section.Name,
                    TeacherName = s.TeacherAssignment.Teacher.User != null
                        ? s.TeacherAssignment.Teacher.User.FullName
                        : ""
                })
                .OrderBy(s => s.DayName)
                .ThenBy(s => s.PeriodNumber)
                .ToListAsync();

            return Ok(schedules);
        }

        /*
    =====================================================
    API عرض حصة واحدة من الجدول الدراسي
    =====================================================

    نستخدم هذا الأكشن في صفحة تعديل الجدول الدراسي.

    مهم:
    نرجع ClassRoomId و SectionId
    حتى تظهر القوائم في صفحة التعديل بالقيم الصحيحة.
*/
        [HttpGet("get-school-schedule/{id}")]
        public async Task<IActionResult> GetSchoolSchedule(int id)
        {
            var schedule = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.TeacherAssignmentId,

                    /*
                        هذه مهمة جدًا لصفحة التعديل
                    */
                    ClassRoomId = s.TeacherAssignment.ClassRoomId,
                    SectionId = s.TeacherAssignment.SectionId,

                    s.DayName,
                    s.PeriodNumber,
                    s.StartTime,
                    s.EndTime,
                    s.RoomName,
                    s.Notes,

                    SubjectName = s.TeacherAssignment.Subject.Name,
                    ClassRoomName = s.TeacherAssignment.ClassRoom.Name,
                    SectionName = s.TeacherAssignment.Section.Name,
                    TeacherName = s.TeacherAssignment.Teacher.User != null
                        ? s.TeacherAssignment.Teacher.User.FullName
                        : ""
                })
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound("الحصة غير موجودة.");
            }

            return Ok(schedule);
        }

        /*
            =====================================================
            API جلب التكليفات حسب الصف والشعبة
            =====================================================

            يستخدم في صفحة إضافة الجدول:
            المدير يختار الصف والشعبة
            ثم تظهر فقط التكليفات الخاصة بهذا الصف وهذه الشعبة.
        */
        [HttpGet("get-assignments-by-class-section")]
        public async Task<IActionResult> GetAssignmentsByClassSection(int classRoomId, int sectionId)
        {
            var assignments = await _context.TeacherAssignments
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Where(a => a.ClassRoomId == classRoomId && a.SectionId == sectionId)
                .Select(a => new
                {
                    a.Id,
                    SubjectName = a.Subject.Name,
                    ClassRoomName = a.ClassRoom.Name,
                    SectionName = a.Section.Name,
                    TeacherName = a.Teacher.User != null ? a.Teacher.User.FullName : ""
                })
                .OrderBy(a => a.SubjectName)
                .ToListAsync();

            return Ok(assignments);
        }

        /*
            =====================================================
            API إضافة حصة للجدول الدراسي
            =====================================================

            الحماية:
            - لا نسمح بتكرار نفس الصف والشعبة في نفس اليوم ونفس الحصة.
            - لا نسمح لنفس المدرس أن يكون له حصتان في نفس اليوم ونفس الحصة.
        */
        [HttpPost("create-school-schedule")]
        public async Task<IActionResult> CreateSchoolSchedule(CreateSchoolScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignment = await _context.TeacherAssignments
                .Include(a => a.Teacher)
                .FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId);

            if (assignment == null)
            {
                return NotFound("التكليف التدريسي غير موجود.");
            }

            /*
                منع تكرار نفس الحصة لنفس الصف والشعبة
            */
            var classSectionConflict = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                .AnyAsync(s =>
                    s.DayName == dto.DayName &&
                    s.PeriodNumber == dto.PeriodNumber &&
                    s.TeacherAssignment.ClassRoomId == assignment.ClassRoomId &&
                    s.TeacherAssignment.SectionId == assignment.SectionId);

            if (classSectionConflict)
            {
                return BadRequest("يوجد حصة مسجلة لنفس الصف والشعبة في نفس اليوم ونفس رقم الحصة.");
            }

            /*
                منع أن يكون المدرس في مكانين في نفس الحصة
            */
            var teacherConflict = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                .AnyAsync(s =>
                    s.DayName == dto.DayName &&
                    s.PeriodNumber == dto.PeriodNumber &&
                    s.TeacherAssignment.TeacherId == assignment.TeacherId);

            if (teacherConflict)
            {
                return BadRequest("هذا المدرس لديه حصة أخرى في نفس اليوم ونفس رقم الحصة.");
            }

            var schedule = new SchoolSchedule
            {
                TeacherAssignmentId = dto.TeacherAssignmentId,
                DayName = dto.DayName,
                PeriodNumber = dto.PeriodNumber,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                RoomName = dto.RoomName,
                Notes = dto.Notes
            };

            _context.SchoolSchedules.Add(schedule);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الحصة إلى الجدول الدراسي بنجاح."
            });
        }

        /*
            =====================================================
            API تعديل حصة في الجدول الدراسي
            =====================================================
        */
        [HttpPut("update-school-schedule/{id}")]
        public async Task<IActionResult> UpdateSchoolSchedule(int id, UpdateSchoolScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var schedule = await _context.SchoolSchedules
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound("الحصة غير موجودة.");
            }

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId);

            if (assignment == null)
            {
                return NotFound("التكليف التدريسي غير موجود.");
            }

            /*
                منع تكرار نفس الحصة لنفس الصف والشعبة
                مع استثناء الحصة الحالية التي نعدلها.
            */
            var classSectionConflict = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                .AnyAsync(s =>
                    s.Id != id &&
                    s.DayName == dto.DayName &&
                    s.PeriodNumber == dto.PeriodNumber &&
                    s.TeacherAssignment.ClassRoomId == assignment.ClassRoomId &&
                    s.TeacherAssignment.SectionId == assignment.SectionId);

            if (classSectionConflict)
            {
                return BadRequest("يوجد حصة أخرى لنفس الصف والشعبة في نفس اليوم ونفس رقم الحصة.");
            }

            /*
                منع تعارض المدرس مع حصة أخرى.
            */
            var teacherConflict = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                .AnyAsync(s =>
                    s.Id != id &&
                    s.DayName == dto.DayName &&
                    s.PeriodNumber == dto.PeriodNumber &&
                    s.TeacherAssignment.TeacherId == assignment.TeacherId);

            if (teacherConflict)
            {
                return BadRequest("هذا المدرس لديه حصة أخرى في نفس اليوم ونفس رقم الحصة.");
            }

            schedule.TeacherAssignmentId = dto.TeacherAssignmentId;
            schedule.DayName = dto.DayName;
            schedule.PeriodNumber = dto.PeriodNumber;
            schedule.StartTime = dto.StartTime;
            schedule.EndTime = dto.EndTime;
            schedule.RoomName = dto.RoomName;
            schedule.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الحصة بنجاح."
            });
        }

        /*
            =====================================================
            API حذف حصة من الجدول الدراسي
            =====================================================
        */
        [HttpDelete("delete-school-schedule/{id}")]
        public async Task<IActionResult> DeleteSchoolSchedule(int id)
        {
            var schedule = await _context.SchoolSchedules
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound("الحصة غير موجودة.");
            }

            _context.SchoolSchedules.Remove(schedule);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الحصة من الجدول الدراسي بنجاح."
            });
        }


        /*
    =====================================================
    API عدادات الجديد في بوابة الطالب
    =====================================================

    يحسب عدد العناصر الجديدة لكل قسم حسب LastSeenId.

    الأقسام:
    - Lessons
    - Homeworks
    - Attendance
    - Schedule
    - Notes
*/
        [HttpGet("get-student-portal-new-counts/{studentId}")]
        public async Task<IActionResult> GetStudentPortalNewCounts(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.ClassRoom)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            int GetLastSeen(string areaKey)
            {
                return _context.StudentPortalReadStatuses
                    .Where(x => x.StudentId == studentId && x.AreaKey == areaKey)
                    .Select(x => x.LastSeenId)
                    .FirstOrDefault();
            }

            var lessonsLastSeen = GetLastSeen("Lessons");
            var homeworksLastSeen = GetLastSeen("Homeworks");
            var attendanceLastSeen = GetLastSeen("Attendance");
            var scheduleLastSeen = GetLastSeen("Schedule");
            var notesLastSeen = GetLastSeen("Notes");

            /*
                الدروس الجديدة الخاصة بصف وشعبة الطالب
            */
            var lessonsCount = await _context.Lessons
                .Include(l => l.TeacherAssignment)
                .Where(l =>
                    l.Id > lessonsLastSeen &&
                    l.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                    l.TeacherAssignment.SectionId == student.SectionId)
                .CountAsync();

            /*
                الواجبات الجديدة الخاصة بصف وشعبة الطالب
            */
            var homeworksCount = await _context.Homeworks
                .Include(h => h.TeacherAssignment)
                .Where(h =>
                    h.Id > homeworksLastSeen &&
                    h.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                    h.TeacherAssignment.SectionId == student.SectionId)
                .CountAsync();

            /*
                الحضور والغياب الجديد الخاص بالطالب
            */
            var attendanceCount = await _context.Attendances
                .Where(a =>
                    a.Id > attendanceLastSeen &&
                    a.StudentId == studentId)
                .CountAsync();

            /*
                الجدول الدراسي الجديد الخاص بصف وشعبة الطالب
            */
            var scheduleCount = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                .Where(s =>
                    s.Id > scheduleLastSeen &&
                    s.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                    s.TeacherAssignment.SectionId == student.SectionId)
                .CountAsync();

            /*
                الملاحظات الجديدة الخاصة بالطالب
            */
            var notesCount = await _context.StudentNotes
                .Where(n =>
                    n.Id > notesLastSeen &&
                    n.StudentId == studentId)
                .CountAsync();

            return Ok(new
            {
                LessonsCount = lessonsCount,
                HomeworksCount = homeworksCount,
                AttendanceCount = attendanceCount,
                ScheduleCount = scheduleCount,
                NotesCount = notesCount
            });
        }


        /*
    =====================================================
    API تعليم قسم بأنه تمت مشاهدته
    =====================================================

    عند دخول الطالب إلى صفحة:
    - الدروس
    - الواجبات
    - الحضور
    - الجدول
    - الملاحظات

    نحفظ آخر Id موجود في هذا القسم حتى يصبح عداد الجديد = 0.
*/
        [HttpPost("mark-student-portal-area-as-viewed/{studentId}/{areaKey}")]
        public async Task<IActionResult> MarkStudentPortalAreaAsViewed(int studentId, string areaKey)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            int maxId = 0;

            if (areaKey == "Lessons")
            {
                maxId = await _context.Lessons
                    .Include(l => l.TeacherAssignment)
                    .Where(l =>
                        l.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                        l.TeacherAssignment.SectionId == student.SectionId)
                    .Select(l => l.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else if (areaKey == "Homeworks")
            {
                maxId = await _context.Homeworks
                    .Include(h => h.TeacherAssignment)
                    .Where(h =>
                        h.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                        h.TeacherAssignment.SectionId == student.SectionId)
                    .Select(h => h.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else if (areaKey == "Attendance")
            {
                maxId = await _context.Attendances
                    .Where(a => a.StudentId == studentId)
                    .Select(a => a.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else if (areaKey == "Schedule")
            {
                maxId = await _context.SchoolSchedules
                    .Include(s => s.TeacherAssignment)
                    .Where(s =>
                        s.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                        s.TeacherAssignment.SectionId == student.SectionId)
                    .Select(s => s.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else if (areaKey == "Notes")
            {
                maxId = await _context.StudentNotes
                    .Where(n => n.StudentId == studentId)
                    .Select(n => n.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else
            {
                return BadRequest("نوع القسم غير صحيح.");
            }

            var status = await _context.StudentPortalReadStatuses
                .FirstOrDefaultAsync(x =>
                    x.StudentId == studentId &&
                    x.AreaKey == areaKey);

            if (status == null)
            {
                status = new StudentPortalReadStatus
                {
                    StudentId = studentId,
                    AreaKey = areaKey,
                    LastSeenId = maxId,
                    LastViewedAt = DateTime.Now
                };

                _context.StudentPortalReadStatuses.Add(status);
            }
            else
            {
                status.LastSeenId = maxId;
                status.LastViewedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تحديث حالة المشاهدة بنجاح.",
                areaKey,
                lastSeenId = maxId
            });
        }


    }
}