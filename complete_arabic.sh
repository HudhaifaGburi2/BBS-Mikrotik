#!/bin/bash
# Complete Arabic translation for all pages

cd /home/hg/Desktop/Freelance/isp/BroadbandBillingSystem/frontend

# Update all page titles
find admin client -name "*.html" -exec sed -i 's/Broadband Billing System/نظام فواتير الإنترنت/g' {} \;
find admin -name "*.html" -exec sed -i 's/>BBS Admin</>إدارة النظام</g' {} \;

# Update specific page titles
sed -i 's/<title>Subscribers/<title>المشتركون/g' admin/subscribers.html
sed -i 's/<title>Subscriber Form/<title>نموذج المشترك/g' admin/subscriber-form.html
sed -i 's/<title>Plans/<title>الباقات/g' admin/plans.html
sed -i 's/<title>Plan Form/<title>نموذج الباقة/g' admin/plan-form.html
sed -i 's/<title>Invoices/<title>الفواتير/g' admin/invoices.html
sed -i 's/<title>Invoice Details/<title>تفاصيل الفاتورة/g' admin/invoice-detail.html
sed -i 's/<title>Payments/<title>المدفوعات/g' admin/payments.html
sed -i 's/<title>Reports/<title>التقارير/g' admin/reports.html
sed -i 's/<title>Online Users/<title>المستخدمون المتصلون/g' admin/online-users.html

# Client pages titles
sed -i 's/<title>Client Dashboard/<title>لوحة التحكم/g' client/dashboard.html
sed -i 's/<title>My Subscription/<title>اشتراكي/g' client/subscription.html
sed -i 's/<title>Make Payment/<title>الدفع/g' client/payment.html
sed -i 's/<title>My Invoices/<title>فواتيري/g' client/invoices.html
sed -i 's/<title>Usage Statistics/<title>الاستخدام/g' client/usage.html

# Headers and main content
find admin client -name "*.html" -exec sed -i 's/>Manage Subscribers</>إدارة المشتركين</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Manage Plans</>إدارة الباقات</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>All Invoices</>جميع الفواتير</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>All Payments</>جميع المدفوعات</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>System Reports</>تقارير النظام</g' {} \;

# Table headers
find admin client -name "*.html" -exec sed -i 's/>ID</>الرقم</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Customer</>العميل</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Plan</>الباقة</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Total</>الإجمالي</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Created</>تاريخ الإنشاء</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Updated</>تاريخ التحديث</g' {} \;

# Buttons and actions
find admin client -name "*.html" -exec sed -i 's/>Add New</>إضافة جديد</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>New Subscriber</>مشترك جديد</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>New Plan</>باقة جديدة</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Export CSV</>تصدير CSV</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Export to CSV</>تصدير إلى CSV</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Export to Excel</>تصدير إلى Excel</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Download PDF</>تحميل PDF</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Download Report</>تحميل التقرير</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Generate Report</>إنشاء تقرير</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Print Invoice</>طباعة الفاتورة</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Mark as Paid</>تحديد كمدفوع</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Disconnect</>قطع الاتصال</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Suspend</>إيقاف</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Activate</>تفعيل</g' {} \;

# Form fields and labels
find admin client -name "*.html" -exec sed -i 's/>National ID</>رقم الهوية</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Username</>اسم المستخدم</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Password</>كلمة المرور</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Select Plan</>اختر الباقة</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Select Status</>اختر الحالة</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>All Status</>جميع الحالات</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Billing Cycle</>دورة الفوترة</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>MikroTik Profile</>ملف MikroTik</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Data Cap</>حد البيانات</g' {} \;

# Client-specific terms
find client -name "*.html" -exec sed -i 's/>My Account</>حسابي</g' {} \;
find client -name "*.html" -exec sed -i 's/>Account Balance</>رصيد الحساب</g' {} \;
find client -name "*.html" -exec sed -i 's/>Current Plan</>الباقة الحالية</g' {} \;
find client -name "*.html" -exec sed -i 's/>Expiry Date</>تاريخ الانتهاء</g' {} \;
find client -name "*.html" -exec sed -i 's/>Auto Renew</>التجديد التلقائي</g' {} \;
find client -name "*.html" -exec sed -i 's/>Renew Subscription</>تجديد الاشتراك</g' {} \;
find client -name "*.html" -exec sed -i 's/>Payment History</>سجل المدفوعات</g' {} \;
find client -name "*.html" -exec sed -i 's/>Card Number</>رقم البطاقة</g' {} \;
find client -name "*.html" -exec sed -i 's/>Expiry</>الانتهاء</g' {} \;
find client -name "*.html" -exec sed -i 's/>CVV</>رمز الأمان</g' {} \;
find client -name "*.html" -exec sed -i 's/>Cardholder Name</>اسم حامل البطاقة</g' {} \;
find client -name "*.html" -exec sed -i 's/>Pay Amount</>مبلغ الدفع</g' {} \;
find client -name "*.html" -exec sed -i 's/>Proceed to Payment</>متابعة الدفع</g' {} \;
find client -name "*.html" -exec sed -i 's/>Daily Usage</>الاستخدام اليومي</g' {} \;
find client -name "*.html" -exec sed -i 's/>Total Usage</>الاستخدام الإجمالي</g' {} \;
find client -name "*.html" -exec sed -i 's/>Average Usage</>متوسط الاستخدام</g' {} \;
find client -name "*.html" -exec sed -i 's/>Remaining Data</>البيانات المتبقية</g' {} \;

# Misc terms
find admin client -name "*.html" -exec sed -i 's/>Show</>عرض</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>entries</>إدخالات</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Showing</>عرض</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>to</>إلى</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>of</>من</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Previous</>السابق</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Next</>التالي</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Loading...</>جاري التحميل...</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>No data available</>لا توجد بيانات</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Recent Activity</>النشاط الأخير</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>Quick Actions</>الإجراءات السريعة</g' {} \;
find admin client -name "*.html" -exec sed -i 's/>System Status</>حالة النظام</g' {} \;

# Report types
find admin -name "*.html" -exec sed -i 's/>Revenue Report</>تقرير الإيرادات</g' {} \;
find admin -name "*.html" -exec sed -i 's/>Subscriber Growth</>نمو المشتركين</g' {} \;
find admin -name "*.html" -exec sed -i 's/>Payment Summary</>ملخص المدفوعات</g' {} \;

# Placeholders
find admin client -name "*.html" -exec sed -i 's/placeholder="Search"/placeholder="بحث"/g' {} \;
find admin client -name "*.html" -exec sed -i 's/placeholder="Enter"/placeholder="أدخل"/g' {} \;

echo "✅ Complete Arabic translation finished!"
