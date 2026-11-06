# DOTJOB – Website tuyển dụng (mô phỏng TopCV.vn)

Bài tập lớn Lập trình Web (BTL_LTW). Ứng dụng **ASP.NET Core MVC** + **Entity Framework Core (Code First)**, giao diện Bootstrap/jQuery, kết nối giữa ứng viên và nhà tuyển dụng, có quản trị hệ thống.

---

## 1. Công nghệ & cấu trúc

| Thành phần   | Chi tiết                                                                                       |
|--------------|------------------------------------------------------------------------------------------------|
| Framework    | ASP.NET Core MVC trên .NET 10 (`BTL_LTW-DOTJOB.csproj`)                                        |
| ORM          | Entity Framework Core 10 (Code First), migration `InitDatabase`, `AddUniqueIndexes`           |
| CSDL         | SQL Server                                                                                     |
| Giao diện    | Razor View, Bootstrap 5, jQuery, jQuery Validation, Font Awesome, SweetAlert2 (CDN)           |
| Xác thực     | Cookie Authentication, mật khẩu băm **BCrypt** (`BCrypt.Net-Next`), phân quyền theo Role      |
| Phân trang   | `X.PagedList`                                                                                  |

```
BTL_LTW-DOTJOB/
├── Areas/
│   ├── Admin/        (Controllers, Views)   ← khu vực Quản trị
│   └── Employer/     (Controllers, Views)   ← khu vực Nhà tuyển dụng
├── Controllers/      Home, Auth, Job, CV, Application, CvFile   ← khu vực công khai + Ứng viên
├── Data/             AppDbContext, SeedData
├── Migrations/
├── Models/           User, Role, Company, JobPosting, Category, Application
├── Services/         Auth, Job, Application (interface + implementation)
├── ViewComponents/   Menu, CategoryMenu
├── ViewModels/       Auth, Application
├── Views/            Giao diện dùng chung + Ứng viên
├── App_Data/cvs/     CV ứng viên tải lên (tự tạo khi chạy, KHÔNG đưa lên Git)
└── wwwroot/          css, js, lib
```

---

## 2. Clone, cập nhật mã nguồn và chạy ứng dụng

### 2.1. Yêu cầu
- **.NET 10 SDK** (kiểm tra bằng `dotnet --version`)
- **SQL Server** (LocalDB / Express / Docker); có thể dùng SQL Server Management Studio (SSMS) để xem dữ liệu
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

# Cập nhật CSDL theo các migration có sẵn (BẮT BUỘC chạy trước khi chạy ứng dụng)
dotnet ef database update --project BTL_LTW-DOTJOB/BTL_LTW-DOTJOB.csproj --startup-project BTL_LTW-DOTJOB/BTL_LTW-DOTJOB.csproj

# Chạy ứng dụng
dotnet run --project BTL_LTW-DOTJOB/BTL_LTW-DOTJOB.csproj
```

Hoặc mở `BTL_LTW-DOTJOB.slnx` bằng Visual Studio và nhấn **F5**. Mở địa chỉ hiển thị ở console, thường là `https://localhost:xxxx`.

> Nếu dùng Package Manager Console của Visual Studio (chọn Default project là `BTL_LTW-DOTJOB`): `Update-Database`.

### 2.4. Cấu hình chuỗi kết nối

Chuỗi kết nối mặc định nằm trong `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=DotJobDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Nếu máy bạn dùng SQL Server khác (LocalDB, Express, tài khoản `sa`...), ví dụ LocalDB:

```
Server=(localdb)\\MSSQLLocalDB;Database=DotJobDB;Trusted_Connection=True;TrustServerCertificate=True
```

Mỗi thành viên có máy khác nhau: **không commit chuỗi kết nối cá nhân**. Dùng User Secrets (lần đầu cần `init` vì project chưa có `UserSecretsId`):

```bash
dotnet user-secrets init --project BTL_LTW-DOTJOB/BTL_LTW-DOTJOB.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<chuỗi kết nối>" --project BTL_LTW-DOTJOB/BTL_LTW-DOTJOB.csproj
```

### 2.5. Dữ liệu mẫu & tài khoản test

`Data/SeedData.cs` được gọi trong `Program.cs` mỗi lần ứng dụng khởi động (chỉ thêm dữ liệu khi bảng tương ứng còn trống, **cần chạy `database update` trước**). Dữ liệu mẫu gồm: 3 role, 30 ngành nghề, 1 Admin, 30 Nhà tuyển dụng (kèm 30 công ty), 10 Ứng viên, 30 tin tuyển dụng, 30 đơn ứng tuyển.

Mật khẩu chung của mọi tài khoản mẫu: **`123456`** (chỉ dùng khi phát triển).

| Vai trò        | Email                                                            | Mật khẩu |
|----------------|------------------------------------------------------------------|----------|
| Admin          | `admin@dotjob.vn`                                                | `123456` |
| Nhà tuyển dụng | `employer1@company1.com` … `employer30@company30.com`           | `123456` |
| Ứng viên       | `candidate1@gmail.com` … `candidate10@gmail.com`                 | `123456` |

> Các đơn ứng tuyển mẫu trỏ tới file CV không có thật, nên nút "Xem CV đã nộp" của các đơn này sẽ báo 404. Đơn do bạn tự nộp thì xem được.
>
> Muốn có lại dữ liệu mẫu sạch: xóa database (`dotnet ef database drop`) rồi chạy `database update` và khởi động lại.

### 2.6. Lỗi thường gặp

| Lỗi                                      | Cách xử lý                                                                                  |
|------------------------------------------|---------------------------------------------------------------------------------------------|
| `Cannot open database` / lỗi kết nối     | Kiểm tra SQL Server đang chạy và chuỗi kết nối                                              |
| Ứng dụng báo lỗi ngay khi khởi động (seed) | Chưa tạo CSDL: chạy `dotnet ef database update` rồi chạy lại                              |
| `dotnet ef` không nhận                   | Cài `dotnet-ef` (mục 2.1)                                                                   |
| Pending model changes                    | Tạo migration mới, sau đó chạy `dotnet ef database update`                                  |
| `database update` báo trùng khóa/index   | Dữ liệu cũ bị trùng Email hoặc trùng (tin, ứng viên) trong bảng `Application`: xóa bản ghi trùng rồi chạy lại |
| Không upload được CV                     | Đảm bảo ứng dụng có quyền ghi vào `App_Data/cvs` (thư mục tự tạo khi nộp CV đầu tiên)       |
| Đăng nhập sai quyền / lỗi sau khi pull code auth mới | Xóa cookie `DotJobAuthCookie` (hoặc đăng xuất) rồi đăng nhập lại               |
| Trang lỗi sau khi pull code mới          | Chạy `dotnet ef database update` để áp dụng migration mới                                   |

### 2.7. Lưu ý khi dùng Git

- Luôn pull đúng nhánh đang làm việc; kiểm tra nhánh bằng `git branch --show-current`.
- Nếu có thay đổi cục bộ chưa commit, hãy commit hoặc stash trước khi pull để tránh xung đột.
- Khi xảy ra conflict, xử lý và kiểm tra lại các file trước khi commit; không ghi đè thay đổi của thành viên khác.
- Thêm vào `.gitignore`: `bin/`, `obj/`, `*.csproj.user`, `.vs/`, `App_Data/`, `wwwroot/uploads/` (file CV người dùng tải lên chứa dữ liệu cá nhân, không đưa vào repo).

---

## 3. Quy tắc phân vùng vai trò

Hệ thống có **3 vai trò**: `Admin`, `Employer` (Nhà tuyển dụng), `Candidate` (Ứng viên). Tên role đúng như trên (phân biệt chữ hoa/thường khi dùng trong `[Authorize(Roles = "...")]`). Khách chưa đăng nhập (Guest) chỉ xem được phần công khai.

### 3.1. Mỗi vai trò một vùng riêng

| Vai trò           | Vùng code                | URL                                                   | Quyền truy cập                                   |
|-------------------|--------------------------|-------------------------------------------------------|--------------------------------------------------|
| Guest             | `Controllers/`, `Views/` | `/`, `/jobs/{id}`, `/Auth/...`                        | Công khai (`[AllowAnonymous]`)                   |
| Ứng viên          | `Controllers/`, `Views/` | `/applications`, `/cv/create`, ứng tuyển (`/Application/Apply`) | `[Authorize(Roles = "Candidate")]`     |
| Xem CV đã nộp     | `Controllers/`           | `/cv-files/{applicationId}`                           | Đăng nhập; chỉ Ứng viên chủ đơn, Nhà tuyển dụng sở hữu tin, hoặc Admin |
| Nhà tuyển dụng    | `Areas/Employer/`        | `/Employer/...`                                       | `[Authorize(Roles = "Employer")]`                |
| Quản trị          | `Areas/Admin/`           | `/Admin/...`                                          | `[Authorize(Roles = "Admin")]`                   |

### 3.2. Quy tắc bắt buộc

1. **Đúng Area, đúng Role.** Mỗi controller trong Area phải có `[Area("Admin")]` hoặc `[Area("Employer")]` **và** `[Authorize(Roles = "...")]` ở cấp class, không để cấp action rồi quên.
2. **Mặc định là chặn.** Ứng dụng đã đặt `FallbackPolicy` yêu cầu đăng nhập cho mọi endpoint; chỉ trang công khai mới dùng `[AllowAnonymous]` (hiện là `Home`, `Job`, `Auth`).
3. **Không kiểm tra quyền chỉ bằng cách ẩn nút trên View.** Luôn kiểm tra lại ở server (attribute hoặc trong Service).
4. **Kiểm tra quyền sở hữu dữ liệu.** Nhà tuyển dụng chỉ sửa/xóa tin và xem hồ sơ ứng tuyển **của công ty mình**; ứng viên chỉ xem CV và đơn ứng tuyển **của chính mình**. Lấy `UserId` từ `User.FindFirstValue(ClaimTypes.NameIdentifier)`, không nhận từ form/query string.
5. **Không gọi chéo Controller giữa các Area.** Logic dùng chung đặt trong `Services/` (qua interface `IXxxService`, đăng ký DI trong `Program.cs`).
6. **View/ViewModel tách theo Area.** View của Area nằm trong `Areas/<Tên>/Views/`; ViewModel nên đặt `ViewModels/<Area>/` để tránh lẫn.
7. **Layout riêng cho từng Area** (`_Layout` của Admin, của Employer) thay vì dùng chung menu ứng viên.
8. **Chuyển hướng sau đăng nhập theo role** (cấu hình ở `AuthController.Login`): Admin → `/Admin`, Employer → `/Employer`, Candidate → trang chủ. Sai quyền → `Auth/AccessDenied`. *(Admin và Employer tạm về trang chủ cho đến khi Area có Dashboard.)*
9. **Tài khoản Admin chỉ tạo bằng seed/Admin khác**, không cho tự đăng ký. Form đăng ký công khai chỉ cho chọn Ứng viên hoặc Nhà tuyển dụng (đã kiểm tra ở cả `RegisterViewModel` và `AuthService`).
10. **Đặt tên thống nhất:** Controller đặt tên theo thực thể, số ít, giống code hiện có (`JobController`, `ApplicationController`), route theo Area, không viết trùng tên giữa các Area.
11. **Claim role là tên role** (`Admin` / `Employer` / `Candidate`), không phải `RoleId`, để `[Authorize(Roles = "...")]` hoạt động.
12. **File upload (CV) lưu ngoài `wwwroot`** (`App_Data/cvs`) và chỉ tải qua `CvFileController` có kiểm tra quyền; không để đường dẫn file công khai.

### 3.3. Quy ước làm việc nhóm (gợi ý)
- Mỗi thành viên nhận **một vùng** (Ứng viên / Nhà tuyển dụng / Admin), chỉ sửa trong vùng của mình.
- File dùng chung (`Models/`, `AppDbContext`, `Program.cs`, `Views/Shared/`) thay đổi qua Pull Request và báo cả nhóm.
- Thay đổi Model phải kèm migration, **không sửa migration đã được người khác áp dụng**.
- Nhánh Git: `feature/<vung>-<ten-chuc-nang>`.

---

## 4. Chức năng bắt buộc của từng vai trò (tham chiếu TopCV.vn)

Cột **Hiện trạng**: ✅ đã làm, ⚠ làm một phần, ⬜ chưa làm (đối chiếu source ngày 06/11/2026).

### 4.1. Khách (Guest)

| Chức năng                                                                    | Hiện trạng                                                         |
|------------------------------------------------------------------------------|--------------------------------------------------------------------|
| Xem trang chủ: banner, hero, ô tìm kiếm, việc làm nổi bật, menu ngành nghề   | ✅                                                                  |
| Tìm kiếm việc làm theo từ khóa, địa điểm, ngành nghề                        | ⚠ Có giao diện ô tìm kiếm, **chưa có** action xử lý (`/Job/Search`) |
| Xem chi tiết tin tuyển dụng                                                  | ✅                                                                  |
| Xem trang thông tin công ty                                                  | ⬜                                                                  |
| Đăng ký, đăng nhập                                                           | ✅                                                                  |

### 4.2. Ứng viên (Candidate)

| Chức năng                                                                    | Hiện trạng                                                                          |
|------------------------------------------------------------------------------|-------------------------------------------------------------------------------------|
| Đăng ký / đăng nhập / đăng xuất                                              | ✅                                                                                   |
| Quên & đổi mật khẩu                                                          | ⬜                                                                                   |
| Quản lý hồ sơ cá nhân (họ tên, SĐT, ảnh, giới thiệu)                         | ⬜                                                                                   |
| Tạo CV online                                                                | ⚠ Trình tạo CV (`/cv/create`) chỉ chạy phía trình duyệt, chưa lưu vào CSDL          |
| Quản lý thư viện CV (nhiều CV, đặt mặc định)                                 | ⬜                                                                                   |
| Ứng tuyển tin tuyển dụng (tải CV PDF/DOC/DOCX ≤ 5MB, thư giới thiệu)          | ✅ (`ApplicationController`, `_ApplyModal`); chưa chọn CV đã lưu                    |
| Xem lịch sử ứng tuyển và trạng thái, xem lại CV đã nộp                       | ✅ (`/applications`, lọc theo trạng thái, phân trang)                               |
| Lọc nâng cao: mức lương, kinh nghiệm, cấp bậc, hình thức làm việc            | ⬜                                                                                   |
| Lưu tin tuyển dụng yêu thích / bỏ lưu                                        | ⬜ (nút ♥ mới chỉ là hiệu ứng giao diện, chưa lưu CSDL)                              |
| Theo dõi công ty                                                             | ⬜                                                                                   |
| Gợi ý việc làm phù hợp                                                       | ⬜                                                                                   |
| Thông báo khi trạng thái hồ sơ thay đổi                                      | ⬜                                                                                   |

### 4.3. Nhà tuyển dụng (Employer)

| Chức năng                                                                    | Hiện trạng                                                      |
|------------------------------------------------------------------------------|-----------------------------------------------------------------|
| Đăng ký tài khoản (tự tạo hồ sơ công ty kèm theo)                            | ✅                                                               |
| Sửa hồ sơ công ty                                                            | ⬜ (Model `Company` đã có)                                       |
| Dashboard thống kê: số tin, lượt xem, số hồ sơ nhận được                     | ⬜                                                               |
| Đăng tin tuyển dụng mới                                                      | ⬜                                                               |
| Sửa, ẩn/hiện, gia hạn, đóng, xóa tin của mình                                | ⬜                                                               |
| Xem danh sách ứng viên đã ứng tuyển theo từng tin                            | ⬜                                                               |
| Xem/tải CV của ứng viên                                                      | ⬜ (đã có sẵn `CvFileController` kiểm tra quyền, cần làm giao diện) |
| Cập nhật trạng thái hồ sơ (Pending / Accepted / Rejected)                    | ⬜                                                               |
| Lọc, tìm kiếm trong danh sách ứng viên                                       | ⬜                                                               |
| Tìm kiếm hồ sơ ứng viên chủ động (tùy chọn nâng cao)                         | ⬜                                                               |

### 4.4. Quản trị viên (Admin)

| Chức năng                                                               | Hiện trạng                  |
|-------------------------------------------------------------------------|-----------------------------|
| Đăng nhập khu vực quản trị riêng                                        | ⬜ (đã có tài khoản Admin trong seed) |
| Dashboard thống kê toàn hệ thống                                        | ⬜                           |
| Quản lý người dùng: xem, tìm kiếm, khóa/mở khóa, đổi vai trò, xóa       | ⬜ (khóa tài khoản `IsActive` đã có hiệu lực ngay khi đổi) |
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
