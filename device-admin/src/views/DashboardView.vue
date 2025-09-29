<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { listDevices } from '@/api/devices'
type Device = { status: string }

const onlineCount = ref(0)
const offlineCount = ref(0)
const totalCount = ref(0)

async function refreshSummary() {
  try {
    const list = await listDevices() as Device[]
    totalCount.value = list.length
    onlineCount.value = list.filter(d => d.status === 'online').length
    offlineCount.value = totalCount.value - onlineCount.value
  } catch (err) {
    // 后端未启动或代理失败时，保持默认 0 并在控制台提示
    console.error('获取设备列表失败：', err)
    totalCount.value = 0
    onlineCount.value = 0
    offlineCount.value = 0
  }
}

onMounted(() => { refreshSummary() })
</script>

<template>
  <div class="dashboard">
    <section class="panel">
      <h2>设备状态</h2>
      <div class="stats">
        <div class="stat"><span class="dot green"></span> 在线设备：<b>{{ onlineCount }}</b></div>
        <div class="stat"><span class="dot red"></span> 离线设备：<b>{{ offlineCount }}</b></div>
        <div class="stat"><span class="dot orange"></span> 总设备数：<b>{{ totalCount }}</b></div>
      </div>
    </section>

    <section class="panel">
      <h2>协议状态</h2>
      <div class="placeholder">待接入 gRPC-Web 后显示</div>
    </section>

    <section class="panel">
      <h2>报警信息</h2>
      <div class="placeholder">暂无报警</div>
    </section>
  </div>
</template>

<style scoped>
.dashboard { display: grid; gap: 16px; grid-template-columns: 2fr 1fr; }
.panel { background: #fff; border: 1px solid #E0E0E0; border-radius: 8px; padding: 16px; }
h2 { margin: 0 0 12px; font-size: 16px; color: #2E3440; }
.stats { display: grid; gap: 8px; }
.stat { display: flex; align-items: center; gap: 8px; }
.dot { width: 10px; height: 10px; border-radius: 50%; display: inline-block; }
.green { background: green; }
.red { background: red; }
.orange { background: orange; }
.placeholder { color: #888; font-size: 13px; }
@media (max-width: 900px) { .dashboard { grid-template-columns: 1fr; } }
</style>


