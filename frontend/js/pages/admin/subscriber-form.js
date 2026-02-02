// Admin subscriber form page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    const mode = utils.getQueryParam('mode') || 'create';
    const subscriberId = utils.getQueryParam('id');

    document.getElementById('form-title').textContent = mode === 'edit' ? 'Edit Subscriber' : 'Add Subscriber';

    const today = new Date().toISOString().split('T')[0];
    document.getElementById('startDate').value = today;

    async function loadPlans() {
        try {
            const response = await api.get('/plans');
            const plans = response.data || [];
            
            const planSelect = document.getElementById('planId');
            planSelect.innerHTML = '<option value="">Select a plan...</option>';
            
            plans.forEach(plan => {
                const option = document.createElement('option');
                option.value = plan.id;
                option.textContent = plan.name + ' - ' + utils.formatCurrency(plan.price) + ' (' + plan.speedMbps + ' Mbps)';
                planSelect.appendChild(option);
            });
        } catch (error) {
            console.error('Error loading plans:', error);
            utils.showToast('Failed to load plans', 'error');
        }
    }

    async function loadSubscriber() {
        if (mode !== 'edit' || !subscriberId) return;

        try {
            utils.showLoading();
            const response = await api.get('/subscribers/' + subscriberId);
            
            if (response && response.data) {
                const subscriber = response.data;
                document.getElementById('fullName').value = subscriber.fullName || '';
                document.getElementById('email').value = subscriber.email || '';
                document.getElementById('phoneNumber').value = subscriber.phoneNumber || '';
                document.getElementById('nationalId').value = subscriber.nationalId || '';
                document.getElementById('address').value = subscriber.address || '';
            }
        } catch (error) {
            console.error('Error loading subscriber:', error);
            utils.showToast('Failed to load subscriber data', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    function validateForm() {
        const errors = [];
        
        const fullName = document.getElementById('fullName').value.trim();
        const email = document.getElementById('email').value.trim();
        const phoneNumber = document.getElementById('phoneNumber').value.trim();
        const address = document.getElementById('address').value.trim();
        const planId = document.getElementById('planId').value;

        if (!fullName) {
            errors.push({ field: 'fullName', message: 'Full name is required' });
        }

        if (!email) {
            errors.push({ field: 'email', message: 'Email is required' });
        } else if (!utils.validateEmail(email)) {
            errors.push({ field: 'email', message: 'Invalid email format' });
        }

        if (!phoneNumber) {
            errors.push({ field: 'phoneNumber', message: 'Phone number is required' });
        } else if (!utils.validatePhone(phoneNumber)) {
            errors.push({ field: 'phoneNumber', message: 'Invalid phone number format (use +966 5XXXXXXXX)' });
        }

        if (!address) {
            errors.push({ field: 'address', message: 'Address is required' });
        }

        if (!planId) {
            errors.push({ field: 'planId', message: 'Please select a plan' });
        }

        document.querySelectorAll('.error-message').forEach(el => {
            el.classList.add('hidden');
            el.textContent = '';
        });

        errors.forEach(error => {
            const field = document.getElementById(error.field);
            if (field) {
                const errorEl = field.parentElement.querySelector('.error-message');
                if (errorEl) {
                    errorEl.textContent = error.message;
                    errorEl.classList.remove('hidden');
                }
                field.classList.add('border-red-500');
            }
        });

        return errors.length === 0;
    }

    document.getElementById('subscriber-form').addEventListener('submit', async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            utils.showToast('Please fix the errors in the form', 'error');
            return;
        }

        const formData = {
            fullName: document.getElementById('fullName').value.trim(),
            email: document.getElementById('email').value.trim(),
            phoneNumber: document.getElementById('phoneNumber').value.trim(),
            nationalId: document.getElementById('nationalId').value.trim() || null,
            address: document.getElementById('address').value.trim(),
            planId: document.getElementById('planId').value,
            startDate: document.getElementById('startDate').value,
            autoRenew: document.getElementById('autoRenew').checked
        };

        try {
            utils.showLoading();

            if (mode === 'edit' && subscriberId) {
                await api.put('/subscribers/' + subscriberId, formData);
                utils.showToast('Subscriber updated successfully', 'success');
            } else {
                await api.post('/subscribers', formData);
                utils.showToast('Subscriber created successfully', 'success');
            }

            setTimeout(() => {
                window.location.href = 'subscribers.html';
            }, 1500);
        } catch (error) {
            console.error('Error saving subscriber:', error);
            utils.showToast(error.message || 'Failed to save subscriber', 'error');
        } finally {
            utils.hideLoading();
        }
    });

    document.querySelectorAll('input, select, textarea').forEach(field => {
        field.addEventListener('input', () => {
            field.classList.remove('border-red-500');
            const errorEl = field.parentElement.querySelector('.error-message');
            if (errorEl) {
                errorEl.classList.add('hidden');
            }
        });
    });

    loadPlans();
    loadSubscriber();
})();
