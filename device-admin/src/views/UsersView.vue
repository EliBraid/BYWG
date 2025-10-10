<template>
  <div class="users-container">
    <!-- 用户统计概览 -->
    <div class="stats-overview">
      <div class="stat-card">
        <div class="stat-icon total">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M16 4c0-1.11.89-2 2-2s2 .89 2 2-.89 2-2 2-2-.89-2-2zm4 18v-6h2.5l-2.54-7.63A1.5 1.5 0 0 0 18.54 8H16c-.8 0-1.54.37-2.01.99L12 11l-1.99-2.01A2.5 2.5 0 0 0 8 8H5.46c-.8 0-1.54.37-2.01.99L1 13.5V16h2v6h2v-6h2.5l2.5-7.5h2l2.5 7.5H14v6h2z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ userStats.total }}</div>
          <div class="stat-label">总用户数</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon active">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ userStats.active }}</div>
          <div class="stat-label">活跃用户</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon admin">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ userStats.admins }}</div>
          <div class="stat-label">管理员</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon online">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ userStats.online }}</div>
          <div class="stat-label">在线用户</div>
        </div>
      </div>
    </div>

    <!-- 用户管理面板 -->
    <div class="management-panel">
      <div class="panel-header">
        <h3>用户管理</h3>
        <div class="panel-actions">
          <button class="btn-secondary" @click="refreshUsers">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
            刷新
          </button>
          <button class="btn-primary" @click="addUser">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" fill="currentColor"/>
            </svg>
            添加用户
          </button>
        </div>
      </div>

      <!-- 搜索和筛选 -->
      <div class="filter-section">
        <div class="search-box">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z" fill="currentColor"/>
          </svg>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="搜索用户名、邮箱或角色..."
            class="search-input"
          />
        </div>
        <div class="filter-controls">
          <select v-model="roleFilter" class="filter-select">
            <option value="">所有角色</option>
            <option value="admin">管理员</option>
            <option value="operator">操作员</option>
            <option value="viewer">观察员</option>
          </select>
          <select v-model="statusFilter" class="filter-select">
            <option value="">所有状态</option>
            <option value="active">活跃</option>
            <option value="inactive">非活跃</option>
            <option value="online">在线</option>
            <option value="offline">离线</option>
          </select>
          <select v-model="sortBy" class="filter-select">
            <option value="username">按用户名排序</option>
            <option value="role">按角色排序</option>
            <option value="lastLogin">按最后登录排序</option>
            <option value="createdAt">按创建时间排序</option>
          </select>
        </div>
      </div>
    </div>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading-container">
      <div class="loading-spinner"></div>
      <p>加载中...</p>
    </div>

    <!-- 错误信息 -->
    <div v-if="error" class="error-container">
      <div class="error-message">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
        </svg>
        {{ error }}
        <button @click="refreshUsers" class="retry-btn">重试</button>
      </div>
    </div>

    <!-- 用户列表 -->
    <div v-if="!loading && !error" class="users-table-container">
      <table class="users-table">
        <thead>
          <tr>
            <th class="checkbox-column">
              <input 
                type="checkbox" 
                v-model="selectAll"
                @change="toggleSelectAll"
                class="checkbox-input"
              />
            </th>
            <th>用户信息</th>
            <th>角色</th>
            <th>状态</th>
            <th>最后登录</th>
            <th>创建时间</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="user in filteredUsers" 
            :key="user.id"
            class="user-row"
            :class="{ 'selected': selectedUsers.includes(user.id) }"
          >
            <td class="checkbox-column">
              <input 
                type="checkbox" 
                :value="user.id"
                v-model="selectedUsers"
                class="checkbox-input"
              />
            </td>
            <td class="user-info">
              <div class="user-avatar">
                <div class="avatar-placeholder">
                  {{ user.username.charAt(0).toUpperCase() }}
                </div>
              </div>
              <div class="user-details">
                <div class="username">{{ user.username }}</div>
                <div class="email">{{ user.email }}</div>
              </div>
            </td>
            <td>
              <span class="role-badge" :class="user.role">
                {{ getRoleText(user.role) }}
              </span>
            </td>
            <td>
              <div class="status-indicator" :class="user.online ? 'online' : (!user.isEnabled ? 'inactive' : 'offline')">
                <div class="status-dot"></div>
                <span>{{ user.online ? '在线' : (!user.isEnabled ? '禁用' : '离线') }}</span>
              </div>
            </td>
            <td class="last-login">
              <div class="login-time">{{ user.lastLoginAt ? formatDate(user.lastLoginAt) : '从未登录' }}</div>
            </td>
            <td class="created-at">{{ formatDate(user.createdAt) }}</td>
            <td class="actions">
              <button class="btn-icon" @click="viewUser(user)" title="查看详情">
                <i class="fas fa-eye"></i>
              </button>
              <button class="btn-icon" @click="editUser(user)" title="编辑用户">
                <i class="fas fa-edit"></i>
              </button>
              <button class="btn-icon" @click="resetPassword(user)" title="重置密码">
                <i class="fas fa-key"></i>
              </button>
              <button class="btn-icon danger" @click="deleteUser(user)" title="删除用户">
                <i class="fas fa-trash"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      
      <!-- 分页控件 -->
      <div class="pagination-container">
        <div class="pagination-info">
          显示 {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, totalCount) }} 条，共 {{ totalCount }} 条
        </div>
        <div class="pagination-controls">
          <button 
            class="pagination-btn" 
            :disabled="currentPage <= 1"
            @click="currentPage = Math.max(1, currentPage - 1)"
          >
            上一页
          </button>
          <span class="pagination-page">
            第 {{ currentPage }} 页，共 {{ totalPages }} 页
          </span>
          <button 
            class="pagination-btn" 
            :disabled="currentPage >= totalPages"
            @click="currentPage = Math.min(totalPages, currentPage + 1)"
          >
            下一页
          </button>
        </div>
      </div>
    </div>

    <!-- 批量操作 -->
    <div v-if="selectedUsers.length > 0" class="batch-actions">
      <div class="batch-info">
        已选择 {{ selectedUsers.length }} 个用户
      </div>
      <div class="batch-buttons">
        <button class="btn-secondary" @click="batchActivate">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
          批量激活
        </button>
        <button class="btn-secondary" @click="batchDeactivate">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
          批量停用
        </button>
        <button class="btn-danger" @click="batchDelete">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z" fill="currentColor"/>
          </svg>
          批量删除
        </button>
      </div>
    </div>
  </div>

  <!-- 添加用户对话框 -->
  <AddUserDialog 
    v-model:visible="showAddUserDialog" 
    @success="onUserCreated"
  />

  <!-- 用户详情对话框 -->
  <UserDetailDialog 
    v-model:visible="showUserDetailDialog"
    :user="selectedUser"
    @edit="onEditUser"
  />

  <!-- 编辑用户对话框 -->
  <EditUserDialog 
    v-model:visible="showEditUserDialog"
    :user="selectedUser"
    @success="refreshUsers"
  />

  <!-- 重置密码对话框 -->
  <ResetPasswordDialog 
    v-model:visible="showResetPasswordDialog"
    :user="selectedUser"
    @success="refreshUsers"
  />
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { 
  getUsers, 
  getUserStats, 
  deleteUser as deleteUserApi, 
  batchUpdateStatus, 
  batchDelete as batchDeleteApi,
  type UserDto,
  type UserListParams,
  type UserStats
} from '../api/users'
import AddUserDialog from '../components/AddUserDialog.vue'
import UserDetailDialog from '../components/UserDetailDialog.vue'
import EditUserDialog from '../components/EditUserDialog.vue'
import ResetPasswordDialog from '../components/ResetPasswordDialog.vue'

// 搜索和筛选
const searchQuery = ref('')
const roleFilter = ref('')
const statusFilter = ref('')
const sortBy = ref('username')
const selectAll = ref(false)
const selectedUsers = ref<number[]>([])

// 分页
const currentPage = ref(1)
const pageSize = ref(10)
const totalPages = ref(0)
const totalCount = ref(0)

// 加载状态
const loading = ref(false)
const error = ref('')

// 用户数据
const users = ref<UserDto[]>([])
const userStats = ref<UserStats>({
  total: 0,
  active: 0,
  admins: 0,
  online: 0
})

// 在线用户状态（由后端返回online字段，不再前端模拟）

// 对话框状态
const showAddUserDialog = ref(false)
const showUserDetailDialog = ref(false)
const showEditUserDialog = ref(false)
const showResetPasswordDialog = ref(false)
const selectedUser = ref<UserDto | null>(null)

// 计算属性
const filteredUsers = computed(() => {
  return users.value
})

// 监听搜索和筛选条件变化，重新加载数据
watch([searchQuery, roleFilter, statusFilter, currentPage], () => {
  loadUsers()
}, { deep: true })

// 监听分页变化
watch(currentPage, () => {
  loadUsers()
})

// 方法
function getRoleText(role: string) {
  const roleMap: Record<string, string> = {
    admin: '管理员',
    operator: '操作员',
    viewer: '观察员',
    user: '用户'
  }
  return roleMap[role] || '未知'
}

// 状态显示由后端online字段决定

// 移除旧的前端状态推断函数，直接由后端online决定

function formatDate(dateString: string) {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMinutes = Math.floor(diffMs / (1000 * 60))
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))
  
  if (diffMinutes < 60) {
    return `${diffMinutes}分钟前`
  } else if (diffHours < 24) {
    return `${diffHours}小时前`
  } else if (diffDays < 7) {
    return `${diffDays}天前`
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}

function toggleSelectAll() {
  if (selectAll.value) {
    selectedUsers.value = filteredUsers.value.map(user => user.id)
  } else {
    selectedUsers.value = []
  }
}

// 加载用户列表
async function loadUsers() {
  try {
    loading.value = true
    error.value = ''
    
    const params: UserListParams = {
      page: currentPage.value,
      pageSize: pageSize.value,
      search: searchQuery.value || undefined,
      role: roleFilter.value || undefined,
      status: statusFilter.value || undefined
    }
    
    const response = await getUsers(params)
    users.value = response.users
    totalCount.value = response.totalCount
    totalPages.value = response.totalPages
    
  } catch (err: any) {
    error.value = err.message || '加载用户列表失败'
    console.error('加载用户列表失败:', err)
  } finally {
    loading.value = false
  }
}

// 加载用户统计
async function loadUserStats() {
  try {
    const stats = await getUserStats()
    userStats.value = stats
  } catch (err: any) {
    console.error('加载用户统计失败:', err)
  }
}

async function refreshUsers() {
  await Promise.all([loadUsers(), loadUserStats()])
}

function addUser() {
  showAddUserDialog.value = true
}

function onUserCreated() {
  refreshUsers()
}

function viewUser(user: UserDto) {
  selectedUser.value = user
  showUserDetailDialog.value = true
}

function editUser(user: UserDto) {
  selectedUser.value = user
  showEditUserDialog.value = true
}

function resetPassword(user: UserDto) {
  selectedUser.value = user
  showResetPasswordDialog.value = true
}

function onEditUser(user: UserDto) {
  selectedUser.value = user
  showEditUserDialog.value = true
}

async function deleteUser(user: UserDto) {
  if (!confirm(`确定要删除用户 "${user.username}" 吗？此操作不可撤销。`)) {
    return
  }
  
  try {
    await deleteUserApi(user.id)
    await refreshUsers()
  } catch (err: any) {
    alert('删除用户失败: ' + (err.message || '未知错误'))
  }
}

async function batchActivate() {
  if (selectedUsers.value.length === 0) {
    alert('请先选择要激活的用户')
    return
  }
  
  try {
    await batchUpdateStatus({
      userIds: selectedUsers.value,
      isEnabled: true
    })
    selectedUsers.value = []
    selectAll.value = false
    await refreshUsers()
  } catch (err: any) {
    alert('批量激活失败: ' + (err.message || '未知错误'))
  }
}

async function batchDeactivate() {
  if (selectedUsers.value.length === 0) {
    alert('请先选择要停用的用户')
    return
  }
  
  try {
    await batchUpdateStatus({
      userIds: selectedUsers.value,
      isEnabled: false
    })
    selectedUsers.value = []
    selectAll.value = false
    await refreshUsers()
  } catch (err: any) {
    alert('批量停用失败: ' + (err.message || '未知错误'))
  }
}

async function batchDelete() {
  if (selectedUsers.value.length === 0) {
    alert('请先选择要删除的用户')
    return
  }
  
  if (!confirm(`确定要删除选中的 ${selectedUsers.value.length} 个用户吗？此操作不可撤销。`)) {
    return
  }
  
  try {
    await batchDeleteApi({
      userIds: selectedUsers.value
    })
    selectedUsers.value = []
    selectAll.value = false
    await refreshUsers()
  } catch (err: any) {
    alert('批量删除失败: ' + (err.message || '未知错误'))
  }
}

// 组件挂载时加载数据
onMounted(() => {
  refreshUsers()
})
</script>

<style scoped>
.users-container {
  display: flex;
  flex-direction: column;
  gap: 24px;
  width: 100%;
  max-width: 100%;
  min-height: calc(100vh - 120px);
}

.stats-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 12px;
  color: white;
}

.stat-icon.total {
  background: linear-gradient(135deg, #4a90e2, #357abd);
}

.stat-icon.active {
  background: linear-gradient(135deg, #27ae60, #2ecc71);
}

.stat-icon.admin {
  background: linear-gradient(135deg, #9b59b6, #8e44ad);
}

.stat-icon.online {
  background: linear-gradient(135deg, #f39c12, #e67e22);
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #6c757d;
  font-weight: 500;
}

.management-panel {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.panel-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
}

.panel-actions {
  display: flex;
  gap: 12px;
}

.btn-primary, .btn-secondary {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 14px;
}

.btn-primary {
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(74, 144, 226, 0.4);
}

.btn-secondary {
  background: #f8f9fa;
  color: #6c757d;
  border: 1px solid #dee2e6;
}

.btn-secondary:hover {
  background: #e9ecef;
  color: #495057;
}

.filter-section {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px 24px;
  background: #f8f9fa;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.search-box {
  position: relative;
  flex: 1;
  max-width: 400px;
}

.search-box svg {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #6c757d;
  width: 20px;
  height: 20px;
}

.search-input {
  width: 100%;
  padding: 10px 12px 10px 40px;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  transition: all 0.3s ease;
}

.search-input:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
}

.filter-controls {
  display: flex;
  gap: 12px;
}

.filter-select {
  padding: 10px 12px;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-select:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
}

.users-table-container {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
}

.users-table th {
  background: #f8f9fa;
  padding: 16px 20px;
  text-align: left;
  font-weight: 600;
  color: #2c3e50;
  border-bottom: 1px solid #dee2e6;
  font-size: 14px;
}

.users-table td {
  padding: 16px 20px;
  border-bottom: 1px solid #f1f3f4;
  vertical-align: middle;
}

.user-row:hover {
  background: rgba(74, 144, 226, 0.05);
}

.user-row.selected {
  background: rgba(74, 144, 226, 0.1);
}

.checkbox-column {
  width: 50px;
  text-align: center;
}

.checkbox-input {
  width: 18px;
  height: 18px;
  accent-color: #4a90e2;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0;
}

.user-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 16px;
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.username {
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
}

.email {
  color: #6c757d;
  font-size: 12px;
}

.role-badge {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}

.role-badge.admin {
  background: rgba(155, 89, 182, 0.1);
  color: #9b59b6;
}

.role-badge.operator {
  background: rgba(74, 144, 226, 0.1);
  color: #4a90e2;
}

.role-badge.viewer {
  background: rgba(108, 117, 125, 0.1);
  color: #6c757d;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 500;
}

.status-indicator.online {
  color: #27ae60;
}

.status-indicator.recent {
  color: #f39c12;
}

.status-indicator.active {
  color: #4a90e2;
}

.status-indicator.inactive {
  color: #6c757d;
}

.status-indicator.offline {
  color: #e74c3c;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
}

.status-indicator.online .status-dot {
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.5; }
  100% { opacity: 1; }
}

.last-login {
  font-size: 12px;
  color: #6c757d;
}

.login-time {
  font-weight: 500;
  color: #2c3e50;
}

.login-ip {
  color: #adb5bd;
  margin-top: 2px;
}

.created-at {
  font-size: 12px;
  color: #6c757d;
}

.actions {
  display: flex;
  gap: 8px;
}

.btn-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  background: white;
  color: #6b7280 !important;
  cursor: pointer;
  transition: all 0.2s ease;
  position: relative;
  flex-shrink: 0;
}

.btn-icon i {
  font-size: 14px;
  line-height: 1;
  display: block;
}

.btn-icon:hover {
  background: #f3f4f6;
  color: #374151 !important;
  border-color: #9ca3af;
  transform: translateY(-1px);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.btn-icon:hover i {
  color: #374151 !important;
}

.btn-icon:active {
  transform: translateY(0);
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
}

.btn-icon.danger {
  color: #dc2626 !important;
  border-color: #fecaca;
  background: #fef2f2;
}

.btn-icon.danger i {
  color: #dc2626 !important;
}

.btn-icon.danger:hover {
  background: #fee2e2;
  color: #b91c1c !important;
  border-color: #fca5a5;
}

.btn-icon.danger:hover i {
  color: #b91c1c !important;
}

.btn-icon.danger:active {
  background: #fecaca;
  color: #991b1b !important;
}

.btn-icon.danger:active i {
  color: #991b1b !important;
}

.batch-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  background: rgba(74, 144, 226, 0.05);
  border: 1px solid rgba(74, 144, 226, 0.1);
  border-radius: 8px;
}

.batch-info {
  font-size: 14px;
  color: #2c3e50;
  font-weight: 500;
}

.batch-buttons {
  display: flex;
  gap: 12px;
}

.btn-danger {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: #dc3545;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-danger:hover {
  background: #c82333;
  transform: translateY(-1px);
}

@media (max-width: 768px) {
  .users-table-container {
    overflow-x: auto;
  }
  
  .users-table {
    min-width: 800px;
  }
  
  .filter-section {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }
  
  .search-box {
    max-width: none;
  }
  
  .filter-controls {
    flex-wrap: wrap;
  }
  
  .batch-actions {
    flex-direction: column;
    gap: 12px;
    align-items: stretch;
  }
  
  .batch-buttons {
    justify-content: center;
  }
}

/* 加载状态样式 */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  color: #6c757d;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #4a90e2;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 错误状态样式 */
.error-container {
  display: flex;
  justify-content: center;
  padding: 40px 20px;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 20px;
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
  border-radius: 8px;
  max-width: 500px;
}

.retry-btn {
  padding: 6px 12px;
  background: #dc3545;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  transition: background 0.3s ease;
}

.retry-btn:hover {
  background: #c82333;
}

/* 分页样式 */
.pagination-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  background: #f8f9fa;
  border-top: 1px solid #dee2e6;
}

.pagination-info {
  color: #6c757d;
  font-size: 14px;
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 16px;
}

.pagination-btn {
  padding: 8px 16px;
  border: 1px solid #dee2e6;
  background: white;
  color: #495057;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 14px;
}

.pagination-btn:hover:not(:disabled) {
  background: #f8f9fa;
  border-color: #adb5bd;
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination-page {
  color: #6c757d;
  font-size: 14px;
}
</style>
