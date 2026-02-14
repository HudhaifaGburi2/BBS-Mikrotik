<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUsersStore } from '@/stores/users'
import { useToastStore } from '@/stores/toast'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import AppPagination from '@/components/AppPagination.vue'

const router = useRouter()
const store = useUsersStore()
const toast = useToastStore()

const search = ref('')
const roleFilter = ref('')
const confirmVisible = ref(false)
const selectedUserId = ref('')
let searchTimeout: ReturnType<typeof setTimeout> | null = null

function onSearchInput() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    store.fetchAll(1, 20, search.value, roleFilter.value)
  }, 400)
}

function onRoleChange() {
  store.fetchAll(1, 20, search.value, roleFilter.value)
}

function goToPage(page: number) {
  store.fetchAll(page, 20, search.value, roleFilter.value)
}

function confirmToggle(id: string) {
  selectedUserId.value = id
  confirmVisible.value = true
}

async function handleToggle() {
  confirmVisible.value = false
  try {
    await store.toggleActive(selectedUserId.value)
    toast.success('تم تحديث حالة المستخدم')
  } catch {
    toast.error('فشل تحديث حالة المستخدم')
  }
}

onMounted(() => store.fetchAll())
</script>

<template>
  <AppLoader :visible="store.isLoading" />
  <ConfirmDialog
    :visible="confirmVisible"
    title="تغيير حالة المستخدم"
    message="هل أنت متأكد من تغيير حالة هذا المستخدم؟"
    @confirm="handleToggle"
    @cancel="confirmVisible = false"
  />

  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-coastal-blue">إدارة المستخدمين</h1>
      <button
        class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors shadow-md"
        @click="router.push('/admin/users/new')"
      >
        + إضافة مستخدم
      </button>
    </div>

    <div class="bg-white rounded-xl shadow-md border border-soft-beige">
      <div class="p-4 border-b border-soft-beige flex flex-col sm:flex-row gap-4">
        <input
          v-model="search"
          type="text"
          placeholder="بحث بالاسم أو اسم المستخدم..."
          class="flex-1 px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green"
          @input="onSearchInput"
        />
        <select
          v-model="roleFilter"
          class="px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green"
          @change="onRoleChange"
        >
          <option value="">جميع الأدوار</option>
          <option value="Admin">مسؤول</option>
          <option value="Client">عميل</option>
        </select>
      </div>

      <div class="overflow-x-auto">
        <table v-if="store.users.length" class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">اسم المستخدم</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الاسم الكامل</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">البريد</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الدور</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المشترك</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الحالة</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">آخر دخول</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">إجراءات</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="user in store.users" :key="user.id" class="hover:bg-soft-beige-light">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-charcoal">{{ user.username }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-charcoal">{{ user.fullName }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ user.email }}</td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span :class="[
                  'px-2 py-1 text-xs font-semibold rounded-full',
                  user.role === 'Admin' ? 'bg-coastal-blue/15 text-coastal-blue' : 'bg-jazan-green/15 text-jazan-green'
                ]">
                  {{ user.role === 'Admin' ? 'مسؤول' : 'عميل' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">
                {{ user.subscriberName || '—' }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <StatusBadge :status="user.isActive ? 'Active' : 'Suspended'" />
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">
                {{ user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleDateString('ar-SA') : '—' }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <div class="flex gap-2">
                  <button class="text-coastal-blue hover:text-coastal-blue-dark font-medium" @click="router.push(`/admin/users/${user.id}/edit`)">
                    تعديل
                  </button>
                  <button
                    :class="user.isActive ? 'text-golden-sand-dark hover:text-golden-sand' : 'text-teal hover:text-teal-dark'"
                    class="font-medium"
                    @click="confirmToggle(user.id)"
                  >
                    {{ user.isActive ? 'تعطيل' : 'تفعيل' }}
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-else class="text-center py-12 text-light-gray">لا يوجد مستخدمون</div>
      </div>

      <AppPagination :current-page="store.currentPage" :total-pages="store.totalPages" @page-change="goToPage" />
    </div>
  </div>
</template>
