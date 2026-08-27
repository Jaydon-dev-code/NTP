<script setup lang="ts">
import { RouterView } from 'vue-router'
</script>

<template>
  <el-container class="layout">
    <el-header class="header">
      <div class="brand">设备数据采集管理</div>
      <el-menu mode="horizontal" :ellipsis="false" class="nav" router>
        <el-sub-menu index="1">
          <template #title>数采</template>
          <el-menu-item index="/">设备管理</el-menu-item>
          <el-menu-item index="/collection">采集服务</el-menu-item>
        </el-sub-menu>
      </el-menu>
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
  flex: 1;
  border-bottom: none !important;
  height: 100%;
}
.main {
  padding: 0;
}
</style>
