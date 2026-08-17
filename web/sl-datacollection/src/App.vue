<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <el-container class="layout">
    <el-header class="header">
      <div class="brand">设备数据采集管理</div>
      <nav class="nav">
        <RouterLink to="/" class="nav-item">设备管理</RouterLink>
        <RouterLink to="/collection" class="nav-item">采集服务</RouterLink>
      </nav>
    </el-header>
    <el-main class="main">
      <RouterView />
    </el-main>
  </el-container>
</template>

<style scoped>
.layout {
  min-height: 100vh;
}
.header {
  display: flex;
  align-items: center;
  gap: 24px;
  border-bottom: 1px solid var(--el-border-color-light);
  background: #fff;
}
.brand {
  font-size: 18px;
  font-weight: bold;
}
.nav {
  display: flex;
  gap: 12px;
}
.nav-item {
  padding: 6px 14px;
  border-radius: 4px;
  text-decoration: none;
  color: var(--el-text-color-primary);
}
.nav-item:hover {
  background: var(--el-fill-color-light);
}
.nav-item.router-link-exact-active {
  color: var(--el-color-primary);
  background: var(--el-color-primary-light-9);
}
.main {
  padding: 0;
}
</style>
