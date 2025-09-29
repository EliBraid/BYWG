<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { listDevices, type DeviceDto } from '@/api/devices'

const devices = ref<DeviceDto[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

async function fetchDevices() {
  loading.value = true
  error.value = null
  try {
    devices.value = await listDevices()
  } catch (e: any) {
    error.value = e.message || String(e)
  } finally {
    loading.value = false
  }
}

onMounted(fetchDevices)
</script>

<template>
  <div class="panel">
    <h2>设备管理</h2>
    <div class="toolbar">
      <button class="btn" @click="fetchDevices">刷新</button>
      <button class="btn primary" disabled>添加设备（即将支持）</button>
    </div>
    <div v-if="loading">加载中...</div>
    <div v-else>
      <table class="table">
        <thead>
          <tr>
            <th>名称</th>
            <th>IP</th>
            <th>状态</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="d in devices" :key="d.id">
            <td>{{ d.name }}</td>
            <td>{{ d.ip }}</td>
            <td>
              <span :class="['badge', d.status === 'online' ? 'green' : 'red']">{{ d.status || 'offline' }}</span>
            </td>
            <td>
              <button class="btn" disabled>编辑</button>
              <button class="btn danger" disabled>删除</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="error" class="error">{{ error }}</div>
    </div>
  </div>
</template>

<style scoped>
.panel { background: #fff; border: 1px solid #E0E0E0; border-radius: 8px; padding: 16px; }
h2 { margin: 0 0 12px; font-size: 16px; color: #2E3440; }
.toolbar { display: flex; gap: 8px; margin-bottom: 12px; }
.btn { padding: 6px 10px; border: 0; background: #e1e7ef; border-radius: 4px; cursor: pointer; }
.btn.primary { background: #2196F3; color: #fff; }
.btn.danger { background: #F44336; color: #fff; }
.table { width: 100%; border-collapse: collapse; }
.table th, .table td { text-align: left; padding: 8px; border-bottom: 1px solid #eee; }
.badge { padding: 2px 8px; border-radius: 999px; color: #fff; font-size: 12px; }
.badge.green { background: #4CAF50; }
.badge.red { background: #F44336; }
.error { margin-top: 10px; color: #d9534f; }
</style>


