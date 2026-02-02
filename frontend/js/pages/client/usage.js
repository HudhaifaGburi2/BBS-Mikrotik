// Client usage page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    let usageChart = null;
    let usageData = [];

    async function loadUsageStatistics() {
        try {
            utils.showLoading();
            
            const response = await api.get('/usage/daily?days=30');
            
            if (response && response.data) {
                usageData = response.data;
                
                const totalUsage = usageData.reduce((sum, item) => sum + (item.dataGB || 0), 0);
                const avgUsage = usageData.length > 0 ? totalUsage / usageData.length : 0;
                
                document.getElementById('total-usage').textContent = totalUsage.toFixed(2) + ' GB';
                document.getElementById('avg-usage').textContent = avgUsage.toFixed(2) + ' GB';
                
                const subscription = await api.get('/subscriptions/me');
                if (subscription && subscription.data) {
                    const dataLimit = subscription.data.dataLimitGB || 0;
                    const remaining = Math.max(0, dataLimit - totalUsage);
                    document.getElementById('remaining-usage').textContent = dataLimit > 0 ? remaining.toFixed(2) + ' GB' : 'Unlimited';
                }
                
                renderUsageChart();
                renderUsageTable();
            }
        } catch (error) {
            console.error('Error loading usage statistics:', error);
            utils.showToast('Failed to load usage statistics', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    function renderUsageChart() {
        const ctx = document.getElementById('usage-chart').getContext('2d');
        
        if (usageChart) {
            usageChart.destroy();
        }

        usageChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: usageData.map(item => utils.formatDate(item.date)),
                datasets: [{
                    label: 'Data Usage (GB)',
                    data: usageData.map(item => item.dataGB || 0),
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    borderColor: 'rgba(59, 130, 246, 1)',
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4,
                    pointRadius: 3,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Data Usage (GB)'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Date'
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return 'Usage: ' + context.parsed.y.toFixed(2) + ' GB';
                            }
                        }
                    }
                }
            }
        });
    }

    function renderUsageTable() {
        const columns = [
            { label: 'Date', field: 'date', type: 'date' },
            { 
                label: 'Download (GB)', 
                field: 'downloadGB',
                render: (row) => (row.downloadGB || 0).toFixed(2)
            },
            { 
                label: 'Upload (GB)', 
                field: 'uploadGB',
                render: (row) => (row.uploadGB || 0).toFixed(2)
            },
            { 
                label: 'Total (GB)', 
                field: 'dataGB',
                render: (row) => (row.dataGB || 0).toFixed(2)
            },
            { 
                label: 'Session Duration', 
                field: 'sessionDuration',
                render: (row) => {
                    const minutes = row.sessionDuration || 0;
                    const hours = Math.floor(minutes / 60);
                    const mins = minutes % 60;
                    return hours + 'h ' + mins + 'm';
                }
            }
        ];
        
        components.renderTable(usageData.slice().reverse(), columns, 'usage-table');
    }

    window.downloadReport = function() {
        if (usageData.length === 0) {
            utils.showToast('No data to export', 'warning');
            return;
        }

        const headers = ['Date', 'Download (GB)', 'Upload (GB)', 'Total (GB)', 'Session Duration (min)'];
        const rows = usageData.map(item => [
            utils.formatDate(item.date),
            (item.downloadGB || 0).toFixed(2),
            (item.uploadGB || 0).toFixed(2),
            (item.dataGB || 0).toFixed(2),
            item.sessionDuration || 0
        ]);

        let csvContent = headers.join(',') + '\n';
        rows.forEach(row => {
            csvContent += row.map(cell => '"' + cell + '"').join(',') + '\n';
        });

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        
        link.setAttribute('href', url);
        link.setAttribute('download', 'usage_report_' + new Date().toISOString().split('T')[0] + '.csv');
        link.style.visibility = 'hidden';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        utils.showToast('Usage report downloaded successfully', 'success');
    };

    loadUsageStatistics();
})();
