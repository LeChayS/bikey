let selectedAdditionalFiles = [];

        function renderAdditionalImagePreview() {
            const fileInput = document.getElementById('hinhAnhKhac');
            const grid = document.getElementById('imagePreviewGrid');
            const stats = document.getElementById('uploadStats');
            const statsText = document.getElementById('statsText');

            if (!fileInput || !grid) return;
            grid.innerHTML = '';

            if (!selectedAdditionalFiles.length) {
                fileInput.value = '';
                if (stats) stats.style.display = 'none';
                if (statsText) statsText.textContent = 'Chưa có hình ảnh nào được chọn';
                return;
            }

            const dataTransfer = new DataTransfer();
            selectedAdditionalFiles.forEach(file => dataTransfer.items.add(file));
            fileInput.files = dataTransfer.files;

            if (stats) stats.style.display = 'block';
            if (statsText) statsText.textContent = `Đã chọn ${selectedAdditionalFiles.length} hình ảnh`;

            selectedAdditionalFiles.forEach((file, index) => {
                const url = URL.createObjectURL(file);

                const item = document.createElement('div');
                item.className = 'image-preview-item';
                item.innerHTML = `
                    <button type="button" class="remove-btn" title="Xóa ảnh">&times;</button>
                    <img src="${url}" alt="Ảnh xe" />
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

            if (!file) {
                previewContent.innerHTML = `
                    <i class="bi bi-cloud-upload upload-icon"></i>
                    <p class="mb-0">Click để chọn ảnh chính</p>
                    <small class="text-muted">JPG, PNG (Max: 5MB)</small>
                `;
                return;
            }

            const url = URL.createObjectURL(file);
            previewContent.innerHTML = `<img src="${url}" alt="Ảnh chính" />`;
        }

        const MAX_TOTAL_IMAGES = 10;

        function hasNewMainImage() {
            const input = document.getElementById('hinhAnh');
            return input && input.files && input.files.length > 0;
        }

        function getAvailableSlots() {
            const mainSlot = hasNewMainImage() ? 1 : 0;
            return Math.max(0, MAX_TOTAL_IMAGES - mainSlot);
        }

        function previewMultipleImages(event) {
            const newFiles = Array.from(event.target.files || []);
            const available = getAvailableSlots() - selectedAdditionalFiles.length;
            if (available <= 0) {
                alert(`Đã đạt giới hạn ${MAX_TOTAL_IMAGES} ảnh. Vui lòng xóa bớt ảnh trước khi thêm mới.`);
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