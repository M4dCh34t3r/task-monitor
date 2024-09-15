import '@/assets/main.css';

import axios from '@/plugins/axios';
import pinia from '@/plugins/pinia';
import router from '@/plugins/router';
import vuetify from '@/plugins/vuetify';
import { createApp } from 'vue';
import App from './App.vue';

createApp(App).use(axios).use(pinia).use(router).use(vuetify).mount('#app');
