<template>
  <span 
    :class="iconClass" 
    :style="iconStyle"
    :title="title"
  >
    <i v-if="useFontAwesome" :class="fontAwesomeClass"></i>
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  name: string
  size?: 'sm' | 'md' | 'lg'
  color?: string
  title?: string
  useFontAwesome?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  color: '#666',
  useFontAwesome: false
})

// 图标名称映射
const iconMap: Record<string, string> = {
  // 设备相关
  'device': 'icon-device',
  'device-online': 'icon-device-online',
  'device-offline': 'icon-device-offline',
  
  // 操作相关
  'add': 'icon-add',
  'edit': 'icon-edit',
  'delete': 'icon-delete',
  'refresh': 'icon-refresh',
  'close': 'icon-close',
  'settings': 'icon-settings',
  
  // 导航相关
  'dashboard': 'icon-dashboard',
  'monitoring': 'icon-monitoring',
  'gateway': 'icon-gateway',
  'user': 'icon-user',
  'logout': 'icon-logout',
  
  // Font Awesome 映射
  'plus': 'fas fa-plus',
  'edit-alt': 'fas fa-edit',
  'trash': 'fas fa-trash',
  'sync': 'fas fa-sync',
  'times': 'fas fa-times',
  'cog': 'fas fa-cog',
  'tachometer-alt': 'fas fa-tachometer-alt',
  'chart-line': 'fas fa-chart-line',
  'server': 'fas fa-server',
  'user-circle': 'fas fa-user-circle',
  'sign-out-alt': 'fas fa-sign-out-alt'
}

// 计算图标类名
const iconClass = computed(() => {
  const baseClass = 'icon'
  const sizeClass = `icon-${props.size}`
  const iconName = iconMap[props.name] || 'icon-device'
  
  if (props.useFontAwesome) {
    return `${baseClass} ${sizeClass} font-awesome-icon`
  } else {
    return `${baseClass} ${sizeClass} ${iconName}`
  }
})

// Font Awesome 类名
const fontAwesomeClass = computed(() => {
  return iconMap[props.name] || 'fas fa-question'
})

// 图标样式
const iconStyle = computed(() => {
  if (props.useFontAwesome) {
    return {
      color: props.color
    }
  } else {
    return {
      filter: `brightness(0) saturate(100%) ${props.color === '#666' ? '' : `invert(${hexToRgb(props.color)})`}`
    }
  }
})

// 十六进制颜色转RGB
function hexToRgb(hex: string): string {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex)
  if (result) {
    const r = parseInt(result[1], 16)
    const g = parseInt(result[2], 16)
    const b = parseInt(result[3], 16)
    return `rgb(${r}, ${g}, ${b})`
  }
  return hex
}
</script>

<style scoped>
.icon {
  display: inline-block;
  vertical-align: middle;
}

.icon-sm {
  width: 14px;
  height: 14px;
}

.icon-md {
  width: 16px;
  height: 16px;
}

.icon-lg {
  width: 20px;
  height: 20px;
}

.font-awesome-icon {
  font-style: normal;
}

.font-awesome-icon i {
  font-size: inherit;
}
</style>
