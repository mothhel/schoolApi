using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.StudentPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        StudentNoteController
        =====================================================

        هذا الكنترولر مسؤول عن عرض ملاحظات الطالب.

        الطالب لا يضيف ولا يعدل ولا يحذف.
        فقط يشاهد الملاحظات الخاصة به.

        عند فتح صفحة الملاحظات:
        يتم تصفير عداد الملاحظات الجديدة في لوحة الطالب.

        مهم:
        تصفير العداد لا يحذف الملاحظات.
        فقط يخبر النظام أن الطالب شاهد الملاحظات الحالية.
    */
    public class StudentNoteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentNoteController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            التحقق من دخول الطالب
            =====================================================

            نحتاج:
            - StudentId
            - RoleName = Student
        */
        private bool IsStudentLoggedIn()
        {
            return HttpContext.Session.GetInt32("StudentId") != null
                   && HttpContext.Session.GetString("RoleName") == "Student";
        }

        /*
            =====================================================
            تجهيز HttpClient مع التوكن
            =====================================================
        */
        private HttpClient CreateClientWithToken()
        {
            var client = _httpClientFactory.CreateClient("SchoolApi");

            var token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        /*
            =====================================================
            تعليم قسم الملاحظات بأنه تمت مشاهدته
            =====================================================

            هذا الرابط يعتمد على التوكن.
            لذلك لا نرسل StudentId في الرابط.

            API المستخدم:
            api/Student/mark-my-area-as-viewed/Notes
        */
        private async Task MarkNotesAsViewed(HttpClient client)
        {
            try
            {
                var response = await client.PostAsync(
                    "api/Student/mark-my-area-as-viewed/Notes",
                    null);

                if (!response.IsSuccessStatusCode)
                {
                    /*
                        لا نوقف عرض الملاحظات إذا فشل تصفير العداد.
                        لأن عرض الملاحظات أهم من العداد.
                    */
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"فشل تصفير عداد الملاحظات: {response.StatusCode} - {error}");
                }
            }
            catch
            {
                /*
                    لا نوقف صفحة الملاحظات إذا حدث خطأ في تصفير العداد.
                */
            }
        }

        /*
            =====================================================
            عرض ملاحظات الطالب
            =====================================================

            طريقة العمل:
            1) نتحقق من دخول الطالب.
            2) نقرأ بيانات الطالب من api/Student/my-profile.
            3) نقرأ الملاحظات من api/Student/my-notes.
            4) نعتبر أن الطالب شاهد الملاحظات الحالية، فيختفي عداد "جديد".
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsStudentLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken();

            /*
                =====================================================
                1) جلب بيانات الطالب
                =====================================================
            */
            var profileResponse = await client.GetAsync("api/Student/my-profile");

            if (profileResponse.IsSuccessStatusCode)
            {
                var profileJson = await profileResponse.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(profileJson))
                {
                    using var profileDoc = JsonDocument.Parse(profileJson);
                    var root = profileDoc.RootElement;

                    ViewBag.StudentName =
                        root.TryGetProperty("fullName", out var nameProp)
                            ? nameProp.GetString()
                            : "غير متوفر";

                    ViewBag.ClassRoomName =
                        root.TryGetProperty("classRoomName", out var classProp)
                            ? classProp.GetString()
                            : "غير متوفر";

                    ViewBag.SectionName =
                        root.TryGetProperty("sectionName", out var secProp)
                            ? secProp.GetString()
                            : "غير متوفر";
                }
            }
            else
            {
                ViewBag.StudentName = "غير متوفر";
                ViewBag.ClassRoomName = "غير متوفر";
                ViewBag.SectionName = "غير متوفر";
            }

            /*
                =====================================================
                2) جلب الملاحظات
                =====================================================

                حسب كودك السابق:
                API يرجع بهذا الشكل:
                {
                    notes: [...]
                }
            */
            var notes = new List<StudentNoteViewModel>();

            var notesResponse = await client.GetAsync("api/Student/my-notes");

            if (notesResponse.IsSuccessStatusCode)
            {
                var json = await notesResponse.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(json))
                {
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("notes", out var notesElement))
                    {
                        notes = JsonSerializer.Deserialize<List<StudentNoteViewModel>>(
                            notesElement.GetRawText(),
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            })
                            ?? new List<StudentNoteViewModel>();
                    }
                }
            }

            /*
                =====================================================
                3) تصفير عداد الملاحظات الجديدة
                =====================================================

                بعد تحميل الملاحظات الخاصة بالطالب،
                نعتبرها تمت مشاهدتها.
                لذلك سيتم تصفير عداد الملاحظات الجديدة.
            */
            await MarkNotesAsViewed(client);

            return View(notes);
        }
    }
}