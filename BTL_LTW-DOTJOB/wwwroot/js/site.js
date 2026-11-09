document.addEventListener('click', async function (e) {
    const heartBtn = e.target.closest('.btn-heart');
    if (!heartBtn) return;

    e.preventDefault();

    const jobId = heartBtn.dataset.jobId;
    if (!jobId || heartBtn.dataset.busy) return;

    heartBtn.dataset.busy = '1';

    try {
        const res = await fetch(`/saved-jobs/${encodeURIComponent(jobId)}/toggle`, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': getCsrfToken()
            }
        });

        if (res.ok) {
            const data = await res.json();
            setHeartState(heartBtn, data.saved);
            updateSavedJobsCounter(data.count);

            if (!data.saved) {
                const list = heartBtn.closest('[data-remove-on-unsave]');
                const card = list ? heartBtn.closest('.col') : null;
                if (card) card.remove();
            }
        } else if (res.status === 401) {
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

function getCsrfToken() {
    const meta = document.querySelector('meta[name="csrf-token"]');
    return meta ? meta.content : '';
}

function setHeartState(heartBtn, saved) {
    const icon = heartBtn.querySelector('i');
    if (!icon) return;

    icon.classList.toggle('fa-solid', saved);
    icon.classList.toggle('text-danger', saved);
    icon.classList.toggle('fa-regular', !saved);

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

function updateSavedJobsCounter(count) {
    const badge = document.querySelector('.floating-toolbar-wrapper .toolbar-badge');
    if (badge) {
        badge.innerText = count;
    }
}

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