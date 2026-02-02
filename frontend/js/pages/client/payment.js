// Client payment page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    let selectedMethod = 'mada';

    const invoiceId = utils.getQueryParam('invoiceId');
    if (invoiceId) {
        loadInvoiceAmount(invoiceId);
    }

    window.selectPaymentMethod = function(method) {
        selectedMethod = method;
        
        document.getElementById('btn-mada').className = 'flex-1 px-4 py-3 border-2 border-gray-300 bg-white text-gray-700 rounded-lg font-medium hover:border-gray-400 transition-colors';
        document.getElementById('btn-visa').className = 'flex-1 px-4 py-3 border-2 border-gray-300 bg-white text-gray-700 rounded-lg font-medium hover:border-gray-400 transition-colors';
        document.getElementById('btn-mastercard').className = 'flex-1 px-4 py-3 border-2 border-gray-300 bg-white text-gray-700 rounded-lg font-medium hover:border-gray-400 transition-colors';
        
        document.getElementById('btn-' + method).className = 'flex-1 px-4 py-3 border-2 border-blue-600 bg-blue-50 text-blue-600 rounded-lg font-medium transition-colors';
    };

    document.getElementById('amount').addEventListener('input', (e) => {
        const amount = parseFloat(e.target.value) || 0;
        document.getElementById('summary-amount').textContent = utils.formatCurrency(amount);
        document.getElementById('summary-total').textContent = utils.formatCurrency(amount);
    });

    document.getElementById('card-number').addEventListener('input', (e) => {
        let value = e.target.value.replace(/\s/g, '');
        let formatted = value.match(/.{1,4}/g);
        e.target.value = formatted ? formatted.join(' ') : value;
    });

    document.getElementById('expiry').addEventListener('input', (e) => {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length >= 2) {
            value = value.slice(0, 2) + '/' + value.slice(2, 4);
        }
        e.target.value = value;
    });

    document.getElementById('cvv').addEventListener('input', (e) => {
        e.target.value = e.target.value.replace(/\D/g, '');
    });

    async function loadInvoiceAmount(invoiceId) {
        try {
            const response = await api.get('/invoices/' + invoiceId);
            if (response && response.data) {
                document.getElementById('amount').value = response.data.totalAmount || 0;
                document.getElementById('amount').dispatchEvent(new Event('input'));
            }
        } catch (error) {
            console.error('Error loading invoice:', error);
        }
    }

    document.getElementById('payment-form').addEventListener('submit', async (e) => {
        e.preventDefault();

        const amount = parseFloat(document.getElementById('amount').value);
        const cardNumber = document.getElementById('card-number').value.replace(/\s/g, '');
        const expiry = document.getElementById('expiry').value;
        const cvv = document.getElementById('cvv').value;
        const cardholderName = document.getElementById('cardholder-name').value;

        if (!amount || amount <= 0) {
            utils.showToast('Please enter a valid amount', 'error');
            return;
        }

        if (cardNumber.length < 15 || cardNumber.length > 16) {
            utils.showToast('Please enter a valid card number', 'error');
            return;
        }

        if (!/^\d{2}\/\d{2}$/.test(expiry)) {
            utils.showToast('Please enter a valid expiry date (MM/YY)', 'error');
            return;
        }

        if (cvv.length < 3 || cvv.length > 4) {
            utils.showToast('Please enter a valid CVV', 'error');
            return;
        }

        if (!cardholderName.trim()) {
            utils.showToast('Please enter cardholder name', 'error');
            return;
        }

        try {
            utils.showLoading();

            const paymentData = {
                amount: amount,
                paymentMethod: selectedMethod,
                cardLast4: cardNumber.slice(-4),
                invoiceId: invoiceId || null
            };

            const response = await api.post('/payments/initiate', paymentData);

            if (response && response.data) {
                utils.showToast('Payment processed successfully!', 'success');
                
                setTimeout(() => {
                    if (invoiceId) {
                        window.location.href = 'invoices.html';
                    } else {
                        window.location.href = 'dashboard.html';
                    }
                }, 2000);
            }
        } catch (error) {
            console.error('Error processing payment:', error);
            utils.showToast('Payment failed. Please try again.', 'error');
        } finally {
            utils.hideLoading();
        }
    });

    async function loadTransactions() {
        try {
            const response = await api.get('/payments?pageSize=10');
            
            if (response && response.data) {
                const transactions = response.data.items || response.data;
                
                const columns = [
                    { label: 'Date', field: 'paymentDate', type: 'datetime' },
                    { label: 'Amount', field: 'amount', type: 'currency' },
                    { label: 'Method', field: 'paymentMethod' },
                    { label: 'Transaction ID', field: 'transactionId' },
                    { 
                        label: 'Status', 
                        field: 'status',
                        render: (row) => {
                            const colors = {
                                'Completed': 'bg-green-100 text-green-800',
                                'Pending': 'bg-yellow-100 text-yellow-800',
                                'Failed': 'bg-red-100 text-red-800'
                            };
                            const colorClass = colors[row.status] || 'bg-gray-100 text-gray-800';
                            return '<span class="px-2 py-1 text-xs font-semibold rounded-full ' + colorClass + '">' + row.status + '</span>';
                        }
                    }
                ];
                
                components.renderTable(transactions, columns, 'transactions-table');
            }
        } catch (error) {
            console.error('Error loading transactions:', error);
        }
    }

    loadTransactions();
})();
