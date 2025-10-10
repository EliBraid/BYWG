<template>
  <div class="placeholder-container">
    <div class="placeholder-content">
      <div class="placeholder-icon">
        <svg width="64" height="64" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
        </svg>
      </div>
      <h2>{{ title }}</h2>
      <p>{{ description }}</p>
      <div class="placeholder-actions">
        <button class="btn-primary" @click="goBack">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z" fill="currentColor"/>
          </svg>
          返回首页
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

defineProps<{
  title: string
  description: string
}>()

function goBack() {
  router.push('/dashboard')
}
</script>

<style scoped>
.placeholder-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: calc(100vh - 120px);
  width: 100%;
  padding: 40px 20px;
}

.placeholder-content {
  text-align: center;
  max-width: 500px;
  padding: 40px;
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 20px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(0, 0, 0, 0.08);
}

.placeholder-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 120px;
  height: 120px;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  border-radius: 50%;
  margin: 0 auto 24px;
  color: white;
  box-shadow: 0 8px 24px rgba(74, 144, 226, 0.3);
}

.placeholder-content h2 {
  margin: 0 0 16px;
  font-size: 24px;
  font-weight: 700;
  color: #2c3e50;
}

.placeholder-content p {
  margin: 0 0 32px;
  font-size: 16px;
  color: #6c757d;
  line-height: 1.6;
}

.placeholder-actions {
  display: flex;
  gap: 12px;
  justify-content: center;
}

.btn-primary {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(74, 144, 226, 0.4);
}

@media (max-width: 768px) {
  .placeholder-content {
    padding: 30px 20px;
  }
  
  .placeholder-icon {
    width: 80px;
    height: 80px;
  }
  
  .placeholder-content h2 {
    font-size: 20px;
  }
  
  .placeholder-content p {
    font-size: 14px;
  }
}
</style>
