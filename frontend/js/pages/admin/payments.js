// Admin payments page initialization
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

    let currentPage = 1;
    const pageSize = 20;
    let paymentsData = [];

    async function loadPayments() {
        try {
            utils.showLoading();
            
            const fromDate = document.getElementById('from-date').value;
            const toDate = document.getElementById('to-date').value;

            const queryParams = new URLSearchParams({
                pageNumber: currentPage,
                pageSize: pageSize
            });

            if (fromDate) queryParams.append('fromDate', fromDate);
            if (toDate) queryParams.append('toDate', toDate);

            const response = await api.get('/payments?' + queryParams.toString());
            
            if (response && response.data) {
                paymentsData = response.data.items || response.data;
                const totalPages = response.data.totalPages || 1;
                
                const columns = [
                    { label: 'Date', field: 'paymentDate', type: 'datetime' },
                    { label: 'Subscriber', field: 'subscriberName' },
                    { label: 'Amount', field: 'amount', type: 'currency' },
                    { 
                        label: 'Method', 
                        field: 'paymentMethod',
                        render: (row) => {
                            const methodIcons = {
                                'Cash': '💵',
                                'Card': '💳',
                                'BankTransfer': '🏦',
                                'Online': '🌐'
                            };
                            const icon = methodIcons[row.paymentMethod] || '💰';
                            return icon + ' ' + (row.paymentMethod || 'Unknown');
                        }
                    },
                    { 
                        label: 'Transaction ID', 
                        field: 'transactionId',
                        render: (row) => row.transactionId || '-'
                    },
                    { 
                        label: 'Status', 
                        field: 'status',
                        render: (row) => {
                            const statusColors = {
                                'Completed': 'bg-green-100 text-green-800',
                                'Pending': 'bg-yellow-100 text-yellow-800',
                                'Failed': 'bg-red-100 text-red-800',
                                'Refunded': 'bg-gray-100 text-gray-800'
                            };
                            const colorClass = statusColors[row.status] || 'bg-gray-100 text-gray-800';
                            return '<span class="px-2 py-1 text-xs font-semibold rounded-full ' + colorClass + '">' + (row.status || 'Unknown') + '</span>';
                        }
                    }
                ];
                
                components.renderTable(paymentsData, columns, 'payments-table');
                components.renderPagination(currentPage, totalPages, 'goToPage');
            }
        } catch (error) {
            console.error('Error loading payments:', error);
            utils.showToast('Failed to load payments', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    window.goToPage = function(page) {
        currentPage = page;
        loadPayments();
    };

    window.exportToCSV = function() {
        if (paymentsData.length === 0) {
            utils.showToast('No data to export', 'warning');
            return;
        }

        const headers = ['Date', 'Subscriber', 'Amount', 'Method', 'Transaction ID', 'Status'];
        const rows = paymentsData.map(payment => [
            utils.formatDateTime(payment.paymentDate),
            payment.subscriberName || '',
            payment.amount || 0,
            payment.paymentMethod || '',
            payment.transactionId || '',
            payment.status || ''
        ]);

        let csvContent = headers.join(',') + '\n';
        rows.forEach(row => {
            csvContent += row.map(cell => '"' + cell + '"').join(',') + '\n';
        });

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        
        link.setAttribute('href', url);
        link.setAttribute('download', 'payments_' + new Date().toISOString().split('T')[0] + '.csv');
        link.style.visibility = 'hidden';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        utils.showToast('CSV exported successfully', 'success');
    };

    window.loadPayments = loadPayments;

    const today = new Date().toISOString().split('T')[0];
    const firstDay = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0];
    document.getElementById('from-date').value = firstDay;
    document.getElementById('to-date').value = today;

    loadPayments();
})();
