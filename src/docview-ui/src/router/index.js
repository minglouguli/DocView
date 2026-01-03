import {
  createRouter,
  createWebHistory,
  createWebHashHistory
} from 'vue-router';
import HomeView from '../views/EntityView.vue';

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/entity',
      name: 'entity',
      component: () => import('../views/EntityView.vue')
    }
  ]
});

export default router;
