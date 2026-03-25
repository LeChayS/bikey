let selectedAdditionalFiles = [];

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
            selectedAdditionalFiles = Array.from(event.target.files || []);
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
        }