// Client invoices page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    let currentPage = 1;
    const pageSize = 20;

    async function loadInvoices() {
        try {
            utils.showLoading();
            
            const statusFilter = document.getElementById('status-filter').value;

            const queryParams = new URLSearchParams({
                pageNumber: currentPage,
                pageSize: pageSize
            });

            if (statusFilter) {
                queryParams.append('status', statusFilter);
            }

            const response = await api.get('/invoices/me?' + queryParams.toString());
            
            if (response && response.data) {
                const invoices = response.data.items || response.data;
                const totalPages = response.data.totalPages || 1;
                
                const columns = [
                    { label: 'Invoice #', field: 'invoiceNumber' },
                    { label: 'Amount', field: 'totalAmount', type: 'currency' },
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
                        render: (row) => {
                            const viewButton = '<a href="../admin/invoice-detail.html?id=' + row.id + '" target="_blank" class="text-blue-600 hover:text-blue-800 mr-3">عرض</a>';
                            const payButton = row.status !== 'Paid' ? 
                                '<a href="payment.html?invoiceId=' + row.id + '" class="text-green-600 hover:text-green-800">Pay Now</a>' : 
                                '<span class="text-gray-400">مدفوع</span>';
                            return '<div class="flex items-center space-x-3">' + viewButton + payButton + '</div>';
                        }
                    }
                ];
                
                components.renderTable(invoices, columns, 'invoices-table');
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

    document.getElementById('status-filter').addEventListener('change', () => {
        currentPage = 1;
        loadInvoices();
    });

    loadInvoices();
})();
