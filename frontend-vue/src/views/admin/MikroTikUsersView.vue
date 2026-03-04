<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { apiPost } from '@/services/http'
import { useToastStore } from '@/stores/toast'
import { useMikroTikStore } from '@/stores/mikrotik'
import AppLoader from '@/components/AppLoader.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

const toast = useToastStore()
const mikroTikStore = useMikroTikStore()

const showAddForm = ref(false)
const newUser = ref({ name: '', password: '', profile: 'default', service: 'pppoe' })

// Delete confirmation
const confirmVisible = ref(false)
const pendingDeleteName = ref('')

// Computed from store
const users = computed(() => mikroTikStore.pppUsers)
const isLoading = computed(() => mikroTikStore.isLoading)
const errorMessage = computed(() => mikroTikStore.errorMessage)
const hasLoaded = computed(() => mikroTikStore.isConnected || mikroTikStore.pppUsers.length > 0)

async function loadUsers(force = false) {
  await mikroTikStore.fetchPppUsers(force)
}

async function addUser() {
  if (!newUser.value.name || !newUser.value.password) {
    toast.error('الرجاء إدخال اسم المستخدم وكلمة المرور')
    return
  }
  try {
    const payload = {
      pppUsername: newUser.value.name,
      pppPassword: newUser.value.password,
      profile: newUser.value.profile,
      service: newUser.value.service
    }
    const res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-users/add', payload)
    if (res.data?.success) {
      toast.success('تم إضافة المستخدم بنجاح')
      showAddForm.value = false
      newUser.value = { name: '', password: '', profile: 'default', service: 'pppoe' }
      await loadUsers(true)
    } else {
      toast.error(res.data?.message || 'فشل إضافة المستخدم')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل إضافة المستخدم')
  }
}

function confirmDelete(username: string) {
  pendingDeleteName.value = username
  confirmVisible.value = true
}

async function handleDelete() {
  confirmVisible.value = false
  const username = pendingDeleteName.value
  try {
    const res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-users/delete', { pppUsername: username })
    if (res.data?.success) {
      toast.success('تم حذف المستخدم')
      await loadUsers(true)
    } else {
      toast.error(res.data?.message || 'فشل حذف المستخدم')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل حذف المستخدم')
  }
}

async function toggleUser(username: string, disabled: boolean) {
  const endpoint = disabled ? '/mikrotik/ppp-users/activate' : '/mikrotik/ppp-users/deactivate'
  try {
    const res = await apiPost<{ success: boolean; message: string }>(endpoint, { pppUsername: username })
    if (res.data?.success) {
      toast.success(disabled ? 'تم تفعيل المستخدم' : 'تم تعطيل المستخدم')
      await loadUsers(true)
    } else {
      toast.error(res.data?.message || 'فشل تغيير حالة المستخدم')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل تغيير حالة المستخدم')
  }
}

// Auto-load on mount
onMounted(() => {
  loadUsers()
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-coastal-blue">مستخدمو PPP - MikroTik</h1>
      <div class="flex items-center gap-2">
        <span v-if="hasLoaded" class="text-sm text-light-gray">{{ users.length }} مستخدم</span>
        <button v-if="hasLoaded" class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:opacity-90 text-sm" @click="showAddForm = !showAddForm">
          {{ showAddForm ? 'إلغاء' : '+ إضافة مستخدم' }}
        </button>
        <button class="px-4 py-2 bg-coastal-blue text-white rounded-lg hover:opacity-90 text-sm" @click="loadUsers(true)">
          {{ hasLoaded ? 'تحديث' : 'اتصال' }}
        </button>
      </div>
    </div>

    <!-- Add User Form -->
    <div v-if="showAddForm" class="bg-white rounded-xl shadow-md p-4 mb-4 border border-jazan-green">
      <h3 class="text-sm font-semibold text-jazan-green mb-3">إضافة مستخدم PPP جديد</h3>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
        <input v-model="newUser.name" placeholder="اسم المستخدم" class="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        <input v-model="newUser.password" type="password" placeholder="كلمة المرور" class="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        <input v-model="newUser.profile" placeholder="البروفايل" class="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        <button class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:opacity-90 text-sm" @click="addUser">إضافة</button>
      </div>
    </div>

    <!-- Content -->
    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <div class="overflow-x-auto">
        <!-- Not connected yet -->
        <div v-if="!hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-12 w-12 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
          </svg>
          <p class="text-charcoal font-medium">أدخل بيانات الاتصال بجهاز MikroTik ثم اضغط "اتصال"</p>
          <p class="text-light-gray text-sm mt-1">لم يتم الاتصال بعد</p>
        </div>

        <!-- Has data -->
        <table v-else-if="users.length" class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الاسم</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">البروفايل</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الخدمة</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الحالة</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">إجراءات</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="u in users" :key="u.name" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-charcoal">{{ u.name }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ u.profile }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ u.service }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <span :class="u.disabled ? 'text-red-coral' : 'text-jazan-green'" class="font-medium">
                  {{ u.disabled ? 'معطل' : 'نشط' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm space-x-2 space-x-reverse">
                <button class="text-coastal-blue hover:underline font-medium" @click="toggleUser(u.name, u.disabled)">
                  {{ u.disabled ? 'تفعيل' : 'تعطيل' }}
                </button>
                <button class="text-red-coral hover:underline font-medium" @click="confirmDelete(u.name)">حذف</button>
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
          <p v-else class="text-light-gray text-sm mt-1">لا يوجد مستخدمون PPP في جهاز MikroTik</p>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Dialog -->
    <ConfirmDialog
      :visible="confirmVisible"
      title="تأكيد الحذف"
      :message="`هل أنت متأكد من حذف المستخدم '${pendingDeleteName}'؟ هذا الإجراء لا يمكن التراجع عنه.`"
      confirm-text="حذف"
      cancel-text="إلغاء"
      @confirm="handleDelete"
      @cancel="confirmVisible = false"
    />
  </div>
</template>
