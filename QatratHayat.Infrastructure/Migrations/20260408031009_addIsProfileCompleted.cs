using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addIsProfileCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProfileCompleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "ScreeningQuestions",
                columns: new[] { "Id", "AdditionalTextLabelAr", "AdditionalTextLabelEn", "ConditionalDateLabelAr", "ConditionalDateLabelEn", "CreatedAt", "DecisionMode", "DeferralPeriodDays", "DeferralType", "DisplayOrder", "IsActive", "IsForFemaleOnly", "RequiresAdditionalText", "RequiresDateValue", "SessionType", "TextAr", "TextEn" },
                values: new object[,]
                {
                    { 1, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 3, 1, true, false, false, false, 1, "هل سبق لك تبرعت بالدم؟", "Have you donated blood before?" },
                    { 2, "اذكر ما حدث", "Please specify what happened", null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 2, true, false, true, false, 1, "هل حدثت لك ردود فعل نتيجة التبرع مثل الدوخة أو الإغماء أو غيرها؟", "Have you ever experienced reactions after blood donation such as dizziness, fainting, or others?" },
                    { 3, "اذكر اسم المرض", "Please specify the condition", null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 3, true, false, true, false, 1, "هل أنت مصاب بأمراض الدم الوراثية مثل الثلاسيميا أو فقر الدم المنجلي أو فقر الدم المزمن أو غيرها؟", "Do you have hereditary blood disorders such as thalassemia, sickle cell disease, chronic anemia, or others?" },
                    { 4, "اذكر اسم المرض", "Please specify the condition", null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 4, true, false, true, false, 1, "هل أنت مصاب بأمراض نزف الدم أو أي نقص خلقي في أحد عوامل التخثر مثل الهيموفيليا أو غيرها؟", "Do you have bleeding disorders or congenital clotting factor deficiencies such as hemophilia or others?" },
                    { 5, "اذكر اسم المرض", "Please specify the condition", null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 5, true, false, true, false, 1, "هل أنت مصاب بأمراض المناعة الذاتية مثل مرض بهجت أو الذئبة أو حمى البحر الأبيض المتوسط أو الروماتيزم أو الصدفية أو البهاق أو الثعلبة أو غيرها؟", "Do you have autoimmune diseases such as Behçet’s disease, lupus, familial Mediterranean fever, rheumatism, psoriasis, vitiligo, alopecia, or others?" },
                    { 6, "اذكر نوع الحساسية", "Please specify the allergy", null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 6, true, false, true, false, 1, "هل أنت مصاب بأمراض الحساسية المزمنة والشديدة؟", "Do you have severe or chronic allergic conditions?" },
                    { 7, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 7, true, false, false, false, 1, "هل تشكو من نوبات إغماء أو تشنج؟", "Do you suffer from fainting episodes or seizures?" },
                    { 8, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 3, 8, true, false, false, false, 1, "هل تسافر كثيرًا إلى الخارج؟", "Do you travel abroad frequently?" },
                    { 101, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 1, true, false, false, false, 2, "هل صحتك اليوم ليست جيدة؟", "Are you feeling unwell today?" },
                    { 102, null, null, "تاريخ الإصابة أو انتهاء العلاج", "Date of illness or completion of treatment", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1095, 1, 2, true, false, false, true, 2, "هل سبق وأن أُصبت بالملاريا خلال الثلاث سنوات الماضية؟", "Have you had malaria during the past 3 years?" },
                    { 103, null, null, "تاريخ العودة من السفر", "Date of return from travel", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, 90, 1, 3, true, false, false, true, 2, "هل سافرت مؤخرًا إلى منطقة يتفشى فيها مرض الملاريا؟", "Have you recently traveled to an area where malaria is endemic?" },
                    { 104, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 4, true, false, false, false, 2, "هل سبق وأن أُصبت بالحمى المالطية أو حمى التيفوئيد؟", "Have you ever had brucellosis or typhoid fever?" },
                    { 105, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 5, true, false, false, false, 2, "هل سبق وأن أُصبت باليرقان؟", "Have you ever had jaundice?" },
                    { 106, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 6, true, false, false, false, 2, "هل سبق وأن أُصبت بالتهاب الكبد الفيروسي؟", "Have you ever had viral hepatitis?" },
                    { 107, null, null, "تاريخ آخر مخالطة", "Date of last contact", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, 365, 1, 7, true, false, false, true, 2, "هل خالطت شخصًا مصابًا بالتهاب الكبد الفيروسي خلال الاثني عشر شهرًا الماضية؟", "Have you had close contact with someone with viral hepatitis during the past 12 months?" },
                    { 108, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, 2, 8, true, false, false, false, 2, "هل أنت مصاب بأحد الأمراض المعدية التي تنتقل عن طريق الدم مثل الإيدز أو السفلس؟", "Do you have any blood-borne infectious diseases such as HIV/AIDS or syphilis?" },
                    { 109, "اذكر الحالة المرضية", "Please specify the condition", null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 9, true, false, true, false, 2, "هل أنت مصاب بأي من أمراض الرئة أو القلب أو الكلى أو السكري المعتمد على الإنسولين أو ارتفاع ضغط الدم؟", "Do you have any of the following conditions: lung disease, heart disease, kidney disease, insulin-dependent diabetes, or high blood pressure?" },
                    { 110, "اذكر اسم الدواء", "Please specify the medication", null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 10, true, false, true, false, 2, "هل تتناول أدوية لأي مرض؟", "Are you taking any medications for any illness?" },
                    { 111, null, null, "تاريخ آخر جرعة", "Date of last dose", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 11, true, false, false, true, 2, "هل تناولت أسبرين أو أي دواء يحتوي على مكونات الأسبرين خلال الثلاثة أيام الماضية؟", "Have you taken aspirin or any medication containing aspirin during the past 3 days?" },
                    { 112, "اذكر اسم الدواء", "Please specify the medication", "تاريخ آخر جرعة", "Date of last dose", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 12, true, false, true, true, 2, "هل تناولت أي مسكنات للألم أو مضادات للالتهاب خلال الثلاثة أيام الماضية؟", "Have you taken any painkillers or anti-inflammatory medications during the past 3 days?" },
                    { 113, null, null, "تاريخ الإجراء", "Date of the procedure", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 13, true, false, false, true, 2, "هل خلعت سنًا أو أجريت علاجًا للأسنان خلال السبعة أيام الماضية؟", "Have you had a tooth extraction or dental treatment during the past 7 days?" },
                    { 114, null, null, "تاريخ الإجراء", "Date of the procedure", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, 365, 1, 14, true, false, false, true, 2, "هل تعرضت لثقب بالأذن أو الجلد أو للعلاج بالإبر الصينية خلال الاثني عشر شهرًا الماضية؟", "Have you had ear/body piercing or acupuncture during the past 12 months?" },
                    { 115, "اذكر نوع العملية", "Please specify the type of surgery", "تاريخ العملية", "Date of surgery", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 15, true, false, true, true, 2, "هل أُجريت لك عملية جراحية خلال الاثني عشر شهرًا الماضية؟", "Have you had surgery during the past 12 months?" },
                    { 116, null, null, "تاريخ نقل الدم", "Date of transfusion", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, 365, 1, 16, true, false, false, true, 2, "هل تلقيت نقل دم أو مشتقاته خلال الاثني عشر شهرًا الماضية؟", "Have you received a blood transfusion or blood products during the past 12 months?" },
                    { 117, null, null, "تاريخ الحجامة", "Date of cupping", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 17, true, false, false, true, 2, "هل عملت حجامة خلال الثمانية أشهر الماضية؟", "Have you had cupping therapy during the past 8 months?" },
                    { 118, null, null, "تاريخ الوشم", "Date of tattoo", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, 365, 1, 18, true, false, false, true, 2, "هل عملت وشمًا خلال الاثني عشر شهرًا الماضية؟", "Have you had a tattoo during the past 12 months?" },
                    { 119, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, 2, 19, true, false, false, false, 2, "هل استخدمت إبرًا لأخذ أي نوع من المخدرات عن طريق الوريد أو تحت الجلد؟", "Have you ever used needles to take non-prescribed drugs intravenously or under the skin?" },
                    { 120, "اذكر اسم المطعوم", "Please specify the vaccine", "تاريخ المطعوم", "Date of vaccination", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 20, true, false, true, true, 2, "هل أخذت أي مطعوم خلال الأربعة أسابيع الماضية؟", "Have you received any vaccine during the past 4 weeks?" },
                    { 121, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, 1, 21, true, true, false, false, 2, "للنساء: هل أنتِ حامل الآن؟", "For women: Are you currently pregnant?" },
                    { 122, null, null, "تاريخ آخر حمل أو إجهاض", "Date of last pregnancy or miscarriage", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 2, 270, 1, 22, true, true, false, true, 2, "للنساء: هل حدث حمل أو إجهاض خلال الأشهر التسعة الماضية؟", "For women: Have you been pregnant or had a miscarriage during the past 9 months?" },
                    { 123, null, null, "تاريخ انتهاء الرضاعة الطبيعية", "Date breastfeeding ended", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 23, true, true, false, true, 2, "للنساء: هل انتهت الرضاعة الطبيعية قبل أقل من ثلاثة أشهر؟", "For women: Did breastfeeding end less than 3 months ago?" },
                    { 124, null, null, null, null, new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 3, 24, true, true, false, false, 2, "للنساء: هل تعانين الآن من الدورة الشهرية مع زيادة في كمية الدم المفقودة؟", "For women: Are you currently menstruating with increased blood loss?" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DropColumn(
                name: "IsProfileCompleted",
                table: "AspNetUsers");
        }
    }
}
