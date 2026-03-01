<script setup lang="ts">
import { ref } from 'vue'
import { apiPost } from '@/services/http'
import { useToastStore } from '@/stores/toast'
import AppLoader from '@/components/AppLoader.vue'
interface PppProfile {
  name: string
  localAddress: string
  remoteAddress: string
  rateLimit: string
  onlyOne: string
  comment: string
}

const toast = useToastStore()
const profiles = ref<PppProfile[]>([])
const isLoading = ref(false)
const showAddForm = ref(false)
const hasLoaded = ref(false)
const errorMessage = ref('')

const newProfile = ref({ name: '', localAddress: '', remoteAddress: '', rateLimit: '' })

async function loadProfiles() {
  isLoading.value = true
  errorMessage.value = ''
  try {
    // Backend uses credentials from appsettings.json
    const res = await apiPost<{ success: boolean; data: PppProfile[]; message: string }>('/mikrotik/ppp-profiles', {})
    hasLoaded.value = true
    if (res.data?.success && res.data.data) {
      profiles.value = res.data.data
    } else {
      profiles.value = []
      errorMessage.value = res.data?.message || 'لا توجد بيانات'
    }
  } catch (err: any) {
    hasLoaded.value = true
    profiles.value = []
    errorMessage.value = err.response?.data?.message || 'فشل الاتصال بجهاز MikroTik - تحقق من إعدادات الخادم'
  } finally {
    isLoading.value = false
  }
}

async function addProfile() {
  if (!newProfile.value.name) {
    toast.error('الرجاء إدخال اسم البروفايل')
    return
  }
  try {
    const res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-profiles/add', newProfile.value)
    if (res.data?.success) {
      toast.success('تم إضافة البروفايل بنجاح')
      showAddForm.value = false
      newProfile.value = { name: '', localAddress: '', remoteAddress: '', rateLimit: '' }
      loadProfiles()
    } else {
      toast.error(res.data?.message || 'فشل إضافة البروفايل')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل إضافة البروفايل')
  }
}

async function deleteProfile(name: string) {
  if (!confirm(`هل أنت متأكد من حذف البروفايل ${name}؟`)) return
  try {
    const res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-profiles/delete', { name })
    if (res.data?.success) {
      toast.success('تم حذف البروفايل')
      loadProfiles()
    } else {
      toast.error(res.data?.message || 'فشل حذف البروفايل')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل حذف البروفايل')
  }
}
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-coastal-blue">بروفايلات PPP - MikroTik</h1>
      <div class="flex items-center gap-2">
        <span v-if="hasLoaded" class="text-sm text-light-gray">{{ profiles.length }} بروفايل</span>
        <button v-if="hasLoaded" class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:opacity-90 text-sm" @click="showAddForm = !showAddForm">
          {{ showAddForm ? 'إلغاء' : '+ إضافة بروفايل' }}
        </button>
        <button class="px-4 py-2 bg-coastal-blue text-white rounded-lg hover:opacity-90 text-sm" @click="loadProfiles">
          {{ hasLoaded ? 'تحديث' : 'اتصال' }}
        </button>
      </div>
    </div>

    <!-- Add Profile Form -->
    <div v-if="showAddForm" class="bg-white rounded-xl shadow-md p-4 mb-4 border border-jazan-green">
      <h3 class="text-sm font-semibold text-jazan-green mb-3">إضافة بروفايل PPP جديد</h3>
      <div class="grid grid-cols-2 md:grid-cols-5 gap-3">
        <input v-model="newProfile.name" placeholder="اسم البروفايل" class="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        <input v-model="newProfile.localAddress" placeholder="العنوان المحلي" class="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        <input v-model="newProfile.remoteAddress" placeholder="العنوان البعيد" class="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        <input v-model="newProfile.rateLimit" placeholder="حد السرعة (مثال: 10M/10M)" class="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        <button class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:opacity-90 text-sm" @click="addProfile">إضافة</button>
      </div>
    </div>

    <!-- Content -->
    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <div class="overflow-x-auto">
        <!-- Not connected yet -->
        <div v-if="!hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-12 w-12 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
          </svg>
          <p class="text-charcoal font-medium">أدخل بيانات الاتصال بجهاز MikroTik ثم اضغط "اتصال"</p>
          <p class="text-light-gray text-sm mt-1">لم يتم الاتصال بعد</p>
        </div>

        <!-- Has data -->
        <table v-else-if="profiles.length" class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الاسم</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">العنوان المحلي</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">العنوان البعيد</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">حد السرعة</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">إجراءات</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="p in profiles" :key="p.name" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-charcoal">{{ p.name }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ p.localAddress || '-' }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ p.remoteAddress || '-' }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ p.rateLimit || 'غير محدود' }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <button class="text-red-coral hover:underline font-medium" @click="deleteProfile(p.name)">حذف</button>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- No data after loading -->
        <div v-else-if="hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-12 w-12 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
          </svg>
          <p class="text-charcoal font-medium">لا توجد بيانات</p>
          <p v-if="errorMessage" class="text-red-coral text-sm mt-1">{{ errorMessage }}</p>
          <p v-else class="text-light-gray text-sm mt-1">لا يوجد بروفايلات PPP في جهاز MikroTik</p>
        </div>
      </div>
    </div>
  </div>
</template>
