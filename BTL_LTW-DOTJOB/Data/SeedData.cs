using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new AppDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

        // 1. Seed Roles
        var roleNames = new[] { "Admin", "Employer", "Candidate" };
        foreach (var roleName in roleNames)
        {
            if (!context.Roles.Any(role => role.Name == roleName))
            {
                context.Roles.Add(new Role { Name = roleName });
            }
        }
        context.SaveChanges();

        var roleIds = context.Roles.ToDictionary(role => role.Name, role => role.Id);

        // 2. Seed Categories (30 ngành nghề)
        if (!context.Categories.Any())
        {
            string[] categoryNames =
            {
                "Công nghệ Thông tin", "Kinh doanh / Bán hàng", "Marketing / PR / Quảng cáo",
                "Nhân sự / Hành chính", "Kế toán / Kiểm toán", "Xây dựng / Kiến trúc",
                "Tài chính / Ngân hàng", "Chăm sóc khách hàng", "Thiết kế / Đồ họa",
                "Y tế / Dược phẩm", "Giáo dục / Đào tạo", "Bất động sản",
                "Du lịch / Khách sạn", "Logistics / Vận tải", "Cơ khí / Tự động hóa",
                "Điện / Điện tử", "Dịch vụ pháp lý", "Truyền thông / Báo chí",
                "Nông / Lâm nghiệp", "An ninh / Bảo vệ", "Quản lý điều hành",
                "Biên - Phiên dịch", "Sản xuất / Vận hành", "Thời trang / May mặc",
                "Công nghệ sinh học", "Ô tô / Xe máy", "Dầu khí / Địa chất",
                "Môi trường", "Tổ chức sự kiện", "Ngành nghề khác"
            };

            context.Categories.AddRange(categoryNames.Select(name => new Category
            {
                Name = name,
                Description = $"Cơ hội việc làm và thông tin tuyển dụng thuộc lĩnh vực {name}."
            }));
            context.SaveChanges();
        }

        // 3. Seed Users (Đúng 30 User, tất cả đều là Employer để tương ứng với 30 công ty)
        if (!context.Users.Any())
        {
            var hasher = new PasswordHasher<User>();
            var users = new List<User>();

            for (int i = 1; i <= 30; i++)
            {
                var user = new User
                {
                    Email = $"employer{i}@company{i}.com",
                    FullName = $"Nhà tuyển dụng {i}",
                    Phone = $"0901234{i:D3}",
                    Avatar = $"/images/avatars/user-{i}.png",
                    RoleId = roleIds["Employer"],
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    IsActive = true
                };

                user.PasswordHash = hasher.HashPassword(user, "123456");
                users.Add(user);
            }

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        // 4. Seed Companies (Đúng 30 Công ty, mỗi công ty gắn liền với 1 Employer duy nhất theo tỉ lệ 1-1)
        if (!context.Companies.Any())
        {
            string[] companyNames =
            {
                "FPT Software", "Công ty CP MISA", "Tập đoàn Viettel", "TopCV Vietnam",
                "Vingroup", "Techcombank", "VNPT Group", "Shopee Việt Nam",
                "Vinamilk", "Base.vn", "VNG Corporation", "Momo (M_Service)",
                "TMA Solutions", "CMC Corporation", "Tiki Corporation", "Lazada Vietnam",
                "Grab Vietnam", "Mobifone", "Ngân hàng TMCP Quân đội (MB)", "VPBank",
                "Agribank", "BIDV", "VietinBank", "Hòa Phát Group",
                "Sun Group", "Samsung Electronics Vietnam", "LG Display Vietnam",
                "Intel Products Vietnam", "Viettel Digital", "FPT Telecom"
            };

            var employerIds = context.Users
                .Where(user => user.RoleId == roleIds["Employer"])
                .Select(user => user.Id)
                .ToList();

            if (employerIds.Any())
            {
                var companies = new List<Company>();
                int totalCount = Math.Min(companyNames.Length, employerIds.Count);

                for (int index = 0; index < totalCount; index++)
                {
                    companies.Add(new Company
                    {
                        CompanyName = companyNames[index],
                        Logo = $"/images/logos/company-{index + 1}.png",
                        WorkLocation = index % 2 == 0 ? "Hà Nội" : "TP. Hồ Chí Minh",
                        CompanyAddress = index % 2 == 0 ? "Cầu Giấy, Hà Nội" : "Quận 1, TP. Hồ Chí Minh",
                        Website = $"https://www.company-{index + 1}.vn",
                        Description = $"Thông tin chi tiết về quy mô, văn hóa và môi trường làm việc chuyên nghiệp tại {companyNames[index]}.",
                        UserId = employerIds[index] // Khớp chính xác 1-1 với User thứ index
                    });
                }

                context.Companies.AddRange(companies);
                context.SaveChanges();
            }
        }

        // 5. Seed JobPostings (Đúng 30 việc làm phân bổ đều cho 30 công ty và danh mục)
        if (!context.JobPostings.Any())
        {
            string[] jobTitles =
            {
                "Kỹ sư phần mềm .NET Senior", "Chuyên viên Kinh doanh phần mềm", "Frontend Developer (ReactJS)",
                "Kế toán tổng hợp", "Chuyên viên Marketing Digital", "Kỹ sư DevOps / Cloud",
                "Fullstack Developer (NodeJS/React)", "Kỹ sư Cơ điện M&E", "Quản trị cơ sở dữ liệu DBA",
                "Nhân viên Chăm sóc khách hàng", "Kỹ sư Giải pháp Hệ thống", "Senior Network Engineer",
                "Kỹ sư An ninh mạng", "Fresher Web Developer", "Fullstack .NET & Vue.js",
                "Cán bộ Kỹ thuật Hiện trường", "Giám sát Dự án Xây dựng", "Kỹ sư Kinh tế Xây dựng",
                "Kỹ sư Dự Toán (QS)", "Nhân viên Giám sát Thi công", "Kế toán trưởng",
                "Kỹ thuật Tòa nhà", "Giám sát Kỹ thuật Điện lạnh", "Kỹ thuật viên sản xuất",
                "Executive Assistant", "Kỹ sư Phát triển Kết cấu", "Kỹ sư AI / Machine Learning",
                "Data Analyst", "UI/UX Designer", "Product Owner / BA"
            };
            string[] jobCategoryNames =
            {
                "Công nghệ Thông tin", "Kinh doanh / Bán hàng", "Công nghệ Thông tin",
                "Kế toán / Kiểm toán", "Marketing / PR / Quảng cáo", "Công nghệ Thông tin",
                "Công nghệ Thông tin", "Điện / Điện tử", "Công nghệ Thông tin",
                "Chăm sóc khách hàng", "Công nghệ Thông tin", "Công nghệ Thông tin",
                "Công nghệ Thông tin", "Công nghệ Thông tin", "Công nghệ Thông tin",
                "Xây dựng / Kiến trúc", "Xây dựng / Kiến trúc", "Xây dựng / Kiến trúc",
                "Xây dựng / Kiến trúc", "Xây dựng / Kiến trúc", "Kế toán / Kiểm toán",
                "Điện / Điện tử", "Điện / Điện tử", "Sản xuất / Vận hành",
                "Nhân sự / Hành chính", "Xây dựng / Kiến trúc", "Công nghệ Thông tin",
                "Công nghệ Thông tin", "Thiết kế / Đồ họa", "Quản lý điều hành"
            };

            var categories = context.Categories.ToList();
            if (!categories.Any()) return;
            var categoryIds = categories.ToDictionary(category => category.Name, category => category.Id);
            var defaultCategoryId = categories.First().Id;

            var companyIds = context.Companies.Select(company => company.Id).ToList();
            if (!companyIds.Any()) return;

            var jobs = new List<JobPosting>();

            for (int i = 0; i < jobTitles.Length; i++)
            {
                decimal minSalary = (8 + i % 12) * 1_000_000m;
                var catName = jobCategoryNames[i];
                var catId = categoryIds.ContainsKey(catName) ? categoryIds[catName] : defaultCategoryId;

                jobs.Add(new JobPosting
                {
                    Title = jobTitles[i],
                    Description = $"Thực hiện các nhiệm vụ chuyên môn và phối hợp với đội ngũ trong vị trí {jobTitles[i]}. Chi tiết công việc sẽ trao đổi trực tiếp khi phỏng vấn.",
                    Requirements = "Có chuyên môn phù hợp, tinh thần trách nhiệm cao, khả năng làm việc nhóm tốt, tối thiểu từ 1-2 năm kinh nghiệm.",
                    SalaryMin = minSalary,
                    SalaryMax = minSalary + 12_000_000m,
                    Location = i % 2 == 0 ? "Hà Nội" : "TP. Hồ Chí Minh",
                    JobType = "Toàn thời gian",
                    Deadline = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow.AddDays(-i % 10),
                    IsActive = true,
                    CategoryId = catId,
                    CompanyId = companyIds[i % companyIds.Count]
                });
            }

            context.JobPostings.AddRange(jobs);
            context.SaveChanges();
        }

        // 6. Seed Applications (Tạo đơn ứng tuyển mẫu mô phỏng dữ liệu lịch sử cho các Job)
        if (!context.Applications.Any())
        {
            var jobIds = context.JobPostings.Select(job => job.Id).ToList();
            var employerIdsForApp = context.Users
                .Where(user => user.RoleId == roleIds["Employer"])
                .Select(user => user.Id)
                .ToList();

            if (!jobIds.Any() || !employerIdsForApp.Any()) return;

            string[] statuses = { "Pending", "Accepted", "Rejected" };
            var applications = new List<Application>();

            for (int i = 0; i < Math.Min(30, jobIds.Count); i++)
            {
                applications.Add(new Application
                {
                    JobPostingId = jobIds[i],
                    UserId = employerIdsForApp[i % employerIdsForApp.Count],
                    CvFilePath = $"/uploads/cvs/sample-cv-{i + 1}.pdf",
                    CoverLetter = "Kính gửi nhà tuyển dụng, tôi rất quan tâm đến cơ hội hợp tác và phát triển tại quý công ty.",
                    AppliedDate = DateTime.UtcNow.AddDays(-i % 15),
                    Status = statuses[i % statuses.Length]
                });
            }

            context.Applications.AddRange(applications);
            context.SaveChanges();
        }
    }
}