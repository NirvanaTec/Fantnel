<template>
  <div v-if="show" class="alert-notice">
    <div class="alert-content">
      <div class="alert-header">
        <strong>{{ title || '提示' }}</strong>
        <button v-if="closable" class="alert-close" @click="handleClose">&times;</button>
      </div>
      <div class="alert-body">
        <ul v-if="Array.isArray(message)">
          <li v-for="(item, index) in message" :key="index">{{ item }}</li>
        </ul>
        <div v-else v-html="message"></div>
      </div>
      <div class="alert-footer">
        <button v-if="showCancel" class="alert-cancel" @click="handleCancel">{{ cancelText }}</button>
        <button class="alert-ok" @click="handleOk">{{ okText }}</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';

// Props
const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  message: {
    type: [String, Array],
    default: ''
  },
  title: {
    type: String,
    default: '提示'
  },
  closable: {
    type: Boolean,
    default: true
  },
  showCancel: {
    type: Boolean,
    default: false
  },
  okText: {
    type: String,
    default: '我知道了'
  },
  cancelText: {
    type: String,
    default: '取消'
  },
  location: {
    type: String,
    default: ''
  }
});

// Emits
const emit = defineEmits(['close', 'ok', 'cancel']);

// Methods
function handleClose() {
  emit('close');
}

function handleOk() {
  emit('ok');
  if (props.location) {
    location.href = props.location;
  }
}

function handleCancel() {
  emit('cancel');
}
</script>

<style scoped>
.alert-notice {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.alert-content {
  background-color: var(--sidebar-bg);
  color: var(--text-color);
  border-radius: 5px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
  width: 400px;
  max-width: 90%;
}

.alert-header {
  padding: 15px 20px;
  border-bottom: 1px solid var(--border-color);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.alert-header strong {
  font-size: 16px;
}

.alert-close {
  background: none;
  border: none;
  font-size: 20px;
  cursor: pointer;
  color: var(--text-color);
  padding: 0;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 3px;
  transition: background-color 0.2s;
}

.alert-close:hover {
  background-color: rgba(0, 0, 0, 0.1);
}

.alert-body {
  padding: 20px;
}

.alert-body ul {
  margin: 0;
  padding-left: 20px;
}

.alert-body li {
  margin: 8px 0;
  line-height: 1.4;
}

.alert-footer {
  padding: 10px 20px;
  border-top: 1px solid var(--border-color);
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.alert-ok {
  padding: 8px 15px;
  background-color: var(--sidebar-active);
  color: white;
  border: none;
  border-radius: 3px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.2s;
}

.alert-ok:hover {
  opacity: 0.9;
}

.alert-cancel {
  padding: 8px 15px;
  background-color: var(--border-color);
  color: var(--text-color);
  border: none;
  border-radius: 3px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.2s;
}

.alert-cancel:hover {
  background-color: rgba(0, 0, 0, 0.1);
}
</style>