<template>
    <Alert 
        :show="true" 
        message="当前版本可能不包含此内容，请更新至最新版本。" 
        :location="'/'"
    />
</template>

<script setup>
    import Alert from "../components/Alert.vue";
</script>