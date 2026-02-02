// Arabic translations for the Broadband Billing System
const translations = {
    // Common
    'Dashboard': 'لوحة التحكم',
    'Subscribers': 'المشتركون',
    'Plans': 'الباقات',
    'Invoices': 'الفواتير',
    'Payments': 'المدفوعات',
    'Reports': 'التقارير',
    'Online Users': 'المستخدمون المتصلون',
    'Logout': 'تسجيل الخروج',
    'Login': 'تسجيل الدخول',
    
    // Stats
    'Total Subscribers': 'إجمالي المشتركين',
    'Active Subscriptions': 'الاشتراكات النشطة',
    'Monthly Revenue': 'الإيرادات الشهرية',
    'Overdue Invoices': 'الفواتير المتأخرة',
    'Current Balance': 'الرصيد الحالي',
    'Data Used': 'البيانات المستخدمة',
    'Days Remaining': 'الأيام المتبقية',
    
    // Actions
    'Add': 'إضافة',
    'Edit': 'تعديل',
    'Delete': 'حذف',
    'Save': 'حفظ',
    'Cancel': 'إلغاء',
    'Search': 'بحث',
    'Filter': 'تصفية',
    'Export': 'تصدير',
    'Print': 'طباعة',
    'Download': 'تحميل',
    'View': 'عرض',
    'Close': 'إغلاق',
    
    // Subscriber
    'Add Subscriber': 'إضافة مشترك',
    'Edit Subscriber': 'تعديل مشترك',
    'Full Name': 'الاسم الكامل',
    'Email': 'البريد الإلكتروني',
    'Phone': 'الهاتف',
    'Address': 'العنوان',
    'City': 'المدينة',
    'Status': 'الحالة',
    'Active': 'نشط',
    'Inactive': 'غير نشط',
    'Suspended': 'موقوف',
    
    // Plans
    'Plan Name': 'اسم الباقة',
    'Speed': 'السرعة',
    'Price': 'السعر',
    'Data Limit': 'حد البيانات',
    'Unlimited': 'غير محدود',
    'Create Plan': 'إنشاء باقة',
    
    // Invoices
    'Invoice Number': 'رقم الفاتورة',
    'Issue Date': 'تاريخ الإصدار',
    'Due Date': 'تاريخ الاستحقاق',
    'Amount': 'المبلغ',
    'Paid': 'مدفوع',
    'Unpaid': 'غير مدفوع',
    'Overdue': 'متأخر',
    
    // Payments
    'Payment Date': 'تاريخ الدفع',
    'Payment Method': 'طريقة الدفع',
    'Reference': 'المرجع',
    'Cash': 'نقدي',
    'Card': 'بطاقة',
    'Bank Transfer': 'تحويل بنكي',
    
    // Client Portal
    'My Dashboard': 'لوحة التحكم الخاصة بي',
    'My Subscription': 'اشتراكي',
    'Make Payment': 'دفع',
    'My Invoices': 'فواتيري',
    'Usage Statistics': 'إحصائيات الاستخدام',
    'Renew Now': 'تجديد الآن',
    'Pay Now': 'ادفع الآن',
    
    // Misc
    'Loading...': 'جاري التحميل...',
    'No data available': 'لا توجد بيانات',
    'Welcome': 'مرحباً',
    'Profile': 'الملف الشخصي',
    'Settings': 'الإعدادات',
    'Help': 'مساعدة'
};

// Auto-translate function
function translatePage() {
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        if (translations[key]) {
            el.textContent = translations[key];
        }
    });
}

// Run on load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', translatePage);
} else {
    translatePage();
}
