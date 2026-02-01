const utils = {
    formatCurrency(amount, currency = 'SAR') {
        return new Intl.NumberFormat('ar-SA', {
            style: 'currency',
            currency: currency
        }).format(amount);
    },

    formatDate(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        }).format(date);
    },

    formatDateTime(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }).format(date);
    },

    validateEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    },

    validatePhone(phone) {
        const phoneRegex = /^(\+966|0)?5\d{8}$/;
        return phoneRegex.test(phone.replace(/\s+/g, ''));
    },

    showToast(message, type = 'info') {
        const toastContainer = document.getElementById('toast-container') || this.createToastContainer();
        
        const colors = {
            success: 'bg-green-500',
            error: 'bg-red-500',
            warning: 'bg-yellow-500',
            info: 'bg-blue-500'
        };

        const toast = document.createElement('div');
        toast.className = `${colors[type]} text-white px-6 py-4 rounded-lg shadow-lg mb-4 transform transition-all duration-300 translate-x-full`;
        toast.textContent = message;

        toastContainer.appendChild(toast);

        setTimeout(() => {
            toast.classList.remove('translate-x-full');
        }, 100);

        setTimeout(() => {
            toast.classList.add('translate-x-full');
            setTimeout(() => {
                toastContainer.removeChild(toast);
            }, 300);
        }, 3000);
    },

    createToastContainer() {
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'fixed top-4 right-4 z-50 max-w-sm';
        document.body.appendChild(container);
        return container;
    },

    showLoading() {
        let loader = document.getElementById('global-loader');
        if (!loader) {
            loader = document.createElement('div');
            loader.id = 'global-loader';
            loader.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
            loader.innerHTML = `
                <div class="bg-white rounded-lg p-8">
                    <div class="animate-spin rounded-full h-16 w-16 border-b-4 border-blue-600"></div>
                </div>
            `;
            document.body.appendChild(loader);
        }
        loader.classList.remove('hidden');
    },

    hideLoading() {
        const loader = document.getElementById('global-loader');
        if (loader) {
            loader.classList.add('hidden');
        }
    },

    debounce(func, delay) {
        let timeoutId;
        return function(...args) {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(() => func.apply(this, args), delay);
        };
    },

    formatBytes(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    },

    getQueryParam(param) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(param);
    },

    validateForm(formId) {
        const form = document.getElementById(formId) || document.querySelector(`form[id="${formId}"]`);
        if (!form) return { isValid: true, errors: [] };

        const errors = [];
        const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');

        inputs.forEach(input => {
            const errorElement = input.parentElement.querySelector('.error-message');
            
            if (errorElement) {
                errorElement.textContent = '';
                errorElement.classList.add('hidden');
            }

            if (!input.value.trim()) {
                const label = input.parentElement.querySelector('label')?.textContent || input.name;
                const error = `${label.replace('*', '').trim()} is required`;
                errors.push({ field: input.name, message: error });
                
                if (errorElement) {
                    errorElement.textContent = error;
                    errorElement.classList.remove('hidden');
                }
                input.classList.add('border-red-500');
            } else {
                input.classList.remove('border-red-500');
            }

            if (input.type === 'email' && input.value && !this.validateEmail(input.value)) {
                const error = 'Please enter a valid email address';
                errors.push({ field: input.name, message: error });
                
                if (errorElement) {
                    errorElement.textContent = error;
                    errorElement.classList.remove('hidden');
                }
                input.classList.add('border-red-500');
            }

            if (input.type === 'tel' && input.value && !this.validatePhone(input.value)) {
                const error = 'Please enter a valid phone number';
                errors.push({ field: input.name, message: error });
                
                if (errorElement) {
                    errorElement.textContent = error;
                    errorElement.classList.remove('hidden');
                }
                input.classList.add('border-red-500');
            }

            if (input.type === 'number' && input.value) {
                const min = input.getAttribute('min');
                const max = input.getAttribute('max');
                const value = parseFloat(input.value);

                if (min && value < parseFloat(min)) {
                    const error = `Value must be at least ${min}`;
                    errors.push({ field: input.name, message: error });
                    
                    if (errorElement) {
                        errorElement.textContent = error;
                        errorElement.classList.remove('hidden');
                    }
                    input.classList.add('border-red-500');
                }

                if (max && value > parseFloat(max)) {
                    const error = `Value must be at most ${max}`;
                    errors.push({ field: input.name, message: error });
                    
                    if (errorElement) {
                        errorElement.textContent = error;
                        errorElement.classList.remove('hidden');
                    }
                    input.classList.add('border-red-500');
                }
            }
        });

        return {
            isValid: errors.length === 0,
            errors: errors
        };
    }
};
