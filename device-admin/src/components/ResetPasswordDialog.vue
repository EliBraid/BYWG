<template>
  <div v-if="visible" class="dialog-overlay" @click="closeDialog">
    <div class="dialog-container" @click.stop>
      <div class="dialog-header">
        <h3>重置密码</h3>
        <button class="close-btn" @click="closeDialog">
          <i class="fas fa-times"></i>
        </button>
      </div>
      
      <div class="dialog-content" v-if="user">
        <div class="user-info">
          <div class="user-avatar">
            <div class="avatar-placeholder">
              {{ user.username.charAt(0).toUpperCase() }}
            </div>
          </div>
          <div class="user-details">
            <h4>{{ user.username }}</h4>
            <p>{{ user.email }}</p>
          </div>
        </div>

        <form @submit.prevent="handleSubmit" class="password-form">
          <div class="form-group">
            <label for="currentPassword">当前密码 *</label>
            <input 
              id="currentPassword"
              v-model="form.currentPassword" 
              type="password" 
              required 
              placeholder="请输入当前密码"
              class="form-input"
            />
          </div>
          
          <div class="form-group">
            <label for="newPassword">新密码 *</label>
            <input 
              id="newPassword"
              v-model="form.newPassword" 
              type="password" 
              required 
              placeholder="请输入新密码"
              class="form-input"
              :class="{ 'error': passwordError }"
            />
            <div v-if="passwordError" class="error-message">
              {{ passwordError }}
            </div>
          </div>
          
          <div class="form-group">
            <label for="confirmPassword">确认新密码 *</label>
            <input 
              id="confirmPassword"
              v-model="form.confirmPassword" 
              type="password" 
              required 
              placeholder="请再次输入新密码"
              class="form-input"
              :class="{ 'error': confirmError }"
            />
            <div v-if="confirmError" class="error-message">
              {{ confirmError }}
            </div>
          </div>

          <div class="password-requirements">
            <h5>密码要求：</h5>
            <ul>
              <li :class="{ 'valid': passwordLength }">至少8个字符</li>
              <li :class="{ 'valid': passwordUppercase }">包含大写字母</li>
              <li :class="{ 'valid': passwordLowercase }">包含小写字母</li>
              <li :class="{ 'valid': passwordNumber }">包含数字</li>
              <li :class="{ 'valid': passwordSpecial }">包含特殊字符</li>
            </ul>
          </div>
        </form>
      </div>
      
      <div class="dialog-footer">
        <button type="button" class="btn-secondary" @click="closeDialog">
          取消
        </button>
        <button type="submit" class="btn-primary" @click="handleSubmit" :disabled="loading || !isFormValid">
          <span v-if="loading">重置中...</span>
          <span v-else>重置密码</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import { resetPassword, type ResetPasswordRequest, type UserDto } from '../api/users'

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
const passwordError = ref('')
const confirmError = ref('')

const form = reactive<ResetPasswordRequest & { confirmPassword: string }>({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

// 密码验证
const passwordLength = computed(() => form.newPassword.length >= 8)
const passwordUppercase = computed(() => /[A-Z]/.test(form.newPassword))
const passwordLowercase = computed(() => /[a-z]/.test(form.newPassword))
const passwordNumber = computed(() => /\d/.test(form.newPassword))
const passwordSpecial = computed(() => /[!@#$%^&*(),.?":{}|<>]/.test(form.newPassword))

const isFormValid = computed(() => {
  return form.currentPassword && 
         form.newPassword && 
         form.confirmPassword &&
         passwordLength.value &&
         passwordUppercase.value &&
         passwordLowercase.value &&
         passwordNumber.value &&
         passwordSpecial.value &&
         form.newPassword === form.confirmPassword
})

// 监听密码变化，清除错误信息
watch([() => form.newPassword, () => form.confirmPassword], () => {
  passwordError.value = ''
  confirmError.value = ''
})

function closeDialog() {
  emit('update:visible', false)
  resetForm()
}

function resetForm() {
  form.currentPassword = ''
  form.newPassword = ''
  form.confirmPassword = ''
  passwordError.value = ''
  confirmError.value = ''
}

function validatePassword() {
  if (!passwordLength.value) {
    passwordError.value = '密码至少需要8个字符'
    return false
  }
  if (!passwordUppercase.value) {
    passwordError.value = '密码必须包含大写字母'
    return false
  }
  if (!passwordLowercase.value) {
    passwordError.value = '密码必须包含小写字母'
    return false
  }
  if (!passwordNumber.value) {
    passwordError.value = '密码必须包含数字'
    return false
  }
  if (!passwordSpecial.value) {
    passwordError.value = '密码必须包含特殊字符'
    return false
  }
  return true
}

function validateConfirmPassword() {
  if (form.newPassword !== form.confirmPassword) {
    confirmError.value = '两次输入的密码不一致'
    return false
  }
  return true
}

async function handleSubmit() {
  if (!props.user) return

  // 清除之前的错误
  passwordError.value = ''
  confirmError.value = ''

  // 验证密码
  if (!validatePassword()) {
    return
  }

  if (!validateConfirmPassword()) {
    return
  }

  try {
    loading.value = true
    await resetPassword(props.user.id, form)
    alert('密码重置成功')
    emit('success')
    closeDialog()
  } catch (error: any) {
    alert('重置密码失败: ' + (error.message || '未知错误'))
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

.user-info {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid #e5e7eb;
}

.user-avatar {
  width: 48px;
  height: 48px;
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
  font-size: 18px;
}

.user-details h4 {
  margin: 0 0 4px 0;
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
}

.user-details p {
  margin: 0;
  color: #6b7280;
  font-size: 14px;
}

.password-form {
  margin-bottom: 0;
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

.form-input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  transition: border-color 0.2s ease;
}

.form-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-input.error {
  border-color: #ef4444;
}

.error-message {
  color: #ef4444;
  font-size: 12px;
  margin-top: 4px;
}

.password-requirements {
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 6px;
  padding: 16px;
  margin-bottom: 20px;
}

.password-requirements h5 {
  margin: 0 0 12px 0;
  font-size: 14px;
  font-weight: 600;
  color: #374151;
}

.password-requirements ul {
  margin: 0;
  padding: 0;
  list-style: none;
}

.password-requirements li {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
  font-size: 13px;
  color: #6b7280;
}

.password-requirements li:before {
  content: '✗';
  color: #ef4444;
  font-weight: bold;
}

.password-requirements li.valid {
  color: #22c55e;
}

.password-requirements li.valid:before {
  content: '✓';
  color: #22c55e;
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
