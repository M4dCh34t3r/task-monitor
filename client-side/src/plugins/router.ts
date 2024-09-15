import { useAuthStore } from '@/stores';
import HomeView from '@/views/HomeView.vue';
import LoginView from '@/views/LoginView.vue';
import { createRouter, createWebHashHistory } from 'vue-router';

const routes = [
  {
    path: '/',
    name: 'Home',
    component: HomeView,
    meta: {
      title: 'Home page'
    }
  },
  {
    name: 'Login',
    path: '/Login',
    component: LoginView,
    meta: {
      title: 'Login page'
    }
  }
];

const router = createRouter({
  history: createWebHashHistory(),
  routes
});

router.beforeEach(async (to, from, next) => {
  if (!to.name || !router.hasRoute(to.name)) return next(from);

  const authStore = useAuthStore();

  if (to.name !== 'Login' && !authStore.id) return next('/login');
  if (to.name === 'Login' && authStore.id) return next('/');

  return next();
});

router.afterEach((to) => {
  if (typeof to.meta.title === 'string') document.title = to.meta.title;
});

export default router;
