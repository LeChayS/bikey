(function () {
    'use strict';

    const boot = window.__THONGKE || {};
    const chartFilter = boot.chartFilter || 'week';

    const pick = (arr, key) => (arr || []).map(x => x[key]);
    const revenueLabels = pick(boot.bieuDoDoanhThu, 'label');
    const revenueData = pick(boot.bieuDoDoanhThu, 'value').map(Number);
    const chiTieuData = pick(boot.bieuDoChiTieu, 'value').map(Number);
    const thietHaiData = pick(boot.bieuDoThietHai, 'value').map(Number);
    const topVehicleLabels = (boot.topXeThueNhieu || []).map(x => `${x.tenXe} (${x.bienSo})`);
    const topVehicleData = (boot.topXeThueNhieu || []).map(x => Number(x.soLanThue));
    const vehicleTypeLabels = pick(boot.bieuDoLoaiXe, 'label');
    const vehicleTypeData = pick(boot.bieuDoLoaiXe, 'value').map(Number);
    const contractStatusLabels = pick(boot.bieuDoHopDongTrangThai, 'label');
    const contractStatusData = pick(boot.bieuDoHopDongTrangThai, 'value').map(Number);

    let revenueChart, topVehiclesChart, vehicleTypesChart, contractStatusChart, profitBreakdownChart;

    function setText(id, text) {
        const el = document.getElementById(id);
        if (el) el.textContent = text;
    }

    function adjustChartHeight(chartId, dataCount) {
        const container = document.getElementById(chartId + 'Container');
        if (!container) return;
        let height;
        if (dataCount <= 7) height = 500;
        else if (dataCount <= 12) height = 550;
        else if (dataCount <= 30) height = 600;
        else height = 800;
        container.style.height = height + 'px';
    }

    function createLineChart(ctx, labels, data) {
        return new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Doanh thu',
                    data: data,
                    backgroundColor: 'transparent',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 2,
                    fill: false,
                    tension: 0.3,
                    pointBackgroundColor: 'rgba(54, 162, 235, 1)',
                    pointRadius: 4,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return 'Doanh thu: ' + context.parsed.y.toLocaleString('vi-VN') + 'đ';
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) {
                                return value.toLocaleString('vi-VN') + 'đ';
                            }
                        }
                    }
                }
            }
        });
    }

    function showNoData(canvasId, message) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        canvas.style.display = 'none';
        const parent = canvas.parentElement;
        if (parent) {
            parent.innerHTML = '<div class="text-center text-muted p-4"><i class="bi bi-info-circle"></i> ' + message + '</div>';
        }
    }

    const doughnutCenterTextPlugin = {
        id: 'doughnutCenterText',
        afterDatasetsDraw(chart) {
            if (chart.config.type !== 'doughnut') return;
            const centerText = chart?.options?.plugins?.centerText;
            if (!centerText || !centerText.value) return;

            const meta = chart.getDatasetMeta(0);
            if (!meta || !meta.data || !meta.data.length) return;
            const arc = meta.data[0];
            const x = arc.x;
            const y = arc.y;
            const ctx = chart.ctx;

            ctx.save();
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillStyle = centerText.color || '#495057';
            ctx.font = centerText.font || '600 18px system-ui, -apple-system, Segoe UI, Roboto, sans-serif';
            ctx.fillText(centerText.value, x, y);
            ctx.restore();
        }
    };
    Chart.register(doughnutCenterTextPlugin);

    const revenueCanvas = document.getElementById('revenueChart');
    if (revenueCanvas) {
        const revenueCtx = revenueCanvas.getContext('2d');
        const hasRevenueData = revenueData.some(v => v > 0);
        if (hasRevenueData) {
            revenueChart = createLineChart(revenueCtx, revenueLabels, revenueData);
        } else {
            showNoData('revenueChart', 'Chưa có dữ liệu doanh thu');
        }
    }

    const topVehiclesCanvas = document.getElementById('topVehiclesChart');
    if (topVehiclesCanvas) {
        const topVehiclesCtx = topVehiclesCanvas.getContext('2d');
        if (topVehicleLabels.length > 0 && topVehicleData.some(v => v > 0)) {
            topVehiclesChart = new Chart(topVehiclesCtx, {
                type: 'bar',
                data: {
                    labels: topVehicleLabels,
                    datasets: [{
                        label: 'Số lần thuê',
                        data: topVehicleData,
                        backgroundColor: 'rgba(75, 192, 192, 0.8)',
                        borderColor: 'rgb(75, 192, 192)',
                        borderWidth: 2,
                        borderRadius: 8,
                        barThickness: topVehicleLabels.length <= 3 ? 40 : 30
                    }]
                },
                options: {
                    indexAxis: 'y',
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return 'Số lần thuê: ' + context.parsed.x + ' lần';
                                }
                            }
                        }
                    },
                    scales: {
                        x: { beginAtZero: true, ticks: { stepSize: 1 } }
                    }
                }
            });
        } else {
            showNoData('topVehiclesChart', 'Chưa có dữ liệu xe được thuê');
        }
    }

    const vehicleTypesCanvas = document.getElementById('vehicleTypesChart');
    if (vehicleTypesCanvas) {
        const vehicleTypesCtx = vehicleTypesCanvas.getContext('2d');
        if (vehicleTypeLabels.length > 0 && vehicleTypeData.some(v => v > 0)) {
            vehicleTypesChart = new Chart(vehicleTypesCtx, {
                type: 'bar',
                data: {
                    labels: vehicleTypeLabels,
                    datasets: [{
                        label: 'Doanh thu',
                        data: vehicleTypeData,
                        backgroundColor: 'rgba(54, 162, 235, 0.9)',
                        borderColor: 'rgb(54, 162, 235)',
                        borderWidth: 0,
                        borderRadius: 15,
                        barThickness: vehicleTypeLabels.length <= 3 ? 35 : 25
                    }]
                },
                options: {
                    indexAxis: 'y',
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return 'Doanh thu: ' + context.parsed.x.toLocaleString('vi-VN') + 'đ';
                                }
                            }
                        }
                    },
                    scales: {
                        x: {
                            beginAtZero: true,
                            ticks: {
                                callback: function (value) {
                                    return value.toLocaleString('vi-VN') + 'đ';
                                }
                            }
                        }
                    }
                }
            });
        } else {
            showNoData('vehicleTypesChart', 'Chưa có dữ liệu loại xe');
        }
    }

    const contractStatusCanvas = document.getElementById('contractStatusChart');
    if (contractStatusCanvas) {
        const contractStatusCtx = contractStatusCanvas.getContext('2d');
        const statusColors = {
            'Đang thuê': 'rgba(255, 193, 7, 1)',
            'Hoàn thành': 'rgba(40, 167, 69, 1)',
            'Hủy': 'rgba(220, 53, 69, 1)'
        };
        if (contractStatusLabels.length > 0 && contractStatusData.some(v => v > 0)) {
            const pointColors = contractStatusLabels.map(l => statusColors[l] || 'rgba(108, 117, 125, 1)');
            const tongHopDong = contractStatusData.reduce(function (sum, value) { return sum + value; }, 0);
            contractStatusChart = new Chart(contractStatusCtx, {
                type: 'doughnut',
                data: {
                    labels: contractStatusLabels,
                    datasets: [{
                        label: 'Số lượng hợp đồng',
                        data: contractStatusData,
                        backgroundColor: pointColors,
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        centerText: {
                            value: tongHopDong.toLocaleString('vi-VN'),
                            font: '700 20px system-ui, -apple-system, Segoe UI, Roboto, sans-serif',
                            color: '#1f2937'
                        },
                        legend: { display: false },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return 'Số lượng: ' + context.parsed + ' hợp đồng';
                                }
                            }
                        }
                    }
                }
            });
        } else {
            showNoData('contractStatusChart', 'Chưa có dữ liệu hợp đồng');
        }
    }

    const profitBreakdownCanvas = document.getElementById('profitBreakdownChart');
    if (profitBreakdownCanvas) {
        const profitCtx = profitBreakdownCanvas.getContext('2d');
        const tongTienThue = revenueData.reduce(function (sum, value, index) {
            const v = value - (chiTieuData[index] || 0) - (thietHaiData[index] || 0);
            return sum + (v > 0 ? v : 0);
        }, 0);
        const tongPhuPhi = chiTieuData.reduce(function (sum, value) { return sum + value; }, 0);
        const tongTienSuaChua = thietHaiData.reduce(function (sum, value) { return sum + value; }, 0);
        const hasProfitData = tongTienThue > 0 || tongPhuPhi > 0 || tongTienSuaChua > 0;

        if (hasProfitData) {
            const tongLoiNhuan = tongTienThue + tongPhuPhi + tongTienSuaChua;
            profitBreakdownChart = new Chart(profitCtx, {
                type: 'doughnut',
                data: {
                    labels: ['Tiền thuê', 'Phụ phí', 'Tiền sửa chữa'],
                    datasets: [{
                        data: [tongTienThue, tongPhuPhi, tongTienSuaChua],
                        backgroundColor: [
                            'rgba(54, 162, 235, 0.85)',
                            'rgba(255, 193, 7, 0.85)',
                            'rgba(220, 53, 69, 0.85)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        centerText: {
                            value: tongLoiNhuan.toLocaleString('vi-VN') + 'đ',
                            font: '700 18px system-ui, -apple-system, Segoe UI, Roboto, sans-serif',
                            color: '#1f2937'
                        },
                        legend: { position: 'top' },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return context.label + ': ' + context.parsed.toLocaleString('vi-VN') + 'đ';
                                }
                            }
                        }
                    }
                }
            });
        } else {
            showNoData('profitBreakdownChart', 'Chưa có dữ liệu lợi nhuận');
        }
    }

    function updateChartTitles(filter) {
        const titles = {
            today: 'hôm nay',
            week: 'tuần này',
            month: 'tháng này',
            quarter: 'quý này',
            year: 'năm nay'
        };
        const period = titles[filter] || 'kỳ đã chọn';
        setText('revenueTitle', 'Doanh thu theo ngày (' + period + ')');
        setText('topVehiclesTitle', 'Top 5 xe được thuê nhiều nhất (' + period + ')');
        setText('vehicleTypesTitle', 'Doanh thu theo loại xe');
        setText('contractStatusTitle', 'Thống kê hợp đồng theo trạng thái (' + period + ')');
        setText('profitBreakdownTitle', 'Cơ cấu lợi nhuận (' + period + ')');
    }

    function showChartLoading(show) {
        ['revenueChart', 'topVehiclesChart', 'vehicleTypesChart', 'contractStatusChart', 'profitBreakdownChart'].forEach(function (id) {
            const c = document.getElementById(id);
            if (c) c.style.opacity = show ? '0.5' : '1';
        });
    }

    function updateStatistics(filter) {
        fetch('/ThongKeBaoCao?handler=Statistics&filter=' + encodeURIComponent(filter))
            .then(r => r.json())
            .then(function (data) {
                if (!data.success) return;
                setText('doanhThuValue', data.doanhThu.toLocaleString('vi-VN') + 'đ');
                setText('doanhThuGocValue', data.doanhThuGoc.toLocaleString('vi-VN') + 'đ');
                setText('tongThietHaiValue', data.tongThietHai.toLocaleString('vi-VN') + 'đ');
                setText('donDatValue', String(data.tongDonDat));
                setText('khachHangMoiValue', String(data.khachHangMoi));
                setText('xeDangThueValue', String(data.xeDangChoThue));

                const periodLabels = {
                    today: 'hôm nay',
                    week: 'tuần này',
                    month: 'tháng này',
                    quarter: 'quý này',
                    year: 'năm nay'
                };
                const p = periodLabels[filter] || 'kỳ đã chọn';
                setText('doanhThuLabel', 'Doanh thu ' + p);
                setText('doanhThuGocLabel', 'Doanh thu gốc ' + p);
                setText('tongThietHaiLabel', 'Tổng phí đền bù ' + p);
                setText('donDatLabel', 'Đơn đặt ' + p);
                setText('khachHangMoiLabel', 'Khách hàng mới ' + p);
            })
            .catch(function () { /* ignore */ });
    }

    function updateCharts(filter) {
        showChartLoading(true);

        fetch('/ThongKeBaoCao?handler=ChartData&filter=' + encodeURIComponent(filter))
            .then(r => r.json())
            .then(function (data) {
                if (!data.success) return;
                updateChartTitles(filter);

                const dThu = data.doanhThu;
                const hasRev = dThu.data.some(function (v) { return v > 0; });

                if (revenueChart && hasRev) {
                    revenueChart.data.labels = dThu.labels;
                    revenueChart.data.datasets[0].data = dThu.data;
                    revenueChart.update();
                    adjustChartHeight('revenueChart', dThu.labels.length);
                }

                if (topVehiclesChart && data.topXe.data.some(function (v) { return v > 0; })) {
                    topVehiclesChart.data.labels = data.topXe.labels;
                    topVehiclesChart.data.datasets[0].data = data.topXe.data;
                    topVehiclesChart.update();
                }

                if (vehicleTypesChart && data.loaiXe.data.some(function (v) { return v > 0; })) {
                    vehicleTypesChart.data.labels = data.loaiXe.labels;
                    vehicleTypesChart.data.datasets[0].data = data.loaiXe.data;
                    vehicleTypesChart.update();
                    adjustChartHeight('vehicleTypesChart', data.loaiXe.labels.length);
                }

                if (contractStatusChart && data.hopDongTrangThai.data.some(function (v) { return v > 0; })) {
                    contractStatusChart.data.labels = data.hopDongTrangThai.labels;
                    contractStatusChart.data.datasets[0].data = data.hopDongTrangThai.data;
                    const sc = {
                        'Đang thuê': 'rgba(255, 193, 7, 1)',
                        'Hoàn thành': 'rgba(40, 167, 69, 1)',
                        'Hủy': 'rgba(220, 53, 69, 1)'
                    };
                    const bg = data.hopDongTrangThai.labels.map(function (l) { return sc[l] || 'rgba(108, 117, 125, 1)'; });
                    contractStatusChart.data.datasets[0].backgroundColor = bg;
                    const tongHopDong = data.hopDongTrangThai.data.reduce(function (sum, value) { return sum + value; }, 0);
                    contractStatusChart.options.plugins.centerText.value = tongHopDong.toLocaleString('vi-VN');
                    contractStatusChart.update();
                }

                if (profitBreakdownChart) {
                    const tienThue = dThu.data.reduce(function (sum, value, index) {
                        const v = value - (data.chiTieu.data[index] || 0) - (data.thietHai.data[index] || 0);
                        return sum + (v > 0 ? v : 0);
                    }, 0);
                    const phuPhi = data.chiTieu.data.reduce(function (sum, value) { return sum + value; }, 0);
                    const tienSuaChua = data.thietHai.data.reduce(function (sum, value) { return sum + value; }, 0);
                    const coDuLieuLoiNhuan = tienThue > 0 || phuPhi > 0 || tienSuaChua > 0;
                    if (coDuLieuLoiNhuan) {
                        profitBreakdownChart.data.labels = ['Tiền thuê', 'Phụ phí', 'Tiền sửa chữa'];
                        profitBreakdownChart.data.datasets[0].data = [tienThue, phuPhi, tienSuaChua];
                        const tongLoiNhuan = tienThue + phuPhi + tienSuaChua;
                        profitBreakdownChart.options.plugins.centerText.value = tongLoiNhuan.toLocaleString('vi-VN') + 'đ';
                        profitBreakdownChart.update();
                    }
                }
            })
            .catch(function () { /* ignore */ })
            .finally(function () { showChartLoading(false); });
    }

    const selFilter = document.getElementById('chartFilter');
    if (selFilter) {
        selFilter.value = chartFilter;
        selFilter.addEventListener('change', function () {
            updateCharts(this.value);
            updateStatistics(this.value);
        });
    }

    updateChartTitles(chartFilter);
    adjustChartHeight('revenueChart', revenueData.length);
    adjustChartHeight('vehicleTypesChart', vehicleTypeLabels.length);
})();
