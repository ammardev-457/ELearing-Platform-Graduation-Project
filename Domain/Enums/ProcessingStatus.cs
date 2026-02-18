namespace ELProject.Domain.Enums
{
    public enum ProcessingStatus
    {
        Pending,    // جاري الرفع
        Processing, // السيرفر بيعمل Transcoding (تقطيع الفيديو)
        Ready,      // جاهز للمشاهدة
        Failed      // حصل خطأ في الرفع
    }

}