<script setup lang="ts">
</script>

<template>
  <div class="layout">
    <header class="header">
      <div class="brand">
        <span class="title">BYWG</span>
        <span class="subtitle">边缘网关管理</span>
      </div>
      <nav class="nav">
        <RouterLink to="/dashboard">数据仪表板</RouterLink>
        <RouterLink to="/devices">设备管理</RouterLink>
        <RouterLink to="/protocols">协议管理</RouterLink>
        <RouterLink to="/nodes">OPC UA 节点</RouterLink>
      </nav>
    </header>
    <main class="main">
      <RouterView />
    </main>
  </div>
  
</template>

<style scoped>
.layout { display: flex; flex-direction: column; min-height: 100vh; background: #F8F9FA; }
.header { display: flex; justify-content: space-between; align-items: center; padding: 12px 20px; background: #2C3E50; color: #fff; }
.brand { display: flex; align-items: baseline; gap: 10px; }
.title { font-weight: 700; font-size: 20px; }
.subtitle { color: #BDC3C7; font-size: 14px; }
.nav { display: flex; gap: 16px; }
.nav a { color: #fff; text-decoration: none; opacity: 0.9; }
.nav a.router-link-active { font-weight: 600; opacity: 1; border-bottom: 2px solid #42b883; }
.main { padding: 16px; max-width: 1200px; width: 100%; margin: 0 auto; }
</style>
