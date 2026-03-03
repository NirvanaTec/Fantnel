import { createApp } from 'vue'
import { createHead } from '@vueuse/head'
import App from './App.vue'
import router from './router'
import './style.css'

// FontAwesome 配置
import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faHome, faGamepad, faServer, faTshirt, faPlug, faStore, faNetworkWired, faCog, faUser, faSignIn, faMoon, faSun } from '@fortawesome/free-solid-svg-icons'

// 添加图标到库
library.add(faHome, faGamepad, faServer, faTshirt, faPlug, faStore, faNetworkWired, faCog, faUser, faSignIn, faMoon, faSun)

const app = createApp(App)
const head = createHead()

app.use(router)
app.use(head)
app.component('font-awesome-icon', FontAwesomeIcon)
app.mount('#app')
