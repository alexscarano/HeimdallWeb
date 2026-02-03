(function(){
    if (!window.__dashboardData) return;
    const data = window.__dashboardData;
    // Debug: expose dataset in console to help diagnose rendering issues
    if (typeof console !== 'undefined' && console.debug) console.debug('dashboard.__dashboardData', data);

    // Global Chart defaults for dark theme
    Chart.defaults.color = '#ffffff';
    Chart.defaults.borderColor = 'rgba(255, 255, 255, 0.1)';
    Chart.defaults.backgroundColor = 'transparent';

    // Safe accessors
    const scanTrendData = data.scanTrendData || [];
    const userRegTrendData = data.userRegTrendData || [];
    const successRate = (typeof data.successRate !== 'undefined') ? data.successRate : 0;
    const failRate = (typeof data.failRate !== 'undefined') ? data.failRate : 0;
    const riskDistData = data.riskDistData || [];
    const topCategoriesData = data.topCategoriesData || [];

    try {
        // Scan Trend
        const scanTrendEl = document.getElementById('scanTrendChart');
        if (scanTrendEl && scanTrendData.length) {
            const ctx = scanTrendEl.getContext('2d');
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: scanTrendData.map(d => d.date),
                    datasets: [
                        { label: 'Total de Scans', data: scanTrendData.map(d => d.count), borderColor: 'rgb(75, 192, 192)', backgroundColor: 'rgba(75, 192, 192, 0.2)', tension: 0.3 },
                        { label: 'Sucesso', data: scanTrendData.map(d => d.success), borderColor: 'rgb(40, 167, 69)', backgroundColor: 'rgba(40, 167, 69, 0.2)', tension: 0.3 },
                        { label: 'Falha', data: scanTrendData.map(d => d.failed), borderColor: 'rgb(220, 53, 69)', backgroundColor: 'rgba(220, 53, 69, 0.2)', tension: 0.3 }
                    ]
                },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: true, position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
            });
        }

        // Success Rate
        const successRateEl = document.getElementById('successRateChart');
        if (successRateEl) {
            const ctx = successRateEl.getContext('2d');
            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: ['Sucesso', 'Falha'],
                    datasets: [{ data: [successRate, failRate], backgroundColor: ['rgba(40, 167, 69, 0.8)', 'rgba(220, 53, 69, 0.8)'], borderColor: ['rgb(40, 167, 69)', 'rgb(220, 53, 69)'], borderWidth: 2 }]
                },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: true, position: 'bottom' } } }
            });
        }

        // New Users
        const userRegEl = document.getElementById('userRegistrationChart');
        if (userRegEl && userRegTrendData.length) {
            const ctx = userRegEl.getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: { labels: userRegTrendData.map(d => d.date), datasets: [{ label: 'Novos Usuários', data: userRegTrendData.map(d => d.count), backgroundColor: 'rgba(54, 162, 235, 0.6)', borderColor: 'rgb(54, 162, 235)', borderWidth: 1 }] },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } } }
            });
        }

        // Risk Distribution
        const riskDistEl = document.getElementById('riskDistributionChart');
        if (riskDistEl && riskDistData.length) {
            const ctx = riskDistEl.getContext('2d');
            const severityColors = { critical: 'rgb(220, 53, 69)', high: 'rgb(255, 193, 7)', medium: 'rgb(255, 165, 0)', low: 'rgb(23, 162, 184)' };
            new Chart(ctx, {
                type: 'line',
                data: { labels: riskDistData.map(d => d.date), datasets: [
                    { label: 'Críticas', data: riskDistData.map(d => d.critical), borderColor: severityColors.critical, backgroundColor: severityColors.critical + '33', tension: 0.4, fill: true },
                    { label: 'Altas', data: riskDistData.map(d => d.high), borderColor: severityColors.high, backgroundColor: severityColors.high + '33', tension: 0.4, fill: true },
                    { label: 'Médias', data: riskDistData.map(d => d.medium), borderColor: severityColors.medium, backgroundColor: severityColors.medium + '33', tension: 0.4, fill: true },
                    { label: 'Baixas', data: riskDistData.map(d => d.low), borderColor: severityColors.low, backgroundColor: severityColors.low + '33', tension: 0.4, fill: true }
                ] },
                options: { responsive: true, maintainAspectRatio: true, interaction: { mode: 'index', intersect: false }, plugins: { legend: { position: 'top', labels: { color: '#fff', padding: 10, font: { size: 11 } }, title: { display: true } } }, scales: { x: { ticks: { color: '#fff' }, grid: { color: 'rgba(255,255,255,0.1)' } }, y: { ticks: { color: '#fff' }, grid: { color: 'rgba(255,255,255,0.1)' }, beginAtZero: true } } }
            });
        }

        // Top Categories
        const topCatsEl = document.getElementById('topCategoriesChart');
        if (topCatsEl && topCategoriesData.length) {
            const ctx = topCatsEl.getContext('2d');
            new Chart(ctx, {
                type: 'doughnut',
                data: { labels: topCategoriesData.map(c => c.category), datasets: [{ data: topCategoriesData.map(c => c.count), backgroundColor: [ 'rgba(220,53,69,0.8)', 'rgba(255,193,7,0.8)', 'rgba(255,165,0,0.8)', 'rgba(23,162,184,0.8)' ], borderColor: '#343a40', borderWidth: 2 }] },
                options: { responsive: true, maintainAspectRatio: true, plugins: { legend: { position: 'bottom', labels: { color: '#fff' } } } }
            });
        }

    } catch (ex) {
        console.error('Error initializing admin dashboard charts', ex);
    }
})();
