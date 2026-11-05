# DOTJOB – Website tuyển dụng (mô phỏng TopCV.vn)

Bài tập lớn Lập trình Web (BTL_LTW). Ứng dụng **ASP.NET Core MVC** + **Entity Framework Core (Code First)**, giao diện Bootstrap/jQuery, kết nối giữa ứng viên và nhà tuyển dụng, có quản trị hệ thống.

> Các mục đánh dấu **(cần xác nhận)** được suy ra từ cấu trúc dự án, hãy đối chiếu với `appsettings.json`, `.csproj`, `Program.cs`, `SeedData.cs` rồi sửa lại cho đúng.

---

## 1. Công nghệ & cấu trúc

| Thành phần   | Chi tiết                                                                                 |
|--------------|------------------------------------------------------------------------------------------|
| Framework    | ASP.NET Core MVC trên .NET 10 (`BTL_LTW-DOTJOB.csproj`)                                  |
| ORM          | Entity Framework Core, có sẵn migration `InitDatabase`                                  |
| CSDL         | SQL Server                                                                              |
| Giao diện    | Razor View, Bootstrap, jQuery, jQuery Validation                                        |
| Xác thực     | Đăng nhập/đăng ký và phân quyền theo Role (có trang `AccessDenied`)                     |

```
BTL_LTW-DOTJOB/
├── Areas/
│   ├── Admin/        (Controllers, Views)   ← khu vực Quản trị
│   └── Employer/     (Controllers, Views)   ← khu vực Nhà tuyển dụng
├── Controllers/      Home, Auth, Job, CV, Application   ← khu vực công khai + Ứng viên
├── Data/             AppDbContext, SeedData
├── Migrations/
├── Models/           User, Role, Company, JobPosting, Category, Application
├── Services/         Auth, Job, Application (interface + implementation)
├── ViewComponents/   Menu, CategoryMenu
├── ViewModels/       Auth, Application
├── Views/            Giao diện dùng chung + Ứng viên
└── wwwroot/          css, js, lib, uploads/cvs
```

---

## 2. Clone, cập nhật mã nguồn và chạy ứng dụng

### 2.1. Yêu cầu
- **.NET 10 SDK** (kiểm tra bằng `dotnet --version`)
- **SQL Server** (LocalDB / Express / Docker)
- Công cụ EF Core: `dotnet tool install --global dotnet-ef`
- Visual Studio 2022 trở lên hoặc VS Code

### 2.2. Lấy mã nguồn từ Git

Lần đầu làm việc trên máy mới, clone repository:

```bash
# Clone repository
git clone https://github.com/thanhhngu/BTL_LTW-DOTJOB.git
cd BTL_LTW-DOTJOB
```

Nếu đã clone repository, cập nhật nhánh hiện tại trước khi làm việc:

```bash
# Kiểm tra nhánh và các thay đổi chưa commit
git status
git branch --show-current

# Lưu/commit hoặc stash thay đổi cục bộ trước khi pull
git pull --ff-only
```

Nếu nhóm sử dụng nhánh `candidate`:

```bash
git switch candidate
git pull --ff-only origin candidate
```

### 2.3. Cài đặt và chạy ứng dụng

Chạy các lệnh sau từ thư mục gốc repository (thư mục có `BTL_LTW-DOTJOB.slnx`):

```bash
# Khôi phục package
dotnet restore BTL_LTW-DOTJOB/BTL_LTW-DOTJOB.csproj

# Cấu hình chuỗi kết nối (xem mục 2.4)

# Cập nhật CSDL theo các migration có sẵn
dotnet ef database update --project BTL_LTW-DOTJOB/BTL_LTW-DOTJOB.csproj --startup-project BTL_LTW-DOTJOB/BTL_LTW-DOTJOB.csproj

# Chạy ứng dụng
dotnet run --project BTL_LTW-DOTJOB/BTL_LTW-DOTJOB.csproj
```

Hoặc mở `BTL_LTW-DOTJOB.slnx` bằng Visual Studio và nhấn **F5** (profile nằm ở `BTL_LTW-DOTJOB/Properties/launchSettings.json`). Mở địa chỉ hiển thị ở console, thường là `https://localhost:xxxx`.

> Nếu dùng Package Manager Console của Visual Studio: `Update-Database`.

### 2.4. Cấu hình chuỗi kết nối

Mở `appsettings.json`, sửa mục `ConnectionStrings` cho đúng máy của bạn, ví dụ:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DotJobDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Mỗi thành viên có máy khác nhau: **không commit chuỗi kết nối cá nhân**. Đặt vào `appsettings.Development.json` hoặc dùng User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<chuỗi kết nối>"
```

### 2.5. Dữ liệu mẫu & tài khoản test

Dự án có `Data/SeedData.cs` và `Models/Mock/*` để nạp dữ liệu mẫu (ngành nghề, công ty, tin tuyển dụng…). Dữ liệu seed chạy khi khởi động ứng dụng **(cần xác nhận trong `Program.cs`)**.

| Vai trò        | Email                  | Mật khẩu |
|----------------|------------------------|----------|
| Admin          | *(điền theo SeedData)* | *(điền)* |
| Nhà tuyển dụng | *(điền theo SeedData)* | *(điền)* |
| Ứng viên       | *(điền theo SeedData)* | *(điền)* |

### 2.6. Lỗi thường gặp

| Lỗi                                    | Cách xử lý                                                                      |
|----------------------------------------|---------------------------------------------------------------------------------|
| `Cannot open database` / lỗi kết nối   | Kiểm tra SQL Server đang chạy và chuỗi kết nối                                  |
| `dotnet ef` không nhận                 | Cài `dotnet-ef` (mục 2.1)                                                       |
| Pending model changes                 | Tạo migration mới, sau đó chạy `dotnet ef database update`                     |
| Không upload được CV                   | Đảm bảo tồn tại thư mục `wwwroot/uploads/cvs` và có quyền ghi                   |
| Trang lỗi sau khi pull code mới        | Chạy `dotnet ef database update` để áp dụng migration mới                      |

### 2.7. Lưu ý khi dùng Git

- Luôn pull đúng nhánh đang làm việc; kiểm tra nhánh bằng `git branch --show-current`.
- Nếu có thay đổi cục bộ chưa commit, hãy commit hoặc stash trước khi pull để tránh xung đột.
- Khi xảy ra conflict, xử lý và kiểm tra lại các file trước khi commit; không ghi đè thay đổi của thành viên khác.
- Thêm vào `.gitignore`: `bin/`, `obj/`, `*.csproj.user`, `.vs/`, `wwwroot/uploads/` (file CV người dùng tải lên không nên đưa vào repo).

---

## 3. Quy tắc phân vùng vai trò

Hệ thống có **3 vai trò**: `Admin`, `Employer` (Nhà tuyển dụng), `Candidate` (Ứng viên, tên role chính xác **cần xác nhận** trong `Models/Role.cs`). Khách chưa đăng nhập (Guest) chỉ xem được phần công khai.

### 3.1. Mỗi vai trò một vùng riêng

| Vai trò           | Vùng code                | URL                      | Quyền truy cập                             |
|-------------------|--------------------------|--------------------------|--------------------------------------------|
| Guest + Ứng viên  | `Controllers/`, `Views/` | `/`, `/Job/...`, `/CV/...` | Công khai; thao tác cá nhân cần đăng nhập |
| Nhà tuyển dụng    | `Areas/Employer/`        | `/Employer/...`          | `[Authorize(Roles = "Employer")]`         |
| Quản trị          | `Areas/Admin/`           | `/Admin/...`             | `[Authorize(Roles = "Admin")]`            |

### 3.2. Quy tắc bắt buộc

1. **Đúng Area, đúng Role.** Mỗi controller trong Area phải có `[Area("Admin")]` hoặc `[Area("Employer")]` **và** `[Authorize(Roles = "...")]` ở cấp class, không để cấp action rồi quên.
2. **Mặc định là chặn.** Controller/action cần đăng nhập thì gắn `[Authorize]`; chỉ trang công khai mới dùng `[AllowAnonymous]`.
3. **Không kiểm tra quyền chỉ bằng cách ẩn nút trên View.** Luôn kiểm tra lại ở server (attribute hoặc trong Service).
4. **Kiểm tra quyền sở hữu dữ liệu.** Nhà tuyển dụng chỉ sửa/xóa tin và xem hồ sơ ứng tuyển **của công ty mình**; ứng viên chỉ xem CV và đơn ứng tuyển **của chính mình**. Lấy `UserId` từ `User.Claims`, không nhận từ form/query string.
5. **Không gọi chéo Controller giữa các Area.** Logic dùng chung đặt trong `Services/` (qua interface `IXxxService`, đăng ký DI trong `Program.cs`).
6. **View/ViewModel tách theo Area.** View của Area nằm trong `Areas/<Tên>/Views/`; ViewModel nên đặt `ViewModels/<Area>/` để tránh lẫn.
7. **Layout riêng cho từng Area** (`_Layout` của Admin, của Employer) thay vì dùng chung menu ứng viên.
8. **Chuyển hướng sau đăng nhập theo role:** Admin → `/Admin`, Employer → `/Employer`, Candidate → trang chủ. Sai quyền → `Auth/AccessDenied`.
9. **Tài khoản Admin chỉ tạo bằng seed/Admin khác**, không cho tự đăng ký. Form đăng ký công khai chỉ cho chọn Ứng viên hoặc Nhà tuyển dụng.
10. **Đặt tên thống nhất:** `Controller` số nhiều/danh từ theo chức năng (`JobsController`), route theo Area, không viết trùng tên giữa các Area.

### 3.3. Quy ước làm việc nhóm (gợi ý)
- Mỗi thành viên nhận **một vùng** (Ứng viên / Nhà tuyển dụng / Admin), chỉ sửa trong vùng của mình.
- File dùng chung (`Models/`, `AppDbContext`, `Program.cs`, `Views/Shared/`) thay đổi qua Pull Request và báo cả nhóm.
- Thay đổi Model phải kèm migration, **không sửa migration đã được người khác áp dụng**.
- Nhánh Git: `feature/<vung>-<ten-chuc-nang>`.

---

## 4. Chức năng bắt buộc của từng vai trò (tham chiếu TopCV.vn)

Cột **Hiện trạng** là ước lượng từ cấu trúc code hiện có: ✅ có dấu hiệu đã làm, ⬜ chưa thấy.

### 4.1. Khách (Guest)

| Chức năng                                                                    | Hiện trạng                    |
|------------------------------------------------------------------------------|-------------------------------|
| Xem trang chủ: banner, hero, ô tìm kiếm, việc làm nổi bật, menu ngành nghề   | ✅                             |
| Tìm kiếm việc làm theo từ khóa, địa điểm, ngành nghề                        | ✅ (cần kiểm tra bộ lọc)       |
| Xem chi tiết tin tuyển dụng                                                  | ✅                             |
| Xem trang thông tin công ty                                                  | ⬜                             |
| Đăng ký, đăng nhập                                                           | ✅                             |

### 4.2. Ứng viên (Candidate)

| Chức năng                                                                    | Hiện trạng                                  |
|------------------------------------------------------------------------------|---------------------------------------------|
| Đăng ký/đăng nhập/đăng xuất, quên & đổi mật khẩu                             | ✅ / ⬜ phần mật khẩu                         |
| Quản lý hồ sơ cá nhân (họ tên, SĐT, ảnh, giới thiệu)                         | ⬜                                           |
| Tải lên và quản lý CV (PDF)                                                  | ✅ (`CVController`)                          |
| Ứng tuyển tin tuyển dụng (chọn CV, thư giới thiệu)                           | ✅ (`ApplicationController`, `_ApplyModal`) |
| Lọc nâng cao: mức lương, kinh nghiệm, cấp bậc, hình thức làm việc            | ⬜                                           |
| Lưu tin tuyển dụng yêu thích / bỏ lưu                                        | ⬜                                           |
| Xem lịch sử ứng tuyển và trạng thái                                          | ⬜                                           |
| Theo dõi công ty                                                             | ⬜                                           |
| Gợi ý việc làm phù hợp                                                       | ⬜                                           |
| Thông báo khi trạng thái hồ sơ thay đổi                                      | ⬜                                           |

### 4.3. Nhà tuyển dụng (Employer)

| Chức năng                                                                    | Hiện trạng                 |
|------------------------------------------------------------------------------|----------------------------|
| Đăng ký tài khoản, tạo/sửa hồ sơ công ty                                     | ⬜ (Model `Company` đã có)  |
| Dashboard thống kê: số tin, lượt xem, số hồ sơ nhận được                     | ⬜                          |
| Đăng tin tuyển dụng mới                                                      | ⬜                          |
| Sửa, ẩn/hiện, gia hạn, đóng, xóa tin của mình                                | ⬜                          |
| Xem danh sách ứng viên đã ứng tuyển theo từng tin                            | ⬜                          |
| Xem/tải CV của ứng viên                                                      | ⬜                          |
| Cập nhật trạng thái hồ sơ                                                    | ⬜                          |
| Lọc, tìm kiếm trong danh sách ứng viên                                       | ⬜                          |
| Tìm kiếm hồ sơ ứng viên chủ động (tùy chọn nâng cao)                         | ⬜                          |

### 4.4. Quản trị viên (Admin)

| Chức năng                                                               | Hiện trạng                  |
|-------------------------------------------------------------------------|-----------------------------|
| Đăng nhập khu vực quản trị riêng                                        | ⬜                           |
| Dashboard thống kê toàn hệ thống                                        | ⬜                           |
| Quản lý người dùng: xem, tìm kiếm, khóa/mở khóa, đổi vai trò, xóa       | ⬜                           |
| Duyệt/khóa tài khoản và hồ sơ công ty                                   | ⬜                           |
| Kiểm duyệt tin tuyển dụng: duyệt, từ chối, gỡ tin vi phạm               | ⬜                           |
| Quản lý danh mục ngành nghề (thêm/sửa/xóa)                              | ⬜ (Model `Category` đã có)  |
| Quản lý banner/nội dung trang chủ                                       | ⬜                           |
| Xem toàn bộ đơn ứng tuyển, xử lý báo cáo vi phạm                        | ⬜                           |

### 4.5. Mức ưu tiên đề xuất
1. **Bắt buộc:** đăng ký/đăng nhập phân quyền, đăng/sửa/xóa tin (Employer), duyệt hồ sơ ứng tuyển (Employer), ứng tuyển + lịch sử ứng tuyển (Candidate), quản lý user/danh mục/duyệt tin (Admin).
2. **Nên có:** lưu việc làm, lọc nâng cao, dashboard thống kê, trang công ty.
3. **Nếu còn thời gian:** theo dõi công ty, gợi ý việc làm, thông báo, tìm kiếm hồ sơ ứng viên.

---

## 5. Thông tin nhóm
- Môn học: Lập trình Web
- Thành viên & phân công: *(điền)*
- Giảng viên: *(điền)*
