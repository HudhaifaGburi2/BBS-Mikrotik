// Admin invoice detail page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    const invoiceId = utils.getQueryParam('id');

    if (!invoiceId) {
        utils.showToast('Invoice ID is required', 'error');
        setTimeout(() => {
            window.location.href = 'invoices.html';
        }, 2000);
        return;
    }

    async function loadInvoice() {
        try {
            utils.showLoading();
            
            const response = await api.get('/invoices/' + invoiceId);
            
            if (response && response.data) {
                const invoice = response.data;
                
                document.getElementById('invoice-number').textContent = 'Invoice #: ' + (invoice.invoiceNumber || '-');
                document.getElementById('subscriber-name').textContent = invoice.subscriberName || '-';
                document.getElementById('subscriber-email').textContent = invoice.subscriberEmail || '-';
                document.getElementById('subscriber-phone').textContent = invoice.subscriberPhone || '-';
                document.getElementById('subscriber-address').textContent = invoice.subscriberAddress || '-';
                
                document.getElementById('issue-date').textContent = utils.formatDate(invoice.issueDate);
                document.getElementById('due-date').textContent = utils.formatDate(invoice.dueDate);
                
                const statusColors = {
                    'Paid': 'text-green-600 font-semibold',
                    'Pending': 'text-yellow-600 font-semibold',
                    'Overdue': 'text-red-600 font-semibold',
                    'Cancelled': 'text-gray-600 font-semibold'
                };
                const statusClass = statusColors[invoice.status] || 'text-gray-600';
                document.getElementById('invoice-status').innerHTML = '<span class="' + statusClass + '">' + (invoice.status || '-') + '</span>';
                
                const lineItemsHtml = '<tr><td class="px-6 py-4 text-sm text-gray-900">' + (invoice.description || 'Subscription Service') + '</td><td class="px-6 py-4 text-sm text-right text-gray-900">' + utils.formatCurrency(invoice.amount || 0) + '</td></tr>';
                document.getElementById('line-items').innerHTML = lineItemsHtml;
                
                document.getElementById('subtotal').textContent = utils.formatCurrency(invoice.amount || 0);
                document.getElementById('tax').textContent = utils.formatCurrency(invoice.taxAmount || 0);
                document.getElementById('total').textContent = utils.formatCurrency(invoice.totalAmount || 0);
                
                if (invoice.status === 'Paid') {
                    document.getElementById('payment-status').classList.remove('hidden');
                    document.getElementById('payment-date').textContent = 'Paid on ' + utils.formatDate(invoice.paidDate);
                } else {
                    document.getElementById('mark-paid-section').classList.remove('hidden');
                }
            }
        } catch (error) {
            console.error('Error loading invoice:', error);
            utils.showToast('Failed to load invoice', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    window.downloadPDF = async function() {
        try {
            utils.showLoading();
            const response = await api.get('/invoices/' + invoiceId + '/pdf');
            
            if (response && response.data && response.data.pdfUrl) {
                const link = document.createElement('a');
                link.href = response.data.pdfUrl;
                link.download = 'invoice_' + invoiceId + '.pdf';
                link.click();
                utils.showToast('PDF download started', 'success');
            } else {
                window.print();
                utils.showToast('Please use print dialog to save as PDF', 'info');
            }
        } catch (error) {
            console.error('Error downloading PDF:', error);
            window.print();
            utils.showToast('Please use print dialog to save as PDF', 'info');
        } finally {
            utils.hideLoading();
        }
    };

    window.markAsPaid = async function() {
        if (!confirm('Are you sure you want to mark this invoice as paid?')) {
            return;
        }

        try {
            utils.showLoading();
            await api.put('/invoices/' + invoiceId + '/pay', {});
            utils.showToast('Invoice marked as paid', 'success');
            setTimeout(() => {
                loadInvoice();
            }, 1500);
        } catch (error) {
            console.error('Error marking invoice as paid:', error);
            utils.showToast('Failed to mark invoice as paid', 'error');
        } finally {
            utils.hideLoading();
        }
    };

    loadInvoice();
})();
