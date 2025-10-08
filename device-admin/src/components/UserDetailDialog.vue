<template>
  <div v-if="visible" class="dialog-overlay" @click="closeDialog">
    <div class="dialog-container" @click.stop>
      <div class="dialog-header">
        <h3>用户详情</h3>
        <button class="close-btn" @click="closeDialog">
          <i class="fas fa-times"></i>
        </button>
      </div>
      
      <div class="dialog-content" v-if="user">
        <div class="user-info-section">
          <div class="user-avatar-large">
            <div class="avatar-placeholder">
              {{ user.username.charAt(0).toUpperCase() }}
            </div>
          </div>
          <div class="user-basic-info">
            <h4>{{ user.username }}</h4>
            <p class="user-email">{{ user.email }}</p>
            <div class="user-status">
              <span class="status-badge" :class="getStatusClass(user)">
                {{ getStatusText(user) }}
              </span>
            </div>
          </div>
        </div>

        <div class="info-grid">
          <div class="info-item">
            <label>真实姓名</label>
            <span>{{ user.fullName || '未设置' }}</span>
          </div>
          <div class="info-item">
            <label>手机号码</label>
            <span>{{ user.phone || '未设置' }}</span>
          </div>
          <div class="info-item">
            <label>部门</label>
            <span>{{ user.department || '未设置' }}</span>
          </div>
          <div class="info-item">
            <label>角色</label>
            <span class="role-badge" :class="user.role">
              {{ getRoleText(user.role) }}
            </span>
          </div>
          <div class="info-item">
            <label>创建时间</label>
            <span>{{ formatDate(user.createdAt) }}</span>
          </div>
          <div class="info-item">
            <label>最后登录</label>
            <span>{{ user.lastLoginAt ? formatDate(user.lastLoginAt) : '从未登录' }}</span>
          </div>
          <div class="info-item">
            <label>登录次数</label>
            <span>{{ user.loginCount }} 次</span>
          </div>
          <div class="info-item">
            <label>账户状态</label>
            <span :class="user.isEnabled ? 'status-enabled' : 'status-disabled'">
              {{ user.isEnabled ? '启用' : '禁用' }}
            </span>
          </div>
        </div>

        <div v-if="user.preferences" class="preferences-section">
          <h5>用户偏好设置</h5>
          <div class="preferences-grid">
            <div class="pref-item">
              <label>语言</label>
              <span>{{ user.preferences.language }}</span>
            </div>
            <div class="pref-item">
              <label>时区</label>
              <span>{{ user.preferences.timezone }}</span>
            </div>
            <div class="pref-item">
              <label>主题</label>
              <span>{{ user.preferences.theme }}</span>
            </div>
            <div class="pref-item">
              <label>两步验证</label>
              <span>{{ user.preferences.twoFactorEnabled ? '已启用' : '未启用' }}</span>
            </div>
          </div>
        </div>
      </div>
      
      <div class="dialog-footer">
        <button type="button" class="btn-secondary" @click="closeDialog">
          关闭
        </button>
        <button type="button" class="btn-primary" @click="editUser">
          编辑用户
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { UserDto } from '../api/users'

interface Props {
  visible: boolean
  user: UserDto | null
}

interface Emits {
  (e: 'update:visible', value: boolean): void
  (e: 'edit', user: UserDto): void
}

defineProps<Props>()
const emit = defineEmits<Emits>()

function closeDialog() {
  emit('update:visible', false)
}

function editUser() {
  if (props.user) {
    emit('edit', props.user)
    closeDialog()
  }
}

function getRoleText(role: string) {
  const roleMap: Record<string, string> = {
    admin: '管理员',
    operator: '操作员',
    viewer: '观察员',
    user: '普通用户'
  }
  return roleMap[role] || '未知'
}

function getStatusText(user: UserDto) {
  if (!user.isEnabled) {
    return '禁用'
  }
  
  if (user.lastLoginAt) {
    const lastLogin = new Date(user.lastLoginAt)
    const now = new Date()
    const diffMinutes = (now.getTime() - lastLogin.getTime()) / (1000 * 60)
    
    if (diffMinutes < 30) {
      return '在线'
    } else if (diffMinutes < 1440) {
      return '活跃'
    } else {
      return '离线'
    }
  }
  
  return '离线'
}

function getStatusClass(user: UserDto) {
  if (!user.isEnabled) {
    return 'inactive'
  }
  
  if (user.lastLoginAt) {
    const lastLogin = new Date(user.lastLoginAt)
    const now = new Date()
    const diffMinutes = (now.getTime() - lastLogin.getTime()) / (1000 * 60)
    
    if (diffMinutes < 30) {
      return 'online'
    } else if (diffMinutes < 1440) {
      return 'active'
    } else {
      return 'offline'
    }
  }
  
  return 'offline'
}

function formatDate(dateString: string) {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN')
}
</script>

<style scoped>
.dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.dialog-container {
  background: white;
  border-radius: 12px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
}

.dialog-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.dialog-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #1f2937;
}

.close-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: none;
  background: #f3f4f6;
  border-radius: 6px;
  cursor: pointer;
  color: #6b7280;
  transition: all 0.2s ease;
}

.close-btn i {
  font-size: 16px;
}

.close-btn:hover {
  background: #e5e7eb;
  color: #374151;
}

.dialog-content {
  padding: 24px;
}

.user-info-section {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid #e5e7eb;
}

.user-avatar-large {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0;
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
  font-size: 24px;
}

.user-basic-info h4 {
  margin: 0 0 4px 0;
  font-size: 20px;
  font-weight: 600;
  color: #1f2937;
}

.user-email {
  margin: 0 0 8px 0;
  color: #6b7280;
  font-size: 14px;
}

.user-status {
  margin: 0;
}

.status-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.status-badge.online {
  background: #d1fae5;
  color: #065f46;
}

.status-badge.active {
  background: #dbeafe;
  color: #1e40af;
}

.status-badge.offline {
  background: #f3f4f6;
  color: #6b7280;
}

.status-badge.inactive {
  background: #fee2e2;
  color: #991b1b;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.info-item label {
  font-size: 12px;
  font-weight: 500;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.info-item span {
  font-size: 14px;
  color: #1f2937;
}

.role-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  display: inline-block;
  width: fit-content;
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

.role-badge.user {
  background: rgba(34, 197, 94, 0.1);
  color: #22c55e;
}

.status-enabled {
  color: #22c55e;
  font-weight: 500;
}

.status-disabled {
  color: #ef4444;
  font-weight: 500;
}

.preferences-section {
  border-top: 1px solid #e5e7eb;
  padding-top: 20px;
}

.preferences-section h5 {
  margin: 0 0 16px 0;
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
}

.preferences-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 12px;
}

.pref-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.pref-item label {
  font-size: 12px;
  font-weight: 500;
  color: #6b7280;
}

.pref-item span {
  font-size: 14px;
  color: #1f2937;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px 24px;
  border-top: 1px solid #e5e7eb;
  background: #f9fafb;
}

.btn-secondary,
.btn-primary {
  padding: 10px 20px;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  border: none;
  font-size: 14px;
}

.btn-secondary {
  background: #f3f4f6;
  color: #374151;
  border: 1px solid #d1d5db;
}

.btn-secondary:hover {
  background: #e5e7eb;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover {
  background: #2563eb;
}
</style>
