import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './router'

const app = createApp(App)
app.use(router)

// 通用组件
import Alert from './components/Alert.vue'
app.component('Alert', Alert)
import Loading from './components/Loading.vue'
app.component('Loading', Loading)
app.mount('#app')
