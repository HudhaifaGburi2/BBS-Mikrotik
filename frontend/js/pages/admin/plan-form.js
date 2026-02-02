// Admin plan form page initialization
(function() {
    if (!auth.requireAuth()) {
        throw new Error('Authentication required');
    }

    const mode = utils.getQueryParam('mode') || 'create';
    const planId = utils.getQueryParam('id');

    document.getElementById('form-title').textContent = mode === 'edit' ? 'Edit Plan' : 'Add Plan';

    async function loadPlan() {
        if (mode !== 'edit' || !planId) return;

        try {
            utils.showLoading();
            const response = await api.get('/plans/' + planId);
            
            if (response && response.data) {
                const plan = response.data;
                document.getElementById('name').value = plan.name || '';
                document.getElementById('description').value = plan.description || '';
                document.getElementById('downloadSpeedMbps').value = plan.downloadSpeedMbps || '';
                document.getElementById('uploadSpeedMbps').value = plan.uploadSpeedMbps || '';
                document.getElementById('price').value = plan.price || '';
                document.getElementById('durationDays').value = plan.durationDays || 30;
                document.getElementById('dataLimitGB').value = plan.dataLimitGB || 0;
                document.getElementById('mikrotikProfile').value = plan.mikrotikProfile || '';
                document.getElementById('isActive').checked = plan.isActive !== false;
            }
        } catch (error) {
            console.error('Error loading plan:', error);
            utils.showToast('Failed to load plan data', 'error');
        } finally {
            utils.hideLoading();
        }
    }

    function validateForm() {
        const errors = [];
        
        const name = document.getElementById('name').value.trim();
        const downloadSpeed = document.getElementById('downloadSpeedMbps').value;
        const uploadSpeed = document.getElementById('uploadSpeedMbps').value;
        const price = document.getElementById('price').value;
        const dataLimit = document.getElementById('dataLimitGB').value;

        if (!name) errors.push({ field: 'name', message: 'Plan name is required' });
        if (!downloadSpeed || downloadSpeed < 1) errors.push({ field: 'downloadSpeedMbps', message: 'Download speed must be at least 1 Mbps' });
        if (!uploadSpeed || uploadSpeed < 1) errors.push({ field: 'uploadSpeedMbps', message: 'Upload speed must be at least 1 Mbps' });
        if (!price || price < 0) errors.push({ field: 'price', message: 'Price must be a positive number' });
        if (dataLimit === '' || dataLimit < 0) errors.push({ field: 'dataLimitGB', message: 'Data limit must be 0 or greater' });

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

    document.getElementById('plan-form').addEventListener('submit', async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            utils.showToast('Please fix the errors in the form', 'error');
            return;
        }

        const formData = {
            name: document.getElementById('name').value.trim(),
            description: document.getElementById('description').value.trim() || null,
            downloadSpeedMbps: parseInt(document.getElementById('downloadSpeedMbps').value),
            uploadSpeedMbps: parseInt(document.getElementById('uploadSpeedMbps').value),
            price: parseFloat(document.getElementById('price').value),
            durationDays: parseInt(document.getElementById('durationDays').value),
            dataLimitGB: parseInt(document.getElementById('dataLimitGB').value),
            mikrotikProfile: document.getElementById('mikrotikProfile').value.trim() || null,
            isActive: document.getElementById('isActive').checked
        };

        try {
            utils.showLoading();

            if (mode === 'edit' && planId) {
                await api.put('/plans/' + planId, formData);
                utils.showToast('Plan updated successfully', 'success');
            } else {
                await api.post('/plans', formData);
                utils.showToast('Plan created successfully', 'success');
            }

            setTimeout(() => {
                window.location.href = 'plans.html';
            }, 1500);
        } catch (error) {
            console.error('Error saving plan:', error);
            utils.showToast(error.message || 'Failed to save plan', 'error');
        } finally {
            utils.hideLoading();
        }
    });

    document.querySelectorAll('input, select, textarea').forEach(field => {
        field.addEventListener('input', () => {
            field.classList.remove('border-red-500');
            const errorEl = field.parentElement.querySelector('.error-message');
            if (errorEl) errorEl.classList.add('hidden');
        });
    });

    loadPlan();
})();
