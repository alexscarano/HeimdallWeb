(function(){
    if (!window.__userStatsData) return;
    const data = window.__userStatsData;

    Chart.defaults.color = '#fff';
    Chart.defaults.borderColor = 'rgba(255, 255, 255, 0.1)';
    Chart.defaults.backgroundColor = 'rgba(0, 0, 0, 0)';

    const darkBackgroundPlugin = {
        id: 'darkBackground',
        beforeDraw: (chart) => {
            const ctx = chart.ctx;
            ctx.save();
            ctx.fillStyle = '#1a1d1e';
            ctx.fillRect(0, 0, chart.width, chart.height);
            ctx.restore();
        }
    };
    Chart.register(darkBackgroundPlugin);

    const findings = data.findingsSummary || null;
    const categories = data.categoryBreakdown || [];
    const trend = data.riskTrend || [];

    try {
        // Severity Doughnut
        const severityEl = document.getElementById('severityChart');
        if (severityEl && findings) {
            const ctx = severityEl.getContext('2d');
            const colors = { critical: 'rgb(220, 53, 69)', high: 'rgb(255, 193, 7)', medium: 'rgb(255, 165, 0)', low: 'rgb(23, 162, 184)' };
            new Chart(ctx, {
                type: 'doughnut',
                data: { labels: ['Críticas','Altas','Médias','Baixas'], datasets:[{ data: [findings.total_critical, findings.total_high, findings.total_medium, findings.total_low], backgroundColor: [colors.critical, colors.high, colors.medium, colors.low], borderColor: '#343a40', borderWidth: 2 }] },
                options: { responsive: true, maintainAspectRatio: true, plugins: { legend: { position: 'bottom', labels: { color: '#fff', padding: 15, font: { size: 12 } } }, title: { display: true, text: 'Distribuição por Severidade', color: '#fff', font: { size: 16 } } } }
            });
        }

        // Categories Horizontal Bar
        const catEl = document.getElementById('categoryChart');
        if (catEl && categories.length) {
            const ctx = catEl.getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: { labels: categories.map(c => c.main_category || 'Outros'), datasets: [{ label: 'Quantidade', data: categories.map(c => c.category_count), backgroundColor: 'rgba(54,162,235,0.8)', borderColor: 'rgba(54,162,235,1)', borderWidth: 1 }] },
                options: { indexAxis: 'y', responsive: true, maintainAspectRatio: true, plugins: { legend: { display: false }, title: { display: true, text: 'Top Categorias de Vulnerabilidades', color: '#fff', font: { size: 16 } } }, scales: { x: { ticks: { color: '#fff' }, grid: { color: 'rgba(255,255,255,0.1)' } }, y: { ticks: { color: '#fff' }, grid: { color: 'rgba(255,255,255,0.1)' } } } }
            });
        }

        // Trend Line
        const trendEl = document.getElementById('riskTrendChart');
        if (trendEl && trend.length) {
            const ctx = trendEl.getContext('2d');
            const dates = trend.map(r => r.risk_date);
            new Chart(ctx, {
                type: 'line',
                data: { labels: dates.reverse(), datasets: [
                    { label: 'Críticas', data: trend.map(r => r.critical_count).reverse(), borderColor: 'rgb(220,53,69)', backgroundColor: 'rgba(220,53,69,0.2)', tension: 0.4, fill: true },
                    { label: 'Altas', data: trend.map(r => r.high_count).reverse(), borderColor: 'rgb(255,193,7)', backgroundColor: 'rgba(255,193,7,0.2)', tension: 0.4, fill: true },
                    { label: 'Médias', data: trend.map(r => r.medium_count).reverse(), borderColor: 'rgb(255,165,0)', backgroundColor: 'rgba(255,165,0,0.2)', tension: 0.4, fill: true },
                    { label: 'Baixas', data: trend.map(r => r.low_count).reverse(), borderColor: 'rgb(23,162,184)', backgroundColor: 'rgba(23,162,184,0.2)', tension: 0.4, fill: true }
                ] },
                options: { responsive: true, maintainAspectRatio: true, interaction: { mode: 'index', intersect: false }, plugins: { legend: { position: 'top', labels: { color: '#fff', padding: 15, font: { size: 12 } } } }, scales: { x: { ticks: { color: '#fff' }, grid: { color: 'rgba(255,255,255,0.1)' } }, y: { ticks: { color: '#fff' }, grid: { color: 'rgba(255,255,255,0.1)' }, beginAtZero: true } } }
            });
        }

    } catch (ex) {
        console.error('Error initializing user statistics charts', ex);
    }
})();
