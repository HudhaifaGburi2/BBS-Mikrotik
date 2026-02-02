// Login page initialization
(function() {
    'use strict';
    
    // Check if already authenticated - redirect immediately without reload loop
    if (auth.isAuthenticated()) {
        const user = auth.getUser();
        const targetUrl = (user && user.userType === 'Admin') 
            ? '/admin/dashboard.html' 
            : '/client/dashboard.html';
        window.location.replace(targetUrl);
        return; // Stop execution - page will redirect
    }

    const loginForm = document.getElementById('login-form');
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');
    const rememberCheckbox = document.getElementById('remember');
    
    let isSubmitting = false;

    async function handleLogin(e) {
        // Prevent default form submission
        e.preventDefault();
        e.stopPropagation();
        
        // Prevent double submission
        if (isSubmitting) return;
        
        const username = usernameInput.value.trim();
        const password = passwordInput.value;
        const rememberMe = rememberCheckbox ? rememberCheckbox.checked : false;

        if (!username || !password) {
            utils.showToast('الرجاء إدخال اسم المستخدم وكلمة المرور', 'error');
            return;
        }

        isSubmitting = true;
        
        try {
            utils.showLoading();
            
            const result = await auth.login(username, password, rememberMe);
            
            if (result && result.success) {
                // Verify storage before redirect
                if (!auth.isAuthenticated()) {
                    throw new Error('خطأ في حفظ بيانات الجلسة');
                }
                
                utils.showToast('تم تسجيل الدخول بنجاح!', 'success');
                
                // Small delay to show success message, then redirect
                const targetUrl = (result.userType === 'Admin') 
                    ? '/admin/dashboard.html' 
                    : '/client/dashboard.html';
                
                setTimeout(() => {
                    window.location.replace(targetUrl);
                }, 500);
            }
        } catch (error) {
            isSubmitting = false;
            const message = error.message || 'فشل تسجيل الدخول. الرجاء التحقق من بيانات الاعتماد.';
            utils.showToast(message, 'error');
        } finally {
            utils.hideLoading();
        }
    }

    // Attach form submit handler
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }
})();
