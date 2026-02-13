<template>
  <div class="tooltip-container">
    <slot></slot>
    <span class="tooltip-text" :style="{ width: width, marginLeft: `-${parseInt(width) / 2}px` }">
      <slot name="tooltip"></slot>
    </span>
  </div>
</template>

<script setup>
defineProps({
  width: {
    type: String,
    default: '200px'
  }
})
</script>

<style scoped>
.tooltip-container {
  position: relative;
  display: inline-block;
  cursor: help;
}

.tooltip-text {
  visibility: hidden;
  background-color: rgba(0, 0, 0, 0.8);
  color: #fff;
  text-align: center;
  border-radius: 6px;
  padding: 8px;
  position: absolute;
  z-index: 1;
  bottom: 125%;
  left: 50%;
  opacity: 0;
  transition: opacity 0.3s;
  font-size: 0.8rem;
  white-space: nowrap;
}

.tooltip-container:hover .tooltip-text {
  visibility: visible;
  opacity: 1;
}

.tooltip-text::after {
  content: "";
  position: absolute;
  top: 100%;
  left: 50%;
  margin-left: -5px;
  border-width: 5px;
  border-style: solid;
  border-color: rgba(0, 0, 0, 0.8) transparent transparent transparent;
}
</style>