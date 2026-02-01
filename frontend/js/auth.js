const auth = {
    async login(username, password) {
        try {
            const response = await api.post('/auth/login', {
                username: username,
                password: password
            });

            if (response.data && response.data.token) {
                localStorage.setItem('authToken', response.data.token);
                localStorage.setItem('userData', JSON.stringify(response.data.user));
                return true;
            }
            
            throw new Error('Invalid response from server');
        } catch (error) {
            throw error;
        }
    },

    logout() {
        localStorage.removeItem('authToken');
        localStorage.removeItem('userData');
        window.location.href = '/index.html';
    },

    isAuthenticated() {
        const token = localStorage.getItem('authToken');
        const userData = localStorage.getItem('userData');
        return !!(token && userData);
    },

    getUser() {
        const userData = localStorage.getItem('userData');
        if (userData) {
            try {
                return JSON.parse(userData);
            } catch (error) {
                console.error('Error parsing user data:', error);
                return null;
            }
        }
        return null;
    },

    getToken() {
        return localStorage.getItem('authToken');
    },

    getUserRole() {
        const user = this.getUser();
        return user ? user.role : null;
    },

    requireAuth() {
        if (!this.isAuthenticated()) {
            window.location.href = '/index.html';
            return false;
        }
        return true;
    },

    requireRole(role) {
        if (!this.requireAuth()) {
            return false;
        }
        const userRole = this.getUserRole();
        if (userRole !== role) {
            utils.showToast('Access denied. Insufficient permissions.', 'error');
            window.location.href = '/index.html';
            return false;
        }
        return true;
    }
};
