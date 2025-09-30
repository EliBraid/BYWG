<template>
  <div class="integrations-container">
    <div class="header">
      <h2>下游集成总览</h2>
      <div class="actions">
        <RouterLink class="btn" to="/integrations/connections">连接管理</RouterLink>
      </div>
    </div>

    <div class="grid">
      <RouterLink class="card" to="/integrations/opcua">
        <div class="icon opcua" />
        <div class="title">OPC UA 集成</div>
        <div class="desc">服务器配置、认证、节点映射与订阅</div>
      </RouterLink>

      <RouterLink class="card" to="/integrations/mqtt">
        <div class="icon mqtt" />
        <div class="title">MQTT 集成</div>
        <div class="desc">Broker配置、认证、主题映射、QoS</div>
      </RouterLink>

      <RouterLink class="card" to="/integrations/rest">
        <div class="icon rest" />
        <div class="title">REST 集成</div>
        <div class="desc">MES/ERP/上位机 API 对接与调用监控</div>
      </RouterLink>
    </div>
  </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
.integrations-container { padding: 16px; display: flex; flex-direction: column; gap: 16px; }
.header { display: flex; align-items: center; justify-content: space-between; }
.actions .btn { padding: 8px 12px; border: 1px solid #e6e9ee; border-radius: 8px; color: #2c3e50; text-decoration: none; }
.grid { display: grid; grid-template-columns: repeat(auto-fill,minmax(280px,1fr)); gap: 16px; }
.card { padding: 16px; background: #fff; border: 1px solid #e6e9ee; border-radius: 12px; text-decoration: none; color: inherit; display: flex; flex-direction: column; gap: 8px; }
.card:hover { box-shadow: 0 8px 24px rgba(0,0,0,0.08); }
.icon { width: 40px; height: 40px; border-radius: 8px; }
.opcua { background: linear-gradient(135deg,#4a90e2,#357abd); }
.mqtt { background: linear-gradient(135deg,#8e44ad,#6c3483); }
.rest { background: linear-gradient(135deg,#27ae60,#1e8449); }
.title { font-weight: 600; font-size: 16px; }
.desc { color: #6c757d; font-size: 13px; }
</style>


