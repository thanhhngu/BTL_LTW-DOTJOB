// Nút trái tim (lưu / bỏ lưu việc làm). Nút cần có data-job-id
document.addEventListener('click', async function (e) {
    const heartBtn = e.target.closest('.btn-heart');
    if (!heartBtn) return;

    e.preventDefault(); // nút nằm trong thẻ <a> nên chặn không cho chuyển sang trang chi tiết

    const jobId = heartBtn.dataset.jobId;
    if (!jobId || heartBtn.dataset.busy) return;

    heartBtn.dataset.busy = '1'; // chống bấm liên tục

    try {
        const res = await fetch(`/saved-jobs/${encodeURIComponent(jobId)}/toggle`, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest', // để server trả 401/403 thay vì chuyển hướng 302
                'RequestVerificationToken': getCsrfToken()
            }
        });

        if (res.ok) {
            const data = await res.json();
            setHeartState(heartBtn, data.saved);
            updateSavedJobsCounter(data.count);

            // Ở trang "Việc làm đã lưu": bỏ lưu xong thì xóa thẻ khỏi danh sách
            if (!data.saved) {
                const list = heartBtn.closest('[data-remove-on-unsave]');
                const card = list ? heartBtn.closest('.col') : null;
                if (card) card.remove();
            }
        } else if (res.status === 401) {
            // Chưa đăng nhập: chuyển sang trang đăng nhập rồi quay lại trang này
            window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
        } else if (res.status === 403) {
            showHeartMessage('Chỉ tài khoản Ứng viên mới có thể lưu việc làm.', 'warning');
        } else {
            showHeartMessage(await res.text(), 'error');
        }
    } catch {
        showHeartMessage('Không kết nối được máy chủ.', 'error');
    } finally {
        delete heartBtn.dataset.busy;
    }
});

// Token chống giả mạo request được _Layout đặt trong <meta name="csrf-token">
function getCsrfToken() {
    const meta = document.querySelector('meta[name="csrf-token"]');
    return meta ? meta.content : '';
}

// Đổi giao diện nút theo trạng thái đã lưu / chưa lưu
function setHeartState(heartBtn, saved) {
    const icon = heartBtn.querySelector('i');
    if (!icon) return;

    icon.classList.toggle('fa-solid', saved);
    icon.classList.toggle('text-danger', saved);
    icon.classList.toggle('fa-regular', !saved);

    // Nút dạng icon nhỏ trên thẻ việc làm thì đổi màu chữ, nút "Lưu tin" ở trang chi tiết thì đổi chữ
    if (heartBtn.classList.contains('btn-link')) {
        heartBtn.classList.toggle('text-muted', !saved);
    }
    const label = heartBtn.querySelector('.heart-label');
    if (label) label.textContent = saved ? 'Đã lưu' : 'Lưu tin';

    icon.style.transform = 'scale(1.3)';
    icon.style.transition = 'transform 0.2s';
    setTimeout(() => { icon.style.transform = 'scale(1)'; }, 200);
}

function showHeartMessage(message, icon) {
    if (window.Swal) {
        Swal.fire({ text: message, icon: icon });
    } else {
        alert(message);
    }
}

// Cập nhật số trên thanh công cụ nổi theo số tin đã lưu mà server trả về
function updateSavedJobsCounter(count) {
    const badge = document.querySelector('.floating-toolbar-wrapper .toolbar-badge');
    if (badge) {
        badge.innerText = count;
    }
}

// Load category page content via AJAX

async function loadCategoryPage(page) {
    const categoryMenu = document.getElementById('category-menu-container');
    if (!categoryMenu) return;

    try {
        const response = await fetch(`/Home/LoadCategoryMenu?page=${encodeURIComponent(page)}`);
        if (!response.ok) {
            throw new Error(`Failed to load category page: ${response.status}`);
        }

        categoryMenu.outerHTML = await response.text();
    } catch (error) {
        console.error('Could not load category page:', error);
    }
}