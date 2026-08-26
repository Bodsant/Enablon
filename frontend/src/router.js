import { createRouter, createWebHistory } from "vue-router";

import Login from "./pages/Login.vue";
import Dashboard from "./pages/Dashboard.vue";
import Events from "./pages/Events.vue";
import Assets from "./pages/Assets.vue";

// auth sederhana (sementara) -> simpan token palsu
function isAuthed() {
  return !!localStorage.getItem("token");
}

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", redirect: "/dashboard" },
    { path: "/login", component: Login },
    { path: "/dashboard", component: Dashboard, meta: { requiresAuth: true } },
    { path: "/events", component: Events, meta: { requiresAuth: true } },
    { path: "/assets", component: Assets, meta: { requiresAuth: true } },
  ],
});

// proteksi halaman (kalau belum login -> lempar ke /login)
router.beforeEach((to) => {
  if (to.meta.requiresAuth && !isAuthed()) return "/login";
  if (to.path === "/login" && isAuthed()) return "/dashboard";
  return true;
});

export default router;
