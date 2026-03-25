let selectedAdditionalFiles = [];
const MAX_TOTAL_IMAGES = 10;

function getExistingImageCount() {
    const items = document.querySelectorAll('.image-item');
    let count = 0;
    items.forEach(item => {
        const cb = item.querySelector('.remove-image-checkbox');
        if (!cb || !cb.checked) count++;
    });
    return count;
}

function hasNewMainImage() {
    const input = document.getElementById('hinhAnh');
    return input && input.files && input.files.length > 0;
}

function getAvailableSlots() {
    const existing = getExistingImageCount();
    const existingMainActive = !!document.querySelector('.image-item.main-image:not(.marked-for-delete)');
    const newMainAddsSlot = hasNewMainImage() && !existingMainActive;
    return Math.max(0, MAX_TOTAL_IMAGES - existing - (newMainAddsSlot ? 1 : 0));
}

function updateSlotInfo() {
    const slotsEl = document.getElementById('remainingSlots');
    if (slotsEl) {
        const slots = getAvailableSlots() - selectedAdditionalFiles.length;
        slotsEl.textContent = `Còn ${Math.max(0, slots)} chỗ trống (tối đa ${MAX_TOTAL_IMAGES} ảnh)`;
    }
}

function renderAdditionalImagePreview() {
    const fileInput = document.getElementById('hinhAnhKhac');
    const grid = document.getElementById('newImagePreviewGrid');
    const stats = document.getElementById('uploadStats');
    const statsText = document.getElementById('statsText');

    if (!fileInput || !grid) return;
    grid.innerHTML = '';

    if (!selectedAdditionalFiles.length) {
        fileInput.value = '';
        if (stats) stats.style.display = 'none';
        if (statsText) statsText.textContent = 'Chưa có hình ảnh mới nào được chọn';
        updateSlotInfo();
        return;
    }

    const dataTransfer = new DataTransfer();
    selectedAdditionalFiles.forEach(file => dataTransfer.items.add(file));
    fileInput.files = dataTransfer.files;

    if (stats) stats.style.display = 'block';
    if (statsText) statsText.textContent = `Đã chọn ${selectedAdditionalFiles.length} ảnh mới`;

    selectedAdditionalFiles.forEach((file, index) => {
        const url = URL.createObjectURL(file);
        const item = document.createElement('div');
        item.className = 'new-image-preview-item';
        item.innerHTML = `
            <button type="button" class="remove-btn" title="Bỏ ảnh này">&times;</button>
            <img src="${url}" alt="Ảnh mới" />
            <div class="file-name" title="${file.name}">${file.name}</div>
        `;

        const removeBtn = item.querySelector('.remove-btn');
        if (removeBtn) {
            removeBtn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                selectedAdditionalFiles.splice(index, 1);
                renderAdditionalImagePreview();
            });
        }
        grid.appendChild(item);
    });
    updateSlotInfo();
}

function previewMainImage(event) {
    const fileInput = event.target;
    const file = fileInput.files && fileInput.files[0];
    const previewContent = document.getElementById('preview-content');
    if (!previewContent) return;

    if (!file) return;
    const url = URL.createObjectURL(file);
    previewContent.innerHTML = `<img src="${url}" alt="Ảnh chính mới" />`;
}

function previewMultipleImages(event) {
    const newFiles = Array.from(event.target.files || []);
    const available = getAvailableSlots() - selectedAdditionalFiles.length;
    if (available <= 0) {
        alert(`Đã đạt giới hạn ${MAX_TOTAL_IMAGES} ảnh. Vui lòng xóa bớt ảnh cũ trước khi thêm mới.`);
        event.target.value = '';
        return;
    }
    const toAdd = newFiles.slice(0, available);
    if (toAdd.length < newFiles.length) {
        alert(`Chỉ thêm được ${toAdd.length}/${newFiles.length} ảnh do giới hạn ${MAX_TOTAL_IMAGES} ảnh.`);
    }
    selectedAdditionalFiles = selectedAdditionalFiles.concat(toAdd);
    renderAdditionalImagePreview();
}

function toggleRemoveImage(button) {
    const wrapper = button.closest('.image-item');
    const checkbox = wrapper ? wrapper.querySelector('.remove-image-checkbox') : null;
    if (!wrapper || !checkbox) return;
    const isMainImage = button.dataset.isMain === 'true';
    const canRemoveMain = button.dataset.canRemoveMain === 'true';

    if (isMainImage && !canRemoveMain) {
        alert('Không thể xóa ảnh chính vì xe chưa có ảnh phụ.');
        return;
    }

    checkbox.checked = !checkbox.checked;
    wrapper.classList.toggle('marked-for-delete', checkbox.checked);
    button.classList.toggle('btn-outline-danger', !checkbox.checked);
    button.classList.toggle('btn-danger', checkbox.checked);
    button.innerHTML = checkbox.checked
        ? '<i class="bi bi-x-circle"></i> Hoàn tác'
        : '<i class="bi bi-trash"></i> Xóa ảnh';
    updateSlotInfo();
}