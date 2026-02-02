// Login page initialization
(function() {
    console.log('🔍 Checking authentication on login page load...');
    if (auth.isAuthenticated()) {
        const user = auth.getUser();
        console.log('✅ User already authenticated:', user);
        if (user && user.userType === 'Admin') {
            console.log('→ Redirecting to admin dashboard');
            window.location.replace('/admin/dashboard.html');
        } else {
            console.log('→ Redirecting to client dashboard');
            window.location.replace('/client/dashboard.html');
        }
    } else {
        console.log('ℹ️ Not authenticated - showing login form');
    }

    document.getElementById('login-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;
        const rememberMe = document.getElementById('remember').checked;

        if (!username || !password) {
            utils.showToast('الرجاء إدخال اسم المستخدم وكلمة المرور', 'error');
            return;
        }

        try {
            utils.showLoading();
            
            const success = await auth.login(username, password, rememberMe);
            
            if (success) {
                console.log('✅ Login successful, verifying storage...');
                
                const storedToken = localStorage.getItem('authToken');
                const storedUser = localStorage.getItem('userData');
                
                if (storedToken && storedUser) {
                    console.log('✅ Storage verified, showing success message');
                    utils.showToast('تم تسجيل الدخول بنجاح! جاري التحويل...', 'success');
                    
                    setTimeout(() => {
                        const user = auth.getUser();
                        console.log('→ Redirecting user:', user);
                        if (user && user.userType === 'Admin') {
                            window.location.replace('/admin/dashboard.html');
                        } else {
                            window.location.replace('/client/dashboard.html');
                        }
                    }, 800);
                } else {
                    console.error('❌ Storage verification failed!');
                    console.error('Token:', !!storedToken, 'User:', !!storedUser);
                    utils.showToast('خطأ في حفظ بيانات الجلسة', 'error');
                }
            }
        } catch (error) {
            utils.showToast(error.message || 'فشل تسجيل الدخول. الرجاء التحقق من بيانات الاعتماد.', 'error');
        } finally {
            utils.hideLoading();
        }
    });

    document.getElementById('password').addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            document.getElementById('login-form').dispatchEvent(new Event('submit'));
        }
    });
})();
