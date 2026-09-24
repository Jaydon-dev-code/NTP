import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'equipment',
      component: () => import('../views/EquipmentView.vue'),
    },
    {
      path: '/collection',
      name: 'collection',
      component: () => import('../views/CollectionView.vue'),
    },
    {
      path: '/service',
      name: 'service',
      component: () => import('../views/ServiceView.vue'),
    },
    {
      path: '/spc',
      name: 'spc',
      component: () => import('../views/SpcView.vue'),
    },
  ],
})

export default router
