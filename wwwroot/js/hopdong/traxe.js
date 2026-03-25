// Trả xe: tính tiền thuê + phụ phí + đền bù theo input người dùng.
// View truyền dữ liệu qua hidden inputs: hopDongStartDate, tongGiaThueNgay.

(function () {
    'use strict';

    function formatVND(value) {
        const v = Number(value) || 0;
        return Math.round(v).toLocaleString('vi-VN') + 'đ';
    }

    function getDaysBetween(startStr, endStr) {
        if (!startStr || !endStr) return 1;
        const [sy, sm, sd] = startStr.split('-').map(Number);
        const [ey, em, ed] = endStr.split('-').map(Number);
        const start = new Date(Date.UTC(sy, sm - 1, sd));
        const end = new Date(Date.UTC(ey, em - 1, ed));
        const diff = end.getTime() - start.getTime();
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));
        return Math.max(1, days);
    }

    function readConfig() {
        const startDateEl = document.getElementById('hopDongStartDate');
        const tongGiaThueNgayEl = document.getElementById('tongGiaThueNgay');

        const hopDongStartDate = startDateEl ? startDateEl.value : '';
        const tongGiaThueNgay = tongGiaThueNgayEl ? Number(tongGiaThueNgayEl.value) : 0;

        return { hopDongStartDate, tongGiaThueNgay };
    }

    function handleTinhTrangXeChange() {
        const tinhTrangXeSelect = document.getElementById('tinhTrangXeSelect');
        const thietHaiForm = document.getElementById('thietHaiForm');

        const tinhTrangXe = tinhTrangXeSelect ? tinhTrangXeSelect.value : '';
        const isCoSuCo = tinhTrangXe === 'Có sự cố';
        if (thietHaiForm) {
            thietHaiForm.style.display = isCoSuCo ? 'block' : 'none';
        }

        calculateFinal();
    }

    function formatOtherFeeDisplay() {
        calculateFinal();
    }

    function calculateFinal() {
        const ngayTraInput = document.querySelector('input[name="ngayTraThucTe"]');
        if (!ngayTraInput || !ngayTraInput.value) return;

        const otherFeeInput = document.getElementById('otherFeeInput');
        const tinhTrangXeSelect = document.getElementById('tinhTrangXeSelect');
        const chiPhiThietHaiInput = document.getElementById('chiPhiThietHai');

        const soNgayThueEl = document.getElementById('soNgayThue');
        const tienThueXeEl = document.getElementById('tienThueXe');
        const tongTienFinalEl = document.getElementById('tongTienFinal');

        const { hopDongStartDate, tongGiaThueNgay } = readConfig();

        const days = getDaysBetween(hopDongStartDate, ngayTraInput.value);
        if (soNgayThueEl) soNgayThueEl.textContent = days + ' ngày';

        const tienThueXe = days * tongGiaThueNgay;
        if (tienThueXeEl) tienThueXeEl.textContent = formatVND(tienThueXe);

        const phuPhi = Number(otherFeeInput ? otherFeeInput.value : 0) || 0;
        const chiPhiThietHai = Number(chiPhiThietHaiInput ? chiPhiThietHaiInput.value : 0) || 0;
        const laCoSuCo = tinhTrangXeSelect ? tinhTrangXeSelect.value === 'Có sự cố' : false;
        const phiDenBu = laCoSuCo ? chiPhiThietHai : 0;

        // Cọc được hoàn lại, nên tổng hóa đơn chỉ gồm: tiền thuê + phụ phí + đền bù.
        const tongCong = tienThueXe + phuPhi + phiDenBu;
        const soTien = Math.max(0, tongCong);

        if (tongTienFinalEl) tongTienFinalEl.textContent = formatVND(soTien);
    }

    // Expose for inline handlers in TraXe.cshtml.
    window.handleTinhTrangXeChange = handleTinhTrangXeChange;
    window.formatOtherFeeDisplay = formatOtherFeeDisplay;
    window.calculateFinal = calculateFinal;

    document.addEventListener('DOMContentLoaded', function () {
        const tinhTrangXeSelect = document.getElementById('tinhTrangXeSelect');
        const otherFeeInput = document.getElementById('otherFeeInput');
        const ngayTraInput = document.querySelector('input[name="ngayTraThucTe"]');
        const chiPhiThietHaiInput = document.getElementById('chiPhiThietHai');

        if (tinhTrangXeSelect) tinhTrangXeSelect.addEventListener('change', handleTinhTrangXeChange);
        if (otherFeeInput) otherFeeInput.addEventListener('input', calculateFinal);
        if (ngayTraInput) {
            ngayTraInput.addEventListener('change', calculateFinal);
            ngayTraInput.addEventListener('input', calculateFinal);
        }
        if (chiPhiThietHaiInput) chiPhiThietHaiInput.addEventListener('input', calculateFinal);

        handleTinhTrangXeChange();
        calculateFinal();
    });
})();
