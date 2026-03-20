$(document).ready(function () {
    // File upload preview functionality
    function handleFileUpload(inputId, previewId) {
        const input = document.getElementById(inputId);
        const preview = document.getElementById(previewId);

        input.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                // Validate file size (5MB limit)
                if (file.size > 5 * 1024 * 1024) {
                    alert('File quá lớn! Vui lòng chọn file nhỏ hơn 5MB.');
                    input.value = '';
                    preview.classList.remove('show');
                    return;
                }

                // Validate file type
                const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'application/pdf'];
                if (!allowedTypes.includes(file.type)) {
                    alert('Chỉ chấp nhận file JPG, PNG hoặc PDF!');
                    input.value = '';
                    preview.classList.remove('show');
                    return;
                }

                // Update preview content
                const previewContent = preview.querySelector('.file-preview-content');
                const previewInfo = preview.querySelector('.file-preview-info');
                const previewName = preview.querySelector('.file-preview-name');
                const previewSize = preview.querySelector('.file-preview-size');

                // Clear any existing image
                const existingImage = previewContent.querySelector('.file-preview-image');
                if (existingImage) {
                    existingImage.remove();
                }

                // Add image preview if it's an image file
                if (file.type.startsWith('image/')) {
                    const img = document.createElement('img');
                    img.className = 'file-preview-image';
                    img.src = URL.createObjectURL(file);
                    previewContent.insertBefore(img, previewInfo);
                }

                // Update file info
                previewName.textContent = file.name;
                previewSize.textContent = `${(file.size / 1024 / 1024).toFixed(2)} MB`;

                // Add PDF class if it's a PDF
                if (file.type === 'application/pdf') {
                    preview.classList.add('file-preview-pdf');
                } else {
                    preview.classList.remove('file-preview-pdf');
                }

                preview.classList.add('show');
            } else {
                preview.classList.remove('show');
            }
        });
    }

    // Initialize file upload previews
    handleFileUpload('cccdMatTruoc', 'cccdMatTruocPreview');
    handleFileUpload('cccdMatSau', 'cccdMatSauPreview');
    handleFileUpload('bangLaiXe', 'bangLaiXePreview');
    handleFileUpload('giayToKhac', 'giayToKhacPreview');
});

// Function to preview file in a modal
function previewFile(inputId) {
    const input = document.getElementById(inputId);
    const file = input.files[0];

    if (!file) {
        alert('Không có file để xem!');
        return;
    }

    // Create modal for preview
    const modal = document.createElement('div');
    modal.className = 'modal fade';
    modal.id = 'filePreviewModal';
    modal.innerHTML = `
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Xem trước: ${file.name}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body text-center">
                            ${file.type.startsWith('image/')
            ? `<img src="${URL.createObjectURL(file)}" class="img-fluid" style="max-height: 500px;" />`
            : `<div class="alert alert-info">
                                    <i class="bi bi-file-pdf"></i>
                                    <strong>File PDF:</strong> ${file.name}
                                    <br><small>Không thể xem trước file PDF. Vui lòng tải về để xem.</small>
                                   </div>`
        }
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                            <a href="${URL.createObjectURL(file)}" download="${file.name}" class="btn btn-primary">
                                <i class="bi bi-download"></i> Tải về
                            </a>
                        </div>
                    </div>
                </div>
            `;

    // Remove existing modal if any
    const existingModal = document.getElementById('filePreviewModal');
    if (existingModal) {
        existingModal.remove();
    }

    // Add modal to body and show
    document.body.appendChild(modal);
    const bootstrapModal = new bootstrap.Modal(modal);
    bootstrapModal.show();

    // Clean up modal when hidden
    modal.addEventListener('hidden.bs.modal', function () {
        modal.remove();
    });
}

// Function to delete file
function deleteFile(inputId) {
    if (confirm('Bạn có chắc chắn muốn xóa file này?')) {
        const input = document.getElementById(inputId);
        const preview = document.getElementById(inputId + 'Preview');

        // Clear the file input
        input.value = '';

        // Hide the preview
        preview.classList.remove('show');

        // Clear preview content
        const previewContent = preview.querySelector('.file-preview-content');
        const previewName = preview.querySelector('.file-preview-name');
        const previewSize = preview.querySelector('.file-preview-size');

        // Remove image if exists
        const existingImage = previewContent.querySelector('.file-preview-image');
        if (existingImage) {
            existingImage.remove();
        }

        // Clear text content
        previewName.textContent = '';
        previewSize.textContent = '';

        // Remove PDF class
        preview.classList.remove('file-preview-pdf');
    }
}