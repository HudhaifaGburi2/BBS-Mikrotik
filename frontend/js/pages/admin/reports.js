// Admin reports page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    const user = auth.getUser();
    if (user && user.fullName) {
        document.getElementById('user-name').textContent = user.fullName;
    }

    const menuToggle = document.getElementById('menu-toggle');
    const sidebar = document.getElementById('sidebar');
    const closeSidebar = document.getElementById('close-sidebar');

    menuToggle.addEventListener('click', () => {
        sidebar.classList.toggle('-translate-x-full');
    });

    closeSidebar.addEventListener('click', () => {
        sidebar.classList.add('-translate-x-full');
    });

    const today = new Date().toISOString().split('T')[0];
    const firstDay = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0];
    document.getElementById('from-date').value = firstDay;
    document.getElementById('to-date').value = today;

    let reportChart = null;
    let reportData = [];

    window.generateReport = async function() {
        try {
            utils.showLoading();
            
            const reportType = document.getElementById('report-type').value;
            const fromDate = document.getElementById('from-date').value;
            const toDate = document.getElementById('to-date').value;

            const queryParams = new URLSearchParams({
                from: fromDate,
                to: toDate
            });

            const response = await api.get('/reports/' + reportType + '?' + queryParams.toString());
            
            if (response && response.data) {
                reportData = response.data;
                renderChart(reportType, reportData);
                renderTable(reportType, reportData);
                
                document.getElementById('chart-section').classList.remove('hidden');
                document.getElementById('table-section').classList.remove('hidden');
                
                utils.showToast('Report generated successfully', 'success');
            }
        } catch (error) {
            console.error('Error generating report:', error);
            utils.showToast('Failed to generate report', 'error');
        } finally {
            utils.hideLoading();
        }
    };

    function renderChart(reportType, data) {
        const ctx = document.getElementById('report-chart').getContext('2d');
        
        if (reportChart) {
            reportChart.destroy();
        }

        let chartConfig = {};

        if (reportType === 'revenue') {
            chartConfig = {
                type: 'bar',
                data: {
                    labels: data.map(item => item.period || item.month || item.date),
                    datasets: [{
                        label: 'Revenue (SAR)',
                        data: data.map(item => item.revenue || item.amount || 0),
                        backgroundColor: 'rgba(59, 130, 246, 0.5)',
                        borderColor: 'rgba(59, 130, 246, 1)',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: { y: { beginAtZero: true } }
                }
            };
        } else if (reportType === 'usage') {
            chartConfig = {
                type: 'line',
                data: {
                    labels: data.map(item => item.date || item.period),
                    datasets: [{
                        label: 'Data Usage (GB)',
                        data: data.map(item => item.usage || item.dataGB || 0),
                        backgroundColor: 'rgba(16, 185, 129, 0.1)',
                        borderColor: 'rgba(16, 185, 129, 1)',
                        borderWidth: 2,
                        fill: true,
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: { y: { beginAtZero: true } }
                }
            };
        } else {
            chartConfig = {
                type: 'bar',
                data: {
                    labels: data.map(item => item.label || item.name || item.subscriber),
                    datasets: [{
                        label: 'Count',
                        data: data.map(item => item.count || item.value || 1),
                        backgroundColor: 'rgba(249, 115, 22, 0.5)',
                        borderColor: 'rgba(249, 115, 22, 1)',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: { y: { beginAtZero: true } }
                }
            };
        }

        reportChart = new Chart(ctx, chartConfig);
    }

    function renderTable(reportType, data) {
        let columns = [];

        if (reportType === 'revenue') {
            columns = [
                { label: 'Period', field: 'period' },
                { label: 'Revenue', field: 'revenue', type: 'currency' },
                { label: 'Invoices', field: 'invoiceCount' },
                { label: 'Subscribers', field: 'subscriberCount' }
            ];
        } else if (reportType === 'usage') {
            columns = [
                { label: 'Date', field: 'date', type: 'date' },
                { label: 'Data Usage (GB)', field: 'usage', render: (row) => (row.usage || 0) + ' GB' },
                { label: 'Active Users', field: 'activeUsers' }
            ];
        } else if (reportType === 'overdue') {
            columns = [
                { label: 'Subscriber', field: 'subscriberName' },
                { label: 'Invoice #', field: 'invoiceNumber' },
                { label: 'Amount', field: 'amount', type: 'currency' },
                { label: 'Due Date', field: 'dueDate', type: 'date' },
                { label: 'Days Overdue', field: 'daysOverdue' }
            ];
        } else if (reportType === 'newsubscribers') {
            columns = [
                { label: 'Name', field: 'fullName' },
                { label: 'Email', field: 'email' },
                { label: 'Plan', field: 'planName' },
                { label: 'Join Date', field: 'createdAt', type: 'date' }
            ];
        }

        components.renderTable(data, columns, 'report-table');
    }

    window.exportToExcel = function() {
        if (reportData.length === 0) {
            utils.showToast('No data to export', 'warning');
            return;
        }

        const reportType = document.getElementById('report-type').value;
        let headers = [];
        let rows = [];

        if (reportType === 'revenue') {
            headers = ['Period', 'Revenue', 'Invoices', 'Subscribers'];
            rows = reportData.map(item => [item.period || '', item.revenue || 0, item.invoiceCount || 0, item.subscriberCount || 0]);
        } else if (reportType === 'usage') {
            headers = ['Date', 'Usage (GB)', 'Active Users'];
            rows = reportData.map(item => [utils.formatDate(item.date), item.usage || 0, item.activeUsers || 0]);
        } else if (reportType === 'overdue') {
            headers = ['Subscriber', 'Invoice #', 'Amount', 'Due Date', 'Days Overdue'];
            rows = reportData.map(item => [item.subscriberName || '', item.invoiceNumber || '', item.amount || 0, utils.formatDate(item.dueDate), item.daysOverdue || 0]);
        } else if (reportType === 'newsubscribers') {
            headers = ['Name', 'Email', 'Plan', 'Join Date'];
            rows = reportData.map(item => [item.fullName || '', item.email || '', item.planName || '', utils.formatDate(item.createdAt)]);
        }

        let csvContent = headers.join(',') + '\n';
        rows.forEach(row => {
            csvContent += row.map(cell => '"' + cell + '"').join(',') + '\n';
        });

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        
        link.setAttribute('href', url);
        link.setAttribute('download', 'report_' + reportType + '_' + new Date().toISOString().split('T')[0] + '.csv');
        link.style.visibility = 'hidden';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        utils.showToast('Report exported successfully', 'success');
    };
})();
