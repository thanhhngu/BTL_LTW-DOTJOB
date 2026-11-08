using Microsoft.Extensions.Localization;

// Khai báo cho thư viện bản địa hóa biết namespace gốc của file tài nguyên. Cần thiết vì tên assembly
// (BTL_LTW-DOTJOB, có dấu gạch ngang) khác namespace gốc (BTL_LTW_DOTJOB)
[assembly: RootNamespace("BTL_LTW_DOTJOB")]

namespace BTL_LTW_DOTJOB
{
    // Lớp rỗng làm "đại diện" cho file Resources/SharedResource.resx (tiếng Việt, mặc định)
    // và Resources/SharedResource.en.resx (tiếng Anh). View dùng: @inject IStringLocalizer<SharedResource> Loc
    public class SharedResource
    {
    }
}
