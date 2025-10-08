<template>
  <div v-if="visible" class="dialog-overlay" @click="closeDialog">
    <div class="dialog-container" @click.stop>
      <div class="dialog-header">
        <h3>编辑用户</h3>
        <button class="close-btn" @click="closeDialog">
          <i class="fas fa-times"></i>
        </button>
      </div>
      
      <form @submit.prevent="handleSubmit" class="dialog-content">
        <div class="form-group">
          <label for="username">用户名 *</label>
          <input 
            id="username"
            v-model="form.username" 
            type="text" 
            required 
            placeholder="请输入用户名"
            class="form-input"
          />
        </div>
        
        <div class="form-group">
          <label for="email">邮箱 *</label>
          <input 
            id="email"
            v-model="form.email" 
            type="email" 
            required 
            placeholder="请输入邮箱"
            class="form-input"
          />
        </div>
        
        <div class="form-group">
          <label for="fullName">真实姓名</label>
          <input 
            id="fullName"
            v-model="form.fullName" 
            type="text" 
            placeholder="请输入真实姓名"
            class="form-input"
          />
        </div>
        
        <div class="form-group">
          <label for="phone">手机号码</label>
          <input 
            id="phone"
            v-model="form.phone" 
            type="tel" 
            placeholder="请输入手机号码"
            class="form-input"
          />
        </div>
        
        <div class="form-group">
          <label for="department">部门</label>
          <input 
            id="department"
            v-model="form.department" 
            type="text" 
            placeholder="请输入部门"
            class="form-input"
          />
        </div>
        
        <div class="form-group">
          <label for="role">角色 *</label>
          <select id="role" v-model="form.role" required class="form-select">
            <option value="">请选择角色</option>
            <option value="admin">管理员</option>
            <option value="operator">操作员</option>
            <option value="viewer">观察员</option>
            <option value="user">普通用户</option>
          </select>
        </div>
        
        <div class="form-group checkbox-group">
          <label class="checkbox-label">
            <input 
              type="checkbox" 
              v-model="form.isEnabled"
              class="checkbox-input"
            />
            <span class="checkbox-text">启用用户</span>
          </label>
        </div>
      </form>
      
      <div class="dialog-footer">
        <button type="button" class="btn-secondary" @click="closeDialog">
          取消
        </button>
        <button type="submit" class="btn-primary" @click="handleSubmit" :disabled="loading">
          <span v-if="loading">保存中...</span>
          <span v-else>保存更改</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue'
import { updateUser, type UpdateUserRequest, type UserDto } from '../api/users'

interface Props {
  visible: boolean
  user: UserDto | null
}

interface Emits {
  (e: 'update:visible', value: boolean): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const loading = ref(false)

const form = reactive<UpdateUserRequest>({
  username: '',
  email: '',
  fullName: '',
  phone: '',
  department: '',
  role: '',
  isEnabled: true
})

// 监听用户数据变化，更新表单
watch(() => props.user, (newUser) => {
  if (newUser) {
    form.username = newUser.username
    form.email = newUser.email
    form.fullName = newUser.fullName || ''
    form.phone = newUser.phone || ''
    form.department = newUser.department || ''
    form.role = newUser.role
    form.isEnabled = newUser.isEnabled
  }
}, { immediate: true })

function closeDialog() {
  emit('update:visible', false)
}

async function handleSubmit() {
  if (!props.user || !form.username || !form.email || !form.role) {
    alert('请填写所有必填字段')
    return
  }
  
  try {
    loading.value = true
    await updateUser(props.user.id, form)
    alert('用户信息更新成功')
    emit('success')
    closeDialog()
  } catch (error: any) {
    alert('更新用户失败: ' + (error.message || '未知错误'))
  } finally {
    loading.value = false
  }
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
  max-width: 500px;
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

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: 500;
  color: #374151;
  font-size: 14px;
}

.form-input,
.form-select {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  transition: border-color 0.2s ease;
}

.form-input:focus,
.form-select:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.checkbox-group {
  margin-bottom: 0;
}

.checkbox-label {
  display: flex;
  align-items: center;
  cursor: pointer;
  font-weight: normal;
}

.checkbox-input {
  width: 16px;
  height: 16px;
  margin-right: 8px;
  accent-color: #3b82f6;
}

.checkbox-text {
  font-size: 14px;
  color: #374151;
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

.btn-primary:hover:not(:disabled) {
  background: #2563eb;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
