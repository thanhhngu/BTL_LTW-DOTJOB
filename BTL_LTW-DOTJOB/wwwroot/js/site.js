document.addEventListener('click', function (e) {
    const heartBtn = e.target.closest('.btn-heart');

    if (heartBtn) {
        e.preventDefault();

        const icon = heartBtn.querySelector('i');
        if (!icon) return;

        const isLiked = icon.classList.contains('fa-solid');

        if (!isLiked) {
            icon.classList.remove('fa-regular');
            icon.classList.add('fa-solid', 'text-danger');
            heartBtn.classList.remove('text-muted');
            updateSavedJobsCounter(1);
        } else {
            icon.classList.remove('fa-solid', 'text-danger');
            icon.classList.add('fa-regular');
            heartBtn.classList.add('text-muted');
            updateSavedJobsCounter(-1);
        }

        icon.style.transform = "scale(1.3)";
        icon.style.transition = "transform 0.2s";
        setTimeout(() => { icon.style.transform = "scale(1)"; }, 200);
    }
});

function updateSavedJobsCounter(change) {
    const badge = document.querySelector('.floating-toolbar-wrapper .toolbar-badge');
    if (badge) {
        let currentCount = parseInt(badge.innerText) || 0;
        let newCount = currentCount + change;
        if (newCount < 0) newCount = 0;
        badge.innerText = newCount;
    }
}