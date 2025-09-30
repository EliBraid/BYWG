<template>
  <div class="page">
    <div class="header">
      <h2>连接管理</h2>
      <div class="actions">
        <button class="btn" @click="refresh" :disabled="loading">刷新</button>
      </div>
    </div>

    <div class="filters">
      <input class="search" v-model="q" placeholder="搜索名称/类型/状态..." />
      <select v-model="type">
        <option value="all">全部类型</option>
        <option value="OpcUa">OPC UA</option>
        <option value="Mqtt">MQTT</option>
        <option value="Rest">REST</option>
      </select>
      <select v-model="state">
        <option value="all">全部状态</option>
        <option value="Online">在线</option>
        <option value="Offline">离线</option>
        <option value="Error">错误</option>
      </select>
    </div>

    <div class="table">
      <table>
        <thead>
          <tr>
            <th>名称</th>
            <th>类型</th>
            <th>状态</th>
            <th>最后错误</th>
            <th>最后心跳</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in filtered" :key="c.id">
            <td>{{ c.name }}</td>
            <td>{{ c.type }}</td>
            <td>{{ c.status }}</td>
            <td class="muted">{{ c.lastError || '-' }}</td>
            <td class="muted">{{ c.lastHeartbeat || '-' }}</td>
            <td class="ops">
              <button class="btn" @click="reconnect(c)">重连</button>
              <button class="btn" @click="disconnect(c)">断开</button>
              <button class="btn" @click="viewLogs(c)">日志</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'

type Connection = { id:number; name:string; type:'OpcUa'|'Mqtt'|'Rest'; status:'Online'|'Offline'|'Error'; lastError?:string; lastHeartbeat?:string }

const loading = ref(false)
const q = ref('')
const type = ref<'all'|'OpcUa'|'Mqtt'|'Rest'>('all')
const state = ref<'all'|'Online'|'Offline'|'Error'>('all')
const list = ref<Connection[]>([
  { id:1, name:'OPC UA 对接', type:'OpcUa', status:'Online', lastHeartbeat: new Date().toLocaleString() },
  { id:2, name:'MQTT 对接', type:'Mqtt', status:'Offline', lastError:'连接被拒绝' },
  { id:3, name:'REST 对接', type:'Rest', status:'Online', lastHeartbeat: new Date().toLocaleString() }
])

const filtered = computed(()=>{
  return list.value.filter(c=>
    (type.value==='all'||c.type===type.value) &&
    (state.value==='all'||c.status===state.value) &&
    (!q.value || [c.name,c.type,c.status].join(' ').toLowerCase().includes(q.value.toLowerCase()))
  )
})

async function refresh(){ loading.value=true; try { await new Promise(r=>setTimeout(r,300)) } finally { loading.value=false } }
async function reconnect(c:Connection){ await new Promise(r=>setTimeout(r,300)); c.status='Online'; c.lastError=''; c.lastHeartbeat=new Date().toLocaleString() }
async function disconnect(c:Connection){ await new Promise(r=>setTimeout(r,300)); c.status='Offline' }
function viewLogs(c:Connection){ alert(`查看 ${c.name} 的日志（占位）`) }
</script>

<style scoped>
.page { padding:16px; display:flex; flex-direction:column; gap:16px; }
.header { display:flex; align-items:center; justify-content:space-between; }
.actions .btn, .btn { padding:8px 12px; border:1px solid #e6e9ee; border-radius:8px; background:#fff; }
.filters { display:flex; gap:12px; }
.filters .search { padding:8px 12px; border:1px solid #e6e9ee; border-radius:8px; }
.table table { width:100%; border-collapse: collapse; background:#fff; border:1px solid #e6e9ee; border-radius:12px; overflow:hidden; }
.table th, .table td { padding:10px 12px; border-bottom:1px solid #f1f3f5; text-align:left; }
.table thead { background:#f8f9fa; }
.muted { color:#6c757d; }
.ops .btn { margin-right:8px; }
</style>


