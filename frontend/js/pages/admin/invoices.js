// Admin invoices page initialization
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
    let invoicesData = [];

    async function loadInvoices() {
        try {
            utils.showLoading();
            
            const statusFilter = document.getElementById('status-filter').value;
            const fromDate = document.getElementById('from-date').value;
            const toDate = document.getElementById('to-date').value;

            const queryParams = new URLSearchParams({
                pageNumber: currentPage,
                pageSize: pageSize
            });

            if (statusFilter) queryParams.append('status', statusFilter);
            if (fromDate) queryParams.append('fromDate', fromDate);
            if (toDate) queryParams.append('toDate', toDate);

            const response = await api.get('/invoices?' + queryParams.toString());
            
            if (response && response.data) {
                invoicesData = response.data.items || response.data;
                const totalPages = response.data.totalPages || 1;
                
                const columns = [
                    { label: 'Invoice #', field: 'invoiceNumber' },
                    { label: 'Subscriber', field: 'subscriberName' },
                    { label: 'Amount', field: 'amount', type: 'currency' },
                    { label: 'Tax', field: 'taxAmount', type: 'currency' },
                    { label: 'Total', field: 'totalAmount', type: 'currency' },
                    { label: 'Issue Date', field: 'issueDate', type: 'date' },
                    { label: 'Due Date', field: 'dueDate', type: 'date' },
                    { 
                        label: 'Status', 
                        field: 'status',
                        render: (row) => {
                            const statusColors = {
                                'Paid': 'bg-green-100 text-green-800',
                                'Pending': 'bg-yellow-100 text-yellow-800',
                                'Overdue': 'bg-red-100 text-red-800',
                                'Cancelled': 'bg-gray-100 text-gray-800'
                            };
                            const colorClass = statusColors[row.status] || 'bg-gray-100 text-gray-800';
                            return '<span class="px-2 py-1 text-xs font-semibold rounded-full ' + colorClass + '">' + row.status + '</span>';
                        }
                    },
                    { 
                        label: 'Actions', 
                        field: 'id',
                        render: (row) => '<div class="flex space-x-2 no-print"><a href="invoice-detail.html?id=' + row.id + '" class="text-blue-600 hover:text-blue-800"><svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path></svg></a></div>'
                    }
                ];
                
                components.renderTable(invoicesData, columns, 'invoices-table');
                components.renderPagination(currentPage, totalPages, 'goToPage');
            }
        } catch (error) {
            console.error('Error loading invoices:', error);
            utils.showToast('Failed to load invoices', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    window.goToPage = function(page) {
        currentPage = page;
        loadInvoices();
    };

    window.exportToCSV = function() {
        if (invoicesData.length === 0) {
            utils.showToast('No data to export', 'warning');
            return;
        }

        const headers = ['Invoice #', 'Subscriber', 'Amount', 'Tax', 'Total', 'Issue Date', 'Due Date', 'Status'];
        const rows = invoicesData.map(invoice => [
            invoice.invoiceNumber || '',
            invoice.subscriberName || '',
            invoice.amount || 0,
            invoice.taxAmount || 0,
            invoice.totalAmount || 0,
            utils.formatDate(invoice.issueDate),
            utils.formatDate(invoice.dueDate),
            invoice.status || ''
        ]);

        let csvContent = headers.join(',') + '\n';
        rows.forEach(row => {
            csvContent += row.map(cell => '"' + cell + '"').join(',') + '\n';
        });

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        
        link.setAttribute('href', url);
        link.setAttribute('download', 'invoices_' + new Date().toISOString().split('T')[0] + '.csv');
        link.style.visibility = 'hidden';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        utils.showToast('CSV exported successfully', 'success');
    };

    window.loadInvoices = loadInvoices;

    const today = new Date().toISOString().split('T')[0];
    const firstDay = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0];
    document.getElementById('from-date').value = firstDay;
    document.getElementById('to-date').value = today;

    loadInvoices();
})();
