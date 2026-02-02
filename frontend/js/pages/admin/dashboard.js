// Admin dashboard page initialization
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

    async function loadDashboardStats() {
        try {
            const response = await api.get('/reports/dashboard-stats');
            
            if (response && response.data) {
                const stats = response.data;
                document.getElementById('stat-subscribers').textContent = stats.totalSubscribers || '0';
                document.getElementById('stat-active').textContent = stats.activeSubscriptions || '0';
                document.getElementById('stat-revenue').textContent = utils.formatCurrency(stats.monthlyRevenue || 0);
                document.getElementById('stat-overdue').textContent = stats.overdueInvoices || '0';
            }
        } catch (error) {
            console.error('Error loading dashboard stats:', error);
            document.getElementById('stat-subscribers').textContent = '0';
            document.getElementById('stat-active').textContent = '0';
            document.getElementById('stat-revenue').textContent = utils.formatCurrency(0);
            document.getElementById('stat-overdue').textContent = '0';
        }
    }

    async function loadRecentActivity() {
        try {
            const response = await api.get('/reports/recent-activity');
            
            if (response && response.data) {
                const columns = [
                    { label: 'Date', field: 'date', type: 'datetime' },
                    { label: 'Activity', field: 'activity' },
                    { label: 'User', field: 'user' },
                    { label: 'Details', field: 'details' }
                ];
                
                components.renderTable(response.data, columns, 'recent-activity-table');
            }
        } catch (error) {
            console.error('Error loading recent activity:', error);
            document.getElementById('recent-activity-table').innerHTML = '<div class="text-center py-8 text-gray-500"><p>No recent activity available</p></div>';
        }
    }

    async function refreshDashboard() {
        await loadDashboardStats();
        await loadRecentActivity();
        document.getElementById('last-sync').textContent = utils.formatDateTime(new Date().toISOString());
    }

    refreshDashboard();

    setInterval(refreshDashboard, 60000);
})();
