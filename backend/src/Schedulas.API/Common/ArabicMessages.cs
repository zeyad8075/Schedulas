namespace Schedulas.API.Common;

/// <summary>
/// The single place reason codes become Arabic strings, per Architecture
/// §7: Domain/Application never contain Arabic literals, only codes; this
/// dictionary is the translation layer. Centralizing it here means
/// updating wording never touches business logic — exactly the
/// "administrators can edit... without changing code" spirit extended to
/// user-facing messages (Constitution §18).
/// </summary>
public static class ArabicMessages
{
    private static readonly IReadOnlyDictionary<string, string> ReasonCodeToArabic = new Dictionary<string, string>
    {
        // Rule Engine outcomes
        ["RULE_MAX_ACTIVITIES_PER_DAY_EXCEEDED"] = "تم تجاوز الحد الأقصى لعدد الأنشطة المسموح بها في هذا اليوم.",
        ["RULE_MAX_EXAMS_PER_WEEK_EXCEEDED"] = "تم تجاوز الحد الأقصى لعدد الاختبارات المسموح بها في هذا الأسبوع.",
        ["RULE_MIN_DAYS_BEFORE_EXAM_VIOLATED"] = "لا يوجد عدد كافٍ من الأيام الفاصلة بين هذا الاختبار واختبار آخر.",
        ["RULE_NO_ACTIVITY_ON_HOLIDAY"] = "لا يمكن جدولة نشاط في يوم عطلة رسمية.",
        ["RULE_CONFLICT_DETECTED"] = "يوجد نشاط آخر من نفس النوع مجدول لنفس الفصل في هذا التاريخ.",
        ["RULE_PASSED"] = "تم اجتياز القاعدة بنجاح.",

        // Activity state
        ["ACTIVITY_CANNOT_EDIT_CANCELLED"] = "لا يمكن تعديل نشاط تم إلغاؤه.",
        ["ENTITY_NOT_FOUND"] = "العنصر المطلوب غير موجود.",
        ["TENANT_SCOPE_MISMATCH"] = "لا تملك صلاحية الوصول إلى هذا المورد.",
        ["NOT_AUTHENTICATED"] = "يجب تسجيل الدخول أولاً.",
        ["NOT_A_STUDENT_PROFILE"] = "هذا الحساب غير مرتبط بسجل طالب.",
        ["NOT_A_TEACHER_PROFILE"] = "هذا الحساب غير مرتبط بسجل معلم.",
        ["CANNOT_GRANT_ROLE"] = "لا تملك صلاحية منح هذا الدور للمستخدم.",

        // Generic fallbacks
        ["VALIDATION_FAILED"] = "توجد أخطاء في البيانات المدخلة.",
        ["UNEXPECTED_ERROR"] = "حدث خطأ غير متوقع، الرجاء المحاولة لاحقاً.",
    };

    public static string Resolve(string? reasonCode) =>
        reasonCode is not null && ReasonCodeToArabic.TryGetValue(reasonCode, out var message)
            ? message
            : ReasonCodeToArabic["UNEXPECTED_ERROR"];
}
