#!/bin/bash
# Batch Arabic translation script for all pages

cd /home/hg/Desktop/Freelance/isp/BroadbandBillingSystem/frontend

# Admin pages
for file in admin/*.html client/*.html; do
    echo "Translating $file..."
    
    # Common navigation and buttons
    sed -i 's/>Dashboard</>لوحة التحكم</g' "$file"
    sed -i 's/>Subscribers</>المشتركون</g' "$file"
    sed -i 's/>Plans</>الباقات</g' "$file"
    sed -i 's/>Invoices</>الفواتير</g' "$file"
    sed -i 's/>Payments</>المدفوعات</g' "$file"
    sed -i 's/>Reports</>التقارير</g' "$file"
    sed -i 's/>Online Users</>المستخدمون المتصلون</g' "$file"
    sed -i 's/>Logout</>تسجيل الخروج</g' "$file"
    
    # Common actions
    sed -i 's/>Add</>إضافة</g' "$file"
    sed -i 's/>Edit</>تعديل</g' "$file"
    sed -i 's/>Delete</>حذف</g' "$file"
    sed -i 's/>Save</>حفظ</g' "$file"
    sed -i 's/>Cancel</>إلغاء</g' "$file"
    sed -i 's/>Search</>بحث</g' "$file"
    sed -i 's/>Filter</>تصفية</g' "$file"
    sed -i 's/>Export</>تصدير</g' "$file"
    sed -i 's/>View</>عرض</g' "$file"
    sed -i 's/>Close</>إغلاق</g' "$file"
    sed -i 's/>Back</>رجوع</g' "$file"
    
    # Status
    sed -i 's/>Active</>نشط</g' "$file"
    sed -i 's/>Inactive</>غير نشط</g' "$file"
    sed -i 's/>Suspended</>موقوف</g' "$file"
    sed -i 's/>Paid</>مدفوع</g' "$file"
    sed -i 's/>Unpaid</>غير مدفوع</g' "$file"
    sed -i 's/>Overdue</>متأخر</g' "$file"
    
    # Fields
    sed -i 's/>Full Name</>الاسم الكامل</g' "$file"
    sed -i 's/>Email</>البريد الإلكتروني</g' "$file"
    sed -i 's/>Phone</>الهاتف</g' "$file"
    sed -i 's/>Address</>العنوان</g' "$file"
    sed -i 's/>City</>المدينة</g' "$file"
    sed -i 's/>Status</>الحالة</g' "$file"
    sed -i 's/>Actions</>الإجراءات</g' "$file"
    sed -i 's/>Name</>الاسم</g' "$file"
    sed -i 's/>Description</>الوصف</g' "$file"
    sed -i 's/>Price</>السعر</g' "$file"
    sed -i 's/>Speed</>السرعة</g' "$file"
    sed -i 's/>Amount</>المبلغ</g' "$file"
    sed -i 's/>Date</>التاريخ</g' "$file"
    
done

echo "Translation complete!"
